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
if exist "!PACKAGEPATH!" rmdir /s /q "!PACKAGEPATH!"
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
copy "!BATPATH!..\EdiabasLib\CanAdapterElm\CanAdapterElm.X\dist\default\production\*.hex" "!CANADAPTERELMPATH!"

set PACKAGEZIP="!BATPATH!Android-!DATESTR!.zip"
if exist "!PACKAGEZIP!" del /f /q "!PACKAGEZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!PACKAGEZIP!" "!PACKAGEPATH!*"
