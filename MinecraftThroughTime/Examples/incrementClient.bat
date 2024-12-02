@echo off

:: if MTT.exe use it, else use MinecraftThroughTime.exe
if exist MTT.exe set MTT=MTT.exe
if not exist MTT.exe set MTT=MinecraftThroughTime.exe

echo Updating MinecraftThroughTime...
%MTT% update client -i
echo Done!
pause
