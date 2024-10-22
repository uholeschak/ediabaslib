# EdiabasLib simuklation file
EdiabasLib is supporting simulation files lie teh standard EDIABAS, but with some additional features.
Simulation files are `.ini` files with the `.sim` file extension.
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
