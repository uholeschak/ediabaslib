@echo off
SETLOCAL EnableDelayedExpansion

set "BATPATH=%~dp0"
set "WEBCLIENT_SRC_PATH=!BATPATH!..\WebPsdzClient\"
set "WEBCLIENT_DST_PATH=%USERPROFILE%\Documents\My Web Sites\WebPsdzClient\"
set "RPC_SERVER_SRC_PATH=!BATPATH!..\PsdzRpcServer\artifacts\bin\PsdzRpcServer\debug_net10.0-windows10.0.26100.0\"
set "RPC_SERVER_DST_PATH=%USERPROFILE%\Documents\PsdzRpcServer\"

set "CONFIG_SRC_FILE=!BATPATH!applicationhost.config"
set "CONFIG_DST_PATH=%USERPROFILE%\Documents\IISExpress\config\"

echo WebPsdzClient dir: "!WEBCLIENT_DST_PATH!"
if exist "!WEBCLIENT_DST_PATH!" rmdir /s /q "!WEBCLIENT_DST_PATH!"
xcopy /y /q /s /e "!WEBCLIENT_SRC_PATH!*.*" "!WEBCLIENT_DST_PATH!" || EXIT /b 1

echo PsdzRpcServer dir: "!RPC_SERVER_DST_PATH!"
if exist "!RPC_SERVER_DST_PATH!" rmdir /s /q "!RPC_SERVER_DST_PATH!"
xcopy /y /q /s /e "!RPC_SERVER_SRC_PATH!*.*" "!RPC_SERVER_DST_PATH!" || EXIT /b 1

echo Config dir: "!CONFIG_DST_PATH!"
xcopy /y /q "!CONFIG_SRC_FILE!" "!CONFIG_DST_PATH!" || EXIT /b 1

echo WebPsdzClient files copied

timeout /T 2
exit /b 0
