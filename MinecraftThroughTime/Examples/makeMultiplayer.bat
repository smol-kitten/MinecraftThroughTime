@echo off

:: if MTT.exe use it, else use MinecraftThroughTime.exe
if exist MTT.exe set MTT=MTT.exe
if not exist MTT.exe set MTT=MinecraftThroughTime.exe

echo Making MinecraftThroughTime Multiplayer Profile...
%MTT% make -s -i 3 -t old_alpha,old_beta,release
echo Done!
pause



