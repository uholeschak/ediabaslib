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

Device connection:
------------------
Pinout: http://www.instructables.com/id/AT-command-mode-of-HC-05-Bluetooth-module/

| Signal | FT232RL pin | FTDI pin name | FTDI GPIO bit | CSR pin  |
|--------|-------------|---------------|---------------|----------|
| CS#    | 2           | DTR#          | D4            | SPI_CS#  |
| CLK    | 3           | RTS#          | D2            | SPI_CLK  |
| MOSI   | 6           | RI#           | D7            | SPI_MOSI |
| MISO   | 9           | DSR#          | D5            | SPI_MISO |
| GND    | 7, 18, 21   | GND           | --            | GND      |


Test connection:
----------------
- Open command promt in C:\Program Files (x86)\CSR\BlueSuite XXX
- If there are communication problems add: -trans SPIMAXCLOCK=100 
- BlueFlashCmd.exe chipver
- BlueFlashCmd.exe identify
- Dump flash: BlueFlashCmd.exe dump <filename_without_extension>

Programming:
------------
- BlueFlashCmd.exe <filename_without_extension>
