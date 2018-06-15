@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0
set ASSEM_PATH=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\Profile\Client

echo "Output: %1"
echo "Configuration: %2"

if "%2"=="Debug" (
set CS_FLAGS=/debug+ /define:DEBUG
set BT_BIN_PATH=DebugModule
) else (
set CS_FLAGS=/optimize
set BT_BIN_PATH=ReleaseModule
)

echo "Building: InTheHand.Net.Personal.Netmodule"
msbuild "!BATPATH!..\InTheHand.Net.Personal\InTheHand.Net.Personal.Netmodule.csproj" /t:Rebuild /p:Configuration=%2

echo "Building: EdiabasLib.Netmodule"
msbuild "!BATPATH!..\EdiabasLib\EdiabasLib.Netmodule.csproj" /t:Rebuild /p:Configuration=%2

echo "Building: ApiInternal.netmodule"
msbuild "!BATPATH!..\ApiInternal\ApiInternal.Netmodule.csproj" /t:Rebuild /p:Configuration=%2
rem "%programfiles(x86)%\MSBuild\14.0\Bin\csc.exe" /out:%1 !CS_FLAGS! /nologo /noconfig /nostdlib+ /target:module /define:BLUETOOTH /addmodule:"!BATPATH!\..\32feetNET\InTheHand.Net.Personal\ITH.Net.Personal.FX4\bin\!BT_BIN_PATH!\InTheHand.Net.Personal.netmodule" /reference:"!ASSEM_PATH!\mscorlib.dll" /reference:"!ASSEM_PATH!\System.dll" /reference:"!ASSEM_PATH!\System.Data.dll" /reference:"!ASSEM_PATH!\System.Xml.dll" /reference:"!ASSEM_PATH!\System.Core.dll" "!BATPATH!..\ApiInternal\ApiInternal.cs" "!BATPATH!..\EdiabasLib\EdiabasNet.cs" "!BATPATH!..\EdiabasLib\EdOperations.cs" "!BATPATH!..\EdiabasLib\MemoryStreamReader.cs" "!BATPATH!..\EdiabasLib\EdInterfaceBase.cs" "!BATPATH!..\EdiabasLib\EdInterfaceObd.cs" "!BATPATH!..\EdiabasLib\EdInterfaceAds.cs" "!BATPATH!..\EdiabasLib\EdInterfaceEdic.cs" "!BATPATH!..\EdiabasLib\EdInterfaceEnet.cs" "!BATPATH!..\EdiabasLib\EdFtdiInterface.cs" "!BATPATH!..\EdiabasLib\Ftd2xx.cs" "!BATPATH!..\EdiabasLib\EdBluetoothInterface.cs" "!BATPATH!..\EdiabasLib\EdCustomAdapterCommon.cs" "!BATPATH!..\EdiabasLib\EdCustomWiFiInterface.cs" "!BATPATH!..\EdiabasLib\EdElmInterface.cs" "!BATPATH!..\EdiabasLib\EdElmWifiInterface.cs" "!BATPATH!..\EdiabasLib\TcpClientWithTimeout.cs" "!BATPATH!..\EdiabasLib\IniFile.cs" "!BATPATH!..\EdiabasLib\TcpClientWithTimeout.cs"
