# MinecraftThroughTime

[![.NET Build](https://github.com/smol-kitten/MinecraftThroughTime/actions/workflows/dotnet.yml/badge.svg)](https://github.com/smol-kitten/MinecraftThroughTime/actions/workflows/dotnet.yml)

A command-line tool that lets you create and use profiles to set a Minecraft client or server to a specific game version. Profiles contain date-based version schedules, allowing you to automatically progress through Minecraft's version history over time.

Use it to create a singleplayer or multiplayer experience that relives old versions of Minecraft. Profiles can be loaded from a URL, so friends or a community can share the same version schedule without manual coordination.

## Features

- Create a profile with a list of dates and versions
- Load a profile from a URL or file
- Set a specified version in the Minecraft Launcher
- Replace a server jar with a specified version
- Override the profile and set a custom version from the command line
- Log4J fix for affected server versions
- Caches all downloaded files for faster access (profiles are re-downloaded on every use)

## Usage

The tool is a Windows command-line application with the following commands:

### Update

```shell
update server/client [-f <profileFile/url>] [-j <serverJarPath>] [-v <version>] [-i]
    -f  uses profile from path, tries relative profile.json if not provided. Can also use a url to download the profile
    -j  uses server jar from path, tries relative server.jar if not provided
    -v  force a specific version
    -i  increment version, ignores date and sets next version according to profile
```

### Make

```shell
make [-f <version_manifestv2.json>] [-o <outputFile>] -t [old_alpha,old_beta,snapshot,release] [-s] -i <interval> [-u]
    -f  uses version_manifestv2.json from path
        if not given, tries %appdata%\.minecraft\versions\version_manifest_v2.json
    -o  output file, falls back to relative profile.json
    -t  types of versions to use, comma-separated
    -s  only versions with a server jar available
    -i  interval in days between version changes, use -1 to only increment manually
    -u  unofficial, allow server jars sourced from the internet (may be insecure)
```

### Cache Management

```shell
cache clean/open/list
    clean   deletes cache
    open    opens cache directory
    list    lists cache directory
```

### Bake a Profile into a Copy of the Executable

```shell
bake <url/path> [full]
    Bakes a profile path or url into a copy of the executable.
    Useful for portable applications.
    Use 'full' to embed the entire profile instead of just a path.
```

`bake` writes two files next to the executable:

| bake | executable | profile |
|---|---|---|
| `bake <url/path>` | `MinecraftThroughTime_PathBaked.exe` | `MinecraftThroughTime_PathBaked.mttprofile` |
| `bake <url/path> full` | `MinecraftThroughTime_FullyBaked.exe` | `MinecraftThroughTime_FullyBaked.mttprofile` |

Keep the two files together. The baked executable is a byte-identical copy of the original, so its signature stays valid (see [Signed releases](#signed-releases)).
Copies baked by versions before v1.1.0 have the profile appended to the executable itself. They still work, but they are not signed.

### Log4J Fix

When updating a server, an `include.txt` file is placed next to the server jar.
If a fix is needed, the tool writes the appropriate Log4J fix arguments into it.
You can manually add them to the server start command, or auto-include the file.
If additional files are needed (e.g. `log4j2_112-116.xml`, `log4j2_7-112.xml`), they will be placed in the same directory.
See `serverstart.bat` and `ServerFull.bat` in the Examples folder for auto-include usage.

## Signed releases

From v1.1.0, `MinecraftThroughTime.exe` in each [release](https://github.com/smol-kitten/MinecraftThroughTime/releases) has an Authenticode signature with an RFC 3161 timestamp.
CI signs the exe after the build and verifies it on Linux and on Windows before the release is published.

Each release contains:

| file | content |
|---|---|
| `MinecraftThroughTime.exe` | the signed executable |
| `SHA256SUMS` | SHA-256 of the signed files |
| `SIGNATURES.md` | signed files, their SHA-256, and the root fingerprint |
| `r0.crt` | the root certificate the signature chains to |
| `*.bat` | the example scripts (not signed; they are text files) |

### The chain

```
Marc Schneider Root CA R0 (Staging)          SHA-256 ed5eaa1b7a66e154e8c82a3d45f65ddd36a3fbbad1f41a68d2bf4334ccc2d375
└─ catboy.systems CA CB0 (Staging)
   └─ catboy.systems Code Signing CA CS0-2026 (Staging)
      └─ catboy.systems Runner CA <runner> 2026
         └─ smol-kitten/MinecraftThroughTime (CA for this repository on that runner)
            └─ smol-kitten/MinecraftThroughTime (1-hour signing certificate for one CI run)
Timestamp: catboy.systems TSA0 (Staging), under catboy.systems Timestamp CA T0 (Staging), under the same root
```

The signing certificate is valid for one hour only. The timestamp proves that the exe was signed while the certificate was valid, so the signature stays valid after the certificate expires.

R0 is a **staging** root. No operating system trusts it by default.

### Verify on Windows

```powershell
Get-AuthenticodeSignature .\MinecraftThroughTime.exe | Format-List Status, StatusMessage, SignerCertificate, TimeStamperCertificate
```

| `Status` | meaning |
|---|---|
| `UnknownError` or `NotTrusted` | The signature and the file are intact, but Windows does not trust the staging root. This is the expected result on a normal machine (CI measured `UnknownError`). |
| `Valid` | The signature chains to a root that this machine trusts (for example, after you imported R0 as shown below). |
| `HashMismatch` | The file was changed after it was signed. Do not use it. |
| `NotSigned` | The file has no signature. It is not a release file, or it is a copy baked by a version before v1.1.0. |

Also compare the SHA-256 with `SHA256SUMS`:

```powershell
(Get-FileHash .\MinecraftThroughTime.exe -Algorithm SHA256).Hash
```

#### Trust R0 on a test machine (optional)

Do this only on a test machine. A trusted root can make Windows trust anything that the root signs.

1. Download `r0.crt` from the release.
2. Make sure that its SHA-256 fingerprint is `ED5EAA1B7A66E154E8C82A3D45F65DDD36A3FBBAD1F41A68D2BF4334CCC2D375`:

   ```powershell
   (New-Object Security.Cryptography.X509Certificates.X509Certificate2 (Resolve-Path .\r0.crt)).GetCertHashString('SHA256')
   ```

3. Import it into the root store of the current user (no administrator rights necessary):

   ```powershell
   Import-Certificate -FilePath .\r0.crt -CertStoreLocation Cert:\CurrentUser\Root
   ```

4. Run `Get-AuthenticodeSignature` again. The status is now `Valid`.

To remove R0 again:

```powershell
Get-ChildItem Cert:\CurrentUser\Root | Where-Object { $_.GetCertHashString('SHA256') -eq 'ED5EAA1B7A66E154E8C82A3D45F65DDD36A3FBBAD1F41A68D2BF4334CCC2D375' } | Remove-Item
```

MinecraftThroughTime itself never reads or changes a certificate store.

### Verify on Linux

You need `osslsigncode` (2.x) and `openssl`. Download the release files and the three public certificates, then check the root fingerprint before you use it:

```sh
# the server sends DER; osslsigncode reads PEM only
for c in r0 cb0 t0; do curl -fsS "http://pki.catboy.systems/certs/$c.crt" | openssl x509 -inform DER -out "$c.pem"; done
openssl x509 -in r0.pem -outform DER | sha256sum   # must be ed5eaa1b7a66e154e8c82a3d45f65ddd36a3fbbad1f41a68d2bf4334ccc2d375
cat r0.pem cb0.pem t0.pem > tsa-ca.pem
osslsigncode verify -in MinecraftThroughTime.exe -CAfile r0.pem -TSA-CAfile tsa-ca.pem
sha256sum -c SHA256SUMS
```

`osslsigncode` must report `Signature verification: ok` and a timestamp. [`tools/verify-release.sh`](tools/verify-release.sh) does the same checks (and the revocation lists) for a directory: `tools/verify-release.sh <release-dir> <dir-with-r0-cb0-t0>`.

### SmartScreen

Windows SmartScreen can show "Windows protected your PC" and "Unknown publisher" when you start the exe. The reason is the staging root: Windows does not know the publisher. This stays so until a production root is available. The signature still lets you check that the file is the one that CI built.

## Examples

#### Make a profile with all versions that have server support, changing every 7 days:

```shell
make -f version_manifestv2.json -o output -t old_alpha,old_beta,snapshot,release -s -i 7
```

#### Update a server with a profile from a file:

```shell
update server -f profile.json -j server.jar
```

#### Update the client with a profile from a URL:

```shell
update client -f https://example.com/profile.json
```

#### Update the client to a specific version:

```shell
update client -v 1.17
```

#### Remove all cached files:

```shell
cache clean
```

## Example Files

| File | Description |
|------|-------------|
| `makeMultiplayer.bat` | Creates a profile using `old_alpha`, `old_beta`, `release` with server support and an interval of 3 days |
| `ServerFull.bat` | Updates the server from a profile, applies the Log4J fix, and starts a server with 4 GB of RAM |
| `serverstart.bat` | Starts the server with 1 GB of RAM and includes the Log4J fix (no update) |
| `updateClient.bat` | Updates the client with a profile relative to the batch file |
| `updateServer.bat` | Updates the server with a profile relative to the batch file |
| `incrementClient.bat` | Advances the client to the next version in the profile, ignoring the date |
| `MakeProfile.bat` | Creates a profile with all versions available in the launcher |