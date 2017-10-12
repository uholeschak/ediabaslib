@echo off
if "%1" == "" goto usage
C:/Programs/BlueLab41/tools/bin/make -R BLUELAB=C:/Programs/BlueLab41/tools -f spp_uart.release.mak %*
exit /b 0

:usage
echo valid arguments are: clean, build or flash
exit /b 1
