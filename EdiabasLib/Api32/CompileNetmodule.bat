@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0

"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe" %* /nologo /target:module "!BATPATH!..\ApiInternal\ApiInternal.cs" "!BATPATH!..\EdiabasLib\EdiabasNet.cs" "!BATPATH!..\EdiabasLib\EdOperations.cs" "!BATPATH!..\EdiabasLib\MemoryStreamReader.cs" "!BATPATH!..\EdiabasLib\EdInterfaceBase.cs" "!BATPATH!..\EdiabasLib\EdInterfaceObd.cs" "!BATPATH!..\EdiabasLib\EdInterfaceAds.cs" "!BATPATH!..\EdiabasLib\EdFtdiInterface.cs" "!BATPATH!..\EdiabasLib\Ftd2xx.cs" "!BATPATH!..\EdiabasLib\NativeUsbLib\Device.cs" "!BATPATH!..\EdiabasLib\NativeUsbLib\UsbApi.cs" "!BATPATH!..\EdiabasLib\NativeUsbLib\UsbBus.cs" "!BATPATH!..\EdiabasLib\NativeUsbLib\UsbController.cs" "!BATPATH!..\EdiabasLib\NativeUsbLib\UsbDevice.cs" "!BATPATH!..\EdiabasLib\NativeUsbLib\UsbHub.cs"
