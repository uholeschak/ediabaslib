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
;
; Author        Date        Comment
; ************************************************************************
; E. Schlunder  07/20/2010  Software Boot Block Write Protect code 
;                           improved. 96KB memory size devices should
;                           work now.
; E. Schlunder  02/26/2010  Changed order of start up code so that PLLEN
;                           is enabled before we wait for RXD IDLE state. 
;                           This improves connection time/reliablity on 
;                           J devices.
; E. Schlunder  08/28/2009  Software Boot Block Write Protect option.
; E. Schlunder  07/09/2009  Brought back support for bootloader at 
;                           address 0 for hardware boot block write 
;                           protection on certain devices.
; E. Schlunder  05/07/2009  Replaced the simple checksum with a
;                           16-bit CCIT CRC checksum. 
;                           Added ReadFlashCrc command for quick verify.
; E. Schlunder  05/02/2009  Improved autobaud code to handle 1Mbps
;                           and BRG16/BRGH.
; E. Schlunder  05/01/2009  Added support for DEVICES.INC generated
;                           from Device Database tool. 
; E. Schlunder  04/29/2009  Added support for locating the bootloader
;                           at the end of program memory instead of
;                           the beginning. This will eventually let us
;                           use normal application firmware code without
;                           linker script modifications.
; E. Schlunder  04/26/2009  Optimized Config Write routine to avoid 
;                           re-writing values matching existing config
;                           data.
; E. Schlunder  04/24/2009  Optimized EEPROM Write routine a little bit.
;                           Optimized FLASH Read routine to stream data
;                           directly from FLASH instead of using RAM
;                           buffer.
;                           Added option for faster STX acknowledgements.
; E. Schlunder  04/17/2009  Added code to enter bootloader mode if
;                           a serial break condition is detected on
;                           RXD as we come out of reset. This will
;                           make it possible to re-enter the bootloader
;                           even if the application firmware is missing
;                           code to re-enter bootloader mode.
;                           This also simplies the bootloader, as
;                           we do not need a boot flag any more.
; E. Schlunder  04/15/2009  Removed EOF command 8, new PC software
;                           does 64 byte block aligned writes at all
;                           times on J device, so there is no need for
;                           this command going forward.
; E. Schlunder  04/14/2009  Added a BootloadMode vector back at the 
;                           beginning of program memory so that user
;                           applications can jump back into the boot 
;                           loader without having to erase the boot flag.
; E. Schlunder  04/08/2009  Now initializes FSR2 to 0 so that the code 
;                           can operate under Extended Instruction Set
;                           mode if necessary.
; E. Schlunder  04/01/2009  Fixed bug in J_FLASH erase address increment.
;                           Added support for enabling PLL.
;                           Added support for inverted UART signaling.
;                           Added support for fixed (non-autobaud) 
;                           operation, helps with debugging code under ICD.
; E. Schlunder  03/25/2009  No longer attempts to use EEADRH on PIC18F4321.
;
; UART Bootloader for PIC18F by Ross Fosler 
; 09/01/2006  Modified to support PIC18xxJxx & 160k PIC18Fxxx Flash Devices
; 03/01/2002 ... First full implementation
; 03/07/2002 Changed entry method to use last byte of EEDATA.
;            Also removed all possible infinite loops w/ clrwdt.
; 03/07/2002 Added code for direct boot entry. I.E. boot vector.
; 03/09/2002 Changed the general packet format, removed the LEN field.
; 03/09/2002 Modified the erase command to do multiple row erase.
; 03/12/2002 Fixed write problem to CONFIG area. Write was offset by a byte.
; 03/15/2002 Added work around for 18F8720 tblwt*+ problem.
; 03/20/2002 Modified receive & parse engine to vector to autobaud on a checksum 
;            error since a chechsum error could likely be a communications problem.
; 03/22/2002 Removed clrwdt from the startup. This instruction affects the TO and 
;            PD flags. Removing this instruction should have no affect on code 
;       operation since the wdt is cleared on a reset and boot entry is always
;       on a reset.
; 03/22/2002    Modified the protocol to incorporate the autobaud as part of the 
;       first received <STX>. Doing this improves robustness by allowing
;       re-sync under any condition. Previously it was possible to enter a 
;       state where only a hard reset would allow re-syncing.
; 03/27/2002    Removed the boot vector and related code. This could lead to customer
;       issues. There is a very minute probability that errent code execution
;       could jump into the boot area and cause artificial boot entry.
; *****************************************************************************
#ifdef __PICAS
    #if __PICAS_VERSION < 2300
	#define ACC ,a
    #else
	#define ACC
    #endif
    #define __XC
#endif
#ifdef __XC
    #ifndef ACC
	#define ACC ,a
    #endif
#include <xc.inc>
#else
#include <p18cxxx.inc>
#endif
#include "devices.inc"
#include "bootconfig.inc"
#include "preprocess.inc"
; *****************************************************************************

; *****************************************************************************
#define STX             0x0F
#define ETX             0x04
#define DLE             0x05
#define NTX             0xFF
; *****************************************************************************

; *****************************************************************************
; RAM Address Map
CRCL                equ 0x00
CRCH                equ 0x01
RXDATA              equ 0x02
TXDATA              equ 0x03

; Framed Packet Format
; <STX>[<COMMAND><ADDRL><ADDRH><ADDRU><0x00><DATALEN><...DATA...>]<CRCL><CRCH><ETX>
COMMAND             equ 0x05        ; receive buffer
ADDRESS_L           equ 0x06
ADDRESS_H           equ 0x07
ADDRESS_U           equ 0x08
ADDRESS_X           equ 0x09
DATA_COUNTL         equ 0x0A
PACKET_DATA         equ 0x0B
DATA_COUNTH         equ 0x0B        ; only for certain commands
; *****************************************************************************

; *****************************************************************************
#ifndef __XC
    errorlevel -311                 ; don't warn on HIGH() operator values >16-bits
#endif

#ifdef USE_SOFTBOOTWP
  #ifndef SOFTWP
    #define SOFTWP
  #endif
#endif

#ifdef USE_SOFTCONFIGWP
  #ifdef CONFIG_AS_FLASH
    #ifndef SOFTWP
      #define SOFTWP
    #endif
  #endif
#endif

#ifndef AppVector
    ; The application startup GOTO instruction will be written just before the Boot Block,
    ; courtesy of the host PC bootloader application.
    #define AppVector (BootloaderStart-0x4)
#endif
; *****************************************************************************

 
; *****************************************************************************
#ifdef __XC
#define END_LABEL BootloaderStart
PSECT ramtop,class=RAM,delta=1
PSECT smallconst,class=CONST,delta=1
PSECT mediumconst,class=CONST,delta=1
PSECT const,class=CONST,delta=1
PSECT eeprom_data,class=EEDATA,delta=1
PSECT rdata,class=COMRAM,delta=1
PSECT nvrram,class=COMRAM,delta=1
PSECT nvbit,class=COMRAM,delta=1
PSECT rbss,class=COMRAM,delta=1
PSECT rbit,class=COMRAM,delta=1
PSECT farbss,class=FARRAM,delta=1
PSECT fardata,class=FARRAM,delta=1
PSECT nvFARRAM,class=FARRAM,delta=1
PSECT intsave_regs,class=BIGRAM,delta=1
PSECT bigbss,class=BIGRAM,delta=1
PSECT bigdata,class=BIGRAM,delta=1
PSECT bss,class=RAM,delta=1
PSECT idata,class=CODE,delta=1
PSECT irdata,class=CODE,delta=1
PSECT ibigdata,class=CODE,delta=1
PSECT ifardata,class=CODE,delta=1
PSECT param0,class=BANK0,delta=1
PSECT init,class=CODE,delta=1
PSECT powerup,class=CODE,delta=1
PSECT intcodelo,class=CODE,delta=1
PSECT intcode,class=CODE,delta=1
PSECT reset_vec,class=CODE,delta=1
PSECT code_abs,abs,class=CODE,delta=1
#else
#define END_LABEL
#endif
#if BOOTLOADER_ADDRESS != 0
    ORG     0
    ; The following GOTO is not strictly necessary, but may startup faster
    ; for large microcontrollers running at extremely slow clock speeds.
    ;goto    BootloaderBreakCheck  

    ORG     BOOTLOADER_ADDRESS
BootloaderStart:
    bra     BootloadMode

; *****************************************************************************
; Determine if the application is supposed to be started or if we should
; go into bootloader mode.
;
; If RX pin is in BREAK state when we come out of MCLR reset, immediately 
; enter bootloader mode, even if there exists some application firmware in 
; program memory.
BootloaderBreakCheck:
    DigitalInput                ; set RX pin as digital input on certain parts
#ifdef INVERT_UART
    btfss   RXPORT, RXPIN, ACCESS
GotoAppVector:
    goto    AppVector           ; no BREAK state, attempt to start application
#else
    btfsc   RXPORT, RXPIN, ACCESS
GotoAppVector:
    goto    AppVector           ; no BREAK state, attempt to start application
#endif

BootloadMode:
    DigitalInput                ; set RX pin as digital input on certain parts
	; BOOTLOADER_ADDRESS == 0 ****************************************************************
#else
    ORG     0
BootloaderStart:
    DigitalInput                ; set RX pin as digital input on certain parts
    movlw   low(AppVector)      ; load address of application reset vector
    bra     BootloaderBreakCheck

	ORG	    0x0008
HighPriorityInterruptVector:
	goto    AppHighIntVector    ; Re-map Interrupt vector

	ORG	    0x0018
LowPriorityInterruptVector:
	goto    AppLowIntVector     ; Re-map Interrupt vector

BootloaderBreakCheck:
    ; [UH] set digital ports
    banksel ANCON0
    movlw   0x01 ;b'00000001'
    movwf   ANCON0, BANKED
    banksel ANCON1
    clrf    ANCON1, BANKED
    ; enable port B pull up
    banksel WPUB
#if (ADAPTER_TYPE == 0x06) || (ADAPTER_TYPE == 0x07)
    ; use PB5 for bootloader detection
    movlw   0x20 ;b'00100000'
#else
    ; use PB4 for bootloader detection
    movlw   0x10 ;b'00010000'
#endif
    movwf   WPUB, BANKED
    movlb   0x0F
    bcf     _RBPU_, ACCESS
    ; check for software reset
    btfss   _RI_, ACCESS
    bra     BootloadMode
CheckApplication:
    ; check for adapter type
    movlw   low(END_FLASH - 4)
    movwf   TBLPTRL, ACCESS
    movlw   high(END_FLASH - 4)
    movwf   TBLPTRH, ACCESS
    movlw   upper(END_FLASH - 4)
    movwf   TBLPTRU, ACCESS
    tblrd   *+
    movlw   low(ADAPTER_TYPE)
    xorwf   TABLAT, w, ACCESS
    bnz     BootloadMode
    tblrd   *+
    movlw   high(ADAPTER_TYPE)
    xorwf   TABLAT, w, ACCESS
    bnz     BootloadMode

#if 0
    ; wait for stable input signal
    movlw   0x04 ;b'00000100'         ; 1:16 prescaler (0.52s)
    movwf   T0CON
    clrf    TMR0H               ; reset timer count value
    clrf    TMR0L
    bcf     _TMR0IF_
    bsf     _TMR0ON_		; start timer
StableWait:
    clrwdt
    btfss   _TMR0IF_		; wait for TMR0 overflow
    bra     StableWait
#ifdef BRG16
    movlw   0x02 ;b'00000010'         ; 1:8 prescaler - no division required later (but no rounding possible)
#else
    movlw   0x03 ;b'00000011'         ; 1:16 prescaler - thus we only have to divide by 2 later on.
#endif
    movwf   T0CON

#ifdef INVERT_UART
    btfsc   RXPORT, RXPIN, ACCESS
    bra     BootloadMode
#else
    btfss   RXPORT, RXPIN, ACCESS
    bra     BootloadMode
#endif
#endif
    ; [UH] test if checkum is correct
    movlw   low(AppVector)
    movwf   TBLPTRL, ACCESS
    movlw   high(AppVector)
    movwf   TBLPTRH, ACCESS
    movlw   upper(AppVector)
    movwf   TBLPTRU, ACCESS

    movlw   low(END_FLASH - AppVector - 2)
    movwf   DATA_COUNTL, ACCESS
    movlw   high(END_FLASH - AppVector - 2)
    movwf   DATA_COUNTH, ACCESS

    clrf    CRCL, ACCESS
    clrf    CRCH, ACCESS
CalcCheckum:
    clrwdt
    tblrd   *+                  ; read from FLASH memory into TABLAT
    movf    TABLAT, w, ACCESS
    addwf   CRCL, f, ACCESS
    movlw   0
    addwfc  CRCH, f, ACCESS

    decf    DATA_COUNTL, f, ACCESS      ; decrement counter
    movlw   0
    subwfb  DATA_COUNTH, f, ACCESS

    movf    DATA_COUNTL, w, ACCESS      ; DATA_COUNTH:DATA_COUNTH == 0?
    iorwf   DATA_COUNTH, w, ACCESS
    bnz     CalcCheckum         ; no, loop
    ; compare checksum
    tblrd   *+
    movf    TABLAT, w, ACCESS
    xorwf   CRCL, w, ACCESS
    bnz     BootloadMode
    tblrd   *+
    movf    TABLAT, w, ACCESS
    xorwf   CRCH, w, ACCESS
    bnz     BootloadMode

#if (ADAPTER_TYPE == 0x06) || (ADAPTER_TYPE == 0x07)
    ; test if PB5 is low
    btfss   PORTB, 5, ACCESS
#else
    ; test if PB4 is low
    btfss   PORTB, 4, ACCESS
#endif
    bra     BootloadMode

CheckAppVector:
    ; Read instruction at the application reset vector location. 
    ; If we read 0xFFFF, assume that the application firmware has
    ; not been programmed yet, so don't try going into application mode.
    movlw   low(AppVector)
    movwf   TBLPTRL, ACCESS
    movlw   high(AppVector)
    movwf   TBLPTRH, ACCESS
    ;bra     CheckAppVector2

;CheckAppVector2:
    movlw   upper(AppVector)
    movwf   TBLPTRU, ACCESS
    tblrd   *+                  ; read instruction from program memory
    incfsz  TABLAT, W, ACCESS   ; if the lower byte != 0xFF, 
GotoAppVector:
    goto    AppVector           ; run application.

    tblrd   *+                  ; read instruction from program memory
    incfsz  TABLAT, W, ACCESS   ; if the lower byte == 0xFF but upper byte != 0xFF,
    bra     GotoAppVector       ; run application.
    ; otherwise, assume application firmware is not present because we read a NOP (0xFFFF).
    ; fall through to bootloader mode...
BootloadMode:
	; end BOOTLOADER_ADDRESS == 0 ******************************************
#endif
    lfsr    _FSR2_, 0           ; for compatibility with Extended Instructions mode.
    bcf     _RI_, ACCESS	; [UH] clear hardware reset bit

#ifdef USE_MAX_INTOSC
    movlw   0x70 ;b'01110000'         ; set INTOSC to maximum speed (usually 8MHz)
    iorwf   OSCCON, f
#endif

#ifdef USE_PLL
    #ifdef PLLEN
        #ifdef OSCTUNE
            bsf     OSCTUNE, PLLEN      ; enable PLL for faster internal clock
        #else
            ; 18F8680, 18F8585, 18F6680, and 18F6585 doesn't have OSCTUNE register.
            ; Instead, PLLEN bit is in OSCCON.
            bsf     OSCCON, PLLEN      ; enable PLL for faster internal clock
        #endif
    #else
        #ifdef SPLLEN
            bsf     OSCTUNE, SPLLEN     ; PIC18F14K50 has SPLLEN at bit 6
        #endif
    #endif
#endif

#ifdef INVERT_UART
    btfsc   RXPORT, RXPIN, ACCESS       ; wait for RX pin to go IDLE
    bra     $-2
#else
    btfss   RXPORT, RXPIN, ACCESS       ; wait for RX pin to go IDLE
    bra     $-2
#endif

#ifdef PPS_UTX_PIN
    banksel PPSCON
    ; unlock PPS registers
    movlw   0x55
    movwf   EECON2, ACCESS
    movlw   0xAA
    movwf   EECON2, ACCESS
    bcf     PPSCON, IOLOCK, BANKED

    ; assign UART RX/TX to PPS remappable pins
    movlw   PPS_UTX
    movwf   PPS_UTX_PIN, BANKED

    movlw   PPS_URX_PIN
    movwf   PPS_URX, BANKED

    ; lock PPS registers from inadvertent changes
    movlw   0x55
    movwf   EECON2, ACCESS
    movlw   0xAA
    movwf   EECON2, ACCESS
    bsf     PPSCON, IOLOCK, BANKED
    movlb   0x0F
#endif

    movlw   0x90 ;b'10010000'         ; Setup UART
    movwf   UxRCSTA, ACCESS
    movlw   0x26 ;b'00100110'         ; BRGH = 1, TXEN = 1
    movwf   UxTXSTA, ACCESS

#ifdef INVERT_UART
    bsf     UxBAUDCON, RXDTP, ACCESS
    bsf     UxBAUDCON, TXCKP, ACCESS
#endif

#ifdef BRG16
    bsf     _UxBRG16_, ACCESS
    movlw   0x02 ;b'00000010'         ; 1:8 prescaler - no division required later (but no rounding possible)
#else
    movlw   0x03 ;b'00000011'         ; 1:16 prescaler - thus we only have to divide by 2 later on.
#endif
    movwf   T0CON, ACCESS

#ifdef PICDEM_LCD2
    bsf     LATB, LATB0         ; PICDEM LCD 2 demoboard requires RB0 high to enable MAX3221 TX output to PC.
    bcf     TRISB, TRISB0
#endif
    ; [UH] switch on both LED
#if (ADAPTER_TYPE == 0x06) || (ADAPTER_TYPE == 0x07)
    bcf     _LATB4_, ACCESS
    bcf     _LATB6_, ACCESS
    bcf     _TRISB4_, ACCESS
    bcf     _TRISB6_, ACCESS
#else
    bcf     _LATB6_, ACCESS
    bcf     _LATB7_, ACCESS
    bcf     _TRISB6_, ACCESS
    bcf     _TRISB7_, ACCESS
#endif

; *****************************************************************************


; *****************************************************************************
#ifdef USE_AUTOBAUD
DoAutoBaud:
; ___    __________            ________
;    \__/          \__________/
;       |                     |
;       |-------- p ----------|
;
;   p = The number of instructions between the first and last
;           rising edge of the RS232 control sequence 0x0F. Other 
;       possible control sequences are 0x01, 0x03, 0x07, 0x1F, 
;       0x3F, 0x7F.
;
;   SPBRG = (p / 32) - 1    BRGH = 1, BRG16 = 0
;   SPBRG = (p / 8) - 1     BRGH = 1, BRG16 = 1

    bcf     _UxCREN_, ACCESS		; Stop receiving
    movf    UxRCREG, W, ACCESS          ; Empty the buffer
    movf    UxRCREG, W, ACCESS

RetryAutoBaud:
    clrf    TMR0H, ACCESS               ; reset timer count value
    clrf    TMR0L, ACCESS
    bcf     _TMR0IF_, ACCESS
    rcall   WaitForRise         ; wait for a start bit to pass by
    bsf     _TMR0ON_, ACCESS  	; start timer counting for entire D7..D0 data bit period.
    rcall   WaitForRise         ; wait for stop bit
    bcf     _TMR0ON_, ACCESS	; stop the timer from counting further. 

    btfsc   _TMR0IF_, ACCESS    ; if TMR0 overflowed, we did not get a good baud capture
    bra     RetryAutoBaud       ; try again

    #ifdef BRG16
    ; save new baud rate generator value
    movff   TMR0L, UxSPBRG      ; warning: must read TMR0L before TMR0H holds real data
    movff   TMR0H, UxSPBRGH
    #else 
    movff   TMR0L, UxSPBRG      ; warning: must read TMR0L before TMR0H holds real data
    ; TMR0H:TMR0L holds (p / 16).
    rrcf    TMR0H, w, ACCESS    ; divide by 2
    rrcf    UxSPBRG, F, ACCESS
    btfss   _CARRY_, ACCESS	; rounding
    decf    UxSPBRG, F, ACCESS
    #endif

    bsf     _UxCREN_, ACCESS	; start receiving

WaitForHostCommand:
    rcall   ReadHostByte        ; get start of transmission <STX>
    xorlw   STX
    bnz     DoAutoBaud          ; got something unexpected, perform autobaud
	; not using autobaud
#else
    movlw   low(BAUDRG)         ; set fixed baud rate generator value
    movwf   UxSPBRG
    #ifdef UxSPBRGH
        #if high(BAUDRG) != 0
    movlw   high(BAUDRG)
    movwf   UxSPBRGH
        #endif
    #endif
    bsf     _UxCREN_, ACCESS    ; start receiving
DoAutoBaud:
WaitForHostCommand:
    rcall   ReadHostByte        ; get start of transmission <STX>
    xorlw   STX
    bnz     WaitForHostCommand  ; got something unexpected, keep waiting for <STX>
	; end #ifdef USE_AUTOBAUD
#endif

; *****************************************************************************

; *****************************************************************************
; Read and parse packet data.
StartOfLine:
    movlw   STX                     ; send back start of response
    rcall   SendHostByte

    lfsr    _FSR0_, COMMAND-1         ; Point to the buffer

ReceiveDataLoop:
    rcall   ReadHostByte            ; Get the data
    xorlw   STX                     ; Check for an unexpected STX
    bz      StartOfLine             ; unexpected STX: abort packet and start over.

NoSTX:
    movf    RXDATA, W, ACCESS
    xorlw   ETX                     ; Check for a ETX
    bz      VerifyPacketCRC         ; Yes, verify CRC

NoETX:
    movf    RXDATA, W, ACCESS
    xorlw   DLE                     ; Check for a DLE
    bnz     AppendDataBuffer

    rcall   ReadHostByte            ; DLE received, get the next byte and store it
    
AppendDataBuffer:
    movff   RXDATA, PREINC0         ; store the data to the buffer
    bra     ReceiveDataLoop

VerifyPacketCRC:
    lfsr    _FSR1_, COMMAND
    clrf    CRCL, ACCESS
    clrf    CRCH, ACCESS
    movff   POSTDEC0, PRODH         ; Save host packet's CRCH to PRODH for later comparison
                                    ; CRCL is now available as INDF0
VerifyPacketCrcLoop:
    movf    POSTINC1, w, ACCESS
    rcall   AddCrc                  ; add new data to the CRC

    movf    FSR1H, w, ACCESS
    cpfseq  FSR0H, ACCESS
    bra     VerifyPacketCrcLoop     ; we aren't at the end of the received data yet, loop
    movf    FSR1L, w, ACCESS
    cpfseq  FSR0L, ACCESS
    bra     VerifyPacketCrcLoop     ; we aren't at the end of the received data yet, loop

    movf    CRCH, w, ACCESS
    cpfseq  PRODH, ACCESS
    bra     DoAutoBaud              ; invalid CRC, reset baud rate generator to re-sync with host
    movf    CRCL, w, ACCESS
    cpfseq  INDF0, ACCESS
    bra     DoAutoBaud              ; invalid CRC, reset baud rate generator to re-sync with host

; ***********************************************
; Pre-setup, common to all commands.
    clrf    CRCL, ACCESS
    clrf    CRCH, ACCESS

    movf    ADDRESS_L, W, ACCESS            ; Set all possible pointers
    movwf   TBLPTRL, ACCESS
#ifdef EEADR
    movwf   EEADR, ACCESS
#endif
    movf    ADDRESS_H, W, ACCESS
    movwf   TBLPTRH, ACCESS
#ifdef EEADRH
    movwf   EEADRH, ACCESS
#endif
    movff   ADDRESS_U, TBLPTRU
    lfsr    _FSR0_, PACKET_DATA
; ***********************************************

 

; ***********************************************
; Test the command field and sub-command.
CheckCommand:
    movlw   0x0A
    cpfslt  COMMAND, ACCESS
    bra     DoAutoBaud          ; invalid command - reset baud generator to re-sync with host

    ; This jump table must exist entirely within one 256 byte block of program memory.
#ifndef __XC
#if ($ & 0xFF) > (0xFF - 24)
    ; Too close to the end of a 256 byte boundary, push address forward to get code
    ; into the next 256 byte block.
    messg   "Wasting some code space to ensure jump table is aligned."
    ORG     $+(0x100 - ($ & 0xFF))
#endif
#endif
JUMPTABLE_BEGIN:
    movf    PCL, w, ACCESS      ; 0 do a read of PCL to set PCLATU:PCLATH to current program counter.
    rlncf   COMMAND, W, ACCESS  ; 2 multiply COMMAND by 2 (each BRA instruction takes 2 bytes on PIC18)
    addwf   PCL, F, ACCESS      ; 4 Jump in command jump table based on COMMAND from host
    bra     BootloaderInfo      ; 6 00h
    bra     ReadFlash           ; 8 01h
    bra     VerifyFlash         ; 10 02h
    bra     EraseFlash          ; 12 03h
    bra     WriteFlash          ; 14 04h
    bra     ReadEeprom          ; 16 05h
    bra     WriteEeprom         ; 18 06h
    bra     WriteConfig         ; 20 07h
    bra     CheckApplication    ; 22 08 [UH] replaced GotoAppVector
    reset                       ; 24 09h

#ifndef __XC
#if (JUMPTABLE_BEGIN & 0xFF) > ($ & 0xFF)
    error "Jump table is not aligned to fit within a single 256 byte address range."
#endif
#endif
; *****************************************************************************

#ifdef INVERT_UART
WaitForRise:
    clrwdt

WaitForRiseLoop:
    btfsc   INTCON, TMR0IF  ; if TMR0 overflowed, we did not get a good baud capture
    return                  ; abort

    btfss   RXPORT, RXPIN, ACCESS   ; Wait for a falling edge
    bra     WaitForRiseLoop

WtSR:
    btfsc   RXPORT, RXPIN, ACCESS   ; Wait for starting edge
    bra     WtSR
    return
	; not inverted UART pins
#else
WaitForRise:
    clrwdt

WaitForRiseLoop:
    btfsc   _TMR0IF_, ACCESS	    ; if TMR0 overflowed, we did not get a good baud capture
    return                  ; abort

    btfsc   RXPORT, RXPIN, ACCESS   ; Wait for a falling edge
    bra     WaitForRiseLoop

WtSR:
    btfss   RXPORT, RXPIN, ACCESS   ; Wait for rising edge
    bra     WtSR
    return
	; end #ifdef INVERT_UART
#endif
; *****************************************************************************

; 16-bit CCITT CRC
; Adds WREG byte to the CRC checksum CRCH:CRCL. WREG destroyed on return.
AddCrc:                           ; Init: CRCH = HHHH hhhh, CRCL = LLLL llll
    xorwf   CRCH, w, ACCESS       ; Pre:  HHHH hhhh     WREG =      IIII iiii
    movff   CRCL, CRCH            ; Pre:  LLLL llll     CRCH =      LLLL llll
    movwf   CRCL, ACCESS          ; Pre:  IIII iiii     CRCL =      IIII iiii
    swapf   WREG, f, ACCESS       ; Pre:  IIII iiii     WREG =      iiii IIII
    andlw   0x0F                  ; Pre:  iiii IIII     WREG =      0000 IIII
    xorwf   CRCL, f, ACCESS       ; Pre:  IIII iiii     CRCL =      IIII jjjj
    swapf   CRCL, w, ACCESS       ; Pre:  IIII jjjj     WREG =      jjjj IIII
    andlw   0xF0                  ; Pre:  jjjj IIII     WREG =      jjjj 0000
    xorwf   CRCH, f, ACCESS       ; Pre:  LLLL llll     CRCH =      MMMM llll
    swapf   CRCL, w, ACCESS       ; Pre:  IIII jjjj     WREG =      jjjj IIII
    rlncf   WREG, w, ACCESS       ; Pre:  jjjj IIII     WREG =      jjjI IIIj
    xorwf   CRCH, f, ACCESS       ; Pre:  MMMM llll     CRCH =      XXXN mmmm
    andlw   0xE0 ;b'11100000'     ; Pre:  jjjI IIIj     WREG =      jjj0 0000
    xorwf   CRCH, f, ACCESS       ; Pre:  jjj0 0000     CRCH =      MMMN mmmm
    xorwf   CRCL, f, ACCESS       ; Pre:  MMMN mmmm     CRCL =      JJJI jjjj
    return

; ***********************************************
; Commands
; ***********************************************

; Provides information about the Bootloader to the host PC software.
BootInfoBlock:
    dw      BOOTBLOCKSIZE
    dw      (MINOR_VERSION << 8) | MAJOR_VERSION
    dw      0x84FF             ; command mask : family id 
    dw      BootloaderStart
    dw      upper(BootloaderStart)
    dw      0x0000             ; device id (reserved)
    dw      ADAPTER_TYPE
BootInfoBlockEnd:

; In:   <STX>[<0x00>]<CRCL><CRCH><ETX>
; Out:  <STX><BOOTBYTESL><BOOTBYTESH><VERL><VERH><STARTBOOTL><STARTBOOTH><STARTBOOTU><0x00><CRCL><CRCH><ETX>
BootloaderInfo:
    movlw   low(BootInfoBlock)
    movwf   TBLPTRL, ACCESS
    movlw   high(BootInfoBlock)
    movwf   TBLPTRH, ACCESS
    movlw   upper(BootInfoBlock)
    movwf   TBLPTRU, ACCESS

    movlw   (BootInfoBlockEnd - BootInfoBlock)
    movwf   DATA_COUNTL, ACCESS
    clrf    DATA_COUNTH, ACCESS
    ;; fall through to ReadFlash code -- send Bootloader Information Block from FLASH.

; In:   <STX>[<0x01><ADDRL><ADDRH><ADDRU><0x00><BYTESL><BYTESH>]<CRCL><CRCH><ETX>
; Out:  <STX>[<DATA>...]<CRCL><CRCH><ETX>
ReadFlash:
    tblrd   *+                  ; read from FLASH memory into TABLAT
    movf    TABLAT, w, ACCESS
    rcall   SendEscapeByte
    rcall   AddCrc

    decf    DATA_COUNTL, f, ACCESS      ; decrement counter
    movlw   0
    subwfb  DATA_COUNTH, f, ACCESS

    movf    DATA_COUNTL, w, ACCESS      ; DATA_COUNTH:DATA_COUNTH == 0?
    iorwf   DATA_COUNTH, w, ACCESS
    bnz     ReadFlash           ; no, loop
    bra     SendChecksum        ; yes, send end of packet

; In:   <STX>[<0x02><ADDRL><ADDRH><ADDRU><0x00><BLOCKSL><BLOCKSH>]<CRCL><CRCH><ETX>
; Out:  <STX>[<CRCL1><CRCH1>...<CRCLn><CRCHn>]<ETX>
VerifyFlash:
    tblrd   *+
    movf    TABLAT, w, ACCESS
    rcall   AddCrc

    movf    TBLPTRL, w, ACCESS          ; have we crossed into the next block?
#if ERASE_FLASH_BLOCKSIZE > 0xFF
    bnz     VerifyFlash
    movf    TBLPTRH, w, ACCESS
    andlw   high(ERASE_FLASH_BLOCKSIZE-1)
#else
    andlw   (ERASE_FLASH_BLOCKSIZE-1)    
#endif
    bnz     VerifyFlash

    movf    CRCL, w, ACCESS
    call    SendEscapeByte
    movf    CRCH, w, ACCESS
    call    SendEscapeByte

    decf    DATA_COUNTL, f, ACCESS      ; decrement counter
    movlw   0
    subwfb  DATA_COUNTH, f, ACCESS

    movf    DATA_COUNTL, w, ACCESS      ; DATA_COUNTH:DATA_COUNTH == 0?
    iorwf   DATA_COUNTH, w, ACCESS
    bnz     VerifyFlash         ; no, loop
    bra     SendETX             ; yes, send end of packet

#ifdef SOFTWP
    reset                       ; this code -should- never be executed, but 
    reset                       ; just in case of errant execution or buggy
    reset                       ; firmware, these reset instructions may protect
    reset                       ; against accidental erases.
#endif

; In:   <STX>[<0x03><ADDRL><ADDRH><ADDRU><0x00><PAGESL>]<CRCL><CRCH><ETX>
; Out:  <STX>[<0x03>]<CRCL><CRCH><ETX>
EraseFlash:
#ifdef SOFTWP
  #define ERASE_ADDRESS_MASK  (~(ERASE_FLASH_BLOCKSIZE-1))
  #if upper(ERASE_ADDRESS_MASK) != 0xFF
    movlw   upper(ERASE_ADDRESS_MASK)    ; force starting address to land on a FLASH Erase Block boundary
    andwf   TBLPTRU, f
  #endif
  #if high(ERASE_ADDRESS_MASK) != 0xFF
    movlw   high(ERASE_ADDRESS_MASK)    ; force starting address to land on a FLASH Erase Block boundary
    andwf   TBLPTRH, f
  #endif
  #if low(ERASE_ADDRESS_MASK) != 0xFF
    movlw   low(ERASE_ADDRESS_MASK)     ; force starting address to land on a FLASH Erase Block boundary
    andwf   TBLPTRL, f
  #endif

    ; Verify Erase Address does not attempt to erase beyond the end of FLASH memory
    movlw   low(END_FLASH)
    subwf   TBLPTRL, w
    movlw   high(END_FLASH)
    subwfb  TBLPTRH, w
    movlw   upper(END_FLASH)
    subwfb  TBLPTRU, w
    bn      EraseEndFlashAddressOkay

    clrf    EECON1              ; inhibit writes for this block
    bra     NextEraseBlock      ; move on to next erase block
	; end #ifdef USE_SOFTBOOTWP
#endif

EraseEndFlashAddressOkay:
#ifdef USE_SOFTCONFIGWP
    #ifdef CONFIG_AS_FLASH
    movlw   low(END_FLASH - ERASE_FLASH_BLOCKSIZE)
    subwf   TBLPTRL, w
    movlw   high(END_FLASH - ERASE_FLASH_BLOCKSIZE)
    subwfb  TBLPTRH, w
    movlw   upper(END_FLASH - ERASE_FLASH_BLOCKSIZE)
    subwfb  TBLPTRU, w
    bn      EraseConfigAddressOkay

    clrf    EECON1              ; inhibit writes for this block
    bra     NextEraseBlock      ; move on to next erase block

EraseConfigAddressOkay:
	    ; end CONFIG_AS_FLASH
    #endif
	; end USE_SOFTCONFIGWP
#endif

#ifdef USE_SOFTBOOTWP
    movlw   low(BOOTLOADER_ADDRESS)
    subwf   TBLPTRL, w
    movlw   high(BOOTLOADER_ADDRESS)
    subwfb  TBLPTRH, w
    movlw   upper(BOOTLOADER_ADDRESS)
    subwfb  TBLPTRU, w
    bn      EraseAddressOkay

    movlw   low(BOOTLOADER_ADDRESS + BOOTBLOCKSIZE)
    subwf   TBLPTRL, w
    movlw   high(BOOTLOADER_ADDRESS + BOOTBLOCKSIZE)
    subwfb  TBLPTRH, w
    movlw   upper(BOOTLOADER_ADDRESS + BOOTBLOCKSIZE)
    subwfb  TBLPTRU, w
    bnn     EraseAddressOkay

    clrf    EECON1              ; inhibit writes for this block
    bra     NextEraseBlock      ; move on to next erase block

    reset                       ; this code -should- never be executed, but 
    reset                       ; just in case of errant execution or buggy
    reset                       ; firmware, these reset instruction may protect
    reset                       ; against accidental writes.
#endif

EraseAddressOkay:
#ifdef EEADR
    movlw   0x94 ;b'10010100'         ; setup FLASH erase
#else
    movlw   0x14 ;b'00010100'         ; setup FLASH erase for J device (no EEPROM bit)
#endif
    movwf   EECON1, ACCESS

    rcall   StartWrite          ; erase the page

NextEraseBlock:
    ; Decrement address by erase block size
#if ERASE_FLASH_BLOCKSIZE >= 0x100
    movlw   high(ERASE_FLASH_BLOCKSIZE)
    subwf   TBLPTRH, F, ACCESS
    clrf    WREG, ACCESS
    subwfb  TBLPTRU, F, ACCESS
#else
    movlw   ERASE_FLASH_BLOCKSIZE
    subwf   TBLPTRL, F, ACCESS
    clrf    WREG, ACCESS
    subwfb  TBLPTRH, F, ACCESS
    subwfb  TBLPTRU, F, ACCESS
#endif

    decfsz  DATA_COUNTL, F, ACCESS
    bra     EraseFlash    
    bra     SendAcknowledge     ; All done, send acknowledgement packet

#ifdef SOFTWP
    reset                       ; this code -should- never be executed, but 
    reset                       ; just in case of errant execution or buggy
    reset                       ; firmware, these reset instructions may protect
    reset                       ; against accidental writes.
#endif

; In:   <STX>[<0x04><ADDRL><ADDRH><ADDRU><0x00><BLOCKSL><DATA>...]<CRCL><CRCH><ETX>
; Out:  <STX>[<0x04>]<CRCL><CRCH><ETX>
WriteFlash:
#ifdef SOFTWP
  #define WRITE_ADDRESS_MASK (~(WRITE_FLASH_BLOCKSIZE-1))
  #if upper(WRITE_ADDRESS_MASK) != 0xFF
    movlw   upper(WRITE_ADDRESS_MASK)    ; force starting address to land on a FLASH Write Block boundary
    andwf   TBLPTRU, f
  #endif
  #if high(WRITE_ADDRESS_MASK) != 0xFF
    movlw   high(WRITE_ADDRESS_MASK)    ; force starting address to land on a FLASH Write Block boundary
    andwf   TBLPTRH, f
  #endif
  #if low(WRITE_ADDRESS_MASK) != 0xFF
    movlw   low(WRITE_ADDRESS_MASK)     ; force starting address to land on a FLASH Write Block boundary
    andwf   TBLPTRL, f
  #endif

    ; Verify Write Address does not attempt to write beyond the end of FLASH memory
    movlw   low(END_FLASH)
    subwf   TBLPTRL, w
    movlw   high(END_FLASH)
    subwfb  TBLPTRH, w
    movlw   upper(END_FLASH)
    subwfb  TBLPTRU, w
    bn      WriteEndFlashAddressOkay

    clrf    EECON1              ; inhibit writes for this block
    bra     LoadHoldingRegisters; fake the write so we can move on to real writes
	; end #ifdef SOFTWP
#endif

WriteEndFlashAddressOkay:
#ifdef USE_SOFTCONFIGWP
    #ifdef CONFIG_AS_FLASH
    movlw   low(END_FLASH - ERASE_FLASH_BLOCKSIZE)
    subwf   TBLPTRL, w
    movlw   high(END_FLASH - ERASE_FLASH_BLOCKSIZE)
    subwfb  TBLPTRH, w
    movlw   upper(END_FLASH - ERASE_FLASH_BLOCKSIZE)
    subwfb  TBLPTRU, w
    bn      WriteConfigAddressOkay

    clrf    EECON1              ; inhibit writes for this block
    bra     LoadHoldingRegisters; fake the write so we can move on to real writes

WriteConfigAddressOkay:
	    ; end CONFIG_AS_FLASH
    #endif
	; end USE_SOFTCONFIGWP
#endif

#ifdef USE_SOFTBOOTWP
    movlw   low(BOOTLOADER_ADDRESS)
    subwf   TBLPTRL, w
    movlw   high(BOOTLOADER_ADDRESS)
    subwfb  TBLPTRH, w
    movlw   upper(BOOTLOADER_ADDRESS)
    subwfb  TBLPTRU, w
    bn      WriteAddressOkay

    movlw   low(BOOTLOADER_ADDRESS + BOOTBLOCKSIZE)
    subwf   TBLPTRL, w
    movlw   high(BOOTLOADER_ADDRESS + BOOTBLOCKSIZE)
    subwfb  TBLPTRH, w
    movlw   upper(BOOTLOADER_ADDRESS + BOOTBLOCKSIZE)
    subwfb  TBLPTRU, w
    bnn     WriteAddressOkay

    clrf    EECON1                      ; inhibit writes for this block
    bra     LoadHoldingRegisters        ; fake the write so we can move on to real writes

    reset                       ; this code -should- never be executed, but 
    reset                       ; just in case of errant execution or buggy
    reset                       ; firmware, these reset instruction may protect
    reset                       ; against accidental writes.
#endif

WriteAddressOkay:
#ifdef EEADR
    movlw   0x84 ;b'10000100'         ; Setup FLASH writes
#else
    movlw   0x04 ;b'00000100'         ; Setup FLASH writes for J device (no EEPROM bit)
#endif
    movwf   EECON1, ACCESS

LoadHoldingRegisters:
    movff   POSTINC0, TABLAT    ; Load the holding registers
    pmwtpi                      ; Same as tblwt *+

    movf    TBLPTRL, w, ACCESS  ; have we crossed into the next write block?
    andlw   (WRITE_FLASH_BLOCKSIZE-1)
    bnz     LoadHoldingRegisters; Not finished writing holding registers, repeat

    tblrd   *-                  ; Point back into the block to write data
    rcall   StartWrite          ; initiate a page write
    tblrd   *+                  ; Restore pointer for loading holding registers with next block

    decfsz  DATA_COUNTL, F, ACCESS
    bra     WriteFlash          ; Not finished writing all blocks, repeat
    bra     SendAcknowledge     ; all done, send ACK packet

; In:   <STX>[<0x05><ADDRL><ADDRH><0x00><0x00><BYTESL><BYTESH>]<CRCL><CRCH><ETX>
; Out:  <STX>[<DATA>...]<CRCL><CRCH><ETX>
			    ; some devices do not have EEPROM, so no need for this code
#ifdef EEADR
ReadEeprom:
    clrf    EECON1, ACCESS
ReadEepromLoop:
    bsf     _RD_, ACCESS	; Read the data
    movf    EEDATA, w, ACCESS
    #ifdef EEADRH
    infsnz  EEADR, F, ACCESS    ; Adjust EEDATA pointer
    incf    EEADRH, F, ACCESS
    #else
    incf    EEADR, F            ; Adjust EEDATA pointer
    #endif
    rcall   SendEscapeByte
    rcall   AddCrc

    #ifdef EEADRH
    decf    DATA_COUNTL, f, ACCESS      ; decrement counter
    movlw   0
    subwfb  DATA_COUNTH, f, ACCESS

    movf    DATA_COUNTL, w, ACCESS      ; DATA_COUNTH:DATA_COUNTH == 0?
    iorwf   DATA_COUNTH, w, ACCESS
    bnz     ReadEepromLoop      ; no, loop
    bra     SendChecksum        ; yes, send end of packet
    #else
    decfsz  DATA_COUNTL, F, ACCESS
    bra     ReadEepromLoop      ; Not finished then repeat
    bra     SendChecksum
    #endif
	; end #ifdef EEADR
#endif

; In:   <STX>[<0x06><ADDRL><ADDRH><0x00><0x00><BYTESL><BYTESH><DATA>...]<CRCL><CRCH><ETX>
; Out:  <STX>[<0x06>]<CRCL><CRCH><ETX>
			    ; some devices do not have EEPROM, so no need for this code
#ifdef EEADR
WriteEeprom:
    movlw   0x04 ;b'00000100'     ; Setup for EEPROM data writes
    movwf   EECON1, ACCESS

WriteEepromLoop:
    movff   PREINC0, EEDATA
    rcall   StartWrite      

    btfsc   _WR_, ACCESS	    ; wait for write to complete before moving to next address
    bra     $-2

    #ifdef EEADRH
    infsnz  EEADR, F, ACCESS        ; Adjust EEDATA pointer
    incf    EEADRH, F, ACCESS
    #else
    incf    EEADR, f, ACCESS        ; Adjust EEDATA pointer
    #endif

    #ifdef EEADRH
    decf    DATA_COUNTL, f, ACCESS      ; decrement counter
    movlw   0
    subwfb  DATA_COUNTH, f, ACCESS

    movf    DATA_COUNTL, w, ACCESS      ; DATA_COUNTH:DATA_COUNTH == 0?
    iorwf   DATA_COUNTH, w, ACCESS
    bnz     WriteEepromLoop     ; no, loop
    bra     SendAcknowledge     ; yes, send end of packet
    #else
    decfsz  DATA_COUNTL, f
    bra     WriteEepromLoop
    bra     SendAcknowledge
    #endif
	; end #ifdef EEADR
#endif

; In:   <STX>[<0x07><ADDRL><ADDRH><ADDRU><0x00><BYTES><DATA>...]<CRCL><CRCH><ETX>
; Out:  <STX>[<0x07>]<CRCL><CRCH><ETX>
			    ; J flash devices store config words in FLASH, so no need for this code
#ifndef CONFIG_AS_FLASH
    #ifndef USE_SOFTCONFIGWP
WriteConfig:
    movlw   0xC4 ;b'11000100'
    movwf   EECON1, ACCESS
    tblrd   *               ; read existing value from config memory

WriteConfigLoop:
    movf    POSTINC0, w, ACCESS
    cpfseq  TABLAT, ACCESS  ; is the proposed value already the same as existing value?
    rcall   TableWriteWREG  ; write config memory only if necessary (save time and endurance)
    tblrd   +*              ; increment table pointer to next address and read existing value
    decfsz  DATA_COUNTL, F, ACCESS
    bra     WriteConfigLoop ; If more data available in packet, keep looping

    bra     SendAcknowledge ; Send acknowledge
	    ; end #ifndef USE_SOFTCONFIGWP
    #endif
	    ; end #ifndef CONFIG_AS_FLASH
#endif

;************************************************

; ***********************************************
; Send an acknowledgement packet back
;
; <STX><COMMAND><CRCL><CRCH><ETX>

; Some devices only have config words as FLASH memory. Some devices don't have EEPROM.
; For these devices, we can save code by jumping directly to sending back an
; acknowledgement packet if the PC application erroneously requests them.
#ifdef CONFIG_AS_FLASH
WriteConfig:
#else
  #ifdef USE_SOFTCONFIGWP
WriteConfig:
  #endif
	; end #ifdef CONFIG_AS_FLASH
#endif

#ifndef EEADR
ReadEeprom:
WriteEeprom:
#endif

SendAcknowledge:
    clrf    EECON1, ACCESS      ; inhibit write cycles to FLASH memory

    movf    COMMAND, w, ACCESS
    rcall   SendEscapeByte      ; Send only the command byte (acknowledge packet)
    rcall   AddCrc

SendChecksum:
    movf    CRCL, W, ACCESS
    rcall   SendEscapeByte

    movf    CRCH, W, ACCESS
    rcall   SendEscapeByte

SendETX:
    movlw   ETX             ; Send stop condition
    rcall   SendHostByte

    bra     WaitForHostCommand
; *****************************************************************************




; *****************************************************************************
; Write a byte to the serial port while escaping control characters with a DLE
; first.
SendEscapeByte:
    movwf   TXDATA, ACCESS  ; Save the data
 
    xorlw   STX             ; Check for a STX
    bz      WrDLE           ; No, continue WrNext

    movf    TXDATA, W, ACCESS
    xorlw   ETX             ; Check for a ETX
    bz      WrDLE           ; No, continue WrNext

    movf    TXDATA, W, ACCESS
    xorlw   DLE             ; Check for a DLE
    bnz     WrNext          ; No, continue WrNext

WrDLE:
    movlw   DLE             ; Yes, send DLE first
    rcall   SendHostByte

WrNext:
    movf    TXDATA, W, ACCESS       ; Then send STX

SendHostByte:
    clrwdt
    btfss   _UxTXIF_, ACCESS	    ; Write only if TXREG is ready
    bra     $-2

    movwf   UxTXREG, ACCESS         ; Start sending

    return
; *****************************************************************************




; *****************************************************************************
ReadHostByte:
    btfsc   _UxOERR_, ACCESS	    ; Reset on overun
    reset

WaitForHostByte:
    clrwdt
    btfss   _UxRCIF_, ACCESS		; Wait for data from RS232
    bra     WaitForHostByte

    movf    UxRCREG, W, ACCESS          ; Save the data
    movwf   RXDATA, ACCESS
 
    return
; *****************************************************************************

    reset                       ; this code -should- never be executed, but 
    reset                       ; just in case of errant execution or buggy
    reset                       ; firmware, these instructions may protect
    clrf    EECON1, ACCESS      ; against accidental erase/write operations.

; *****************************************************************************
; Unlock and start the write or erase sequence.
TableWriteWREG:
    movwf   TABLAT, ACCESS
    tblwt   *

StartWrite:
    clrwdt

    movlw   0x55            ; Unlock
    movwf   EECON2, ACCESS
    movlw   0xAA
    movwf   EECON2, ACCESS
    bsf     _WR_, ACCESS    ; Start the write
    nop

    return
; *****************************************************************************

    END END_LABEL
