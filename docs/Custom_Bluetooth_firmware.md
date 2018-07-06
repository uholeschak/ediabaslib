# Custom Bluetooth firmware
The newest generation of Bluetooth adapters use a custom Bluetooth firmware for the EXT4 chip that has HC05 compatible commands but without the requirement of setting the _mode_ pin for configuration. All commands have to be terminated by a `<CR><LF>`.  
It's also possible to configure the device via the Bluetooth interface, to activate this mode the first command after connection has to be `AT+CONF`.  
In contrast to the UART interface the commands via Bluetooth interface must be send in one telegram.  
The firmware supports the following commands:
* `AT`: Simply responds with `OK`.
* `AT+ORGL`: Performs a factory reset. The default values are: Baud rate=115200 8N1, Name=Deep OBD BMW, Pin=1234. The settings will become active immediately.
* `AT+RESET`: Resets the Bluetooth firmware without changing any values.
* `AT+CONF`: Switch the Bluetooth interface to configuration mode. This must be the first command after connecting, otherwise the devices switches to transparent data mode.
* `AT+DATA`: Switch the Bluetooth interface to transparent data mode. This command is only valid for the Bluetooth interface.
* `AT+PSWD?`: Read Bluetooth pin. The response is `+PSWD:<pin>`.
* `AT+PSWD=<pin>`: Write Bluetooth pin. The minimum pin length is 4 and the maximum length is 16 digits.
* `AT+NAME?`: Read the Bluetooth name. The response is `+NAME:<name>`.
* `AT+NAME=<name>`: Write the Bluetooth name. The maximum name length is 31 chars.
* `AT+UART?`: Read the UART settings. The response is `+UART:<baud rate>,<stop bits>,<parity>`. Possible baud rate values are: 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600, 1382400. The coding for stop bits is: 0=1 stop bit, 1=2 stop bits and the coding for parity is: 0=None, 1=Odd, 2=Even.
* `AT+UART=<baud rate>,<stop bits>,<parity>`: Write the UART settings. The settings will become active immediately.
* `AT+VERSION?`: Read the Firmware version. The response is `+VERSION:<version>`.
* `AT+FWUPDATE`: Switches the device to firmware update mode. In this mode firmware update is possible via UART interface using the _BlueSuite_ tool _DfuWizard_. The interface setting in this mode is BCSP 115200 8N1.
