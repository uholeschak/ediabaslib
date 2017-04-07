# Properties of the EdiabasLib.config file
The _EdiabasLib.config_ file is a replacement for the standard _EDIABAS.INI_ file. It must be located in the same directory as the EdiabasLib.dll or Api32.dll file.
The following properties could be specified in this file:
## Standard properties
* _Interface_: Interface to use: _STD:OBD_, _ADS_ (ADS operates also with FTDI USB converter) or _ENET_.
* _ApiTrace_: API debug level (0..1)
* _IfhTrace_: Interface debug level (0..3)
* _TraceBuffering_: 1=Enable buffering of API trace files
* _EcuPath_: Path to the ecu files
* _RetryComm_: 1=Retry communication
* _EnetRemoteHost_: Remote host for ENET protocol. Possible values are:
	* _{"ip address"}_: No broadcast, directly connect to specified host.
	* _{"auto"}_: Broadcast to 169.254.255.255, Identical to EDIABAS "RemoteHost = Autodetect.
	* _{"auto:all"}_: Broadcast to all network interfaces.
	* _{"auto:<interface name>"}_: Broadcast to all interfaces that start with <interface name> (case ignored).
* _EnetTesterAddress_: Tester address for ENET protocol, standard is 0xF4
* _EnetControlPort_: Control port for ENET protocol, standard is 6811
* _EnetDiagnosticPort_: Diagnostic port for ENET protocol, standard is 6801
## Non standard properties
* _ObdComPort_: COM port name for OBD interface.
* _AdsComPort_: COM port name for ADS interface (if not specifed ObdComPort will be used).
* _AppendTrace_: 0=Override old log file, 1=Always append the logfiles.
* _LockTrace_: 0=Allow changing _IfhTrace_ level from the application, 1=Prevent changing the _IfhTrace_ level from the application.
## FTDI D2XX driver properties
For improved timing (especially required for ADS adapter) it's possible to access the FTDI D2XX driver directly. Android also supports access to FTDI USB D-CAN/K-Line adapters. To activate this mode use FTDI instead of COM in the com port name for _ObdComPort_ or _AdsComPort_. There are multiply ways the select the USB device:
* _{"FTDIx"}_: select device by index x (starting with 0)
* _{"FTDI:SER=[serial number](serial-number)"}_: select device by serial number
* _{"FTDI:DESC=[description](description)"}_: select device by description (not supported for Android)
* _{"FTDI:LOC=[location](location)"}_: select device by location id (either decimal or hex with 0x) (not supported for Android)
To get the device location information you could use FT Prog from the FTDI web site.
## Bluetooth (Android)
Since Android devices normally have no COM ports, it's possible to connect via Bluetooth using the Bluetooth Serial Port Protocol (SPP).
To select a specific Bluetooth device use BLUETOOTH:<device address> instead of COMx.
When using ELM327 adapters append ";ELM327" after the Bluetooth address to specify ELM327 mode.
## Bluetooth (PC)
It's possible to use the [Replacement firmware for ELM327](Replacement_firmware_for_ELM327.md) also with a PC. When connecting the adapter with the PC two serial COM ports are created (incoming and outgoing).
To use the adapter specify _STD:OBD_ for the _interface_ and _BLUETOOTH:<outgoing COM port>_ for _ObdComPort_.
## ELM327 WiFi
It's possible to use an ELM327 WiFi adapter. You have to specify _STD:OBD_ for the _interface_ and _ELM327WIFI_ for _ObdComPort_.