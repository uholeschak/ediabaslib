@echo off
SETLOCAL EnableDelayedExpansion

set "BATPATH=%~dp0"
set "SAMPLEPATH=!BATPATH!..\Xml\Sample\"

set SAMPLEZIP="!BATPATH!..\Assets\Sample.zip"
if exist "!SAMPLEZIP!" del /f /q "!SAMPLEZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!SAMPLEZIP!" "!SAMPLEPATH!*" || EXIT /b 1

echo !SAMPLEZIP! successfully created


set "CACERTPATH=!BATPATH!..\..\Tools\CarSimulator\TestCert\"
set "SSLTRUSTPATH=!EDIABAS_PATH!\Security\SSL_Truststore\"
if not exist !SSLTRUSTPATH! (
    echo SSL_Truststore not found: !SSL_Truststore!
    exit /b 1
)
set CACERTZIP="!BATPATH!..\Assets\CaCerts.zip"
if exist "!CACERTZIP!" del /f /q "!CACERTZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!CACERTZIP!" "!CACERTPATH!rootCA_EC.pfx" "!SSLTRUSTPATH!*" || EXIT /b 1

echo !CACERTZIP! successfully created

exit /b 0
