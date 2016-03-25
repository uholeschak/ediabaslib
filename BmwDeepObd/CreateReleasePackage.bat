@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0

set DATESTR=%date:~6,4%%date:~3,2%%date:~0,2%
echo !DATESTR!
set PACKAGEPATH="!BATPATH!Package\"
set CONFIGPATH="!PACKAGEPATH!Config\"
set ECUPATH="!PACKAGEPATH!Ecu\"
set CANADAPTERPATH="!PACKAGEPATH!CanAdapter\"
set CANADAPTERELMPATH="!PACKAGEPATH!CanAdapterElm\"
set CANADAPTERELM_DEFAULT_PATH="!PACKAGEPATH!CanAdapterElm\default\"
set CANADAPTERELM_FAST_PATH="!PACKAGEPATH!CanAdapterElm\fast\"
if exist "!PACKAGEPATH!" rmdir /s /q "!PACKAGEPATH!"
timeout /T 1 /NOBREAK > nul
mkdir "!PACKAGEPATH!"
copy "!BATPATH!ReadMe.txt" "!PACKAGEPATH!"

mkdir "!CONFIGPATH!"
xcopy /y /e "!BATPATH!Xml\*.*" "!CONFIGPATH!"

mkdir "!ECUPATH!"
copy "!BATPATH!..\EdiabasLib\Test\Ecu\adapter_prg.prg" "!ECUPATH!"

mkdir "!CANADAPTERPATH!"
copy "!BATPATH!..\EdiabasLib\CanAdapter\CanAdapter\Release\*.hex" "!CANADAPTERPATH!"
copy "!BATPATH!..\EdiabasLib\CanAdapter\Pld\*.jed" "!CANADAPTERPATH!"
copy "!BATPATH!..\EdiabasLib\CanAdapter\UpdateLoader\bin\*.exe" "!CANADAPTERPATH!"

mkdir "!CANADAPTERELMPATH!"
mkdir "!CANADAPTERELM_DEFAULT_PATH!"
mkdir "!CANADAPTERELM_FAST_PATH!"
copy "!BATPATH!..\EdiabasLib\CanAdapterElm\CanAdapterElm.X\dist\default\production\*.hex" "!CANADAPTERELM_DEFAULT_PATH!"
copy "!BATPATH!..\EdiabasLib\CanAdapterElm\CanAdapterElm.X\dist\fast\production\*.hex" "!CANADAPTERELM_FAST_PATH!"

set PACKAGEZIP="!BATPATH!Android-!DATESTR!.zip"
if exist "!PACKAGEZIP!" del /f /q "!PACKAGEZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!PACKAGEZIP!" "!PACKAGEPATH!*"
