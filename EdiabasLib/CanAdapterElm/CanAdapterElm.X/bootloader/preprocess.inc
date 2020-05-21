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

#ifdef __XC
    #define upper(_x) (low((_x) >> 16))
    #define ACCESS a
    #define BANKED b
    #define _FSR0_ 0
    #define _FSR1_ 1
    #define _FSR2_ 2

    #define _LATB4_ LATB4
    #define _LATB6_ LATB6
    #define _LATB7_ LATB7
    #define _TRISB4_ TRISB4
    #define _TRISB6_ TRISB6
    #define _TRISB7_ TRISB7

    #define _TMR0IF_ TMR0IF
    #define _TMR0ON_ TMR0ON

    #define _RBPU_ RBPU
    #define _RI_ RI
    #define _CARRY_ CARRY
    #define _WR_ WR
#else
    #define _FSR0_ FSR0
    #define _FSR1_ FSR1
    #define _FSR2_ FSR2

    #define _LATB4_ LATB, LATB4
    #define _LATB6_ LATB, LATB6
    #define _LATB7_ LATB, LATB7
    #define _TRISB4_ TRISB, TRISB4
    #define _TRISB6_ TRISB, TRISB6
    #define _TRISB7_ TRISB, TRISB7

    #define _TMR0IF_ INTCON, TMR0IF
    #define _TMR0ON_ T0CON, TMR0ON

    #define _RBPU_ INTCON2, RBPU
    #define _RI_ RCON, RI
    #define _CARRY_ STATUS, C
    #define _WR_ EECON1, WR
#endif

DigitalInput macro
#ifdef __18F1320
    #if BOOTLOADER_ADDRESS == 0
    nop                         ; start up GOTO instruction errata
    #endif
    bsf     ADCON1, PCFG6, ACCESS       ; RB4/AN6/RX pin on PIC18F1x20 requires digital mode
#endif
#ifdef __18F1220
    #if BOOTLOADER_ADDRESS == 0
    nop                         ; start up GOTO instruction errata
    #endif
    bsf     ADCON1, PCFG6, ACCESS       ; RB4/AN6/RX pin on PIC18F1x20 requires digital mode
#endif

#ifdef RXANSEL
    banksel RXANSEL
    bcf     RXANSEL, RXAN, BANKED
#else
    #ifdef ANSC7
        banksel ANSELC		; ANSELC is in non-access bank 0x0F on PIC18F46K22 family
        bcf     ANSELC, ANSC7, BANKED	; Digital input enable on RC7/RX pin for PIC18F46K22 family
    #endif
#endif
    endm

pmwtpi macro                        ; tblwt*+ macro for PIC18Fxx20 bug
#ifdef TBLWT_BUG
    tblwt   *
    tblrd   *+
#else
    tblwt   *+
#endif
    endm

#ifndef BAUDRG
    #ifndef USE_AUTOBAUD
        #define USE_AUTOBAUD
    #endif
#endif

#ifndef RXPORT
    #ifdef PORTC
        #define RXPORT PORTC
    #else
				; PIC18F1220, PIC18F1320
    #define RXPORT PORTB
    #endif
#endif

#ifndef RXPIN
    #ifdef PORTC
				; most PIC18's have RX on RC7
    #define RXPIN 0x7
    #else
				; PIC18F1220, PIC18F1320 have RX on RB4
        #define RXPIN 0x4
    #endif
#endif

#ifdef BRG16
    #ifndef SPBRGH
				; PIC18F87J10 doesn't define SPBRGH by default.
    #define SPBRGH SPBRGH1
    #endif

    #ifndef BAUDCON
        #ifdef BAUDCON1
					; PIC18F85J90 / PIC18F84J90
            #define BAUDCON BAUDCON1
        #else
            #ifdef BAUDCTL
					; PIC18F1220, PIC18F1320
                #define BAUDCON BAUDCTL
            #endif
        #endif
    #endif
#endif

#ifndef RCREG
    #ifdef RCREG1
					; PIC18F85J90/PIC18F84J90
        #define RCREG RCREG1
    #endif
#endif

#ifndef TXIF
    #ifdef TX1IF
					; PIC18F97J60 doesn't define TXIF by default
        #define TXIF TX1IF
    #endif
#endif

#ifndef RCIF
    #ifdef RC1IF
					; Not a problem on PIC18F97J60, but just in case future parts might need it
        #define RCIF RC1IF
    #endif
#endif

#ifndef RXDTP
    #ifdef DTRXP
					; PIC18F14K50 doesn't define RXDTP, instead they call it DTRXP
        #define RXDTP DTRXP
    #endif
#endif

#ifndef TXCKP
    #ifdef CKTXP
					; PIC18F14K50
        #define TXCKP CKTXP
    #endif
#endif


#if BOOTLOADERSIZE < ERASE_FLASH_BLOCKSIZE
    ; This device has a large Erase FLASH Block Size, so we need to reserve a full Erase Block
    ; page for the bootloader. Reserving an entire erase block prevents the PC application
    ; from trying to accidently erase a portion of the bootloader.
    #define BOOTBLOCKSIZE ERASE_FLASH_BLOCKSIZE
    #ifndef BOOTLOADER_ADDRESS
        #ifdef CONFIG_AS_FLASH
            #define BOOTLOADER_ADDRESS  (END_FLASH - BOOTBLOCKSIZE - ERASE_FLASH_BLOCKSIZE)
        #else
            #define BOOTLOADER_ADDRESS  (END_FLASH - BOOTBLOCKSIZE)
        #endif
    #endif
#else
    #if (BOOTLOADERSIZE % ERASE_FLASH_BLOCKSIZE) == 0
        #define BOOTBLOCKSIZE BOOTLOADERSIZE
    #else
        #define BOOTBLOCKSIZE (BOOTLOADERSIZE / ERASE_FLASH_BLOCKSIZE + 1) * ERASE_FLASH_BLOCKSIZE
    #endif
    #ifndef BOOTLOADER_ADDRESS
        #define BOOTLOADER_ADDRESS  (END_FLASH - BOOTBLOCKSIZE)
    #endif
#endif
