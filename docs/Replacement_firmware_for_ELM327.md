# Replacement firmware for ELM327
There is now a replacement firmware available for ELM327L based Bluetooth and WiFi adapters, that has the following advantages over the standard firmware:
* Faster and more stable CAN communication.
* K-Line support (all protocols).
* New: Support for the VAG protocols KWP2000, KWP1281, TP2.0 (cars until 4.2012).
* Contains a bootstrap loader that allows firmware update without opening the device.
* Firmware updates are possible with _[Deep OBD for BMW and VAG](Deep_OBD_for_BMW_and_VAG.md)_, if the repacement firmware [has already been programmed](#programming-of-the-processor).
* Reduced power consumption due to use of sleep mode.
* Flashing of a modified [ELM327 firmware](#elm327-v15-firmware) is possible.
* Two firmware versions are available:
  * Unmodified Bluetooth and WiFi adapter: Baud rate 38400.
  * Modified Bluetooth adapter (recommended) with replaced [OpenSource Bluetooth firmware](Custom_Bluetooth_firmware.md): Baud rate 115200 and alterable Bluetooth pin (16 digits) and name (31 chars).

![Bluetooth adapter top](Replacement_firmware_for_ELM327_BluetoothAdapterTopSmall.png) ![Bluetooth adapter bottom](Replacement_firmware_for_ELM327_BluetoothAdapterBottomSmall.png)

## Buy a standard Bluetooth adapter
It is required to buy adapters based on a PIC18F25K80 microcontroller.  
Search for `PIC18F25K80 ELM327` on Aliexpress. It's best to buy it with a Bluetooth module based on a _CSR BC417_ chip. The ones with _BK3231_ chip are supported too.  
For programming the firmware see: [programming](#programming-elm327-adapter-with-deep-obd-firmware)

## Buy a preprogrammed adapter
You could buy the preprogrammed [Bluetooth adapter](https://www.ebay.de/itm/255873781501) also at EBAY.  
Adapters from this link include a **license** for the [BMW coding](BMW_Coding.md) function of one F series or higher vehicle.  
If the link is outdated (what it is most of the time) all adapters are sold, in this case please simply wait for an update of the link.  

Vehicles `E36`, `E38`, `E39`, `E46`, `E52`, `E53`, `E83` , `E85` and `E86` additionally require a connection between OBD pin 7 and 8 (or a pin7-pin8 adapter) to access all ECUs.  
For vehicles with OBD I socket in the engine bay additionally the pin 8 of the OBD II socket has to be connected at the vehicle side ([`OBD1-OBD2.pdf`](OBD1-OBD2.pdf))!  
_Hint:_ For some Android radio models the Bluetooth name `OBDII` is required for pairing!  
_Hint:_ To prevent entering Bluetooth PIN manually on the android smartphone you could use the app [Bluetooth Force Pin Pair](https://play.google.com/store/apps/details?id=com.solvaig.forcepair) and configure the PIN accordingly.  
For BMW F-models use the [ENET WiFi Adapter](ENET_WiFi_Adapter.md).

## Factory reset
Beginning with firmware version 0.6 there is the possibility to perform a factory reset of the adapter. This resets the Bluetooth pin to 1234, the Bluetooth name to Deep OBD BMW and the mode to D-CAN.  
To perform the factory reset you have to open the adapter and connect the unused pad of R26 with GND during power on.

## Use the adapter with INPA, Tool32 or ISTA-D
You could use the Bluetooth adapter on a windows PC with INPA, Tool32 or ISTA-D as a replacement for an OBD or ADS adapter. The following steps are required to establish the connection:
* Install [.NET framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472) or higher and [VS2015 C++ runtime](https://www.microsoft.com/de-de/download/details.aspx?id=48145) (recommended, but not required)
* Download the [latest binary](https://github.com/uholeschak/ediabaslib/releases/latest) package and extract the .zip file. Start `Api32\EdiabasLibConfigTool.exe` and follow the instructions in the status window: Search the adapter, select it, click `Check Connection` and patch the required EDIABAS installations.
* For ISTA-D: You have to select the `EDIABAS\bin` directory inside ISTA-D first.
* For ISTA-D: In _Administration_ -> `VCI Config` select as `Interface type`: `Ediabas default settings (ediabas.ini)`

![EdiabasLib Config Tool](Replacement_firmware_for_ELM327_ConfigToolBluetoothSmall.png)

## Programming ELM327 adapter with Deep OBD firmware
First programming of PIC18F25K80 microcontroller should be done with a PICkit 3 programmer. Here is instruction for [flashing ELM327 adapter with Deep OBD firmware](Replace_ELM327_HC04_Firmware.md).

The source for the firmware could be found in the subdirectory `CanAdapterElm`. The subdirectory names below are the Bluetooth chip types:
* `default`: For unmodified ELM327L adapter with any Bluetooth chip. Baud rate 38400
* `def115200`: For ELM327L adapter with any Bluetooth chip but modified baud rate 115200 (E.g. external YC1021 with modified EEPROM)
* `bc04`: For adapter with BK3231 Bluetooth chip and bc04 firmware
* `hc04`: For adapter with BC417 Bluetooth chip and hc04, hc05 and hc06 firmware
* `esp8266`: For adapter with ESP8266 WiFi chip [Replace ESP8266ex firmware](Replace_Elm327_Wifi_Mini_Firmware.md)
* `yc1021`: For adapter with integrated YC1021 Bluetooth chip (with non standard LED connection) [Replace YC1021 firmware](Replace_Elm327_BT_Mini_Firmware.md)
* `spp_uart` and `spp_uart2` (with modified LED configuration): [OpenSource Bluetooth firmware](Custom_Bluetooth_firmware.md) for adapters with BC417 Bluetooth chip (recommended for old Android car radios with Rockchip platform)
  * `spp_uart.xpv` and `spp_uart.xdv`: Firmware for Bluetooth module with BC417 chipset
  * `usbspi.dll`: This is a replacement library for _BlueSuite_ and _BlueLab_ for programming CSR BC03/BC04 Bluetooth chipsets via FT232R breakout boards. For more information see the [`ReadMe.txt`](../EdiabasLib/CanAdapterElm/Bluetooth/spp_uart/ReadMe.txt) file.

There are two firmware files, the complete file (`CanAdapterElm.X.production.unified.hex`) and the update file (`CanAdapterElm.X.production.hex`) without bootloader, that is only needed by _[Deep OBD for BMW and VAG](Deep_OBD_for_BMW_and_VAG.md)_.  
The latest firmware version will be always included in _[Deep OBD for BMW and VAG](Deep_OBD_for_BMW_and_VAG.md)_.  
Also you can get compiled firmware files from the [latest binary](https://github.com/uholeschak/ediabaslib/releases/latest) package.

## ELM327 V1.5 firmware
There is improved ELM327 V1.5 (V1.4 with patched version number) firmware available. It switches adapter to a mode compatible with most ELM327 softwares. 
Flashing can be done over bluetooth with _[Deep OBD for BMW and VAG](Deep_OBD_for_BMW_and_VAG.md)_ (at the moment this is only available for Bluetooth adapters).
Afterwards you can flash the Deep OBD replacement firmware over Bluetooth firmware again.  
Binariy files (complete `ELM327V15.X.production.unified.hex`) and (update `ELM327V15.X.production.hex`) could also found in the the [latest binary](https://github.com/uholeschak/ediabaslib/releases/latest) package.