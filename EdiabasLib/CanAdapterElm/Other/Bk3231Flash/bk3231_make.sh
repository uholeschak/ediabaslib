#!/bin/sh
# Use in mingw shell
# KEIL 5 ARM MDK must be installed at: PATH_KEILARM
# flash_write_loader D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/Other/Bk3231Flash/write_flash.bin

arm_path=$(echo "/$PATH_KEILARM/ARMCC/bin/" | sed 's/\\/\//g' | sed 's/://')
export PATH=$arm_path:$PATH
make -k -B
