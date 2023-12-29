@echo off
SETLOCAL EnableDelayedExpansion

set "BatDir=%~dp0"
set "RootDir=!BatDir!\.."
for %%i in ("!RootDir!") do SET "RootDir=%%~fi"

set "SrcDir=!RootDir!\BmwDeepObdNet"
set "DstDir=!RootDir!\BmwDeepObd"

echo Source: !SrcDir!
echo Dest: !DstDir!

ROBOCOPY "!SrcDir!\Dialogs" "!DstDir!\Dialogs" /MIR
ROBOCOPY "!SrcDir!\DownloaderService" "!DstDir!\DownloaderService" /MIR
ROBOCOPY "!SrcDir!\FilePicker" "!DstDir!\FilePicker" /MIR
ROBOCOPY "!SrcDir!\InternalBroadcastManager" "!DstDir!\InternalBroadcastManager" /MIR
ROBOCOPY "!SrcDir!\Resources" "!DstDir!\Resources" /MIR
ROBOCOPY "!SrcDir!\Scripts" "!DstDir!\Scripts" /MIR
ROBOCOPY "!SrcDir!\Xml" "!DstDir!\Xml" /MIR
ROBOCOPY "!SrcDir!" "!DstDir!" *.cs
ROBOCOPY "!SrcDir!" "!DstDir!\Properties" AndroidManifest.xml
