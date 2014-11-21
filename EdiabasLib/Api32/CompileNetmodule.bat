@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0

"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe" %* /target:module "!BATPATH!..\ApiInternal\ApiInternal.cs" "!BATPATH!..\EdiabasLib\EdiabasNet.cs" "!BATPATH!..\EdiabasLib\EdOperations.cs" "!BATPATH!..\EdiabasLib\MemoryStreamReader.cs" "!BATPATH!..\EdiabasLib\EdInterfaceBase.cs" "!BATPATH!..\EdiabasLib\EdInterfaceObd.cs"
