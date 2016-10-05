Build openocd:
--------------
In cygwin:
libusb (devel) must be installed
./configure
make

Build gdb:
----------
In cygwin:
xpat (devel) library must be installed
./configure --target=arm-elf --prefix=/usr/arm-elf
make
make install

Run gdb:
--------
Start openocd with:
start_openocd.bat

In Source directory call:
/usr/arm-elf/bin/arm-elf-gdb.exe

A .gdbinit with the following content must be present in the directory:

file test_bt.axf
target remote localhost:3333
set remote hardware-breakpoint-limit 2
set remote hardware-watchpoint-limit 2
set mem inaccessible-by-default off

flash test_bt.bin
-----------------
from openocd terminal:
flash_write_area_fast test_bt.bin 0x0000

flash test_bt_cfg.bin:
---------------------
from openocd terminal:
flash_write_area_fast test_bt_cfg.bin 0x3E000
