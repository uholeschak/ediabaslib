/*

   stub.i

   Put a jmp stub at the end of flash.  This is moved from APICALL.INC
   into this file which is assembled separately so the linker can take
   care of poitioning it unto the last flash word.

 */

#include <avr/io.h>
#undef FLASHEND
#include "atmel_def.h"
#include "mangled_case.h"
              .section .text
              .global stub          ; so it shows up in the symbol table
stub:
#ifdef FirstBootStart
              rjmp  api_call
#else   /* no APICALL, so just return */
              ret
#endif
              .end
