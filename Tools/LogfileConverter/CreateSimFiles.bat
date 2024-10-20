@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0
set "RESPONSE_PATH=!BATPATH!..\CarSimulator\Response\"

set "CONVERTER_EXE=!BATPATH!artifacts\bin\LogfileConverter\release\LogfileConverter.exe"
if NOT EXIST "!CONVERTER_EXE!" (
    set CONVERTER_EXE="!BATPATH!artifacts\bin\LogfileConverter\debug\LogfileConverter.exe"
)

if NOT EXIST "!CONVERTER_EXE!" (
    echo !CONVERTER_EXE! not found
    exit /b 1
)

"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\AudiA6_1999.txt" -r --sim "!RESPONSE_PATH!VW\edic_AudiA6_1999.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!\VW\Touran2010.txt" -r --sim "!RESPONSE_PATH!VW\edic_Touran2010.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\touareg1.txt" -r --sim "!RESPONSE_PATH!VW\edic_touareg1.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\GolfFullUds.txt" -r --sim "!RESPONSE_PATH!VW\edic_GolfFullUds.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\LeonUDS.txt" -r --sim "!RESPONSE_PATH!VW\edic_LeonUDS.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!g31_coding.txt" -m "!RESPONSE_PATH!g31_coding_sim.txt" -r --sim "!RESPONSE_PATH!G31\enet.sim" || exit /b 1

echo Sim files successfully created
exit /b 0
