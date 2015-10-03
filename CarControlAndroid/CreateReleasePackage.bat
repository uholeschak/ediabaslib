@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0

set DATESTR=%date:~6,4%%date:~3,2%%date:~0,2%
echo !DATESTR!
set PACKAGEPATH="!BATPATH!Package\"
set APKPATH="!PACKAGEPATH!Apk\"
set CONFIGPATH="!PACKAGEPATH!Config\"
set ECUPATH="!CONFIGPATH!Ecu\"
if exist "!PACKAGEPATH!" rmdir /s /q "!PACKAGEPATH!"
mkdir "!PACKAGEPATH!"
copy "!BATPATH!ReadMe.txt" "!PACKAGEPATH!"

mkdir "!APKPATH!"
copy "!BATPATH!de.holeschak.bmwdiagnostics-Aligned.apk" "!APKPATH!"

mkdir "!CONFIGPATH!"
xcopy /y /e "!BATPATH!Xml\*.*" "!CONFIGPATH!"

mkdir "!ECUPATH!"
copy "!BATPATH!..\EdiabasLib\Test\Ecu\adapter_prg.prg" "!ECUPATH!"

set PACKAGEZIP="!BATPATH!Android-!DATESTR!.zip"
if exist "!PACKAGEZIP!" del /f /q "!PACKAGEZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!PACKAGEZIP!" "!PACKAGEPATH!*"
