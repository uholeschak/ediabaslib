@echo off
SETLOCAL EnableDelayedExpansion

set "BATPATH=%~dp0"

set "DATESTR=%date:~6,4%%date:~3,2%%date:~0,2%"
echo !DATESTR!
set "PACKAGEPATH=!BATPATH!Package\"
set "EDIABASTESTPATH=!PACKAGEPATH!EdiabasTest\"
set "EDIABASTESTSRCPATH=!BATPATH!EdiabasTest\bin\Release\net48\"
set "TOOLPATH=!PACKAGEPATH!EdiabasLibConfigTool\"
set "TOOLSRCPATH=!BATPATH!EdiabasLibConfigTool\artifacts\bin\EdiabasLibConfigTool\release\"
set "S29CERTGENPATH=!PACKAGEPATH!S29CertGenerator\"
set "S29CERTGENSRCPATH=!BATPATH!..\Tools\S29CertGenerator\artifacts\bin\S29CertGenerator\release\"
set "LOGCONVPATH=!PACKAGEPATH!LogfileConverter\"
set "LOGCONVSRCPATH=!BATPATH!..\Tools\LogfileConverter\artifacts\bin\LogfileConverter\release\"
set "APINETPATH=!PACKAGEPATH!ApiNet\"
set "CANADAPTERPATH=!PACKAGEPATH!CanAdapter\"
set "CANADAPTERSRCPATH=!BATPATH!CanAdapter\"
set "CANADAPTERELMPATH=!PACKAGEPATH!CanAdapterElm\"
set "ENETADAPTERPATH=!PACKAGEPATH!EnetAdapter\"
set "ANDROIDSAMPLEPATH=!PACKAGEPATH!AndroidSamples\"
set "ECUPATH=!PACKAGEPATH!Ecu\"
if exist "!PACKAGEPATH!" rmdir /s /q "!PACKAGEPATH!"
timeout /T 1 /NOBREAK > nul
mkdir "!PACKAGEPATH!" || EXIT /b 1

echo Copying EdiabasTest
forfiles /P !EDIABASTESTSRCPATH! /M *.exe /S /D -1 /C "cmd /c echo Old file found: @file @fdate" 2>nul
mkdir "!EDIABASTESTPATH!" || EXIT /b 1
xcopy /y /q "!EDIABASTESTSRCPATH!EdiabasTest.exe" "!EDIABASTESTPATH!" > nul || EXIT /b 1
xcopy /y /q "!EDIABASTESTSRCPATH!*.dll" "!EDIABASTESTPATH!" > nul || EXIT /b 1
xcopy /y /q "!EDIABASTESTSRCPATH!*.config" "!EDIABASTESTPATH!" > nul || EXIT /b 1

echo Copying Tools
forfiles /P !TOOLSRCPATH! /M *.exe /S /D -1 /C "cmd /c echo Old file found: @file @fdate" 2>nul
mkdir "!TOOLPATH!" || EXIT /b 1
xcopy /y /q "!TOOLSRCPATH!*.dll" "!TOOLPATH!" > nul || EXIT /b 1
xcopy /y /e /q "!TOOLSRCPATH!\*.*" "!TOOLPATH!" > nul || EXIT /b 1
del "!TOOLPATH!*.pdb"

echo Copying S29CertGenerator
forfiles /P !S29CERTGENSRCPATH! /M *.exe /S /D -1 /C "cmd /c echo Old file found: @file @fdate" 2>nul
mkdir "!S29CERTGENPATH!" || EXIT /b 1
xcopy /y /q "!S29CERTGENSRCPATH!*.dll" "!S29CERTGENPATH!" > nul || EXIT /b 1
xcopy /y /e /q "!S29CERTGENSRCPATH!\*.*" "!S29CERTGENPATH!" > nul || EXIT /b 1
del "!S29CERTGENPATH!*.pdb"

echo Copying LogFileConverter
forfiles /P !LOGCONVSRCPATH! /M *.exe /S /D -1 /C "cmd /c echo Old file found: @file @fdate" 2>nul
mkdir "!LOGCONVPATH!" || EXIT /b 1
xcopy /y /q "!LOGCONVSRCPATH!*.*" "!LOGCONVPATH!" > nul || EXIT /b 1

echo Copying apiNET
mkdir "!APINETPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!apiNET\bin\Release\net48\*.dll" "!APINETPATH!" > nul || EXIT /b 1

echo Copying CanAdapter
forfiles /P !CANADAPTERSRCPATH! /M *.exe /S /D -1 /C "cmd /c echo Old file found: @file @fdate" 2>nul
mkdir "!CANADAPTERPATH!" || EXIT /b 1
xcopy /y /q "!CANADAPTERSRCPATH!CanAdapter\Release\*.hex" "!CANADAPTERPATH!" > nul || EXIT /b 1
xcopy /y /q "!CANADAPTERSRCPATH!Pld\*.jed" "!CANADAPTERPATH!" > nul || EXIT /b 1
xcopy /y /q "!CANADAPTERSRCPATH!UpdateLoader\bin\*.exe" "!CANADAPTERPATH!" > nul || EXIT /b 1

echo Copying adapter firmware:
mkdir "!CANADAPTERELMPATH!" || EXIT /b 1

echo Firmware default
mkdir "!CANADAPTERELMPATH!default" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\default\production\*.hex" "!CANADAPTERELMPATH!default" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\default\production\*.hex" "!CANADAPTERELMPATH!default" > nul || EXIT /b 1

echo Firmware def115200
mkdir "!CANADAPTERELMPATH!def115200" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\def115200\production\*.hex" "!CANADAPTERELMPATH!def115200" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\def115200\production\*.hex" "!CANADAPTERELMPATH!def115200" > nul || EXIT /b 1

echo Firmware bc04
mkdir "!CANADAPTERELMPATH!bc04" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\bc04\production\*.hex" "!CANADAPTERELMPATH!bc04" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\bc04\production\*.hex" "!CANADAPTERELMPATH!bc04" > nul || EXIT /b 1

echo Firmware hc04
mkdir "!CANADAPTERELMPATH!hc04" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\hc04\production\*.hex" "!CANADAPTERELMPATH!hc04" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\hc04\production\*.hex" "!CANADAPTERELMPATH!hc04" > nul || EXIT /b 1

echo Firmware spp_uart
mkdir "!CANADAPTERELMPATH!spp_uart" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\spp_uart\production\*.hex" "!CANADAPTERELMPATH!spp_uart" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\Bluetooth\spp_uart\release\*.*" "!CANADAPTERELMPATH!spp_uart" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\spp_uart\production\*.hex" "!CANADAPTERELMPATH!spp_uart" > nul || EXIT /b 1

echo Firmware spp_uart2
mkdir "!CANADAPTERELMPATH!spp_uart2" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\spp_uart2\production\*.hex" "!CANADAPTERELMPATH!spp_uart2" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\spp_uart2\production\*.hex" "!CANADAPTERELMPATH!spp_uart2" > nul || EXIT /b 1

echo Firmware esp8266
mkdir "!CANADAPTERELMPATH!esp8266" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\esp8266\production\*.hex" "!CANADAPTERELMPATH!esp8266" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\Esp8266\*.bin" "!CANADAPTERELMPATH!esp8266" > nul || EXIT /b 1

echo Firmware yc1021
mkdir "!CANADAPTERELMPATH!yc1021" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\yc1021\production\*.hex" "!CANADAPTERELMPATH!yc1021" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\YC1021\*.bin" "!CANADAPTERELMPATH!yc1021" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\yc1021\production\*.hex" "!CANADAPTERELMPATH!yc1021" > nul || EXIT /b 1

echo Firmware ENET
mkdir "!ENETADAPTERPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!EnetAdapter\Release\mini.bin" "!ENETADAPTERPATH!" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!EnetAdapter\Release\openwrt*.bin" "!ENETADAPTERPATH!" > nul || EXIT /b 1
xcopy /y /q "!BATPATH!EnetAdapter\Release\*.img" "!ENETADAPTERPATH!" > nul  || EXIT /b 1
xcopy /y /q "!BATPATH!EnetAdapter\EnetWifiSettings.dat" "!ENETADAPTERPATH!" > nul || EXIT /b 1

echo Copying sample config
mkdir "!ANDROIDSAMPLEPATH!" || EXIT /b 1
xcopy /y /e /q "!BATPATH!..\BmwDeepObd\Xml\*.*" "!ANDROIDSAMPLEPATH!" > nul || EXIT /b 1

mkdir "!ECUPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!Test\Ecu\adapter_prg.prg" "!ECUPATH!" > nul || EXIT /b 1

set PACKAGEZIP="!BATPATH!Binaries-!DATESTR!.zip"
if exist "!PACKAGEZIP!" del /f /q "!PACKAGEZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!PACKAGEZIP!" "!PACKAGEPATH!*" || EXIT /b 1

echo Package successfully created
exit /b 0
