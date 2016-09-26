@echo off
set PATH=%PATH_CYGWIN%\bin;%PATH%
openocd.exe -f bk3231.cfg
