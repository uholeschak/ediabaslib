@echo off
SETLOCAL EnableDelayedExpansion

set "BATPATH=%~dp0"
set "WEBCLIENT_SRC_PATH=!BATPATH!..\WebPsdzClient\"
set "WEBCLIENT_DST_PATH=%USERPROFILE%\Documents\My Web Sites\WebPsdzClient\"

echo WebPsdzClient dir: "!WEBCLIENT_DST_PATH!"

if exist "!WEBCLIENT_DST_PATH!" rmdir /s /q "!WEBCLIENT_DST_PATH!"

xcopy /y /q "!WEBCLIENT_SRC_PATH!*.*" "!WEBCLIENT_DST_PATH!" || EXIT /b 1

echo WebPsdzClient files copied
exit /b 0
