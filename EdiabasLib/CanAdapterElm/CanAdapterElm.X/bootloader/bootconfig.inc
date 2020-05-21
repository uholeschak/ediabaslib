; Copyright (c) 2002-2011,  Microchip Technology Inc.
;
; Microchip licenses this software to you solely for use with Microchip
; products.  The software is owned by Microchip and its licensors, and
; is protected under applicable copyright laws.  All rights reserved.
;
; SOFTWARE IS PROVIDED "AS IS."  MICROCHIP EXPRESSLY DISCLAIMS ANY
; WARRANTY OF ANY KIND, WHETHER EXPRESS OR IMPLIED, INCLUDING BUT
; NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS
; FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.  IN NO EVENT SHALL
; MICROCHIP BE LIABLE FOR ANY INCIDENTAL, SPECIAL, INDIRECT OR
; CONSEQUENTIAL DAMAGES, LOST PROFITS OR LOST DATA, HARM TO YOUR
; EQUIPMENT, COST OF PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY
; OR SERVICES, ANY CLAIMS BY THIRD PARTIES (INCLUDING BUT NOT LIMITED
; TO ANY DEFENSE THEREOF), ANY CLAIMS FOR INDEMNITY OR CONTRIBUTION,
; OR OTHER SIMILAR COSTS.
;
; To the fullest extent allowed by law, Microchip and its licensors
; liability shall not exceed the amount of fees, if any, that you
; have paid directly to Microchip to use this software.
;
; MICROCHIP PROVIDES THIS SOFTWARE CONDITIONALLY UPON YOUR ACCEPTANCE
; OF THESE TERMS.

;#define TBLWT_BUG               ; Enable this to work around timing bug found in some PIC18Fxx20's
;#define INVERT_UART             ; If you don't have an RS232 transceiver, you might want this option
;#define USE_MAX_INTOSC          ; Sets OSCCON<IRCF2:IRCF0> for maximum INTOSC frequency (8MHz)
;#define USE_PLL                 ; Sets OSCTUNE.PLLEN bit at start up for frequency multiplication.
;#define PICDEM_LCD2             ; RB0 = 1 required to enable MAX3221 TX output on PICDEM LCD 2 demo board
;#define USE_SOFTBOOTWP          ; enable software boot block write protection
;#define USE_SOFTCONFIGWP        ; enable software config words write protection

; Autobaud will be used by default. To save code space or to force a specific baud rate to be used, 
; you can optionally define a BAUDRG value instead. Most PIC18's support BRG16 mode and use the 
; following equation:
;       BAUDRG = Fosc / (4 * Baud Rate) - 1
;
; Old PIC18's without BRG16 mode need this equation instead:
;       BAUDRG = Fosc / (16 * Baud Rate) - 1
;
; Examples:
;#define BAUDRG .51              ; 19.2Kbps from 4MHz (BRG16 = 1, BRGH = 1)
;#define BAUDRG .103             ; 115.2Kbps from 48MHz (BRG16 = 1, BRGH = 1)
;#define BAUDRG .85              ; 115.2Kbps from 40MHz (BRG16 = 1, BRGH = 1)
;#define BAUDRG .68              ; 115.2Kbps from 32MHz (BRG16 = 1, BRGH = 1)
;#define BAUDRG .16              ; 115.2Kbps from 8MHz (BRG16 = 1, BRGH = 1)
;#define BAUDRG .11              ; 1Mbps from 48MHz (BRG16 = 1, BRGH = 1)
;#define BAUDRG .9               ; 1Mbps from 40MHz (BRG16 = 1, BRGH = 1)
;#define BAUDRG .4               ; 2Mbps from 40MHz (BRG16 = 1, BRGH = 1)
;#define BAUDRG .3               ; 3Mbps from 48MHz (BRG16 = 1, BRGH = 1)
;#define BAUDRG .12              ; 19.2Kbps from 4MHz, 115.2Kbps from 24MHz (BRG16 = 0, BRGH = 1)
;#define BAUDRG .10              ; 115.2Kbps from 19.6608MHz (BRG16 = 0, BRGH = 1)

; Bootloader must start at the beginning of a FLASH Erase Block. If unspecified,
; bootloader will automatically be located at the end of program memory address space.
				; bootloader at beginning, application start/ISR vectors require remapping
#define BOOTLOADER_ADDRESS   0x0000
;#define BOOTLOADER_ADDRESS   END_FLASH - (ERASE_FLASH_BLOCKSIZE * 20)  ; useful for running under debugger (debug executive wants to reside at the end of memory space too)
;#define BOOTLOADER_ADDRESS  (END_FLASH - ERASE_FLASH_BLOCKSIZE) ; use on J parts to locate inside flash config erase block

#ifdef BOOTLOADER_ADDRESS
  #if BOOTLOADER_ADDRESS == 0x0000
    ; For Bootloader located at program memory address 0, the application firmware must 
    ; provide remapped reset and interrupt vectors outside of the Boot Block. The following 
    ; #defines tell the bootloader firmware where application entry points are to be expected:
					; application start up code should be located here.
    #define AppVector           0x800
					; application high priority interrupt should be located here
    #define AppHighIntVector    0x808
					; application low priority interrupt should be located here
    #define AppLowIntVector     0x818
  #endif
#endif

; Define UART pins and registers. 
; Modify the following lines if you want to use a different UART module.
;
; Note: If your UART's RX pin happens to be multiplexed with analog ANx input
;	functionality, you may need to edit the "preprocess.inc" DigitalInput
;	macro. Code there needs to enable the digital input buffer (refer to
;	ADC chapter of your device's datasheet).
#define UARTNUM 1
#if UARTNUM == 1
    #define UxSPBRG         SPBRG
    #define UxSPBRGH        SPBRGH
    #define UxRCSTA         RCSTA
    #define UxTXSTA         TXSTA
    #define UxRCREG         RCREG
    #define UxTXREG         TXREG
    #ifdef __XC
	#define _UxRCIF_        RCIF
	#define _UxTXIF_        TXIF
    #else
	#define _UxRCIF_        PIR1, RCIF
	#define _UxTXIF_        PIR1, TXIF
    #endif
    #define UxBAUDCON       BAUDCON

;    #define RXPORT         PORTC   ; RX on RC7 is used by default for most PIC18's.
;    #define RXPIN          .7

;    #define RXPORT          PORTB   ; PIC18F14K50: RX on RB5/AN11
;    #define RXPIN           .5

;    #define RXANSEL         ANSELH  ; RX/AN11 multiplexed -- must enable digital input buffer
;    #define RXAN            .3      ; ANSELH<3> controls AN11 digital input buffer
#endif

#if UARTNUM == 2
    #define UxSPBRG         SPBRG2
    #define UxSPBRGH        SPBRGH2
    #define UxRCSTA         RCSTA2
    #define UxTXSTA         TXSTA2
    #define UxRCREG         RCREG2
    #define UxTXREG         TXREG2
    #ifdef __XC
	#define _UxRCIF_        RC2IF
	#define _UxTXIF_        TX2IF
    #else
	#define _UxRCIF_        PIR3, RC2IF
	#define _UxTXIF_        PIR3, TX2IF
    #endif
    #define UxBAUDCON       BAUDCON2

;    #define RXPORT          PORTG   ; RG2 is default RX2 pin for some high pin count PIC18's.
;    #define RXPIN           .2

;    #define RXPORT          PORTD   ; RX2 pin PPS'ed to RD4/RP21 on PIC18F46J11 for example.
;    #define RXPIN           .4

;    #define RXANSEL         ANSELH  ; On PICs where RX is multiplexed with ANx analog inputs,
;    #define RXAN            .3      ; the digital input buffer needs to be enabled via ANSELx SFRs

    ; devices that use PPS to remap UART2 pins will need these lines defined:
;    #define PPS_UTX         .5      ; PPS code for TX2/CK2 output function
;    #define PPS_UTX_PIN     RPOR23  ; UART TX assigned to RP23 pin
;    #define PPS_URX_PIN     .21     ; UART RX assigned to RP21 pin
;    #define PPS_URX         RPINR16 ; PPS register for RX2/CK2 input function
#endif

; If you get linker errors complaining "can not fit the absolute section," you might want to
; increase BOOTLOADERSIZE below or set the BOOTLOADER_ADDRESS above to a smaller address number.

; Because we need to know the total size of the bootloader before the assembler has finished
; compiling the source code, we have to estimate the final bootloader size and provide it
; here as BOOTLOADERSIZE. This number is in bytes (twice the instruction word count). 
;
; If you see the bootloader is reserving more FLASH memory than it really needs (you'll
; see a bunch of FFFF/NOP instructions at the end of the bootloader memory region),
; you can try reducing BOOTLOADERSIZE.
					; 708
#define BOOTLOADERSIZE  0x2C4

					; Bootloader Firmware Version
#define MAJOR_VERSION   0x1
#define MINOR_VERSION   0x5
