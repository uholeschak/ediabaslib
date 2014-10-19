@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0
set OPEN_COVER=%OPENCOVER_PATH%\OpenCover.Console.exe
set REPORT_GENERATOR=%REPORTGENERATOR_PATH%\bin\ReportGenerator.exe
set ECU_PATH=!BATPATH!\..\..\..\Ecu
set ECU_TEST_PATH=!BATPATH!\..\Ecu
set REPORTS_PATH=!BATPATH!\Reports

if "%1"=="" (
set EDIABAS_TEST=!BATPATH!\..\..\EdiabasTest\bin\Debug\EdiabasTest.exe
set OUTFILE=output_lib.log
set ADD_ARGS=-p COM4 -o !OUTFILE! -a -c
set FILTERS=+[EdiabasLib]*
set COVERAGE=1
goto argsok
)
if "%1"=="apilib" (
set EDIABAS_TEST=!BATPATH!\..\EdiabasLibCall\bin\Debug\EdiabasLibCall.exe
set OUTFILE=output_apilib.log
set ADD_ARGS=-o !OUTFILE! -a -c
rem set ADD_ARGS=!ADD_ARGS! --cfg="@!BATPATH!\EdiabasLib.config"
set ADD_ARGS=!ADD_ARGS! --cfg=\"ObdComPort=COM4\"
set FILTERS=+[EdiabasLib]* +[apiNET32]*
set COVERAGE=1
goto argsok
)
if "%1"=="ediabas" (
set EDIABAS_TEST=!BATPATH!\..\EdiabasCall\bin\Debug\EdiabasCall.exe
set OUTFILE=output_ediabas.log
set ADD_ARGS=-o !OUTFILE! -a -c
set FILTERS=-[*]*
set COVERAGE=0
goto argsok
)

echo invalid arguments
goto done

:argsok
if exist "!OUTFILE!" del "!OUTFILE!"
"%OPEN_COVER%" "-output:results1.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\d_motor.grp\" -j \"FS_LESEN\" -j \"FS_LESEN_DETAIL#0x4232#F_ART_ANZ;F_UW_ANZ\" -j \"STATUS_RAILDRUCK_IST##STAT_RAILDRUCK_IST_WERT\" -j \"STATUS_MOTORTEMPERATUR##STAT_MOTORTEMPERATUR_WERT\" -j \"STATUS_LMM_MASSE##STAT_LMM_MASSE_WERT\" -j \"STATUS_MOTORDREHZAHL\" -j \"STATUS_SYSTEMCHECK_PM_INFO_1\" -j \"STATUS_SYSTEMCHECK_PM_INFO_2\""
"%OPEN_COVER%" "-output:results2.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\d_ccc.grp\" -j \"IDENT\" -j \"STATUS_GPS_TRACKING\" -j \"STATUS_GPS_ANTENNA\" -j \"STATUS_GPS_POSITION\" -j \"STATUS_GPS_TIME\" -j \"STATUS_GPS_SATINFO\" -j \"STATUS_TACHOPULSE\" -j \"STATUS_GYRO\" -j \"STATUS_GPS_DOP\" -j \"STATUS_DR_POSITION\""
"%OPEN_COVER%" "-output:results3.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\d_ehc.grp\" -j \"IDENT\" -j \"FS_LESEN\" -j \"FS_LESEN_DETAIL#0x5FB4\" -j \"LESEN_ANALOGWERTE\" -j \"LESEN_FILTERWERTE\" -j \"LESEN_REGLERWERTE\" -j \"MODE_CTRL_LESEN\""
"%OPEN_COVER%" "-output:results4.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\d_klima.grp\" -j \"IDENT\" -j \"STATUS_ANALOGEINGAENGE\" -j \"STATUS_DIGITALEINGAENGE\" -j \"STATUS_REGLERGROESSEN\" -j \"STATUS_BEDIENTEIL\" -j \"STATUS_IO\" -j \"STATUS_MOTOR_KLAPPENPOSITION\""
"%OPEN_COVER%" "-output:results5.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\e60.prg\" -j \"IDENT_FUNKTIONAL\" -j \"FS_LESEN_FUNKTIONAL\""

"%OPEN_COVER%" "-output:results10.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_SHMID#ARGS##STDARGS\" -j \"TEST_SREG\" -j \"TEST_MATH\" -j \"TEST_PARY#^|12131415A1A2A3A4A5\" -j \"TEST_PARY#^|\" -j \"TEST_PARL# -5\" -j \"TEST_PARL#0x10 \" -j \"TEST_PARL#0y011001 \" -j \"TEST_PARR# 123.45 \" -j \"TEST_FILES\" -j \"TEST_PROGRESS_INFO\""
"%OPEN_COVER%" "-output:results11.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_SHMID###STDARG1\" -j \"TEST_SUBB_FLAGS\" -j \"TEST_SHMID\" -j \"TEST_SUBC_FLAGS\" -j \"TEST_SHMID\" -j \"TEST_ADDS_FLAGS\" -j \"TEST_ADDC_FLAGS\" -j \"TEST_COMP_FLAGS\" !"^"=!^
 -j \"TEST_MULT_FLAGS\" -j \"TEST_DIVS_FLAGS\" -j \"TEST_LSL_FLAGS\" -j \"TEST_ASL_FLAGS\" -j \"TEST_LSR_FLAGS\" -j \"TEST_ASR_FLAGS\" -j \"TEST_AND_FLAGS\" -j \"TEST_OR_FLAGS\" -j \"TEST_XOR_FLAGS\" -j \"TEST_TEST_FLAGS\" -j \"TEST_NOT_FLAGS\" !"^"=!^
 -j \"TEST_FSUB_FLAGS\" -j \"TEST_FADD_FLAGS\" -j \"TEST_FMUL_FLAGS\" -j \"TEST_FDIV_FLAGS\" -j \"TEST_FCOMP_FLAGS\" -j \"TEST_PAR_FLAGS#hallo;10;3.5\" -j \"TEST_PAR_FLAGS\" -j \"TEST_CLEAR_FLAGS\" -j \"TEST_TABLE_FLAGS\" -j \"TEST_CFG_FLAGS\" -j \"TEST_ERROR_FLAGS\" -j \"TEST_IFACE_FLAGS\" !"^"=!^
 -j \"TEST_SHMID\" -j \"TEST_BASE1_TABLE1\" -j \"TEST_SHMID\" -j \"TEST_BASE1_TABLE2\" -j \"TEST_SHMID\" -j \"TEST_BASE2_TABLE1\" -j \"TEST_SHMID\" -j \"TEST_BASE2_TABLE2\" -j \"TEST_SHMID\" !"^"=!^
 -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUNG;1\" -j \"TEST_SHMID\" -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUNG;0\" -j \"TEST_SHMID\" -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUNG;1\" -j \"TEST_SHMID###STDARG2\" -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUNG;2\" -j \"TEST_SHMID\" -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUN;1\" -j \"TEST_SHMID\""
"%OPEN_COVER%" "-output:results12.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_ident.grp\" -j \"TEST_SHMID#ARGS##STDARGS\" -j \"TEST_SHMID#ARGS##STDARGS\""

set TIMESTR=%TIME:~0,2%;%TIME:~3,2%;%TIME:~6,2%
set TIMESTR=!TIMESTR:^ =0!
"%OPEN_COVER%" "-output:results12.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_TIME_FLAGS#!TIMESTR!\""
"%OPEN_COVER%" "-output:results13_1.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_ERROR#2\""
"%OPEN_COVER%" "-output:results13_2.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_ERROR#3\""
for /l %%x in (0, 1, 32) do (
!EDIABAS_TEST! !ADD_ARGS! -s "!ECU_TEST_PATH!\cmd_test1.prg" -j "TEST_RAISE_ERROR#%%x"
)
"%OPEN_COVER%" "-output:results14.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_BIP1\""
"%OPEN_COVER%" "-output:results15.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_BIP10\""
"%OPEN_COVER%" "-output:results16.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_BREAK\""
"%OPEN_COVER%" "-output:results17.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_RUNTIMEERR#249\""
"%OPEN_COVER%" "-output:results18.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_RUNTIMEERR#250\""
"%OPEN_COVER%" "-output:results19.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_RUNTIMEERR#349\""
"%OPEN_COVER%" "-output:results20.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_RUNTIMEERR#350\""

if "!COVERAGE!"=="1" (
if exist "!REPORTS_PATH!" del /q "!REPORTS_PATH!\*.*"
"%REPORT_GENERATOR%" "-reports:results*.xml" "-targetdir:!REPORTS_PATH!"
)

del results*.xml

:done
