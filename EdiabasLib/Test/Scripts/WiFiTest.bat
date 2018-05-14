@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0
:restart
"!BATPATH!..\EdiabasLibCall\bin\Release\EdiabasLibCall.exe" --ifh="STD:OBD" --cfg="ObdComPort=DEEPOBDWIFI;IfhTrace=2" -s "C:\EDIABAS\Ecu\f01" -j "IDENT_FUNKTIONAL" -j "FS_LESEN_FUNKTIONAL" -j "IS_LESEN_FUNKTIONAL" -j "SVK_LESEN_FUNKTIONAL" -j "STATUS_ENERGIESPARMODE_FUNKTIONAL" -j "SERIENNUMMER_LESEN_FUNKTIONAL" || goto exit
goto restart

:exit
echo transmission error
