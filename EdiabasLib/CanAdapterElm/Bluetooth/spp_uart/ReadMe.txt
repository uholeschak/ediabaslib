Programming HC-05 chip:
-----------------------
- Install CSR Bluesuite from: https://www.csrsupport.com

Install usbspi driver:
----------------------
- Checkout https://github.com/lorf/csr-spi-ftdi.git
- For mingw32 patch Makefile.mingw:
--
#TOOLCHAIN ?= i686-w64-mingw32-
TOOLCHAIN ?= mingw32-
--
- If multiple FTDI chips are connected to the PC patch spi.c:
function spi_enumerate_ports():
--
LOG(WARN, "FTDI: ftdi_usb_get_strings() failed: [%d] %s", rc, ftdi_get_error_string(&ftdic));
continue;   // [UH] ignore if no driver is installed
--

- Compile driver: make -f Makefile.mingw
- Copy obj-win32\usbspi.dll to C:\Program Files (x86)\CSR\BlueSuite XXX (backup old file first!)

- Download Zadig from: http://zadig.akeo.ie/
- Disconnect all other FTDI devices from the PC, otherwise the drivers will be modified!
- Change driver for FTDI breakout board to: libusbK

Test connection:
----------------
- Open command promt in C:\Program Files (x86)\CSR\BlueSuite XXX
- BlueFlashCmd.exe -trans SPIMAXCLOCK=1000 identify