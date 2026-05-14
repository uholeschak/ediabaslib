@echo off
SETLOCAL EnableDelayedExpansion

set "BATPATH=%~dp0"
set "RPC_SERVER_BIN=!BATPATH!..\PsdzRpcServer\artifacts\bin\PsdzRpcServer\debug_net10.0-windows10.0.26100.0\PsdzRpcServer.exe"

echo PsdzRpcServer: "!RPC_SERVER_BIN!"
if NOT EXIST "!RPC_SERVER_BIN!" (
    echo Server not found: !RPC_SERVER_BIN!
    exit /b 1
)

"!RPC_SERVER_BIN!" -p 0 -s || EXIT /b 1

timeout /T 2
exit /b 0
