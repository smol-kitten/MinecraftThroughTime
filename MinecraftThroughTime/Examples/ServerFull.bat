@echo off

:: if MTT.exe use it, else use MinecraftThroughTime.exe
if exist MTT.exe set MTT=MTT.exe
if not exist MTT.exe set MTT=MinecraftThroughTime.exe

echo Updating MinecraftThroughTime...
%MTT% update server

set jarfile=server.jar
set arg_file=include.txt

REM Load arguments from file
if exist %arg_file% (
    for /f "delims=" %%a in (%arg_file%) do set args=%%a
)

echo Starting Minecraft server...

java -Xms4096M -Xmx4096M -jar %jarfile% %args% nogui
pause
