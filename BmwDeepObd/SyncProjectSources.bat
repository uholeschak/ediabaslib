@echo off
SETLOCAL EnableDelayedExpansion

set "BatDir=%~dp0"
set "RootDir=!BatDir!\.."
for %%i in ("!RootDir!") do SET "RootDir=%%~fi"

if "%1"=="FROM_NET" (
  set "FromNet=1"
  set "SrcDir=!RootDir!\BmwDeepObdNet"
  set "DstDir=!RootDir!\BmwDeepObd"
) else (
  if "%1"=="TO_NET" (
    set "FromNet=0"
    set "SrcDir=!RootDir!\BmwDeepObd"
    set "DstDir=!RootDir!\BmwDeepObdNet"
  ) else (
    echo valid modes: TO_NET, FROM_NET    
    exit /b 1
  )
)

echo FromNet: !FromNet!
echo Source: !SrcDir!
echo Dest: !DstDir!

ROBOCOPY "!SrcDir!\Dialogs" "!DstDir!\Dialogs" /MIR
ROBOCOPY "!SrcDir!\DownloaderService" "!DstDir!\DownloaderService" /MIR
ROBOCOPY "!SrcDir!\FilePicker" "!DstDir!\FilePicker" /MIR
ROBOCOPY "!SrcDir!\InternalBroadcastManager" "!DstDir!\InternalBroadcastManager" /MIR
ROBOCOPY "!SrcDir!\Resources" "!DstDir!\Resources" /MIR
ROBOCOPY "!SrcDir!\Scripts" "!DstDir!\Scripts" /MIR
ROBOCOPY "!SrcDir!\Xml" "!DstDir!\Xml" /MIR
ROBOCOPY "!SrcDir!" "!DstDir!" *.cs *.cfg /XF "!SrcDir!\Resource.designer.cs"
if "!FromNet!"=="1" (
  ROBOCOPY "!SrcDir!" "!DstDir!\Properties" AndroidManifest.xml
) else (
  ROBOCOPY "!SrcDir!\Properties" "!DstDir!" AndroidManifest.xml
  IF EXIST "!DstDir!\Resource.designer.cs" DEL /q "!DstDir!\Resource.designer.cs"
  IF EXIST "!DstDir!\Resources\Resource.designer.cs" DEL /q "!DstDir!\Resources\Resource.designer.cs"
)

echo done
exit /b 0
