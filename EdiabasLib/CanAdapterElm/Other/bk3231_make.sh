#!/bin/sh
# Use in mingw shell
# KEIL 5 ARM MDK must be installed at: /c/Programs/Keil_v5
# In GNUmakefile change sed 's!\\!\/!g' to sed 's!\\\\!\/!g'

export PATH=$PATH:/c/Programs/Keil_v5/ARM/ARMCC/bin/
make -k -B
