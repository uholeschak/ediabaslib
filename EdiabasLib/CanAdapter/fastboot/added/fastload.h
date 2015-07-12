/*
  fastload.h 
  
  Written by Peter Dannegger, modified by H. C. Zimmerer

   Time-stamp: <2010-01-14 21:58:08 hcz>

   You may use my modifications here and in the accompanying files of
   this project for whatever you want to do with them provided you
   don't remove this copyright notice.

*/

;*************************************************************************
#include "compat.h" // compatibility definitions
#include "protocol.h"
;-------------------------------------------------------------------------
;				Constant definitions
;-------------------------------------------------------------------------
#define  VERSION 0x0201

#define  XTAL F_CPU	// 8MHz, not critical
//#define  BootDelay XTAL / 3	// 0.33s
//#define  BOOTDELAY XTAL / 3
#define  BootDelay XTAL * 1	// 1s
#define  BOOTDELAY XTAL * 1
// [UH] added led
#define LED_GREEN	PE0
#define LED_RED		PE1
#define LED_PORT	PORTE
#define LED_DDR		PORTE - 1
;------------------------------	select UART mode -------------------------
#if SRX == STX && SRX_PORT == STX_PORT
#define  ONEWIRE 3
#else
#define  ONEWIRE 0
#endif

#define  SRX_PIN SRX_PORT - 2
#define  STX_DDR STX_PORT - 1

;------------------------------	select bootloader size -------------------

#ifndef APICALL
#ifndef FirstBootStart
#define  APICALL 0
#else
#define  APICALL 2*12
#endif
#endif

#ifndef CRC
#define  CRC 2*15
#endif

#ifndef VERIFY
#define  VERIFY 2*14
#endif

#ifndef WDTRIGGER
#define  WDTRIGGER 2*9
#endif

#ifndef SPH
#define  MinSize 2*198
#define  MINSIZE 2*198
#else
#define  MinSize 2*203
#define  MINSIZE 2*203
#endif

#define  BootSize CRC + VERIFY + ONEWIRE + WDTRIGGER + MinSize
#define  BOOTSIZE CRC + VERIFY + ONEWIRE + WDTRIGGER + MinSize

;------------------------------	UART delay loop value --------------------
#if CRC
#define  UartLoop 28	// UART loop time
#define  UARTLOOP 28
#else
#define  UartLoop 24
#define  UARTLOOP 24
#endif

;------------------------------	Bootloader fuse setting ------------------
#ifdef FIRSTBOOTSTART
# if (FlashEnd - FirstBootStart) >= 256 // 256 Words needed
#  define  BootStart FirstBootStart
#  define  BOOTSTART FirstBootStart
# else
#  define  BootStart SecondBootStart
#  define  BOOTSTART SecondBootStart
# endif
  ;----------------------------	max possible buffer size -----------------

  .equ  BufferSize,((SRAM_SIZE / 2) - PAGESIZE)
  .macro testpage
    .if		BootStart % BufferSize
      .set BufferSize, BufferSize - PAGESIZE
      .if	BootStart % BufferSize
        .set Buffersize, BufferSize - PAGESIZE
        testpage
      .endif
    .endif
  .endm
	testpage	; calculate Buffersize to fit into BootStart

  ;-----------------------------------------------------------------------
# define  UserFlash (2*BootStart)
# define  USERFLASH (2*BootStart)
#else  /* FirstBootStart not defined */
# ifndef FLASHEND
#  define FLASHEND FlashEnd
# endif
# define  BootStart (FLASHEND - 255)
# define  BOOTSTART (FLASHEND - 255)
# define  BufferSize PageSize
# define  BUFFERSIZE PageSize
# define  UserFlash (2 * BootStart - 2)
# define  USERFLASH (2 * BootStart - 2)
#endif
;-------------------------------------------------------------------------
;				Using register
;-------------------------------------------------------------------------
#define  zerol r2
#define  ZEROL r2
#define  zeroh r3
#define  ZEROH r3
#define  baudl r4	// baud divider
#define  BAUDL r4
#define  baudh r5
#define  BAUDH r5
#define  crcl r6
#define  CRCL r6
#define  crch r7
#define  CRCH r7

;-------------------------------------------------------------------------
#define  appl r16	// rjmp to application
#define  APPL r16
#define  apph r17
#define  APPH r17
#define  polynoml r18	// CRC polynom 0xA001
#define  POLYNOML r18
#define  polynomh r19
#define  POLYNOMH r19

#define  zx r21	// 3 byte Z pointer
#define  ZX r21
#define  a0 r22	// working registers
#define  A0 r22
#define  a1 r23
#define  A1 r23
#define  twl r24	// wait time
#define  TWL r24
#define  twh r25
#define  TWH r25
;-------------------------------------------------------------------------
;				Using SRAM
;-------------------------------------------------------------------------
.section .bss
.global PROGBUFF,PROGBUFFEND
PROGBUFF: .space 2*BufferSize
PROGBUFFEND:
ProgBuffEnd:
.section .text
;-------------------------------------------------------------------------
;				Macros
;-------------------------------------------------------------------------
#if ONEWIRE
  .macro	IOPortInit
	sbi	STX_PORT, SRX		; weak pullup on
	cbi	STX_DDR, SRX		; as input
  .endm
  .macro	TXD_0
	sbi	STX_DDR, SRX		; strong pullup = 0
  .endm
  .macro	TXD_1
	cbi	STX_DDR, SRX		; weak pullup = 1
  .endm
  .macro	SKIP_RXD_0
	sbis	SRX_PIN, SRX		; low = 1
  .endm
  .macro	SKIP_RXD_1
	sbic	SRX_PIN, SRX		; high = 0
  .endm
#else
  .macro	IOPortInit
	sbi	SRX_PORT, SRX
	sbi	STX_PORT, STX
	sbi	STX_DDR, STX
  .endm
  .macro	TXD_0
	cbi	STX_PORT, STX
  .endm
  .macro	TXD_1
	sbi	STX_PORT, STX
  .endm
  .macro	SKIP_RXD_0
	sbic	SRX_PIN, SRX
  .endm
  .macro	SKIP_RXD_1
	sbis	SRX_PIN, SRX
  .endm
#endif

; [UH] activate both led
  .macro	LedInit
	sbi	LED_DDR, LED_GREEN		; green led as output
	sbi	LED_DDR, LED_RED		; red led as output
	sbi	LED_PORT, LED_GREEN		; green led on
	sbi	LED_PORT, LED_RED		; red led on
  .endm

;-------------------------------------------------------------------------
