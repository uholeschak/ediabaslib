# Custom Bluetooth firmware
For _HC-04_, _HC-05_, _HC-06_ Bluetooth adapters which are built on _CSR BC03_ and _BC04_ (BlueCore 3/4) chipsets, there is a custom firmware available.  
It allows to configure Bluetooth module settings via the Bluetooth interface without the requirement to set _mode_ (PIN34/PIO11/KEY) pin high of the module during powering on.  
To enter this configuration mode, the first command after establishing Bluetooth connection has to be `AT+CONF`. It is required to send this in one telegram, in capital letters and terminated by `<CR><LF>`.  
Examples of terminal programs that support this feature:
* _Serial Bluetooth Terminal_ - Android
* _HTerm_ - Windows

The firmware supports following CSR compatible command list:
* `AT`: Simply responds with `OK`.
* `AT+ADDR`: Displays the MAC address of the module. You can only read the current setting. To change the MAC address you have to use `PSTool.exe` from _CSR BlueSuite_.
* `AT+ORGL`: Performs factory reset of the settings (will become active immediately) to Baud rate=`115200 8N1`, Name=`Deep OBD`, PIN=`1234`. 
* `AT+RESET`: Reboots firmware of the Bluetooth module without changing any settings.
* `AT+DATA`: Exits the configuration mode and switches Bluetooth interface back to transparent data mode.
* `AT+PSWD?`: Displays the Bluetooth PIN. The response is `+PSWD:<pin>`.
* `AT+PSWD=<pin>`: Write the Bluetooth PIN. The minimum pin length is 4 and the maximum length is 16 digits.
* `AT+NAME?`: Displays the Bluetooth name. The response is `+NAME:<name>`.
* `AT+NAME=<name>`: Write the the Bluetooth name. The maximum name length is 31 chars.
* `AT+UART?`: Displays the UART settings. The response is `+UART:<baud rate>,<stop bits>,<parity>`. Possible baud rate values are: 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600, 1382400. The coding for stop bits is: 0=1 stop bit, 1=2 stop bits and the coding for parity is: 0=None, 1=Odd, 2=Even.
* `AT+UART=<baud rate>,<stop bits>,<parity>`: Sets UART settings (will become active immediately).
* `AT+VERSION?`: Displays the firmware version. The response is `+VERSION:<version>`.
* `AT+FWUPDATE`: Switches module to firmware update mode via UART interface using the _BlueSuite_ tool _DfuWizard_. The interface setting in this mode is BCSP 115200 8N1.
