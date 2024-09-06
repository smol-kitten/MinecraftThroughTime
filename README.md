# MinecraftThroughTime

This tool enables you to create and use a profile to set a Minecraft Profile or Server to a specific game version. The profiles contains information about dates and the versions to use at that date.

Using this you can make a Singleplayer or Multiplayer experience to relive old versions of Minecraft. Using a Profile which can be loaded from URL you can have a shared experience with friends or a community without much hassle to communicate which version to use.

Should be Feature complete, but the project we planned did not start.
So this tool is mostly untested and might wont receive any updates.

## Features
- Create a Profile with a list of dates and versions
- Load a Profile from a URL or File
- Set a specified version in the Minecraft Launcher
- Replace a Server Jar with a specified version
- Ignore the Profile and set a custom version from command line
- Log4J fix for affected server versions
- Chaches all downloaded files for faster access (profiles will still be downloaded on every use)

## Usage
The tool is a command line tool made for windows and can be used with the following commands:

#### Log4J Fix:
A "include.txt" is placed next to the server jar.
If a fix is needed, the tool write appropriate log4j fix arguments in it.
You can manually add them to the server start command when needed, or auto include the file.
If additional files are needed, they will be placed in the same directory, e.g. log4j2_112-116.xml and log4j2_7-112.xml.
The "include.txt" will use them when necessary.
A Example to auto include the file is provided in the Examples folder in serverstart.bat and in ServerFull.bat.

#### Update:
```shell
update server/client [-f <profileFile/url>] [-j <serverJarPath>] -i
    -f uses profile from path, tries relative profile.json if not provided. Can also use url to download profile
    -j uses server jar from path, tries relative server.jar if not provided
    -v force version
```

#### Make:
```shell
make [-f <version_manifestv2.json>] [-o <outputFile>] -t [old_alpha,old_beta,snapshot,release] [-s (only versions with server)] -i <interval>
    -f uses version_manifestv2.json from path
        if not given, tries %appdata%\.minecraft\versions\version_manifest_v2.json
    -o output file
        fallsback to relative profile.json
    -t types of versions to use, comma seperated
    -s only versions with server version avaliable
    -i interval in days between version changes, use -1 to only increment manually
    -u unoffical, allow server jar´s i found on the internet MIGHT BE INSECURE
```

#### Cache Management:
```shell
cache clean/open/list
    clean deletes cache
    open opens cache directory
    list lists cache directory
```

#### Bake a Profile into the executable:
```shell
bake <url/path>
    Bakes a profile path or url into the executable
    Usefull for portable applications
    Currently only path to a profile not full profile.
```

## Examples

#### Make a Profile with all versions from the version_manifestv2.json with server support and an interval of 7 days:
```shell
make -f version_manifestv2.json -o output -t old_alpha,old_beta,snapshot,release -s -i 7
```

#### Update a Server with a Profile from a file:
```shell
update server -f profile.json -j server.jar
```

#### Update the Client with a Profile from a URL:
```shell
update client -f https://example.com/profile.json
```

#### Update a Client with a specified version:
```shell
update client -v 1.17
```

#### Remove all cached files:
```shell
cache clean
```

## Example files:
- ### makeMultiplayer.bat
    - Creates a Profile using old_alpha,old_beta,release with server support and an interval of 3 days.
- ## ServerFull.bat
    - Updates the Server with a Profile from a profile file relative to the batch and includes the log4j fix. Starts a Server with 4gb of ram after the update.
- ## serverstart.bat
    - Just starts the Server with 1gb of ram and includes the log4j fix. No update is done.
- ## updateClient.bat
    - Updates the Client with a Profile from a profile relative to the batch.
- ## updateServer.bat
    - Updates the Server with a Profile from a profile relative to the batch.