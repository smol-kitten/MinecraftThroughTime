@echo off

set jarfile=server.jar
set arg_file=include.txt

REM Load arguments from file
if exist %arg_file% (
    for /f "delims=" %%a in (%arg_file%) do set args=%%a
)

echo Starting Minecraft server...

echo Start Parameters: 
echo java -Xms1024M -Xmx1024M -jar %jarfile% %args% nogui

java -Xms1024M -Xmx1024M -jar %jarfile% %args% nogui
pause
