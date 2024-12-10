@echo off
SETLOCAL EnableDelayedExpansion

set "BATPATH=%~dp0"
set "ECU_PATH=!BATPATH!"
set "EDIABAS_BIN_PATH=!EDIABAS_PATH!\BIN\"
set "BEST1_PATH=!EDIABAS_BIN_PATH!Best1.exe"
set "BEST2_PATH=!EDIABAS_BIN_PATH!Best2.exe"

IF NOT EXIST "!BEST1_PATH!" (
  echo !BEST1_PATH! not existing
  EXIT /b 1
)

IF NOT EXIST "!BEST2_PATH!" (
  echo !BEST2_PATH! not existing
  EXIT /b 1
)

echo:
echo compiling base1.b1v
"!BEST1_PATH!" -Q "!ECU_PATH!\base1.b1v" || EXIT /b 1
echo done

echo:
echo compiling base2.b1v
"!BEST1_PATH!" -Q "!ECU_PATH!\base2.b1v" || EXIT /b 1
echo done

echo:
echo compiling cmd_test1.b1v
"!BEST1_PATH!" -Q "!ECU_PATH!\cmd_test1.b1v" || EXIT /b 1
echo done

echo:
echo compiling cmd_test2.b2v
"!BEST2_PATH!" -Q -L "!ECU_PATH!\test.lib" "!ECU_PATH!\cmd_test2.b2v" || EXIT /b 1
echo done

echo:
echo compiling cmd_ident.b1g
"!BEST1_PATH!" -Q  "!ECU_PATH!\cmd_ident.b1g" || EXIT /b 1
echo done

echo:
echo compiling adapter_prg.b2v
"!BEST2_PATH!" -Q "!ECU_PATH!\adapter_prg.b2v" || EXIT /b 1
echo done

echo:
echo compiling d60m47a0_dcan.b1v
"!BEST1_PATH!" -Q  "!ECU_PATH!\d60m47a0_dcan.b1v" || EXIT /b 1
echo done

echo:
echo Files successfully compiled
exit /b 0
