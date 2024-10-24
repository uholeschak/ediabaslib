@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0
set "RESPONSE_PATH=!BATPATH!..\CarSimulator\Response\"
set "XML_PATH=!BATPATH!..\..\BmwDeepObd\Xml\"

if NOT EXIST "!RESPONSE_PATH!" (
    echo !RESPONSE_PATH! not existing
    exit /b 1
)

if NOT EXIST "!XML_PATH!" (
    echo !XML_PATH! not existing
    exit /b 1
)

set "CONVERTER_EXE=!BATPATH!artifacts\bin\LogfileConverter\release\LogfileConverter.exe"
set "CONVERTER_TYPE=Release"
if NOT EXIST "!CONVERTER_EXE!" (
    set CONVERTER_EXE="!BATPATH!artifacts\bin\LogfileConverter\debug\LogfileConverter.exe"
    set "CONVERTER_TYPE=Debug"
)

if NOT EXIST "!CONVERTER_EXE!" (
    echo !CONVERTER_EXE! not found
    exit /b 1
)

echo Using Converter build type: !CONVERTER_TYPE!

"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\AudiA6_1999.txt" --sim "!RESPONSE_PATH!VW\edic_AudiA6_1999.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\AudiA6_2007.txt" --sim "!RESPONSE_PATH!VW\edic_AudiA6_2007.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\Golf3.txt" --sim "!RESPONSE_PATH!VW\edic_Golf3.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\GolfFullUds.txt" --sim "!RESPONSE_PATH!VW\edic_GolfFullUds.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\LeonUDS.txt" --sim "!RESPONSE_PATH!VW\edic_LeonUDS.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\touareg1.txt" --sim "!RESPONSE_PATH!VW\edic_touareg1.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!VW\Touran2010.txt" --sim "!RESPONSE_PATH!VW\edic_Touran2010.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!g31_coding.txt" -m "!RESPONSE_PATH!g31_coding_sim.txt" --sim "!RESPONSE_PATH!G31\enet.sim" || exit /b 1
xcopy /y "!RESPONSE_PATH!G31\enet.sim" "!XML_PATH!G31\" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!e61.txt" -m "!RESPONSE_PATH!e61_sim.txt" --sim "!RESPONSE_PATH!E61\obd.sim" || exit /b 1
xcopy /y "!RESPONSE_PATH!E61\obd.sim" "!XML_PATH!E61\" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!e90.txt" -m "!RESPONSE_PATH!e90_sim.txt" --sim "!RESPONSE_PATH!E90\obd.sim" || exit /b 1
xcopy /y "!RESPONSE_PATH!E90\obd.sim" "!XML_PATH!E90\" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!E61R\E61R.txt" --sim "!RESPONSE_PATH!E61R\obd.sim" || exit /b 1
xcopy /y "!RESPONSE_PATH!E61R\obd.sim" "!XML_PATH!E61R\" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!E36\E36_ISTA.txt" --sim "!RESPONSE_PATH!E36\obd.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!E38\E38.txt" --sim "!RESPONSE_PATH!E38\obd.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!E39\E39_ISTA.txt" --sim "!RESPONSE_PATH!E39\obd.sim" || exit /b 1
"!CONVERTER_EXE!" -m "!RESPONSE_PATH!E53\E53.txt" --sim "!RESPONSE_PATH!E53\obd.sim" || exit /b 1

echo Sim files successfully created
exit /b 0
