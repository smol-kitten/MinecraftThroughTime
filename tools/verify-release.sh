#!/usr/bin/env bash
# Verify a signed MinecraftThroughTime release directory against the pinned catboy.systems root.
# CI runs this on the signed artifact; the README tells people to run the same file on a download.
# Fails (exit 1) on ANY file that does not verify.
#
#   tools/verify-release.sh <dir> <pki-dir>     (pki-dir holds r0.crt, cb0.crt, t0.crt from http://pki.catboy.systems/certs/)
#
# Checks:
#   *.exe          Authenticode chain to R0 + RFC 3161 timestamp + CRLs (osslsigncode verify)
#   SIGNATURES.md  every listed sha256 matches the file next to it
# Recipe: workcollection/fleet-actions README §"Verifying a signed release".
set -uo pipefail

DIR=${1:?usage: verify-release.sh <dir> <pki-dir>}
PKI=${2:?usage: verify-release.sh <dir> <pki-dir>}
R0_SHA256=ed5eaa1b7a66e154e8c82a3d45f65ddd36a3fbbad1f41a68d2bf4334ccc2d375   # staging R0, pinned by catboy-sign

fail=0; ok=0
pass() { ok=$((ok+1)); echo "ok    $1"; }
bad()  { fail=$((fail+1)); echo "FAIL  $1"; }

fp=$(openssl x509 -in "$PKI/r0.crt" -outform DER | sha256sum | cut -c1-64)
[ "$fp" = "$R0_SHA256" ] || { echo "FAIL  r0.crt fingerprint $fp is not the pinned $R0_SHA256"; exit 1; }
echo "root  r0.crt sha256 $fp (staging R0)"
echo "tool  $(osslsigncode --version 2>&1 | head -1)"

tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
cat "$PKI/r0.crt" "$PKI/cb0.crt" "$PKI/t0.crt" > "$tmp/tsa-ca.pem"

# CRLs: fetched once per URL with curl (named User-Agent, retries) instead of by osslsigncode, whose own
# fetches send an empty User-Agent and can fail behind the WAF. Revocation is still checked:
# -ignore-cdp only stops osslsigncode's download, the CRLs come in via -CRLfile.
crls() { # <signed file> → $tmp/crls.pem holds the CRL of every certificate in its signature (incl. the TSA chain)
  local f=$1 url h
  : > "$tmp/crls.pem"
  rm -f "$tmp/sig.der"   # extract-signature refuses to overwrite
  osslsigncode extract-signature -in "$f" -out "$tmp/sig.der" >/dev/null 2>&1 || return 1
  for url in $(grep -aoE 'https?://[A-Za-z0-9./_-]+\.crl' "$tmp/sig.der" | sort -u); do
    h=$(sha256sum <<<"$url" | cut -c1-16)
    if [ ! -s "$tmp/crl-$h.pem" ]; then
      curl -fsS -m 20 --retry 3 --retry-all-errors -A "mtt-verify-release/1" -o "$tmp/crl-$h.der" "$url" \
        && openssl crl -inform DER -in "$tmp/crl-$h.der" -out "$tmp/crl-$h.pem" 2>/dev/null \
        || { echo "      CRL not available: $url"; return 1; }
    fi
    cat "$tmp/crl-$h.pem" >> "$tmp/crls.pem"
  done
}

while IFS= read -r -d '' f; do
  out=
  if crls "$f" && out=$(osslsigncode verify -in "$f" -CAfile "$PKI/r0.crt" -TSA-CAfile "$tmp/tsa-ca.pem" \
       -ignore-cdp -CRLfile "$tmp/crls.pem" -TSA-CRLfile "$tmp/crls.pem" 2>&1) \
     && grep -q 'Signature verification: ok' <<<"$out" && ! grep -q 'Timestamp is not available' <<<"$out"; then
    pass "${f#"$DIR"/}  Authenticode + timestamp"
  else
    bad "${f#"$DIR"/}  Authenticode"; sed 's/^/      /' <<<"${out:-}" | tail -15
  fi
done < <(find "$DIR" -type f -name '*.exe' -print0 | sort -z)

# SIGNATURES.md rows: | `path` | sha256 | kind |  — paths are relative to the signed artifact root.
while IFS= read -r sig; do
  base=$(dirname "$sig")
  while IFS='|' read -r _ path hash _; do
    path=$(tr -d ' `' <<<"$path"); hash=$(tr -d ' ' <<<"$hash")
    [[ $hash =~ ^[0-9a-f]{64}$ ]] || continue
    f=$(find "$base" -maxdepth 3 -type f -path "*/$path" -print -quit)
    if [ -z "$f" ]; then bad "${sig#"$DIR"/}: $path not found"; continue; fi
    [ "$(sha256sum "$f" | cut -c1-64)" = "$hash" ] && pass "${sig#"$DIR"/}: $path sha256" || bad "${sig#"$DIR"/}: $path sha256 differs"
  done < "$sig"
done < <(find "$DIR" -type f -name 'SIGNATURES.md')

echo "verified $ok, failed $fail"
[ "$ok" -gt 0 ] && [ "$fail" = 0 ]
