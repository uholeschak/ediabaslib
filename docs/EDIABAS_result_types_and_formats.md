# Ediabas result types and formats
This text has been extracted from the Ediabas developer guide.
The formatting instruction uses the `apiResultText(...,Format)` function and must be configured as follows:

_\[Specifications\] Conversion characters_

| Conversion character | Conversion |
| -------------------- | ---------- |
| `C[HAR](HAR)` | **APICHAR APITEXT** |
| `B[YTE](YTE)` | **APIBYTE APITEXT** | 
| `I[NTEGER](NTEGER)` | **APIINTEGER APITEXT** | 
| `W[ORD](ORD)` | **APIWORD APITEXT** | 
| `L[ONG](ONG)` | **APILONG APITEXT** | 
| `D[WORD](WORD)` | **APIDWORD APITEXT** | 
| `R[EAL](EAL)` | **APIREAL APITEXT** |
| `T[EXT](EXT)` | **APITEXT APITEXT** |

Specifications:
```
         [-](-) [digit1](digit1)       [.digit2](.digit2)      [e,E](e,E)
         |   |              |              |
         |   |              |              |__  APIREAL: exponential representation
         |   |              |
         |   |              |__     APITEXT: max. number of characters specified
         |   |                      APIREAL: number of decimal places
         |   |
         |   |____ Minimum field size (2)
         |
         |_____    left-justified formatting, otherwise
                   right-justified formatting

    (2)  Pad with blanks for short arguments
         Expand to required number of digits for long arguments
```
Examples:  
Formatting instruction `"B"`: Converts a result (if convertible) to the **APIBYTE** format, then further converting and return of a right-justified **APITEXT** string.  
Formatting instruction `"20T"`: Converts a result (if convertible) to the **APITEXT** format, then further converting and return of a right-justified **APITEXT** string with a total of 20 digits.  
Formatting instruction `"-8.2eR"`: Converts a result (if convertible) to the **APIREAL** format, then further converting and return of a left-justified **APITEXT** string with a total of 8 digits, with the result being stored in exponential representation with 2 decimal places.  
Formatting instruction `""`: Converts a result (if convertible) to the **APITEXT** format and returns the left-justified **APITEXT** string.
