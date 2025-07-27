@echo off
SETLOCAL EnableDelayedExpansion

set "BATPATH=%~dp0"
set "SAMPLEPATH=!BATPATH!..\Xml\Sample\"

set SAMPLEZIP="!BATPATH!..\Assets\Sample.zip"
if exist "!SAMPLEZIP!" del /f /q "!SAMPLEZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!SAMPLEZIP!" "!SAMPLEPATH!*" || EXIT /b 1

echo !SAMPLEZIP! successfully created
exit /b 0
