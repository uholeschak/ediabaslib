/*

  compat.h

  Written by Peter Dannegger, modified by H. C. Zimmerer

   Time-stamp: <2009-07-18 13:14:35 hcz>

   You may use my modifications here and in the accompanying files of
   this project for whatever you want to do with them provided you
   don't remove this copyright notice.


*/
;------------------------------------------------------------------------
;			redefinitions for compatibility
;------------------------------------------------------------------------
#ifndef WDTCSR
#define  WDTCSR WDTCR
#define  wdtcsr WDTCR
#endif
;---------------------------
#ifndef WDCE
#define  WDCE WDTOE
#define  wdce WDTOE
#endif
;---------------------------
#ifndef SPMCSR
#define  SPMCSR SPMCR
#define  spmcsr SPMCR
#endif
;---------------------------
#ifndef RWWSRE
#ifdef ASRE
#define  RWWSRE ASRE
#define  rwwsre ASRE
#endif
#endif
;---------------------------
#ifndef SPMEN
#define  SPMEN SELFPRGEN
#define  spmen SELFPRGEN
#endif
;----------------------	macros for extended IO access -------------------
.macro	xout arg0, arg1
.if	\arg0 > 0x3F
	sts	\arg0, \arg1
.else
	out	\arg0, \arg1
.endif
  .endm
;---------------------------
.macro	xin arg0, arg1
.if	\arg1 > 0x3F
	lds	\arg0, \arg1
.else
	in	\arg0, \arg1
.endif
  .endm
;---------------------------
.macro  xlpm  arg0, arg1
.if FLASHEND > 0x7FFF
	elpm	\arg0, \arg1
.else
	lpm	\arg0, \arg1
.endif
  .endm
;------------------------------------------------------------------------
