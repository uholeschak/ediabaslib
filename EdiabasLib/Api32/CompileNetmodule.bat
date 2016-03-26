@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0
set ASSEM_PATH=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\Profile\Client

"%programfiles(x86)%\MSBuild\14.0\Bin\csc.exe" %* /nologo /noconfig /nostdlib+ /target:module /reference:"!ASSEM_PATH!\mscorlib.dll" /reference:"!ASSEM_PATH!\System.dll" /reference:"!ASSEM_PATH!\System.Data.dll" /reference:"!ASSEM_PATH!\System.Xml.dll" /reference:"!ASSEM_PATH!\System.Core.dll" "!BATPATH!..\ApiInternal\ApiInternal.cs" "!BATPATH!..\EdiabasLib\EdiabasNet.cs" "!BATPATH!..\EdiabasLib\EdOperations.cs" "!BATPATH!..\EdiabasLib\MemoryStreamReader.cs" "!BATPATH!..\EdiabasLib\EdInterfaceBase.cs" "!BATPATH!..\EdiabasLib\EdInterfaceObd.cs" "!BATPATH!..\EdiabasLib\EdInterfaceAds.cs" "!BATPATH!..\EdiabasLib\EdInterfaceEnet.cs" "!BATPATH!..\EdiabasLib\EdFtdiInterface.cs" "!BATPATH!..\EdiabasLib\Ftd2xx.cs" "!BATPATH!..\EdiabasLib\EdBluetoothInterface.cs" "!BATPATH!..\EdiabasLib\EdBluetoothInterfaceBase.cs"
