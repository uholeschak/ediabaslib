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

Sample with BMW-FAST telegrams:
```ini
[REQUEST]
key1=83,01,F1,19,02,0C
key2=83,01,F1,22,20,00

[RESPONSE]
key1=83,F1,01,59,02,7F,4F
key2=93,F1,01,62,20,00,01,00,12,28,80,32,80,28,80,31,80,28,01,00,20,28,3E
```

Sample ISO 9141:
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

### ECU address
With the extended syntax the ECU address could be specified in every section entry.  
Intis case section name has to be prefixed by the ecu address in hex:`[<ECU addr>.<section name>]`.  
The address is either the 8 bit ECU address or the 16 bit CAN address in case of the UDS protocol.

### Request value matching
The request values could be matched by a mask and the and operator.  
The syntax is `<two digit hex value><operator><two digit hex mask>`.  
Valid operators are `&`, `|`.
Example: The two highes bits have to be hex `80` and the 6 lower bits are ignored: `80&C0` or `80|3F`

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

Sample with ISO 9141 telegrams:
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
