@echo off
SETLOCAL EnableDelayedExpansion

set "BATPATH=%~dp0"
set "ECU_PATH=!BATPATH!"
set "BESTNET_PATH=!BATPATH!..\..\..\Tools\BestNet\artifacts\bin\BestNet\release\BestNet.exe"

IF NOT EXIST "!BESTNET_PATH!" (
  echo !BESTNET_PATH! not existing
  EXIT /b 1
)

echo:
echo compiling base1.b1v
"!BESTNET_PATH!" -i "!ECU_PATH!\base1.b1v" || EXIT /b 1
echo done

echo:
echo compiling base2.b1v
"!BESTNET_PATH!" -i "!ECU_PATH!\base2.b1v" || EXIT /b 1
echo done

echo:
echo compiling cmd_test1.b1v
"!BESTNET_PATH!" -i "!ECU_PATH!\cmd_test1.b1v" || EXIT /b 1
echo done

echo:
echo compiling cmd_test2.b2v
"!BESTNET_PATH!" -i "!ECU_PATH!\cmd_test2.b2v" -l "!ECU_PATH!\test.lib" || EXIT /b 1
echo done

echo:
echo compiling cmd_ident.b1g
"!BESTNET_PATH!" -i "!ECU_PATH!\cmd_ident.b1g" || EXIT /b 1
echo done

echo:
echo compiling adapter_prg.b2v
"!BESTNET_PATH!" -i "!ECU_PATH!\adapter_prg.b2v" || EXIT /b 1
echo done

echo:
echo compiling d60m47a0_dcan.b1v
"!BESTNET_PATH!" -i "!ECU_PATH!\d60m47a0_dcan.b1v" || EXIT /b 1
echo done

echo:
echo Files successfully compiled
exit /b 0
