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

### Bake a Profile into the Executable

```shell
bake <url/path> [full]
    Bakes a profile path or url into the executable.
    Useful for portable applications.
    Use 'full' to embed the entire profile instead of just a path.
```

### Log4J Fix

When updating a server, an `include.txt` file is placed next to the server jar.
If a fix is needed, the tool writes the appropriate Log4J fix arguments into it.
You can manually add them to the server start command, or auto-include the file.
If additional files are needed (e.g. `log4j2_112-116.xml`, `log4j2_7-112.xml`), they will be placed in the same directory.
See `serverstart.bat` and `ServerFull.bat` in the Examples folder for auto-include usage.

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