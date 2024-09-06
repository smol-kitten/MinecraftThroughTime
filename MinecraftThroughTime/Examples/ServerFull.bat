@echo off

echo Updating MinecraftThroughTime...
MinecraftThroughTime.exe update server

set jarfile=server.jar
set arg_file=include.txt

REM Load arguments from file
if exist %arg_file% (
    for /f "delims=" %%a in (%arg_file%) do set args=%%a
)

echo Starting Minecraft server...

java -Xms4096M -Xmx4096M -jar %jarfile% %args% nogui
pause
