@echo off

:: if MTT.exe use it, else use MinecraftThroughTime.exe
if exist MTT.exe set MTT=MTT.exe
if not exist MTT.exe set MTT=MinecraftThroughTime.exe

echo Making MinecraftThroughTime Profile...
echo Update every 7 days, and include old_alpha, old_beta, and release versions.
echo All versions not only with server versions.
%MTT% make -i 7 -t old_alpha,old_beta,release 
pause

