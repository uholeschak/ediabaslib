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

| Signal | FT232RL pin | FTDI pin name | FTDI GPIO bit | CSR pin  | HC-05 pin |
|--------|-------------|---------------|---------------|----------|-----------|
| CS#    | 2           | DTR#          | D4            | SPI_CS#  | 16        |
| CLK    | 3           | RTS#          | D2            | SPI_CLK  | 19        |
| MOSI   | 6           | RI#           | D7            | SPI_MOSI | 17        |
| MISO   | 9           | DSR#          | D5            | SPI_MISO | 18        |
| GND    | 7, 18, 21   | GND           | --            | GND      | 21        |


Test connection:
----------------
- Open command promt in C:\Program Files (x86)\CSR\BlueSuite XXX
- If there are communication problems add: -trans SPIMAXCLOCK=100 
- BlueFlashCmd.exe chipver
- BlueFlashCmd.exe identify
- Dump flash: BlueFlashCmd.exe dump <filename_without_extension>

Changing a parameter:
---------------------
- Dump current settings:
pscli dump <xxx.psr>
- Create merge file merge.psr with new settings (e.g. clock 13Mhz):
// PSKEY_ANA_FREQ
&01fe = 32c8
- Merge settings:
pscli merge <merge.psr>

Programming:
------------
- BlueFlashCmd.exe <filename_without_extension>

Compiling:
----------
- Install BlueLab 4.1 to C:\Programs\BlueLab41
- In the directory C:\Programs\BlueLab41\tools\bin replace usbspi.dll
- Download "BlueCore4-External Unified 23i firmware for OEMs, 56-bit encryption"
  (cyt_8unified_fl_bt3.0_23i_0911261257_encr56_oem_prod.zip) from https://www.csrsupport.com
- Replace the firmware in C:\Programs\BlueLab41\firmware\vm\unified\coyote with the downloaded version (backup old firmware first!)
- Open command promt in source directory
- Flash release with BlueFlashCmd.exe first, otherwise the parmeters will be set incorrectly!
- Special setting in original file:
  Bootmode none: Host interface(PSKEY_HOST_INTERFACE)=UART link running BCSP
  Bootmode 1: Host interface(PSKEY_HOST_INTERFACE)=VM access to UART
  Always set bootmode 1 first, otherwise access to the chip is impossible afterwards!
- Compile and flash: compile.bat flash
- Clean: compile.bat clean
- Build: compile.bat build

6 Mbit Flash
----------
For 6 Mbit flahes the compact firmware is required, for this select:
Project Properties -> Build System -> General -> Firmware -> Compact
Additionally a 4.7K pullup resistor to 3.3V is required for the TX line!

When changing the settings in the project, build once with the IDE to update the makefile!
