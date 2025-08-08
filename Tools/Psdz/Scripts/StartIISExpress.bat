@echo off
SETLOCAL EnableDelayedExpansion

set "BATPATH=%~dp0"

"C:\Program Files\IIS Express\iisexpress.exe" /site:"WebPsdzClient" || EXIT /b 1

echo Started WebPsdzClient with IIS express
exit /b 0
