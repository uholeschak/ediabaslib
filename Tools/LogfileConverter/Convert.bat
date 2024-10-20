@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0
set "LOG_PATH=D:\Projects\BMW\Logs\"

set "CONVERTER_EXE=!BATPATH!artifacts\bin\LogfileConverter\release\LogfileConverter.exe"
if NOT EXIST "!CONVERTER_EXE!" (
    set CONVERTER_EXE="!BATPATH!artifacts\bin\LogfileConverter\debug\LogfileConverter.exe"
)

if NOT EXIST "!CONVERTER_EXE!" (
    echo !CONVERTER_EXE! not found
    exit /b 1
)

"!CONVERTER_EXE!" -o "!LOG_PATH!Response.txt" -i "!LOG_PATH!Portmon_rheingold1_start.log" -i "!LOG_PATH!Portmon_rheingold2_start.log" -i "!LOG_PATH!Portmon_rheingold1_test1.log" -i "!LOG_PATH!Portmon_rheingold1_test2.log" -i "!LOG_PATH!Portmon_rheingold2_test1.log" -r -s || exit /b 1

echo Response file successfully created
exit /b 0
