# EdiabasLib simulation file
EdiabasLib is supporting simulation files like the standard EDIABAS, but with some additional features.  
Simulation files are `.ini` files with the `.sim` file extension.  
The simulation file name is either the lower case interface name (e.g. `enet.sim`, `obd.sim`, `edic.sim`) or the lower case SGBD (`.prg`) file name with `.sim` extension.  
Basically the simulation files contains sections for request, response and key bytes.

Simulation files could be auto generated from trace files by the [LogfileConverter](LogfileConverter_parameters.md).

## Configuration properties
EdiabasLib has the following properties to configure the simulation mode:
* `Simulation`: 1=Enable simulation mode, default is 0.
* `SimulationPath`: Path to the simulation directory containing the interface or SGBD simulation files.
* `SimulationCompat`: 1=Enable simulation compatibility mode, default is 0. If disabled, the property `Simulation` is hidden from the SGBD file and always 0.  
With enabled `Simulation` property the SGDB will read only the first response for BMW-FAST telegrams with functional addressing.  
The extended simulation file syntax allows multiple responses for one request in BMW-FAST mode. With this feature, ECU searching could be simulated.
* `SimulationInterfaces`: (Non standard) Comma separated list of interface names to check for corresponding lower case interface `.sim` file names. These names could contain wildcards.  
The Deep OBD app default is: `OBD,ENET,EDIC,OBD_*,ENET_*,EDIC_*`.

## Standard syntax
Section `[REQUEST]`:
Key value pair with request two digit hex bytes separated by comma. For wildcards the symbol `X` could be used.  
An empty line has the value `_`.

Section `[RESPONSE]`:
Key value pair with request two digit hex bytes separated by comma. No wildcards or empty lines are allowed.  
The key must match the request key.

Section `[KEYBYTES]`:
Key value pair with request two digit hex bytes separated by comma. No wildcards or empty lines are allowed.

Sample with BMW-FAST (OBD or ENET interface) telegrams:
The request and the response is the full frame including ECU address and checksum (XOR over all bytes).
```ini
[REQUEST]
key1=83,01,F1,19,02,0C
key2=83,01,F1,22,20,00

[RESPONSE]
key1=83,F1,01,59,02,7F,4F
key2=93,F1,01,62,20,00,01,00,12,28,80,32,80,28,80,31,80,28,01,00,20,28,3E
```

Sample with DS2 (OBD interface) telegrams:
The request and the response is the full frame including ECU address in request and response and checksum (sum over all bytes) only in the response.
```ini
[REQUEST]
key1=00,04,00
key2=00,05,08,00

[RESPONSE]
key1=00,10,A0,88,36,94,83,14,03,02,00,09,96,02,13,82
key2=00,07,A0,00,05,95,37
```

Sample ISO 9141 telegrams (EDIC interface, only valid in SGBD simulation file, because of missing ECU address):
The request and the response is the bare frame without ECU address and checksum.
```ini
[KEYBYTES]
key1=01,8A,00,A0,28,0F,01,F6,34,42,30,39,32,37,31,35,36,42,41,20,03,0F,03,F6,41,47,35,20,30,31,4C,20,34,2E,32,6C,03,0F,05,F6,35,56,20,20,52,64,57,20,31,32,31,34,03,08,07,F6,00,00,02,09,15,03,03,09,09,03

[REQUEST]
key1=03,XX,07
key2=03,XX,05

[RESPONSE]
key1=06,01,FC,FF,FF,88,03
key2=06,01,FC,FF,FF,88,03
```

## Extended syntax
The standard syntax is not very flexible when using wildcards and dynamic responses for variable requests.  
If one simulation file should be used for multiple ECUs the ECU address has to be configured additionally.  

### ECU (wake) address
With the extended syntax the ECU (wake) address could be specified in every section entry.  
The address is defined as follows for the different protocols:
* UDS ISO 14229: The address is the 16 bit CAN address.
* TP2.0: The address is the 8 bit ECU address.
* KWP 2000, ISO 9141: The address is the 8 bit ECU wake address.
* Other: The address is encoded in the request telegram and not required in the section entry.  

The section name has to be prefixed by the address in hex:`[<addr>.<section name>]`.  

### Request value matching
The request values could be matched by a mask combined with an operator.  
The syntax is `<two digit hex value><operator><two digit hex mask>`.  
Valid operators are `&`, `|`.
Example: The two highest bits have to be hex `80` and the 6 lower bits are ignored: `80&C0` or `80|3F`

### Variable request length
If the request length is variable, `..` could be appended to the base request telegram.  
Example: Request with fixed byte 1, variable byte 2 and optional more bytes: `key1=31,XX,..`

### Response value calculation
If the request has variables values (when using wildcards), the response has sometimes variable values using data from the request.  
For this a constant values could be combined via an operator with a variable value.  
Valid operators are `&`, `|`, `^`, `+`, `-`, `*`, `/`.

Using a value from the request: `<two digit hex constant><operator>[<request index in hex>]`.  
Example: Adding 1 to the request byte with index 2: `01+[02]`

Using the ecu address: `<two digit hex constant><operator>#<ECU address byte index in hex>`.  
Example: Adding 3 to the the ECU address low byte: `03+#00`

Calculating a checksum: `<two digit hex constant><operator>$<length in hex>`. If the length is `00` the complete length is used.  
In this case the valid operator are only: `^`, `+`.
Example: Calculate XOR checksum of the complete telegram with XOR start value 1: `01^$00`

Sample with UDS ISO 14229 telegrams (EDIC interface) with ECU address 0700:
The request and the response is the bare frame without ECU address and checksum.
```ini
[0700.REQUEST]
key1=22,06,XX
key2=31,XX,..
key3=2E,XX,XX,..

[0700.RESPONSE]
key1=7F,22,31
key2=7F,31,11
key3=6E,00|[01],00|[02]
```

Sample with TP2.0 telegrams (EDIC interface) with ECU address 10:
The request is the bare frame without ECU address and checksum.  
The response is the full frame including ECU address and checksum (in BMW-FAST format).
```ini
[10.REQUEST]
key1=10,89
key2=1A,9A

[10.RESPONSE]
key1=82,F1,10,50,89,5C
key2=83,F1,10,7F,1A,11,2E
```

It's valid to use UDS and TP2.0 ECUs in the same simulation file. The ECU address has a different format.

Sample with KWP 2000 telegrams (EDIC interface) with ECU wake address 01:
The request is the bare frame without ECU address and checksum.  
The response is the full frame including ECU address and checksum (in BMW-FAST format).
```ini
[01.KEYBYTES]
key1=EF,8F,FE,A0,28

[01.REQUEST]
key1=1A,9B
key2=1A,91

[01.RESPONSE]
key1=B0,F1,10,5A,9B,37,4C,30,39,30,37,34,30,31,20,20,20,30,31,31,30,03,00,2E,03,00,00,00,00,18,B5,33,2E,30,4C,20,56,36,54,44,49,20,20,47,30,30,30,41,47,20,20,FA
key2=91,F1,10,5A,91,0E,38,45,30,39,30,37,34,30,31,41,42,20,20,FF,2F
```

Sample with ISO 9141 (EDIC interface) telegrams with ECU wake address 01:
The request and the response is the bare frame without ECU address and checksum.
```ini
[01.KEYBYTES]
key1=01,8A,00,A0,28,0F,01,F6,34,42,30,39,32,37,31,35,36,42,41,20,03,0F,03,F6,41,47,35,20,30,31,4C,20,34,2E,32,6C,03,0F,05,F6,35,56,20,20,52,64,57,20,31,32,31,34,03,08,07,F6,00,00,02,09,15,03,03,09,09,03

[01.REQUEST]
key1=03,XX,07
key2=03,XX,05

[01.RESPONSE]
key1=06,01+[01],FC,FF,FF,88,03
key2=06,01+[01],FC,FF,FF,88,03
```

It's valid to use ISO 9141 and KWP 2000 ECUs in the same simulation file. The ECU address is identical.

Sample with BMW-FAST (OBD or ENET interface) telegrams:  
Functional request (`CX` header with bit 6-7 set) and multiple responses. Each response is a full frame including ECU address and checksum.
```ini
[REQUEST]
key1=C4,DF,F1,31,01,0F,06

[RESPONSE]
key1=84,F1,08,71,01,0F,06,04,84,F1,2C,71,01,0F,06,28,84,F1,18,71,01,0F,06,14,84,F1,0B,71,01,0F,06,07,84,F1,5E,71,01,0F,06,5A,84,F1,61,71,01,0F,06,5D,84,F1,30,71,01,0F,06,2C,84,F1,63,71,01,0F,06,5F,84,F1,56,71,01,0F,06,52,84,F1,0D,71,01,0F,06,09,84,F1,43,71,01,0F,06,3F,84,F1,44,71,01,0F,06,40,84,F1,10,71,01,0F,06,0C,84,F1,67,71,01,0F,06,63,84,F1,40,71,01,0F,06,3C,84,F1,5D,71,01,0F,06,59,84,F1,76,71,01,0F,06,72,84,F1,78,71,01,0F,06,74,84,F1,01,71,01,0F,06,FD,84,F1,60,71,01,0F,06,5C,84,F1,21,71,01,0F,06,1D,84,F1,29,71,01,0F,06,25,84,F1,22,71,01,0F,06,1E
```
