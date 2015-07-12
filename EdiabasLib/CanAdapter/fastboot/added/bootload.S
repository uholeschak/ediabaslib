/*

   bootload.S

   Time-stamp: <2010-01-14 18:59:04 hcz>

   Main file of Peter Danneger's AVR bootloader for use
   with the avr-gcc tools.

   (c) 2009 H. C. Zimmerer.  You may use this file and the
   accompanying files of this project for whatever you want to do with
   them provided you don't remove this copyright notice. This
   permission covers ONLY the parts which are written/changed by me,
   leaving all original conditions untouched.
   
*/
#define __SFR_OFFSET 0

              .nolist
#include <avr/io.h>
//#include "m168def.h"
              .list

/* 
   atmel_def.h contains some definitions taken from the Atmel's def
   file (which one is defined in the Makefile).  Always keep in mind
   that flash addresses taken from there are word addresses.
*/
#undef FLASHEND
#include "atmel_def.h"
              
#define byte3(x) ((x)>>16)
#define byte2(x) ((x & 0xff00)>>8)
#define byte1(x) (x & 0xff)


/* 
   Atmel's AVR assembler is case insensitive, gcc is not.
   mangled_case.h covers the cases where the representations in the
   program mix case.
*/
#include "mangled_case.h"

/* 
   This includes the program:
*/
#include "bootload.asm"


              .global api_call
