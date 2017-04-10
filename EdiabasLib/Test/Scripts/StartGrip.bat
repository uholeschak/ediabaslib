@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0
"%PATH_PYTHON%\python.exe" -m grip "!BATPATH!\..\..\.."
