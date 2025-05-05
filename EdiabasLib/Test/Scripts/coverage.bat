@echo off
SETLOCAL EnableDelayedExpansion

rem Start CarSimulator with e61.txt config first
rem For EDIABAS set the COM port in obd.ini (max COM9)
rem Compile solution EdiabasLib as Debug first.
rem Arguments: <test type:lib|apilib|ediabas> <interface: ENET|STD:OBD> <port: COM4>

set "BATPATH=%~dp0"
set "OPEN_COVER=%OPENCOVER_PATH%\OpenCover.Console.exe"
set "REPORT_GENERATOR=%REPORTGENERATOR_PATH%\ReportGenerator.exe"
set "ECU_PATH=!BATPATH!\..\..\..\Ecu"
set "ECU_TEST_PATH=!BATPATH!\..\Ecu"
set "REPORTS_PATH=!BATPATH!\Reports"
if "%2"=="" (
set IFH=STD:OBD
) else (
set IFH=%2
)
if "%3"=="" (
set COMPORT=COM4
) else (
set COMPORT=%3
)

if "%1"=="lib" (
set "EDIABAS_TEST=!BATPATH!\..\..\EdiabasTest\bin\Debug\net48\EdiabasTest.exe"
set "OUTFILE=output_lib.log"
rem set ADD_ARGS=-p !COMPORT! -o "!OUTFILE!" -a -c
set ADD_ARGS=--ifh="!IFH!" -o "!OUTFILE!" -a -c
set ADD_ARGS=!ADD_ARGS! --cfg="ObdComPort=!COMPORT!;CompatMode=0"
set FILTERS=+[EdiabasLib]EdiabasLib.EdiabasNet*
set COVERAGE=1
goto argsok
)
if "%1"=="apilib" (
set "EDIABAS_TEST=!BATPATH!\..\EdiabasLibCall\bin\Debug\net48\EdiabasLibCall.exe"
set OUTFILE=output_apilib.log
set ADD_ARGS=-o !OUTFILE! --ifh="!IFH!" --device="_" -a -c
rem set ADD_ARGS=!ADD_ARGS! --cfg="@!BATPATH!\EdiabasLib.config"
set ADD_ARGS=!ADD_ARGS! --cfg="ObdComPort=!COMPORT!;CompatMode=0"
set FILTERS=+[EdiabasLib]EdiabasLib.EdiabasNet* +[apiNET32]*
set COVERAGE=1
goto argsok
)
if "%1"=="ediabas" (
set "EDIABAS_TEST=!BATPATH!\..\EdiabasCall\bin\Debug\net48\EdiabasCall.exe"
set "OUTFILE=output_ediabas.log"
set ADD_ARGS=-o "!OUTFILE!" --ifh="!IFH!" --device="_" -a -c
set ADD_ARGS=!ADD_ARGS! --cfg="RemoteHost=127.0.0.1"
set FILTERS=-[*]*
set COVERAGE=0
goto argsok
)

echo invalid arguments
echo examples:
echo coverage lib ENET
echo coverage apilib STD:OBD COM4
echo coverage ediabas ENET
goto done

:argsok
if exist "!OUTFILE!" del "!OUTFILE!"
"%OPEN_COVER%" "-output:results1_1.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\d_motor.grp\" -j \"_VERSIONINFO\" -j \"_JOBS\" -j \"_JOBCOMMENTS#FS_LESEN_DETAIL\" -j \"_ARGUMENTS#STATUS_MESSWERTBLOCK_LESEN\" -j \"_RESULTS#FS_LESEN_DETAIL\" -j \"_TABLES\" -j \"_TABLE#KONZEPT_TABELLE\" -j \"_TABLE#LIEFERANTEN\" -j \"_TABLE#MISSING\" -j \"_TABLE\" -j \"FS_LESEN\" -j \"FS_LESEN_DETAIL#0x4232#F_ART_ANZ;F_UW_ANZ\" -j \"FS_LESEN_DETAIL#0x4232#F_art_anz;F_uw_anz\" -j \"STATUS_RAILDRUCK_IST##STAT_RAILDRUCK_IST_WERT\" -j \"STATUS_MOTORTEMPERATUR##STAT_MOTORTEMPERATUR_WERT\" -j \"STATUS_LMM_MASSE##STAT_LMM_MASSE_WERT\" -j \"STATUS_MOTORDREHZAHL\" -j \"STATUS_SYSTEMCHECK_PM_INFO_1\" -j \"STATUS_SYSTEMCHECK_PM_INFO_2\" -j \"STATUS_MESSWERTBLOCK_LESEN#JA;IUBAT;ITKUM;CTSCD_tClntLin;ITKRS;ILMKG;ILMMG;SLMMG;ITUMG;IPLAD;SPLAD;ITLAL;IPUMG;IPRDR;SPRDR;ITAVO;ITAVP1;IPDIP;IDSLRE;IREAN;EGT_st;ISRBF;ISOED\""
"%OPEN_COVER%" "-output:results1_2.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\d_motor.grp\" -j \"IDENT\" -j \"STATUS_REGENERATION_CSF\""
"%OPEN_COVER%" "-output:results2.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\d_ccc.grp\" -j \"_VERSIONINFO\" -j \"IDENT\" -j \"STATUS_GPS_TRACKING\" -j \"STATUS_GPS_ANTENNA\" -j \"STATUS_GPS_POSITION\" -j \"STATUS_GPS_TIME\" -j \"STATUS_GPS_SATINFO\" -j \"STATUS_TACHOPULSE\" -j \"STATUS_GYRO\" -j \"STATUS_GPS_DOP\" -j \"STATUS_DR_POSITION\""
"%OPEN_COVER%" "-output:results3.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\d_ehc.grp\" -j \"_VERSIONINFO\" -j \"IDENT\" -j \"FS_LESEN\" -j \"FS_LESEN_DETAIL#0x5FB4\" -j \"LESEN_ANALOGWERTE\" -j \"LESEN_FILTERWERTE\" -j \"LESEN_REGLERWERTE\" -j \"MODE_CTRL_LESEN\""
"%OPEN_COVER%" "-output:results4.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\d_klima.grp\" --store -j \"_VERSIONINFO\" -j \"IDENT\" -j \"STATUS_ANALOGEINGAENGE\" -j \"STATUS_DIGITALEINGAENGE\" -j \"STATUS_REGLERGROESSEN\" -j \"STATUS_BEDIENTEIL\" -j \"STATUS_IO\" -j \"STATUS_MOTOR_KLAPPENPOSITION\""
"%OPEN_COVER%" "-output:results5.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_PATH!\e60.prg\" -j \"_VERSIONINFO\" -j \"IDENT_FUNKTIONAL\" -j \"FS_LESEN_FUNKTIONAL\""

"%OPEN_COVER%" "-output:results10.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_SHMID#ARGS##STDARGS\" -j \"_JOBS\" -j \"TEST_SREG\" -j \"TEST_MATH\" -j \"TEST_PARY#^|12131415A1A2A3A4A5\" -j \"TEST_PARY#^|\" -j \"TEST_PARL# -5\" -j \"TEST_PARL#0x10 \" -j \"TEST_PARL#0y011001 \" -j \"TEST_PARR# 123.45 \" -j \"TEST_FILES\" -j \"TEST_PROGRESS_INFO\""
"%OPEN_COVER%" "-output:results11.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! --alltypes -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_SHMID###STDARG1\" -j \"_JOBS\" -j \"TEST_SUBB_FLAGS\" -j \"TEST_SHMID\" -j \"TEST_SUBC_FLAGS\" -j \"TEST_SHMID\" -j \"TEST_ADDS_FLAGS\" -j \"TEST_ADDC_FLAGS\" -j \"TEST_COMP_FLAGS\" !"^"=!^
 -j \"TEST_MULT_FLAGS\" -j \"TEST_DIVS_FLAGS\" -j \"TEST_LSL_FLAGS\" -j \"TEST_ASL_FLAGS\" -j \"TEST_LSR_FLAGS\" -j \"TEST_ASR_FLAGS\" -j \"TEST_AND_FLAGS\" -j \"TEST_OR_FLAGS\" -j \"TEST_XOR_FLAGS\" -j \"TEST_TEST_FLAGS\" -j \"TEST_NOT_FLAGS\" !"^"=!^
 -j \"TEST_FSUB_FLAGS\" -j \"TEST_FADD_FLAGS\" -j \"TEST_FMUL_FLAGS\" -j \"TEST_FDIV_FLAGS\" -j \"TEST_FCOMP_FLAGS\" -j \"TEST_PAR_FLAGS#hallo;10;3.5\" -j \"TEST_PAR_FLAGS#;;\" -j \"TEST_PAR_FLAGS\" -j \"TEST_CLEAR_FLAGS\" -j \"TEST_TABLE_FLAGS\" -j \"TEST_CFG_FLAGS\" -j \"TEST_ERROR_FLAGS\" -j \"TEST_IFACE_FLAGS\" !"^"=!^
 -j \"TEST_SHMID\" -j \"TEST_BASE1_TABLE1\" -j \"TEST_SHMID\" -j \"TEST_BASE1_TABLE2\" -j \"TEST_SHMID\" -j \"TEST_BASE2_TABLE1\" -j \"TEST_SHMID\" -j \"TEST_BASE2_TABLE2\" -j \"TEST_SHMID\" !"^"=!^
 -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUNG;1\" -j \"TEST_SHMID\" -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUNG;0\" -j \"TEST_SHMID\" -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUNG;1\" -j \"TEST_SHMID###STDARG2\" -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUNG;2\" -j \"TEST_SHMID\" -j \"TEST_ERGSYI_FLAGS#^!INITIALISIERUN;1\" -j \"TEST_SHMID\""
"%OPEN_COVER%" "-output:results12.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_ident.grp\" -j \"TEST_SHMID#ARGS##STDARGS\" -j \"TEST_SHMID#ARGS##STDARGS\""
"%OPEN_COVER%" "-output:results13.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"_VERSIONINFO\" -j "_JOBS" -j \"_JOBCOMMENTS#INITIALISIERUNG\""
"%OPEN_COVER%" "-output:results14.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! --cfg=\"SystemResults=0\" -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"_VERSIONINFO\" -j "_JOBS" -j \"_JOBCOMMENTS#INITIALISIERUNG\" -j \"_JOBCOMMENTS#MISSING\" -j \"_JOBCOMMENTS\" -j \"TEST_SHMID###STDARG1\""
"%OPEN_COVER%" "-output:results15.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_DIVS_ZERO\""
set TIMESTR=%TIME:~0,2%;%TIME:~3,2%;%TIME:~6,2%
set TIMESTR=!TIMESTR:^ =0!
"%OPEN_COVER%" "-output:results15.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_TIME_FLAGS#!TIMESTR!\""
"%OPEN_COVER%" "-output:results16.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_A2FIX_FLAGS#0xabcd\" -j \"TEST_A2FIX_FLAGS#0y110011\" -j \"TEST_A2FIX_FLAGS#1234\" -j \"TEST_A2FIX_FLAGS#12.34\" -j \"TEST_A2FIX_FLAGS#12.78\" -j \"TEST_A2FIX_FLAGS#1234,567\" -j \"TEST_A2FIX_FLAGS#0\" -j \"TEST_A2FIX_FLAGS#-5\" -j \"TEST_A2FIX_FLAGS#0xFFFFFFFFF\" -j \"TEST_A2FIX_FLAGS#-123456789123456789\" -j \"TEST_A2FIX_FLAGS#123456789123456789\" -j \"TEST_A2FIX_FLAGS#invalid\""
"%OPEN_COVER%" "-output:results17_1.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_SHMID###INIT_EXCEPTION1\""
"%OPEN_COVER%" "-output:results17_2.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"INITIALISIERUNG###INIT_EXCEPTION2\""
"%OPEN_COVER%" "-output:results17_3.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"INITIALISIERUNG#INIT_EXCEPTION1\""
"%OPEN_COVER%" "-output:results17_4.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"INITIALISIERUNG#INIT_EXCEPTION1##INIT_EXCEPTION2\""
"%OPEN_COVER%" "-output:results17_5.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_SHMID###EXIT_EXCEPTION\" -j \"TEST_SHMID###EXIT_EXCEPTION#cmd_test1.prg\""
"%OPEN_COVER%" "-output:results17_6.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_SHMID###\" -j \"TEST_SHMID###EXIT_EXCEPTION#cmd_test1.prg\""
"%OPEN_COVER%" "-output:results17_7.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_SHMID###EXIT_EXCEPTION\" -j \"TEST_SHMID####cmd_test1.prg\""
"%OPEN_COVER%" "-output:results17_8.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_SHMID###BOTH_EXCEPTION\" -j \"TEST_SHMID###BOTH_EXCEPTION#cmd_test1.prg\""
"%OPEN_COVER%" "-output:results17_9.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"INITIALISIERUNG###BOTH_EXCEPTION\" -j \"INITIALISIERUNG####cmd_test1.prg\""
"%OPEN_COVER%" "-output:results17_10.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"TEST_SHMID###INIT_ERROR\""
"%OPEN_COVER%" "-output:results17_11.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test2.prg\" -j \"INITIALISIERUNG###INIT_ERROR\""
"%OPEN_COVER%" "-output:results18_1.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_ERROR#2\""
"%OPEN_COVER%" "-output:results18_2.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_ERROR#3\""
for /l %%x in (0, 1, 32) do (
!EDIABAS_TEST! !ADD_ARGS! -s "!ECU_TEST_PATH!\cmd_test1.prg" -j "TEST_RAISE_ERROR#%%x"
)
"%OPEN_COVER%" "-output:results20.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" !"^"=!^
 -f \"ERGB=W\" -f \"ERGB=6.4W\" -f \"ERGB=-6.4W\" -f \"ERGB=-.4W\" -f \"ERGB=8.W\" -f \"ERGB=-.W\" !"^"=!^
 -f \"ERGC=I\" -f \"ERGC=6.4I\" -f \"ERGC=-6.4I\" !"^"=!^
 -f \"ERGW=D\" -f \"ERGW=6.4D\" -f \"ERGW=-6.4D\" -f \"ERGI=6.4L\" -f \"ERGI=-6.4L\" !"^"=!^
 -f \"ERGD=D\" -f \"ERGD=6.4D\" -f \"ERGD=-6.4D\" -f \"ERGD=6.4B\" -f \"ERGD=6.4C\" -f \"ERGL=6.4L\" -f \"ERGL=-6.4L\" -f \"ERGL=-6.4B\" -f \"ERGL=-6.4C\" !"^"=!^
 -f \"ERGR=R\" -f \"ERGR=10.5R\" -f \"ERGR=-10.5R\" -f \"ERGR=10.5ER\" -f \"ERGR=-10.5eR\" -f \"ERGR=-.5R\" -f \"ERGR=-8.R\" -f \"ERGR=8.ER\" -f \"ERGR=0.3R\" -f \"ERGR=3.0R\" -f \"ERGR= 3 . 0 R \" !"^"=!^
 -f \"ERGS=10.4T\" -f \"ERGS=-10.4T\" -f \"ERGS=10.8T\" -f \"ERGS=0.8T\" -f \"ERGS= 0 . 8 T\" -f \"ERGS=T\" -f \"Ergs=T\" -f \"ERGS= \" -f \"ERGS=  \" !"^"=!^
 -j \"TEST_MULTIARG#3;-4;10000;-10000;100000;-100000;0.001345;Ulrich;Erwin\" -j \"TEST_MULTIARG#;-4;z;-10000;100000;-100000;0.001345; ;Erwin\""

"%OPEN_COVER%" "-output:results21.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" !"^"=!^
 -f \"ERGB=W\" -f \"ERGB=6.4W\" -f \"ERGB=-6.4W\" -f \"ERGB=-.4W\" -f \"ERGB=8.W\" -f \"ERGB=-.W\" !"^"=!^
 -f \"ERGC=I\" -f \"ERGC=6.4I\" -f \"ERGC=-6.4I\" !"^"=!^
 -f \"ERGW=D\" -f \"ERGW=6.4D\" -f \"ERGW=-6.4D\" -f \"ERGW=X\" -f \"ERGW=-X\" -f \"ERGW=0X\" -f \"ERGW=06X\" -f \"ERGW=-6X\" -f \"ERGW=6X\" -f \"ERGW=3.2X\" -f \"ERGW=\" -f \"ERGW=T\" -f \"ERGI=6.4L\" -f \"ERGI=-6.4L\" !"^"=!^
 -f \"ERGD=D\" -f \"ERGD=6.4D\" -f \"ERGD=-6.4D\" -f \"ERGD=6.4B\" -f \"ERGD=6.4C\" -f \"ERGL=6.4L\" -f \"ERGL=-6.4L\" -f \"ERGL=-6.4B\" -f \"ERGL=-6.4C\" -f \"ERGL= - 6 . 4 C \" !"^"=!^
 -f \"ERGR=R\" -f \"ERGR=10.5R\" -f \"ERGR=-10.5R\" -f \"ERGR=10.5ER\" -f \"ERGR=-10.5eR\" -f \"ERGR=-.5R\" -f \"ERGR=-8.R\" -f \"ERGR=8.ER\" -f \"ERGR=0.3R\" -f \"ERGR=3.0R\" -f \"ERGR=08X\" -f \"ERGR=\" -f \"ERGR=T\" !"^"=!^
 -f \"ERGS=10.4T\" -f \"ERGS=-10.4T\" -f \"ERGS=10.8T\" -f \"ERGS=0.8T\" -f \"ERGS= 0 . 8 T\" -f \"ERGS=T\" -f \"Ergs=T\" -f \"ERGS= \" -f \"ERGS=  \" !"^"=!^
 -j \"TEST_MULTIARG#-30;40;-12345;12345;-123456;123456;-13.45;Ulrich Test;Erwin\""

"%OPEN_COVER%" "-output:results22.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! --alltypes -s \"!ECU_TEST_PATH!\cmd_test1.prg\" !"^"=!^
 -f \"ERGB=W\" -f \"ERGB=6.4W\" -f \"ERGB=-6.4W\" -f \"ERGB=-.4W\" -f \"ERGB=8.W\" -f \"ERGB=-.W\" !"^"=!^
 -f \"ERGC=I\" -f \"ERGC=6.4I\" -f \"ERGC=-6.4I\" !"^"=!^
 -f \"ERGW=D\" -f \"ERGW=6.4D\" -f \"ERGW=-6.4D\" -f \"ERGI=6.4L\" -f \"ERGI=-6.4L\" !"^"=!^
 -f \"ERGD=D\" -f \"ERGD=6.4D\" -f \"ERGD=-6.4D\" -f \"ERGD=6.4B\" -f \"ERGD=6.4C\" -f \"ERGL=6.4L\" -f \"ERGL=-6.4L\" -f \"ERGL=-6.4B\" -f \"ERGL=-6.4C\" !"^"=!^
 -f \"ERGR=R\" -f \"ERGR=10.5R\" -f \"ERGR=-10.5R\" -f \"ERGR=10.5ER\" -f \"ERGR=-10.5eR\" -f \"ERGR=-.5R\" -f \"ERGR=-8.R\" -f \"ERGR=8.ER\" -f \"ERGR=0.3R\" -f \"ERGR=3.0R\" !"^"=!^
 -f \"ERGS=10.4T\" -f \"ERGS=-10.4T\" -f \"ERGS=10.8T\" -f \"ERGS=0.8T\" -f \"ERGS= 0 . 8 T\" -f \"ERGS=T\" -f \"Ergs=T\" -f \"ERGS= \" -f \"ERGS=  \" !"^"=!^
 -j \"TEST_MULTIARG#3;-4;10000;-10000;100000;-100000;12.3456789123;Ulrich;Erwin\""

"%OPEN_COVER%" "-output:results23.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! --alltypes -s \"!ECU_TEST_PATH!\cmd_test1.prg\" !"^"=!^
 -f \"ERGS=C\" -f \"ERGS=B\" -f \"ERGS=I\" -f \"ERGS=W\" -f \"ERGS=L\" -f \"ERGS=D\" -f \"ERGS=R\" -f \"ERGS=T\" -f \"ERGS=X\" !"^"=!^
 -j \"TEST_MULTIARG#-15;-15;-15;-15;-15;-15;-15;-15;-15\" -j \"TEST_MULTIARG#14;14;14;14;14;14;14;14;14\" -j \"TEST_MULTIARG#0xABCD;0xABCD;0xABCD;0xABCD;0xABCD;0xABCD;0xABCD;0xABCD;0xABCD\" !"^"=!^
 -j \"TEST_MULTIARG#14.7;14.7;14.7;14.7;14.7;14.7;14.7;14.7;14.7\" -j \"TEST_MULTIARG#-13.6;-13.6;-13.6;-13.6;-13.6;-13.6;-13.6;-13.6;-13.6\" -j \"TEST_MULTIARG#xyz;xyz;xyz;xyz;xyz;xyz;xyz;xyz;xyz\""

"%OPEN_COVER%" "-output:results30.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"MISSING\""
"%OPEN_COVER%" "-output:results31.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_BIP1\""
"%OPEN_COVER%" "-output:results32.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_BIP10\""
"%OPEN_COVER%" "-output:results33.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_BREAK\""
"%OPEN_COVER%" "-output:results34.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_RUNTIMEERR#249\""
"%OPEN_COVER%" "-output:results35.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_RUNTIMEERR#250\""
"%OPEN_COVER%" "-output:results36.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_RUNTIMEERR#349\""
"%OPEN_COVER%" "-output:results37.xml" "-target:!EDIABAS_TEST!" "-filter:!FILTERS!" "-targetargs:!ADD_ARGS! -s \"!ECU_TEST_PATH!\cmd_test1.prg\" -j \"TEST_RAISE_RUNTIMEERR#350\""
for /l %%x in (351, 1, 410) do (
!EDIABAS_TEST! !ADD_ARGS! -s "!ECU_TEST_PATH!\cmd_test1.prg" -j "TEST_RAISE_RUNTIMEERR#%%x"
)

if "!COVERAGE!"=="1" (
if exist "!REPORTS_PATH!" del /q "!REPORTS_PATH!\*.*"
"%REPORT_GENERATOR%" "-reports:results*.xml" "-targetdir:!REPORTS_PATH!"
)

del results*.xml

:done
