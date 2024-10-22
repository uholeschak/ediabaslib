# EdiabasLib simuklation file
EdiabasLib is supporting simulation files lie teh standard EDIABAS, but with some additional features.  
Simulation files are `.ini` files with the `.sim` file extension.  
The simulation file name is either the interface name (e.g. `enet.sim`, `obd.sim`, `edic.sim`) or the `.prg` file name with `.sim` extension.  
Basically there are section for request, response and key bytes.

## Standard syntax
Section `[REQUEST]`:
Key value pair with request bytes separated by comma. For wildcards the symbol `X` could be used.  
An empty line has the value `_`.

Section `[RESPONSE]`:
Key value pair with request bytes separated by comma. No wildcards or empty lines are allowed.  
The key must match the request key.

Section `[KEYBYTES]`:
Key value pair with request bytes separated by comma. No wildcards or empty lines are allowed.

Sample BMW-FAST:
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
