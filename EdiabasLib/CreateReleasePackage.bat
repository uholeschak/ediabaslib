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
set "APINETPATH=!PACKAGEPATH!ApiNet\"
set "CANADAPTERPATH=!PACKAGEPATH!CanAdapter\"
set "CANADAPTERELMPATH=!PACKAGEPATH!CanAdapterElm\"
set "ENETADAPTERPATH=!PACKAGEPATH!EnetAdapter\"
set "ANDROIDSAMPLEPATH=!PACKAGEPATH!AndroidSamples\"
set "ECUPATH=!PACKAGEPATH!Ecu\"
if exist "!PACKAGEPATH!" rmdir /s /q "!PACKAGEPATH!"
timeout /T 1 /NOBREAK > nul
mkdir "!PACKAGEPATH!" || EXIT /b 1

echo copy EdiabasTest
mkdir "!EDIABASTESTPATH!" || EXIT /b 1
xcopy /y /q "!EDIABASTESTSRCPATH!EdiabasTest.exe" "!EDIABASTESTPATH!" || EXIT /b 1
xcopy /y /q "!EDIABASTESTSRCPATH!*.dll" "!EDIABASTESTPATH!" || EXIT /b 1
xcopy /y /q "!EDIABASTESTSRCPATH!*.config" "!EDIABASTESTPATH!" || EXIT /b 1

echo copy Tools
mkdir "!TOOLPATH!" || EXIT /b 1
xcopy /y /q "!TOOLSRCPATH!*.dll" "!TOOLPATH!" || EXIT /b 1
xcopy /y /e /q "!TOOLSRCPATH!\*.*" "!TOOLPATH!" || EXIT /b 1
del "!TOOLPATH!*.pdb"

echo copy S29CertGenerator
mkdir "!S29CERTGENPATH!" || EXIT /b 1
xcopy /y /q "!S29CERTGENSRCPATH!*.dll" "!S29CERTGENPATH!" || EXIT /b 1
xcopy /y /e /q "!S29CERTGENSRCPATH!\*.*" "!S29CERTGENPATH!" || EXIT /b 1
del "!S29CERTGENPATH!*.pdb"

echo copy LogFileConverter
mkdir "!LOGCONVPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!..\Tools\LogfileConverter\artifacts\bin\LogfileConverter\release\*.*" "!LOGCONVPATH!" || EXIT /b 1

echo copy apiNET
mkdir "!APINETPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!apiNET\bin\Release\net48\*.dll" "!APINETPATH!" || EXIT /b 1

echo copy CanAdapter
mkdir "!CANADAPTERPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapter\CanAdapter\Release\*.hex" "!CANADAPTERPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapter\Pld\*.jed" "!CANADAPTERPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapter\UpdateLoader\bin\*.exe" "!CANADAPTERPATH!" || EXIT /b 1

echo copy adapter firmware
mkdir "!CANADAPTERELMPATH!" || EXIT /b 1

echo copy default
mkdir "!CANADAPTERELMPATH!default" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\default\production\*.hex" "!CANADAPTERELMPATH!default" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\default\production\*.hex" "!CANADAPTERELMPATH!default" || EXIT /b 1

echo copy def115200
mkdir "!CANADAPTERELMPATH!def115200" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\def115200\production\*.hex" "!CANADAPTERELMPATH!def115200" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\def115200\production\*.hex" "!CANADAPTERELMPATH!def115200" || EXIT /b 1

echo copy bc04
mkdir "!CANADAPTERELMPATH!bc04" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\bc04\production\*.hex" "!CANADAPTERELMPATH!bc04" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\bc04\production\*.hex" "!CANADAPTERELMPATH!bc04" || EXIT /b 1

echo copy hc04
mkdir "!CANADAPTERELMPATH!hc04" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\hc04\production\*.hex" "!CANADAPTERELMPATH!hc04" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\hc04\production\*.hex" "!CANADAPTERELMPATH!hc04" || EXIT /b 1

echo copy spp_uart
mkdir "!CANADAPTERELMPATH!spp_uart" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\spp_uart\production\*.hex" "!CANADAPTERELMPATH!spp_uart" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\Bluetooth\spp_uart\release\*.*" "!CANADAPTERELMPATH!spp_uart" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\spp_uart\production\*.hex" "!CANADAPTERELMPATH!spp_uart" || EXIT /b 1

echo copy spp_uart2
mkdir "!CANADAPTERELMPATH!spp_uart2" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\spp_uart2\production\*.hex" "!CANADAPTERELMPATH!spp_uart2" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\spp_uart2\production\*.hex" "!CANADAPTERELMPATH!spp_uart2" || EXIT /b 1

echo copy esp8266
mkdir "!CANADAPTERELMPATH!esp8266" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\esp8266\production\*.hex" "!CANADAPTERELMPATH!esp8266" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\Esp8266\*.bin" "!CANADAPTERELMPATH!esp8266" || EXIT /b 1

echo copy yc1021
mkdir "!CANADAPTERELMPATH!yc1021" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\yc1021\production\*.hex" "!CANADAPTERELMPATH!yc1021" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\YC1021\*.bin" "!CANADAPTERELMPATH!yc1021" || EXIT /b 1
xcopy /y /q "!BATPATH!CanAdapterElm\CanAdapterElm.X\ELM327V23.X\dist\yc1021\production\*.hex" "!CANADAPTERELMPATH!yc1021" || EXIT /b 1

echo copy ENET
mkdir "!ENETADAPTERPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!EnetAdapter\Release\mini.bin" "!ENETADAPTERPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!EnetAdapter\Release\openwrt*.bin" "!ENETADAPTERPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!EnetAdapter\Release\*.img" "!ENETADAPTERPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!EnetAdapter\EnetWifiSettings.dat" "!ENETADAPTERPATH!" || EXIT /b 1

mkdir "!ANDROIDSAMPLEPATH!" || EXIT /b 1
xcopy /y /e /q "!BATPATH!..\BmwDeepObd\Xml\*.*" "!ANDROIDSAMPLEPATH!" || EXIT /b 1

mkdir "!ECUPATH!" || EXIT /b 1
xcopy /y /q "!BATPATH!Test\Ecu\adapter_prg.prg" "!ECUPATH!" || EXIT /b 1

set PACKAGEZIP="!BATPATH!Binaries-!DATESTR!.zip"
if exist "!PACKAGEZIP!" del /f /q "!PACKAGEZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!PACKAGEZIP!" "!PACKAGEPATH!*" || EXIT /b 1

echo Package successfully created
exit /b 0
