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

DigitalInput macro
#ifdef __18F1320
    #if BOOTLOADER_ADDRESS == 0
    nop                         ; start up GOTO instruction errata
    #endif
    bsf     ADCON1, PCFG6       ; RB4/AN6/RX pin on PIC18F1x20 requires digital mode
#endif
#ifdef __18F1220
    #if BOOTLOADER_ADDRESS == 0
    nop                         ; start up GOTO instruction errata
    #endif
    bsf     ADCON1, PCFG6       ; RB4/AN6/RX pin on PIC18F1x20 requires digital mode
#endif

#ifdef RXANSEL
    banksel RXANSEL
    bcf     RXANSEL, RXAN
#else
    #ifdef ANSC7
        banksel ANSELC		; ANSELC is in non-access bank 0x0F on PIC18F46K22 family
        bcf     ANSELC, ANSC7	; Digital input enable on RC7/RX pin for PIC18F46K22 family
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
        #define RXPORT PORTB    ; PIC18F1220, PIC18F1320
    #endif
#endif

#ifndef RXPIN
    #ifdef PORTC
        #define RXPIN 7            ; most PIC18's have RX on RC7
    #else
        #define RXPIN 4            ; PIC18F1220, PIC18F1320 have RX on RB4
    #endif
#endif

#ifdef BRG16
    #ifndef SPBRGH
        #define SPBRGH SPBRGH1      ; PIC18F87J10 doesn't define SPBRGH by default.
    #endif

    #ifndef BAUDCON
        #ifdef BAUDCON1
            #define BAUDCON BAUDCON1    ; PIC18F85J90 / PIC18F84J90
        #else
            #ifdef BAUDCTL
                #define BAUDCON BAUDCTL ; PIC18F1220, PIC18F1320
            #endif
        #endif
    #endif
#endif

#ifndef RCREG
    #ifdef RCREG1
        #define RCREG RCREG1        ; PIC18F85J90/PIC18F84J90
    #endif
#endif

#ifndef TXIF
    #ifdef TX1IF
        #define TXIF TX1IF          ; PIC18F97J60 doesn't define TXIF by default
    #endif
#endif

#ifndef RCIF
    #ifdef RC1IF
        #define RCIF RC1IF          ; Not a problem on PIC18F97J60, but just in case future parts might need it
    #endif
#endif

#ifndef RXDTP
    #ifdef DTRXP
        #define RXDTP DTRXP         ; PIC18F14K50 doesn't define RXDTP, instead they call it DTRXP
    #endif
#endif

#ifndef TXCKP
    #ifdef CKTXP
        #define TXCKP CKTXP         ; PIC18F14K50
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
