Programming CSR BC03 and BC04 chips:
------------------------------------
- Install CSR BlueSuite
- Get usbspi.dll from zip of latest binary release package: https://github.com/lorf/csr-spi-ftdi/releases (tested on 0.5.3)
- Copy and replace \lib-win32\usbspi.dll to C:\Program Files (x86)\CSR\BlueSuite X.X.X
- Download Zadig from: http://zadig.akeo.ie/
- Disconnect all other FTDI devices from the PC, otherwise the drivers will be modified!
- Open Options -> List All Devices, select FTDI device with name "Breakout Board" from the listbox and Replace Driver with libusbK

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

Tested with chips: BC352, BC358, BC417

Test connection and create backup:
----------------------------------
- Open command prompt in C:\Program Files (x86)\CSR\BlueSuite X.X.X
- If there are communication problems add: -trans SPIMAXCLOCK=100 
- Test: e2cmd info
- Test: BlueFlashCmd.exe identify
- Create backup of the flash: BlueFlashCmd.exe dump <backup file name>

Changing a parameter:
---------------------
- Backup current settings:
pscli dump backup.psr
- Create file merge.psr with new settings (e.g. clock 13Mhz):
// PSKEY_ANA_FREQ
&01fe = 32c8
- Merge settings:
pscli merge merge.psr


Compiling and flashing:
-----------------------
- Install BlueLab 4.1 to C:\Programs\BlueLab41
- In the directory C:\Programs\BlueLab41\tools\bin replace usbspi.dll
- If you have cyt_8unified_fl_bt3.0_23i_0911261257_encr56_oem_prod.zip (BlueCore4-External Unified 23i firmware for OEMs, 56-bit encryption), replace the firmware in C:\Programs\BlueLab41\firmware\vm\unified\coyote with it
- For modules with 4 or 6 Mbit flash chips, select Project Properties -> Build System -> General -> Firmware -> Compact. 
- Press F7 to build with IDE and create a makefile
- Open command promt in source directory and to compile and flash, enter: 
compile.bat flash
- After flashing is finished, run PsTool.exe and set the correct host interface for bootmode 1:
  Bootmode 1: Host interface(PSKEY_HOST_INTERFACE)=VM access to UART
  Bootmode none: Host interface(PSKEY_HOST_INTERFACE)=UART link running BCSP
  Always set bootmode 1 first, otherwise access to the chip is impossible afterwards!
- It's recommended to erase the user areas with erase.psr to get the default settings.

Restoring backup if something went wrong:
-----------------------------------------
BlueFlashCmd.exe <backup file name>

Note: An additional 4.7K pullup resistor to 3.3V maybe required at the TX line for some modules and compact firmware!
