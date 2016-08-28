@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0

set DATESTR=%date:~6,4%%date:~3,2%%date:~0,2%
echo !DATESTR!
set PACKAGEPATH="!BATPATH!Package\"
set EDIABASTESTPATH="!PACKAGEPATH!EdiabasTest\"
set API32PATH="!PACKAGEPATH!Api32\"
set APINET32PATH="!PACKAGEPATH!ApiNet32\"
set CANADAPTERPATH="!PACKAGEPATH!CanAdapter\"
set ENETADAPTERPATH="!PACKAGEPATH!EnetAdapter\"
if exist "!PACKAGEPATH!" rmdir /s /q "!PACKAGEPATH!"
timeout /T 1 /NOBREAK > nul
mkdir "!PACKAGEPATH!"

mkdir "!EDIABASTESTPATH!"
copy "!BATPATH!EdiabasTest\bin\Release\EdiabasTest.exe" "!EDIABASTESTPATH!"
copy "!BATPATH!EdiabasTest\bin\Release\*.dll" "!EDIABASTESTPATH!"
copy "!BATPATH!EdiabasTest\bin\Release\*.config" "!EDIABASTESTPATH!"

mkdir "!API32PATH!"
copy "!BATPATH!Api32\Release\*.dll" "!API32PATH!"
copy "!BATPATH!EdiabasTest\bin\Release\*.config" "!API32PATH!"

mkdir "!APINET32PATH!"
copy "!BATPATH!apiNET32\bin\Release\*.dll" "!APINET32PATH!"
copy "!BATPATH!apiNET32\bin\Release\*.config" "!APINET32PATH!"

mkdir "!CANADAPTERPATH!"
copy "!BATPATH!CanAdapter\CanAdapter\Release\*.hex" "!CANADAPTERPATH!"
copy "!BATPATH!CanAdapter\Pld\*.jed" "!CANADAPTERPATH!"
copy "!BATPATH!CanAdapter\UpdateLoader\bin\*.exe" "!CANADAPTERPATH!"

mkdir "!ENETADAPTERPATH!"
copy "!BATPATH!EnetAdapter\Release\mini.bin" "!ENETADAPTERPATH!"
copy "!BATPATH!EnetAdapter\Release\openwrt*.bin" "!ENETADAPTERPATH!"
copy "!BATPATH!EnetAdapter\Release\*.img" "!ENETADAPTERPATH!"

set PACKAGEZIP="!BATPATH!Binaries-!DATESTR!.zip"
if exist "!PACKAGEZIP!" del /f /q "!PACKAGEZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!PACKAGEZIP!" "!PACKAGEPATH!*"
