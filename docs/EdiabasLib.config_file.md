# Properties of the `EdiabasLib.config` file
Current EDIABAS emulation version is 7.6.0.  
The `EdiabasLib.config` file is a replacement for the standard `EDIABAS.INI` file. It must be located in the same directory as the `EdiabasLib.dll`, `Api32.dll` or `Api64.dll` file.  
The following properties could be specified in this file:

## Standard properties
* `Interface`: Interface to use: _STD:OBD_, _ADS_ (ADS operates also with FTDI USB converter) _ENET_ or _RPLUS_.  
The _RPLUS_ IFH format is: `RPLUS:<name default=ICOM_P>:Remotehost=<ip address>;Port=<port default=6801>`
* `ApiTrace`: API debug level (0..1)
* `IfhTrace`: Interface debug level (0..3)
* `TraceBuffering`: 1=Enable buffering of API trace files
* `TracePath`: Path for API and IFH trace file storage. The directory is created if it does not exist.  
If not specified, the subdirectory `Trace` will be used. If the the subdirectory is not writable, the default trace path is `C:\Users\<User>\AppData\Local\EdiabasLib\Trace`.
* `EcuPath`: Path to the ecu files location.
* `RetryComm`: 1=Retry communication.
* `EnetRemoteHost`: Remote host for ENET protocol. Possible values are:
	* `ip address:<protocol>:<diag port>:<control port>`: No broadcast, directly connect to specified host. Optionally the protocol (`HSFZ` or `DoIP`) and the communication ports could be specified.
	* `auto`: Broadcast to subnet `HostIdentService`, Identical to EDIABAS `RemoteHost = Autodetect`. When multiple network adapters are present, this may work unreliable. Use `auto:all` instead.
	* `auto:all`: Broadcast to all network interfaces.
	* `auto:<interface name>`: Broadcast to all interfaces that start with `<interface name>` (case ignored).
* `EnetVehicleProtocol`, `VehicleProtocol`: Order of vehicle protocols used, separated by comma. Possible vales are `HSFZ` and `DoIP`.
* `EnetHostIdentService`, `HostIdentService`: IPv4 netmask for vehicle searching. Default is `255.255.255.255`. This has changed with Ediabas 7.6.0.
* `EnetTesterAddress`: Tester address for HSFZ protocol, standard is the hex value `0xF4`.
* `EnetDoIPTesterAddress`, `DoIPTesterAddress`: Tester address for DoIP protocol with optional `0x` prefix, default is the hex value `0EF3`.
* `EnetDoipGatewayAddress`, `DoipGatewayAddress`: Gateway address for DoIP protocol with optional `0x` prefix, default is the hex value `0010`.
* `EnetControlPort`, `ControlPort`: Control port for HSFZ protocol, standard port is `6811`.
* `EnetDiagnosticPort`, `DiagnosticPort`: Diagnostic port for HSFZ protocol, standard port is `6801`.
* `EnetDoIPPort`, `DoIPPort`: Port for DoIP protocol, standard port is `13400`
* `EnetTimeoutConnect`, `TimeoutConnect`: Connect timeout for ENET protocol, default is `5000`

When using BMW ICOM, change the values of `EnetControlPort` and `EnetDiagnosticPort` to the output from the BMW ICOM web interface line:  
Example: `Diag Addr: 0x10 Diagnostic Port: 50160 Control Port: 50161`  
For automatic ICOM allocation set `EnetIcomAllocate` to 1, otherwise manual allocation via iToolRadar or the web interface is required!  
Web interface URL: `http://XXXX:60080/cgi-bin/channeldeviceconfig.cgi`  
Login user name: `root`  
Login password: `NZY11502` or `NZY1150263`  
The standard ICOM configuration page could be found at: `http://XXXX:58000` (no login required).

## Non standard properties
* `ObdComPort`: COM port name for OBD interface.
* `AdsComPort`: COM port name for ADS interface (if not specified ObdComPort will be used).
* `AppendTrace`: 0=Override old log file, 1=Always append the logfiles.
* `LockTrace`: 0=Allow changing `IfhTrace` level from the application, 1=Prevent changing the `IfhTrace` level from the application.
* `CompatMode`: EDIABAS 7.6.0 has incompatible changes in mathematical function. This prevents using old ECU files. Setting this value to 1 (what is the default), keeps the behaviour of EDIABAS 7.3.0.
* `EnetAddRecTimeout`: Additional ENET standard additional receive timeout, default is 1000
* `EnetAddRecTimeoutIcom`: Additional ENET ICOM additional receive timeout, default is 2000
* `EnetIcomAllocate`: 1=Allocate ICOM before connecting, default is 0. This parameter is only used, if IFH is `RPLUS` or `ENET` and the diagnostic port has been set.
* `ObdKeepConnectionOpen`: 0=Close the OBD transport (Bluetooth SPP / custom Wi-Fi) after each job (default),  
  1=Keep the transport connection open across jobs.  
  Useful for repeated polling or batch jobs to reduce reconnect delays. May prevent other apps from using the adapter while open and can cause stability issues with some devices.
* `IcomEnetRedirect_<name=ICOM_P>`: 1=Enable redirect `RPLUS` to `HSFZ` protocol if `HSFZ` has been detected by ICOM. Default is 1 if name is `ICOM_P` and port `6801`.

## FTDI D2XX driver properties
Android supports access to FTDI USB D-CAN/K-Line adapters directly. For the PC platform use the COM port to access the adapter.  
There are the following ways the select the USB device:
* `FTDIx`: select device by index x (starting with 0)
* `FTDI:SER=[serial number](serial-number)`: select device by serial number

## Bluetooth (Android)
Since Android devices normally have no COM ports, it's possible to connect via Bluetooth using the Bluetooth Serial Port Protocol (SPP).
To select a specific Bluetooth device use `BLUETOOTH:<device address>`.  
When using ELM327 adapters append `;ELM327` after the Bluetooth address to specify ELM327 mode.
For adapters that simply send data without modification append `;RAW`.

## Bluetooth (PC)
It's possible to use the [Replacement firmware for ELM327](Replacement_firmware_for_ELM327.md) also with a PC.  
Set `ObdComPort` to `BLUETOOTH:<Bluetooth device address>#<Bluetooth PIN>` for EDR or `BLUETOOTH:<Bluetooth device address>#BLE` for BLE.  
Using a Bluetooth COM port is not recommended any more, remove the COM ports before using the Bluetooth interface.  
The recommended way for setting up the configuration is to use the [EdiabasLibConfigTool](Replacement_firmware_for_ELM327.md#use-the-adapter-with-inpa-tool32-or-ista-d).

## WiFi
For WiFi based ELM327 adapters you have to specify `STD:OBD` for the `interface` and `ELM327WIFI` for `ObdComPort`.  
When using the [Replacement firmware for ELM327](Replacement_firmware_for_ELM327.md) with WiFi devices specify `DEEPOBDWIFI` for `ObdComPort`.
