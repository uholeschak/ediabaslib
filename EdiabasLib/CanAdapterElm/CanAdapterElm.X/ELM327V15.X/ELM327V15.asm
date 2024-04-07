		;Select your processor
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
		#define upper(_x) (low((_x) >> 16))
		#define MOD mod
		#define ACCESS a
		#define BANKED b

		#define _SWDTEN_ SWDTEN ACC
		#define _RI_ RI ACC
		#define _POR_ POR ACC
#else
		LIST      P=18F25K80		; modify this
		#include "p18f25k80.inc"		; and this
		#define MOD %

		#define _SWDTEN_ WDTCON, SWDTEN, ACCESS
		#define _RI_ RCON, RI, ACCESS
		#define _POR_ RCON, POR, ACCESS
#endif

		#define ORIGINAL    0

		#if ORIGINAL
		    #define SW_VERSION 0x00
		    #define CODE_OFFSET 0x0000
		    #define BASE_ADDR 0x0000
		    #define DATA_OFFSET BASE_ADDR
		    #define TABLE_OFFSET BASE_ADDR
		    #define EEPROM_PAGE 0x0
		    #define WDT_RESET   0x0
							;38400
		    #define DEFAULT_BAUD 0x68
		#else
		    #define SW_VERSION 0x02
		    #define CODE_OFFSET 0x0800
		    #define BASE_ADDR 0x1000
		    #define DATA_OFFSET BASE_ADDR
		    #define TABLE_OFFSET BASE_ADDR
		    #define EEPROM_PAGE 0x3
		    #define WDT_RESET   0x1

		    #if ADAPTER_TYPE == 0x02
							;38400
			#define DEFAULT_BAUD 0x68
		    #else
							;115200
			#define DEFAULT_BAUD 0x23
		    #endif
		#endif

		; CONFIG1L
		CONFIG RETEN = ON       ; VREG Sleep Enable bit (Ultra low-power regulator is Enabled (Controlled by SRETEN bit))
		CONFIG INTOSCSEL = HIGH ; LF-INTOSC Low-power Enable bit (LF-INTOSC in High-power mode during Sleep)
		CONFIG SOSCSEL = DIG    ; SOSC Power Selection and mode Configuration bits (Digital (SCLKI) mode)
		CONFIG XINST = OFF      ; Extended Instruction Set (Disabled)

		; CONFIG1H
		CONFIG FOSC = HS1       ; Oscillator (HS oscillator (Medium power, 4 MHz - 16 MHz))
		CONFIG PLLCFG = ON      ; PLL x4 Enable bit (Enabled)
		CONFIG FCMEN = OFF      ; Fail-Safe Clock Monitor (Disabled)
		CONFIG IESO = OFF       ; Internal External Oscillator Switch Over Mode (Disabled)

		; CONFIG2L
		CONFIG PWRTEN = ON      ; Power Up Timer (Enabled)
		CONFIG BOREN = SBORDIS  ; Brown Out Detect (Enabled in hardware, SBOREN disabled)
		CONFIG BORV = 0         ; Brown-out Reset Voltage bits (3.0V)
		CONFIG BORPWR = HIGH    ; BORMV Power level (BORMV set to high power level)

#ifdef __XC
		CONFIG CONFIG2H = 0x1E
		CONFIG CONFIG3H = 0x89
#else
		; CONFIG2H
		CONFIG WDTEN = ON       ; Watchdog Timer (WDT controlled by SWDTEN bit setting)
		CONFIG WDTPS = 128      ; Watchdog Postscaler (1:128)

		; CONFIG3H
		CONFIG CANMX = PORTB    ; ECAN Mux bit (ECAN TX and RX pins are located on RB2 and RB3, respectively)
		CONFIG MSSPMSK = MSK7   ; MSSP address masking (7 Bit address masking mode)
		CONFIG MCLRE = ON       ; Master Clear Enable (MCLR Enabled, RE3 Disabled)
#endif

		; CONFIG4L
		CONFIG STVREN = ON      ; Stack Overflow Reset (Enabled)
		CONFIG BBSIZ = BB1K     ; Boot Block Size (1K word Boot Block size)

		; CONFIG5L
		CONFIG CP0 = OFF        ; Code Protect 00800-01FFF (Disabled)
		CONFIG CP1 = OFF        ; Code Protect 02000-03FFF (Disabled)
		CONFIG CP2 = OFF        ; Code Protect 04000-05FFF (Disabled)
		CONFIG CP3 = OFF        ; Code Protect 06000-07FFF (Disabled)

		; CONFIG5H
		CONFIG CPB = OFF        ; Code Protect Boot (Disabled)
		CONFIG CPD = OFF        ; Data EE Read Protect (Disabled)

		; CONFIG6L
		CONFIG WRT0 = OFF       ; Table Write Protect 00800-01FFF (Disabled)
		CONFIG WRT1 = OFF       ; Table Write Protect 02000-03FFF (Disabled)
		CONFIG WRT2 = OFF       ; Table Write Protect 04000-05FFF (Disabled)
		CONFIG WRT3 = OFF       ; Table Write Protect 06000-07FFF (Disabled)

		; CONFIG6H
		CONFIG WRTC = ON        ; Config. Write Protect (Enabled)
		CONFIG WRTB = ON        ; Table Write Protect Boot (Enabled)
		CONFIG WRTD = OFF       ; Data EE Write Protect (Disabled)

		; CONFIG7L
		CONFIG EBTR0 = OFF      ; Table Read Protect 00800-01FFF (Disabled)
		CONFIG EBTR1 = OFF      ; Table Read Protect 02000-03FFF (Disabled)
		CONFIG EBTR2 = OFF      ; Table Read Protect 04000-05FFF (Disabled)
		CONFIG EBTR3 = OFF      ; Table Read Protect 06000-07FFF (Disabled)

		; CONFIG7H
		CONFIG EBTRB = OFF      ; Table Read Protect Boot (Disabled)

#ifdef __XC
#define END_LABEL reset_vector
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
		; EEPROM
#if SW_VERSION == 0
		ORG 0xF00000 + (EEPROM_PAGE * 0x100)
#else
		ORG CODE_OFFSET + 0x000100
#endif
eep_start:	DB 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x06, 0xAE, 0x02, 0x6A, 0xFF, 0xFF, 0xFF, 0xFF
		DB 0xFF, 0xFF, 0x32, 0xFF, 0x01, 0xFF, 0xFF, 0xFF, 0xF1, 0xFF, 0x09, 0xFF, 0xFF, 0xFF, 0x00, 0xFF
		DB 0x0A, 0xFF, 0xFF, 0xFF, DEFAULT_BAUD, 0xFF, 0x0D, 0xFF, 0x9A, 0xFF, 0xFF, 0xFF, 0x0D, 0xFF, 0x00, 0xFF
		DB 0xFF, 0xFF, 0x32, 0xFF, 0xFF, 0xFF, 0x0A, 0xFF, 0xFF, 0xFF, 0x92, 0xFF, 0x00, 0xFF, 0x28, 0xFF
		DB 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
		DB 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
		DB 0x38, 0xFF, 0x02, 0xFF, 0xE0, 0xFF, 0x04, 0xFF, 0x80, 0xFF, 0x0A, 0xFF
#if SW_VERSION == 0
		DB 0x00, 0x00, 0x00, 0x00
		DB 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
#else
		DB 0xFF, 0xFF, 0xFF, 0xFF
		DB "D", "E", "E", "P", "O", "B", "D", " ", 0x30 + (SW_VERSION / 16), 0x30 + (SW_VERSION MOD 16)
		DB 0x30 + (ADAPTER_TYPE / 16), 0x30 + (ADAPTER_TYPE MOD 16)
#endif
		DB 0xFF, 0x00, 0xFF, 0xFF

#if SW_VERSION != 0
eep_end:
p_restart:	btfss	_RI_
		goto	p_reset		; perform wd reset after software reset
		return

eep_copy:	movlw	0x24
		movwf	EEADR, ACCESS
		call	p__838
		xorlw	DEFAULT_BAUD
		bnz	eep_init

		movlw	0x78
		movwf	EEADR, ACCESS
		call	p__838
		xorlw	0x30 + (SW_VERSION / 16)
		bnz	eep_init

		movlw	0x79
		movwf	EEADR, ACCESS
		call	p__838
		xorlw	0x30 + (SW_VERSION MOD 16)
		bnz	eep_init

		movlw	0x7A
		movwf	EEADR, ACCESS
		call	p__838
		xorlw	0x30 + (ADAPTER_TYPE / 16)
		bnz	eep_init

		movlw	0x7B
		movwf	EEADR, ACCESS
		call	p__838
		xorlw	0x30 + (ADAPTER_TYPE MOD 16)
		bnz	eep_init
		return

eep_init:	movlw   low(eep_start)
		movwf   TBLPTRL, ACCESS
		movlw   high(eep_start)
		movwf   TBLPTRH, ACCESS
		movlw   upper(eep_start)
		movwf   TBLPTRU, ACCESS
		bsf	EECON1,2, ACCESS
		movlw	0x00
		movwf	EEADR, ACCESS
eep_loop:	tblrd   *+
	        movf    TABLAT, W, ACCESS
		call	p__A00
		movf    EEADR, W, ACCESS
		xorlw	low(eep_end - eep_start)
		bnz	eep_loop
		bcf	EECON1,2, ACCESS

		movlw	high(DATA_OFFSET) + 0
		movwf	TBLPTRH, ACCESS
		clrf	TBLPTRU, ACCESS
		return
#endif

#if ORIGINAL == 0
		ORG 0x7FFA
		DW 0x0015		; adapter version
		DW ADAPTER_TYPE		; adapter type
#endif

		ORG CODE_OFFSET + 0
reset_vector:
		nop
		goto	p_1654

		ORG CODE_OFFSET + 0x08
		btfsc	0x1C,7, ACCESS
		goto	p_1280
p____E:	movlw	0x70						; entry from: 18h
		goto	p__6CC

		ORG CODE_OFFSET + 0x18
		bra		p____E

		ORG DATA_OFFSET + 0x001A
		DB 'A', 'C', 'T', ' ', 'A', 'L', 'E', 'R', 'T', 0
		DB 'O', 'B', 'D', 'I', 'I', ' ', 't', 'o', ' ', 'R', 'S', '2', '3', '2', ' ', 'I', 'n', 't', 'e', 'r', 'p', 'r', 'e', 't', 'e', 'r', 0, 0
		DB 'B', 'U', 'F', 'F', 'E', 'R', ' ', 'F', 'U', 'L', 'L', 0
		DB 'B', 'U', 'S', ' ', 'B', 'U', 'S', 'Y', 0, 0
		DB 'B', 'U', 'S', ' ', 'E', 'R', 'R', 'O', 'R', 0
		DB 'B', 'U', 'S', ' ', 'I', 'N', 'I', 'T', ':', ' ', 0, 0
		DB 'C', 'A', 'N', ' ', 'E', 'R', 'R', 'O', 'R', 0
		DB '<', 'D', 'A', 'T', 'A', ' ', 'E', 'R', 'R', 'O', 'R', 0
		DB 'E', 'L', 'M', '3', '2', '7', ' ', 'v', '1', '.', '5', 0
		DB '?', 0
		DB 'F', 'B', ' ', 'E', 'R', 'R', 'O', 'R', 0, 0
		DB 'U', 'N', 'A', 'B', 'L', 'E', ' ', 'T', 'O', ' ', 'C', 'O', 'N', 'N', 'E', 'C', 'T', 0
		DB 'N', 'O', ' ', 'D', 'A', 'T', 'A', 0
		DB 'O', 'K', 0, 0
		DB '>', 0
		DB 'S', 'E', 'A', 'R', 'C', 'H', 'I', 'N', 'G', '.', '.', '.', 0, 0
		DB 'S', 'T', 'O', 'P', 'P', 'E', 'D', 0
		DB '>', 'A', 'T', ' ', 'M', 'A', 0, 0
		DB '<', 'R', 'X', ' ', 'E', 'R', 'R', 'O', 'R', 0
		DB 'L', 'V', ' ', 'R', 'E', 'S', 'E', 'T', 0, 0

		ORG TABLE_OFFSET + 0x00F0
		DB '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
p__100:	addwf	PCL, ACCESS						; entry from: 1AECh
		DB "Z", 0x00
#if WDT_RESET
		goto	p_reset
#else
		clrf	0xD1,BANKED
		reset
#endif
		DB "I", 0x00
		goto	p_182A
		DB "D", 0x00
		goto	p__3B2
		DB 0x00, 0x00
		DB "A", "L"
		bsf		0x17,4, ACCESS
		bra		p__302
		DB "A", "R"
		goto	p__CA0
		DB "B", "D"
		goto	p__CAA
		DB "B", "I"
		goto	p__CC4
		DB "C", "S"
		goto	p__E60
		DB "D", "0"
		bcf		0x18,5, ACCESS
		bra		p__302
		DB "D", "1"
		bsf		0x18,5, ACCESS
		bra		p__302
		DB "D", "P"
		goto	p__F40
		DB "E", "0"
		bcf		0x17,2, ACCESS
		bra		p__302
		DB "E", "1"
		bsf		0x17,2, ACCESS
		bra		p__302
		DB "F", "E"
		clrf	0xD2,BANKED
		bra		p__302
		DB "F", "I"
		goto	p_1104
		DB "H", "0"
		bcf		0x17,1, ACCESS
		bra		p__302
		DB "H", "1"
		bsf		0x17,1, ACCESS
		bra		p__302
		DB "J", "E"
		bcf		0x35,2, ACCESS
		bra		p__302
		DB "J", "S"
		bsf		0x35,2, ACCESS
		bra		p__302
		DB "K", "W"
		goto	p_11BC
		DB "L", "0"
		bcf		0x17,7, ACCESS
		bra		p__302
		DB "L", "1"
		bsf		0x17,7, ACCESS
		bra		p__302
		DB "L", "P"
		goto	p_11F8
		DB "M", "0"
		bcf		0x17,5, ACCESS
		bra		p__302
		DB "M", "1"
		bsf		0x17,5, ACCESS
		bra		p__302
		DB "M", "A"
		goto	p_129A
		DB "N", "L"
		bcf		0x17,4, ACCESS
		bra		p__302
		DB "P", "C"
		goto	p_12E0
		DB "R", "0"
		bcf		0x17,3, ACCESS
		bra		p__302
		DB "R", "1"
		bsf		0x17,3, ACCESS
		bra		p__302
		DB "R", "D"
		goto	p_146E
		DB "R", "V"
		goto	p_148A
		DB "S", "0"
		bsf		0x18,0, ACCESS
		bra		p__302
		DB "S", "1"
		bcf		0x18,0, ACCESS
		bra		p__302
		DB "S", "I"
		goto	p_156E
		DB "S", "S"
		bsf		0x10,5, ACCESS
		bra		p__302
		DB "V", "0"
		bcf		0x35,5, ACCESS
		bra		p__302
		DB "V", "1"
		bsf		0x35,5, ACCESS
		bra		p__302
		DB "W", "S"
		goto	p_1650
		DB "@", "1"
		goto	p__C3C
		DB "@", "2"
		goto	p__C42
		DB 0x00, 0x00

		ORG TABLE_OFFSET + 0x0200
		addwf	PCL
		DB "A", "T"
		goto	p__C1E
		DB "C", "E"
		goto	p__D76
		DB "D", "M"
		goto	p__F22
		DB "D", "P"
		goto	p_101A
		DB "I", "G"
		goto	p_1184
		DB "K", "W"
		goto	p_11AA
		DB "P", "P"
		goto	p_13F4
		DB "R", "T"
		goto	p_147A
		DB "S", "P"
		goto	p_15AE
		DB "T", "P"
		goto	p_15AE
		DB 0x00, 0x00
		DB "C", "A"
		goto	p__D5C
		DB "C", "F"
		goto	p__DAC
		DB "C", "P"
		goto	p__DF2
		DB "I", "B"
		goto	p_110C
		DB "I", "F"
		goto	p_1148
		DB "M", "R"
		goto	p_12BA
		DB "M", "T"
		goto	p_12C2
		DB "R", "A"
		goto	p_157E
		DB "S", "P"
		goto	p_15DE
		DB "T", "A"
		goto	p_15A6
		DB "T", "P"
		goto	p_15DE
		DB "S", "D"
		goto	p_152E
		DB "S", "R"
		goto	p_157E
		DB "S", "T"
		goto	p_1586
		DB "S", "W"
		goto	p_1592
		DB 0x00, 0x00
		DB "B", "R"
		goto	p__CD8
		DB "C", "F"
		goto	p__D80
		DB "C", "M"
		goto	p__DC6
		DB "C", "E"
		goto	p_1536
		DB "I", "I"
		goto	p_112C
		DB "S", "H"
		goto	p_1558
		DB 0x00, 0x00
		DB "C", "R"
		goto	p__E02
		DB "C", "V"
		goto	p__EB2
		DB "M", "P"
		goto	p_12A2
		DB "P", "B"
		goto	p_12CE
		DB "P", "P"
		goto	p_12E6
		DB 0x00, 0x00
		DB "P", "P"
		goto	p_12FA
		DB 0x00, 0x00
		DB "M", "P"
		goto	p_12AC
		DB "P", "P"
		goto	p_1312
		DB "S", "H"
		goto	p_1546
		DB 0x00, 0x00

		ORG TABLE_OFFSET + 0x0300
		addwf	PCL
p__302:	goto	p__E9E					; entry from: 11Ah,138h,13Eh,14Ah,150h,156h,162h,168h,16Eh,174h,180h,186h,192h,198h,1A4h,1B0h,1B6h,1C8h,1CEh,1DAh,1E0h,1E6h,3BAh,3C4h
		DB "C", "F"
		goto	p__D92
		DB "C", "M"
		goto	p__DD8
		DB 0x00, 0x00
		DB "C", "R"
		goto	p__E24
		DB 0x00, 0x00
		DB "@", "3"
		goto	p__C72
		DB 0x00, 0x00
text_table2:
		DB 'A', 'U', 'T', 'O', 0, 0
		DB 'S', 'A', 'E', ' ', 'J', '1', '8', '5', '0', ' ', 'P', 'W', 'M', 0
		DB 'S', 'A', 'E', ' ', 'J', '1', '8', '5', '0', ' ', 'V', 'P', 'W', 0
		DB 'I', 'S', 'O', ' ', '9', '1', '4', '1', '-', '2', 0, 0
		DB 'I', 'S', 'O', ' ', '1', '4', '2', '3', '0', '-', '4', ' ', '(', 'K', 'W', 'P', ' ', '5', 'B', 'A', 'U', 'D', ')', 0
		DB 'I', 'S', 'O', ' ', '1', '4', '2', '3', '0', '-', '4', ' ', '(', 'K', 'W', 'P', ' ', 'F', 'A', 'S', 'T', ')', 0, 0
		DB 'I', 'S', 'O', ' ', '1', '5', '7', '6', '5', '-', '4', 0
		DB 'S', 'A', 'E', ' ', 'J', '1', '9', '3', '9', 0
		DB 'U', 'S', 'E', 'R', '1', 0
		DB 'U', 'S', 'E', 'R', '2', 0
		DB ' ', '(', 'C', 'A', 'N', ' ', 0, 0
		DB 'E', 'R', 'R', '7', '1', 0

p__3B2:	btfss	0x17,7					; entry from: 110h
		bra		p__3BC
		call	p__A1E
		bra		p__302
p__3BC:	call	p__A1E					; entry from: 3B4h
		call	p__724
		bra		p__302

		ORG BASE_ADDR + 0x03C6
p__3C6:	clrf	0x2F						; entry from: 0FF2h,14EEh
		clrf	0x30
		movlw	3
		movwf	0x7A,BANKED
		movlw	0xE8
		movwf	0x7B,BANKED
		call	p__AE2
		movff	0x7D,0x44
		iorlw	0
		bz		p__3E0
		retlw	0xFF
p__3E0:	movwf	0x7A,BANKED				; entry from: 3DCh
		movlw	0x64
		movwf	0x7B,BANKED
		call	p__AE2
		movff	0x7D,0x30
		movff	0x44,0x2F
		clrf	0x7D,BANKED
		movlw	0xA
p__3F6:	incf	0x7D,f,BANKED			; entry from: 3FAh
		subwf	0x32
		bc		p__3F6
		addwf	0x32
		decf	0x7D,W,BANKED
		movwf	0x31
		retlw	0

p__404:	movwf	0x3F						; entry from: 1B42h,1B48h,1B5Eh,1DB2h
		tstfsz	0x3F
		bra		p__66A
		return	

p__40C:	btfss	0x72,1,BANKED			; entry from: 18ACh,18E8h,1E4Ch,1E5Ah,1EFAh,1F04h,2DA6h,2DC4h,2DC8h,2F82h,2F98h,2FA0h,2FB0h,2FFEh,3012h,302Ch,3038h,305Eh,3062h
		btfss	PIR1,5
		bra		p__476
		btfsc	0x72,0,BANKED
		bra		p__478
		clrf	0x20
		bcf		LATB,4
		movf	RCSTA1,W
		andlw	6
		bnz		p__47A
		movff	FSR0L,0x41
		movff	0x74,FSR0L
		movf	RCREG1,W
		movwf	INDF0
		btfss	0x17,2
		bra		p__472
		movwf	POSTINC1
		bcf		FSR1H,0
		movf	FSR1L,W
		xorwf	FSR2L,W
		bz		p__470
p__43A:	movf	0x8B,W,BANKED			; entry from: 474h
		xorwf	INDF0,W
		bz		p__484
		movlw	0x20
		cpfsgt	INDF0
		bra		p__48A
		btfss	0x72,7,BANKED
		bra		p__45A
		movlw	0x71
		cpfslt	FSR0L
		bra		p__468
		incf	FSR0L,W
		movwf	0x74,BANKED
		movff	0x41,FSR0L
		retlw	0
p__45A:	movlw	0xB0					; entry from: 448h
		movwf	0x72,BANKED
		incf	FSR0L,W
		movwf	0x74,BANKED
		movff	0x41,FSR0L
		retlw	0
p__468:	bsf		0x72,6,BANKED			; entry from: 44Eh
		movff	0x41,FSR0L
		retlw	0
p__470:	bra		p__7FC					; entry from: 438h
p__472:	bra		p__474					; entry from: 42Eh
p__474:	bra		p__43A					; entry from: 472h
p__476:	bra		p__478					; entry from: 410h

p__478:	bra		p__652					; entry from: 414h,476h
p__47A:	bcf		RCSTA1,4				; entry from: 41Eh
		movf	RCREG1,W
		bra		p__480
p__480:	bsf		RCSTA1,4				; entry from: 47Eh
		bra		p__5D4
p__484:	bsf		0x72,0,BANKED			; entry from: 43Eh
		btfss	0x1C,7
		bsf		LATC,5
p__48A:	movff	0x41,FSR0L				; entry from: 444h
		goto	p__B46
p__492:	call	p__B28					; entry from: 16FAh
		btfsc	PORTB,7
		return	
		movlw	0x68
		movwf	TRISB
		movlw	0xFC
		movwf	LATB
		call	p__B3E
		btfsc	PORTB,7
		return	
		movlw	0xE8
		movwf	TRISB
		setf	0xCD,BANKED
		movlw	0x30
		movwf	0x41
		movlw	0xC
		iorlw	1
		movwf	EEADR
		bsf		EECON1,2
p__4BC:	clrf	0x42						; entry from: 4E4h
		clrf	0x43
		setf	EEDATA
		movlw	0x55
		movwf	EECON2
		movlw	0xAA
		movwf	EECON2
		bsf		EECON1,1

p__4CC:	decfsz	0x43						; entry from: 4CEh,4DCh
		bra		p__4CC
		decfsz	0x42
		bra		p__4DA
p__4D4:	movlw	0x81						; entry from: 0A14h
		movwf	0xD1,BANKED
#if WDT_RESET
		goto	p_reset
#else
		reset
#endif
p__4DA:	btfsc	EECON1,1					; entry from: 4D2h
		bra		p__4CC
		incf	EEADR
		incf	EEADR
		decfsz	0x41
		bra		p__4BC
		bcf		EECON1,2
		movlw	0x3C
		movwf	0x41
p__4EC:	movlw	0xE8					; entry from: 512h
		movwf	TRISB
		dcfsnz	0x41
		bra		p__526
		movlw	0xEC
		movwf	LATB
		call	p__B2A
		movlw	0xFC
		movwf	LATB
		call	p__B26
		movlw	0x68
		movwf	TRISB
		movlw	0xFC
		movwf	LATB
		call	p__B3E
		btfss	PORTB,7
		bra		p__4EC
		movlw	0xE8
		movwf	TRISB
		movlw	8
p__51A:	movwf	0x41						; entry from: 528h
p__51C:	call	p__B2A					; entry from: 522h
		decfsz	0x41
		bra		p__51C
		return	
p__526:	movlw	0x64						; entry from: 4F2h
		bra		p__51A

p__52A:	movwf	0x42						; entry from: 13E2h,19AEh
		movlw	0x47
		cpfslt	0x42
		retlw	0xFF
		movlw	0x40
		cpfsgt	0x42
		bra		p__53C
		movlw	0x37
		bra		p__54A
p__53C:	movlw	0x3A						; entry from: 536h
		cpfslt	0x42
		retlw	0xFF
		movlw	0x2F
		cpfsgt	0x42
		retlw	0xFF
		movlw	0x30
p__54A:	subwf	0x42,W					; entry from: 53Ah
		return	

p__54E:	nop								; entry from: 2522h,298Ch,2ECCh
		btfss	0x1B,7
		bra		p__5BA
		movlw	3
		cpfsgt	0
		bra		p__57E
		btfsc	0x11,4
		bra		p__57A
		movf	0x15,W
		cpfseq	2
		bra		p__56C
		movlw	0x80
		btfsc	0x11,6
		movlw	0x10
		bra		p__5CC
p__56C:	xorwf	3,W						; entry from: 562h
		movlw	0x10
		btfsc	STATUS,2
		btfss	0x11,5
		movlw	0x80
		movwf	0x1B
		bra		p__5D2
p__57A:	movlw	0x10						; entry from: 55Ch
		bra		p__5C6
p__57E:	btfss	0x17,1					; entry from: 558h
		bra		p__5C2
		cpfslt	0
		bra		p__5A8
		btfss	0,1
		bra		p__59A
		movf	0x15,W
		xorwf	2,W
		movlw	0x60
		btfsc	STATUS,2
		btfss	0x11,6
		movlw	0x80
		movwf	0x1B
		return	
p__59A:	decfsz	0,W						; entry from: 588h
		bra		p__5B2
		movlw	0x80
		btfsc	0x11,4
		movlw	0x10
		movwf	0x1B
		return	
p__5A8:	movf	0x15,W					; entry from: 584h
		xorwf	3,W
		movlw	0x70
		btfsc	STATUS,2
		btfss	0x11,5
p__5B2:	movlw	0x80						; entry from: 59Ch
		movwf	0x1B
		nop
		return	
p__5BA:	movf	0x1B,W					; entry from: 552h
		call	p__B48
		bra		p__5C4
p__5C2:	movlw	0x80						; entry from: 580h
p__5C4:	nop								; entry from: 5C0h
p__5C6:	nop								; entry from: 57Ch
		bra		p__5CA
p__5CA:	nop								; entry from: 5C8h
p__5CC:	nop								; entry from: 56Ah
		bra		p__5D0
p__5D0:	movwf	0x1B						; entry from: 5CEh
p__5D2:	return							; entry from: 578h

p__5D4:	tstfsz	0x1A						; entry from: 482h,656h,760h,9AAh,0A10h,0B1Eh,0B4Ah,0B4Ch,0B4Eh,0B50h,0B52h,0B54h,0B56h,0B58h,0B6Eh,0B9Ch,0BE2h,0CBCh,0D14h,0D30h,0D34h,0E86h,1034h,1846h,1984h,1B50h,1B66h,1C96h,1D56h,1D82h,227Ah,22F6h,232Ah,2386h,23E2h,23F0h,23FEh,2414h,24D6h,24DAh,24FAh,251Ah,2556h,2566h,2576h,2586h,25D4h,25D8h,25F0h,25F4h,2716h,27ACh,27B8h,27C8h,2802h,2824h,2844h,28E4h,2914h,291Ch,2924h,2944h,2A94h,2AB8h,2ACAh,2C3Eh,2C66h,2C72h,2DE4h,2E04h,2E1Ah,2EACh,2EFCh,2F18h,2F34h,2F50h,2FBCh,303Ch,3120h,35ACh,3CFCh,3D1Eh,3E26h
		btfsc	TMR0L,7
		bra		p__61E
		btfsc	0x1A,7
		bra		p__5FE
		incf	0x1D
		btfsc	0x1D,3
		bsf		LATB,7
		incf	0x1E
		btfsc	0x1E,3
		bsf		LATB,6
		incf	0x1F
		btfsc	0x1F,3
		bsf		LATB,5
		incf	0x20
		btfsc	0x20,3
		bsf		LATB,4
		btfss	PORTC,4
		bsf		0x19,7
		clrf	0x1A
		return	
p__5FE:	decfsz	0x2B						; entry from: 5DCh
		bra		p__61C
		movlw	5
		movwf	0x2B
		incf	0x2A
p__608:	movf	0x86,W,BANKED			; entry from: 61Ch
		cpfslt	0x2A
		bsf		0x2C,6
		incf	0x21
		movf	0x7F,W,BANKED
		cpfslt	0x21
		bsf		0xF,1
		bsf		0x19,5
		bcf		0x1A,7
		return	
p__61C:	bra		p__608					; entry from: 600h
p__61E:	btfsc	PORTC,4					; entry from: 5D8h
		btfsc	PIR1,5
		bsf		0x19,7
		btfsc	TMR0L,7
		tstfsz	0x1A
		bra		p__63E
		setf	0x1A
		tstfsz	0x3C
		decf	0x3C
		nop
		bra		p__634

p__634:	nop								; entry from: 632h,644h
		btfsc	PORTC,4
		btfsc	PIR1,5
		bsf		0x19,7
		return	

p__63E:	movf	FSR1L,W					; entry from: 628h,1D8Ch,1F78h,1F88h,1FA6h,1FB6h,355Ch,357Ah,35DCh,363Eh,36B4h,3728h,3756h,37B4h,37C6h,3802h,3822h,386Ch,3888h,38AEh,38BCh,38D0h,3906h,392Ah,3942h,395Ah,3980h,3990h,399Eh,39B2h,39BCh,39EEh,3A6Ch,3AACh,3AB6h,3ADEh,3B5Eh
		cpfseq	FSR2L
		btfss	PIR1,4
		bra		p__634
		clrf	0x1F
		bcf		LATB,5
		movf	POSTINC2,W
		bcf		FSR2H,0
		movwf	TXREG1
		return	

p__652:	call	p__B46					; entry from: 478h,2C34h,2C50h,31ACh
		bra		p__5D4

p__658:	rcall	p__65A					; entry from: 0D0Ah,1230h,1762h,1822h,187Eh,198Eh

p__65A:	movf	RCSTA1,W				; entry from: 658h,0D20h
		andlw	6
		btfss	STATUS,2
		bcf		RCSTA1,4
		btfss	STATUS,2
		bsf		RCSTA1,4
		movf	RCREG1,W
		return	

p__66A:	clrf	STKPTR					; entry from: 408h,0CD4h,1C20h,1D14h,1D98h,1E72h
		movlw	1
		cpfseq	0x3F
		bra		p__676
		goto	p_1834
p__676:	bcf		0x19,4					; entry from: 670h
		movlw	2
		cpfseq	0x3F
		bra		p__682
		movlw	0x9A
		bra		p__6C2
p__682:	movlw	3						; entry from: 67Ch
		cpfseq	0x3F
		bra		p__68C
		movlw	0x90
		bra		p__6C2
p__68C:	movlw	4						; entry from: 686h
		cpfseq	0x3F
		bra		p__696
		movlw	0x4C
		bra		p__6C2
p__696:	movlw	5						; entry from: 690h
		cpfseq	0x3F
		bra		p__6A0
		movlw	0x56
		bra		p__6C2
p__6A0:	movlw	6						; entry from: 69Ah
		cpfseq	0x3F
		bra		p__6AA
		movlw	0x6C
		bra		p__6C2
p__6AA:	movlw	0xFF					; entry from: 6A4h
		cpfseq	0x3F
		bra		p__6B4
p__6B0:	movlw	0xDC					; entry from: 6CAh
		bra		p__6C2
p__6B4:	movlw	8						; entry from: 6AEh
		cpfseq	0x3F
		bra		p__6C6
		btfsc	0x11,7
		goto	p_1834
		movlw	0xAC

p__6C2:	goto	p_182C					; entry from: 680h,68Ah,694h,69Eh,6A8h,6B2h
p__6C6:	movlw	0x74						; entry from: 6B8h
		cpfseq	0x3F
		bra		p__6B0

p__6CC:	movwf	0x3F						; entry from: 10h,9BAh,1238h,17ECh
		clrf	STKPTR
		movff	FSR1L,FSR2L
		bcf		LATB,5
		rcall	p__706
		movf	0x8B,W,BANKED
		rcall	p__704
		movf	0x8A,W,BANKED
		btfsc	0x17,7
		rcall	p__704
		movlw	0x45
		rcall	p__704
		movlw	0x52
		rcall	p__704
		movlw	0x52
		rcall	p__704
#if DATA_OFFSET == 0
		clrf	TBLPTRH
#else
		movlw	high(DATA_OFFSET) + 0
		movwf	TBLPTRH
#endif
		clrf	TBLPTRU
		swapf	0x3F,W
		rcall	p__712
		rcall	p__704
		movf	0x3F,W
		rcall	p__712
		rcall	p__704
		bsf		LATB,5
		goto	p_1830

p__704:	movwf	TXREG1					; entry from: 6DAh,6E0h,6E4h,6E8h,6ECh,6F6h,6FCh
p__706:	clrf	0x41						; entry from: 6D6h
p__708:	call	p__B3E					; entry from: 70Eh
		decfsz	0x41
		bra		p__708
		return	

p__712:	iorlw	0xF0					; entry from: 6F4h,6FAh,71Ch,3894h
		movwf	TBLPTRL
		tblrd*
		movf	TABLAT,W
		return	

p__71C:	rcall	p__712					; entry from: 1002h,1008h,100Eh,102Ah,1412h,1418h,14FAh,1500h,150Ch,38AAh,38DAh,3926h,3938h,393Eh,3948h,3956h,3960h,3974h,39A4h,3AA8h
		bra		p__7F2

p__720:	movlw	0x3A						; entry from: 836h,141Ch,3964h
		bra		p__7F2

p__724:	movf	0x8B,W,BANKED			; entry from: 3C0h,826h,848h,1030h,1206h,1450h,17F0h,17F4h,1810h,1814h,181Eh,1830h,1842h,1912h,191Ch,1920h,1E6Ah,1FBAh,24F0h,293Ah,2D2Eh,36A0h,38B8h,3ABAh
		rcall	p__7F2
		btfss	0x17,7
		return	
		movf	0x8A,W,BANKED
		bra		p__7F2

p__730:	movwf	TBLPTRL					; entry from: 846h,0D04h,0F4Eh,0F94h,0F9Eh,0FA4h,11FEh,1218h,180Ch,181Ah,182Ch,1888h,1918h,1FB2h,24ECh,2936h,2BCAh,2D2Ah,30ECh,369Ch

p__732:	tblrd*+							; entry from: 73Eh,74Ah
		movf	TABLAT,W
		btfsc	STATUS,2
		return	
		rcall	p__7F2
		btfss	PIR1,4
		bra		p__732
		clrf	0x1F
		bcf		LATB,5
		movf	POSTINC2,W
		bcf		FSR2H,0
		movwf	TXREG1
		bra		p__732

p__74C:	btfss	0x18,0					; entry from: 0CB8h,0E82h,0E92h,11C8h,11D4h,1426h
		bra		p__7AA
		rcall	p__7AA
		bra		p__7F0
p__754:	call	p__B44					; entry from: 7B0h

p__758:	bra		p__75A					; entry from: 770h,7BCh

p__75A:	nop								; entry from: 758h,790h
p__75C:	call	p__B44					; entry from: 7C2h
		bra		p__5D4
p__762:	movf	FSR1L,W					; entry from: 7EEh
		xorwf	FSR2L,W
		bz		p__7FC
		nop
		return	
p__76C:	bcf		0x1B,6					; entry from: 7B4h
		btfsc	0x72,3,BANKED
		bra		p__758
		swapf	1,W
		iorlw	0xF0
		movwf	TBLPTRL
		tblrd*
		movf	TABLAT,W
		movwf	POSTINC1
		bcf		FSR1H,0
		movf	FSR1L,W
		xorwf	FSR2L,W
		bz		p__7FC
		nop
		movf	1,W
		bra		p__7DA
p__78C:	bcf		0x1B,5					; entry from: 7B8h
		btfsc	0x72,3,BANKED
		bra		p__75A
		swapf	2,W
		iorlw	0xF0
		movwf	TBLPTRL
		tblrd*
		movf	TABLAT,W
		movwf	POSTINC1
		bcf		FSR1H,0
		movf	FSR1L,W
		xorwf	FSR2L,W
		bz		p__7FC
		movf	2,W
		bra		p__7DC

p__7AA:	movwf	0xE						; entry from: 74Eh,750h,1474h,382Eh,38B4h,397Ch,3986h,398Ch,3996h,39B8h,3AB2h
		bra		p__7BE

p__7AE:	btfsc	0x1B,7					; entry from: 2532h,253Eh,254Ah,29AEh,29B2h,29D4h,2AF8h,2AFCh,2B02h,2F30h
		bra		p__754
		btfsc	0x1B,6
		bra		p__76C
		btfsc	0x1B,5
		bra		p__78C
		btfss	0x1B,4
		bra		p__758

p__7BE:	bcf		0x1B,4					; entry from: 7ACh,1F94h
		btfsc	0x72,3,BANKED
		bra		p__75C
		swapf	0xE,W
		iorlw	0xF0
		movwf	TBLPTRL
		tblrd*
		movf	TABLAT,W
		movwf	POSTINC1
		bcf		FSR1H,0
		movf	FSR1L,W
		xorwf	FSR2L,W
		bz		p__7FC
		movf	0xE,W
p__7DA:	nop								; entry from: 78Ah
p__7DC:	iorlw	0xF0					; entry from: 7A8h
		movwf	TBLPTRL
		tblrd*
		movf	TABLAT,W
		movwf	POSTINC1
		bcf		FSR1H,0
		movf	FSR1L,W
		cpfseq	FSR2L
		btfsc	0x18,0
		bra		p__762

p__7F0:	movlw	0x20						; entry from: 752h,830h,0F58h,11E2h,1446h,144Ah

p__7F2:	movwf	POSTINC1				; entry from: 71Eh,722h,726h,72Eh,73Ah,834h,0C4Ch,0E7Ch,0F54h,0FB4h,0FBEh,0FC4h,0FCAh,1014h,1024h,118Ch,1196h,119Eh,11A4h,120Ch,1212h,143Ah,1506h,1512h,1520h,152Ah,1926h,197Ch,2C0Ch,37FEh,3808h,380Eh,38C6h
		bcf		FSR1H,0
		movf	FSR1L,W
		cpfseq	FSR2L
		return	

p__7FC:	clrf	STKPTR					; entry from: 470h,766h,784h,7A4h,7D6h
		movlw	0xFC
		andwf	LATB
		decf	FSR1L
		movlw	2
		movwf	FSR1H
		btfss	0x3E,0
		bra		p__818
		movf	CANSTAT,W
		andlw	0xE0
		xorlw	0x60
		btfsc	STATUS,2
		call	p_3CCE
p__818:	call	p_1034					; entry from: 80Ah
		movf	INDF1,W
		xorwf	0x8B,W,BANKED
		bz		p__826
		incf	FSR1L
		bcf		FSR1H,0
p__826:	rcall	p__724					; entry from: 820h
		movlw	0x40
		goto	p_182C

p__82E:	btfss	0x18,0					; entry from: 3812h,389Ch,38D4h,38DEh,392Eh,3968h,39A8h
		bra		p__7F0
		return	

p__834:	rcall	p__7F2					; entry from: 0E70h,0E8Ch,11BEh,11CEh,11E8h,3898h,38CCh
		bra		p__720

; read eeprom
p__838:	movwf	EEADR						; entry from: 98Eh,99Ah,0C4Ah,0C62h,1382h,13A4h,1422h,142Eh,1470h
		bcf		EECON1,7
		bcf		EECON1,6
		bsf		EECON1,0
		movf	EEDATA,W
		return	

p__844:	movlw	0xBA					; entry from: 1C46h,1DBEh,2D02h,2D52h
		rcall	p__730
		bra		p__724

p__84A:	rcall	p__8EA					; entry from: 1C4Ch,1D76h
		btfsc	STATUS,2
		addlw	1
		movwf	0xC3,BANKED
		movlw	0xC
		cpfslt	0xC3,BANKED
		movwf	0xC3,BANKED
		btfsc	0xD2,7,BANKED
		btfss	0x33,5
		bra		p__866
p__85E:	movlw	5						; entry from: 86Ch
		cpfslt	0xC3,BANKED
		movwf	0xC3,BANKED
		return	
p__866:	btfsc	0xD2,6,BANKED			; entry from: 85Ch
		btfss	0x33,4
		return	
		bra		p__85E

p__86E:	clrf	0x3D						; entry from: 9CCh,0AA2h,0AB2h
		clrf	EEADR
		bcf		EECON1,7
		bcf		EECON1,6
		movlw	8
		movwf	0x41
p__87A:	rrncf	0x3D						; entry from: 886h
		bsf		EECON1,0
		tstfsz	EEDATA
		bsf		0x3D,7
		incf	EEADR
		decfsz	0x41
		bra		p__87A
		bcf		0x3E,7
		btfsc	0x3D,7
		bsf		0x3E,7
		bcf		0x3D,7
		bsf		EECON1,0
		movff	EEDATA,0xC8
		incf	EEADR
		nop
		bsf		EECON1,0
		movff	EEDATA,0xC9
		incf	EEADR
		nop
		bsf		EECON1,0
		movff	EEDATA,0xCA
		incf	EEADR
		nop
		bsf		EECON1,0
		movff	EEDATA,0xCB
		movlw	0xC
		cpfsgt	0x3D
		return	
		clrf	0x3D
		bsf		0x3E,7
		return	
p__8C0:	setf	0xCD,BANKED				; entry from: 1804h
		movlw	0xC
		bra		p__98C
p__8C6:	setf	0xCD,BANKED				; entry from: 0A32h
		movlw	0xE
		bra		p__98C
p__8CC:	setf	0xCD,BANKED				; entry from: 0A24h
		movlw	0x10
		bra		p__98C
p__8D2:	movlw	0x32						; entry from: 0AC4h
		movwf	0xCD,BANKED
		movlw	0x12
		bra		p__98C
p__8DA:	movlw	1						; entry from: 0A62h
		movwf	0xCD,BANKED
		movlw	0x14
		bra		p__98C
p__8E2:	movlw	0xF1					; entry from: 1768h
		movwf	0xCD,BANKED
		movlw	0x18
		bra		p__98C
p__8EA:	movlw	9						; entry from: 84Ah
		movwf	0xCD,BANKED
		movlw	0x1A
		bra		p__98C
p__8F2:	clrf	0xCD,BANKED				; entry from: 176Eh
		movlw	0x1E
		bra		p__98C
p__8F8:	movlw	0xA						; entry from: 1776h
		movwf	0xCD,BANKED
		movlw	0x20
		bra		p__98C
p__900:	movlw	DEFAULT_BAUD					; entry from: 1740h
		movwf	0xCD,BANKED
		movlw	0x24
		bra		p__98C
p__908:	movlw	0xD						; entry from: 177Ch
		movwf	0xCD,BANKED
		movlw	0x26
		bra		p__98C
p__910:	clrf	0xCD,BANKED				; entry from: 0A4Ch
		movlw	0x2E
		bra		p__98C

p__916:	movlw	0x32						; entry from: 1C78h,1CECh
		movwf	0xCD,BANKED
		movlw	0x32
		bra		p__98C
p__91E:	movlw	0xA						; entry from: 0ACCh
		movwf	0xCD,BANKED
		movlw	0x36
		bra		p__98C
p__926:	setf	0xCD,BANKED				; entry from: 1782h
		movlw	0x38
		bra		p__98C
p__92C:	movlw	0x92						; entry from: 0AD2h
		movwf	0xCD,BANKED
		movlw	0x3A
		bra		p__98C

p__934:	clrf	0xCD,BANKED				; entry from: 1C8Ah,1CFAh
		movlw	0x3C
		bra		p__98C
p__93A:	movlw	0x28						; entry from: 1D4Ah
		movwf	0xCD,BANKED
		movlw	0x3E
		bra		p__98C
p__942:	clrf	0xCD,BANKED				; entry from: 0A3Ah
		movlw	0x54
		bra		p__98C
p__948:	clrf	0xCD,BANKED				; entry from: 0A44h
		movlw	0x56
		bra		p__98C
p__94E:	movlw	0						; entry from: 0A94h
		movwf	0xCD,BANKED
		movlw	0x58
		bra		p__98C
p__956:	setf	0xCD,BANKED				; entry from: 0A54h
		movlw	0x5E
		bra		p__98C
p__95C:	movlw	0x38						; entry from: 0A5Ch
		movwf	0xCD,BANKED
		movlw	0x60
		bra		p__98C
p__964:	movlw	2						; entry from: 178Ah
		movwf	0xCD,BANKED
		movlw	0x62
		bra		p__98C
p__96C:	movlw	0xE0					; entry from: 179Ah
		movwf	0xCD,BANKED
		movlw	0x64
		bra		p__98C
p__974:	movlw	4						; entry from: 17A0h
		movwf	0xCD,BANKED
		movlw	0x66
		bra		p__98C
p__97C:	movlw	0x80						; entry from: 17ACh
		movwf	0xCD,BANKED
		movlw	0x68
		bra		p__98C
p__984:	movlw	0xA						; entry from: 17B2h
		movwf	0xCD,BANKED
		movlw	0x6A
		bra		p__98C

p__98C:	iorlw	1						; entry from: 8C4h,8CAh,8D0h,8D8h,8E0h,8E8h,8F0h,8F6h,8FEh,906h,90Eh,914h,91Ch,924h,92Ah,932h,938h,940h,946h,94Ch,954h,95Ah,962h,96Ah,972h,97Ah,982h,98Ah,1680h,16BEh,170Eh
		call	p__838
		bz		p__998
		movf	0xCD,W,BANKED
		return	
p__998:	decf	EEADR,W					; entry from: 992h
		goto	p__838

p__99E:	movlw	1						; entry from: 0ECCh,148Ah,4038h
		movwf	ADCON0
		call	p__B4A
		bsf		ADCON0,1
		clrf	0x41
p__9AA:	rcall	p__5D4					; entry from: 9B2h
		btfss	ADCON0,1
		bra		p__9BE
		decfsz	0x41
		bra		p__9AA
		movlw	0
		movwf	ADCON0
		movlw	0x76
		goto	p__6CC
p__9BE:	goto	p_403E					; entry from: 9AEh
		nop
p__9C4:	movff	0x3D,0x44					; entry from: 184Ch
		btfsc	0x3E,7
		bsf		0x44,7
		call	p__86E
		btfsc	0x3E,7
		bsf		0x3D,7
		movf	0x3D,W
		xorwf	0x44,W
		bz		p__9FC
		bsf		EECON1,2
		clrf	EEADR
		movlw	8
		movwf	0x41
		movlw	0xFF
p__9E4:	btfss	0x44,0					; entry from: 9EEh
		movlw	0
		rcall	p__A00
		rrncf	0x44
		decfsz	0x41
		bra		p__9E4
		bcf		EECON1,2
		bcf		0x3E,7
		btfsc	0x44,7
		bsf		0x3E,7
		movff	0x44,0x3D
p__9FC:	bcf		0x3D,7					; entry from: 9D8h
		return	

; write eeprom
p__A00:	movwf	EEDATA						; entry from: 9E8h,0C88h,0C94h,0C96h,0C98h,0C9Ah,0F08h,0F0Eh,0F14h,0F1Ah,1390h,13AEh
		movlw	0x55
		movwf	EECON2
		movlw	0xAA
		movwf	EECON2
		bsf		EECON1,1
		movlw	0xC
		rcall	p__B12
p__A10:	rcall	p__5D4					; entry from: 0A18h
		btfsc	0xF,1
		bra		p__4D4
		btfsc	EECON1,1
		bra		p__A10
		incf	EEADR
		retlw	0xFF

p__A1E:	movf	0x8C,W,BANKED			; entry from: 3B6h,3BCh,17D2h
		andlw	0xE0
		movwf	0x17
		call	p__8CC
		btfsc	STATUS,2
		bsf		0x17,4
		bsf		0x17,3
		btfsc	0x8D,6,BANKED
		bsf		0x17,2
		call	p__8C6
		btfsc	STATUS,2
		bsf		0x17,1
		call	p__942
		btfsc	STATUS,2
		bsf		0x17,0
		clrf	0x18
		call	p__948
		btfsc	STATUS,2
		bsf		0x18,7
		call	p__910
		btfsc	STATUS,2
		bsf		0x18,6
		call	p__956
		btfsc	STATUS,2
		bsf		0x18,5
		call	p__95C
		movwf	0x33
		call	p__8DA
		movwf	0x41
		decf	0x41
		bz		p__A72
		decfsz	0x41
		bra		p__A74
		bsf		0x18,3
p__A72:	bsf		0x18,4					; entry from: 0A6Ah
p__A74:	clrf	0x3F						; entry from: 0A6Eh
		clrf	0x83,BANKED
		movlw	0xF
		movwf	0xCE,BANKED
		clrf	0x19
		clrf	0x1B
		bsf		0x1B,7
		clrf	0x11
		clrf	0x2D
		btfsc	0x8D,7,BANKED
		bsf		0x2D,1
		movlw	0x33
		movwf	0xBF,BANKED
		clrf	0x34
		clrf	0x35
		clrf	0xA6,BANKED
		call	p__94E
		movwf	0x95,BANKED
		btfss	0x3E,6
		bra		p__AB0
		movff	0x3D,0x44
		call	p__86E
		movf	0x44,W
		xorwf	0x3D,W
		bz		p__ABA
		call	p_1FEA
p__AB0:	clrf	0x3E						; entry from: 0A9Ch
		call	p__86E
		call	p_26DE
p__ABA:	clrf	0xF						; entry from: 0AAAh
		clrf	0x10
		bcf		0x2C,5
		call	p_1FFE
		call	p__8D2
		movwf	0x80,BANKED
		movwf	0x7E,BANKED
		call	p__91E
		movwf	0xC2,BANKED
		call	p__92C
		movwf	0x86,BANKED
		return	

p__ADA:	movwf	0x3F						; entry from: 1F00h,1F84h,229Eh,22A6h,22BAh,273Ah,2742h,2756h,2BD6h,2C8Ah,2C9Ch,2CAEh,2CC8h,2CF6h,2D46h,2DACh,2DB8h,30FEh,3110h,3156h,3164h,32F0h,32FAh,3336h,333Ch,3350h,335Eh,349Eh,34ACh,3594h,3AC8h,3B22h,3D70h
		tstfsz	0x3F
		pop
		return	

p__AE2:	clrf	0x7C,BANKED				; entry from: 3D2h,3E6h,0FE6h,14D4h
		clrf	0x7D,BANKED
p__AE6:	movf	0x7B,W,BANKED			; entry from: 0B00h
		subwf	0x32
		movf	0x7A,W,BANKED
		subwfb	0x31
		movlw	0
		subwfb	0x30
		subwfb	0x2F
		bnc		p__B02
		incf	0x7D,f,BANKED
		btfsc	STATUS,0
		incf	0x7C,f,BANKED
		btfsc	STATUS,0
		retlw	0xFF
		bra		p__AE6
p__B02:	movf	0x7B,W,BANKED			; entry from: 0AF4h
		addwf	0x32
		movf	0x7A,W,BANKED
		addwfc	0x31
		movlw	0
		addwfc	0x30
		addwfc	0x2F
		retlw	0

p__B12:	movwf	0x7F,BANKED				; entry from: 0A0Eh,0B1Ch,18A2h,1C92h,230Eh,2792h,2F74h
		clrf	0x21
		bcf		0xF,1
		return	

p__B1A:	movlw	0xF6					; entry from: 121Eh,1222h,1292h
		rcall	p__B12
p__B1E:	rcall	p__5D4					; entry from: 0B22h
		btfss	0xF,1
		bra		p__B1E
		return	

p__B26:	rcall	p__B28					; entry from: 500h,1720h

p__B28:	rcall	p__B2A					; entry from: 492h,0B26h

p__B2A:	clrf	0x42						; entry from: 4F8h,51Ch,0B28h,1732h
		clrf	0x43

p__B2E:	decfsz	0x42						; entry from: 0B30h,0B34h
		bra		p__B2E
		decfsz	0x43
		bra		p__B2E
		return	
p__B38:	bra		p__B42					; entry from: 2F44h

p__B3A:	rcall	p__B44					; entry from: 252Ch,2538h,2544h,2550h,2560h,2570h,2580h,2C1Eh,2C22h,2F10h,2F3Ch,3124h,3142h

p__B3C:	bra		p__B40					; entry from: 28B6h,296Eh,2BF4h,2EC4h,2F68h

p__B3E:	rcall	p__B40					; entry from: 4A2h,50Ch,708h,2516h,2526h,255Ah,256Ah,257Ah,2972h,2994h,29B6h,29F6h,2A14h,2A32h,2A50h

p__B40:	bra		p__B46					; entry from: 0B3Ch,0B3Eh,237Eh,289Eh,29BAh

p__B42:	rcall	p__B46					; entry from: 0B38h,238Eh,282Eh,2F06h

p__B44:	bra		p__B48					; entry from: 754h,75Ch,0B3Ah,2402h,25BAh,2AD2h,3016h

p__B46:	rcall	p__B48					; entry from: 48Eh,652h,0B40h,0B42h,238Ah,23F8h,2968h,3138h

p__B48:	return							; entry from: 5BCh,0B44h,0B46h,240Ch,25B4h,2836h,28C8h,2ABCh,2AE8h,301Eh,3034h

p__B4A:	rcall	p__5D4					; entry from: 9A2h,1956h
		rcall	p__5D4

p__B4E:	rcall	p__5D4					; entry from: 23C8h,2866h

p__B50:	rcall	p__5D4					; entry from: 2880h,2888h,2892h,289Ah,28AEh,28B2h,28C0h,28C4h,2AE0h,2AE4h,3146h

p__B52:	rcall	p__5D4					; entry from: 27F2h,29F2h,2A10h,2A2Eh,2A4Ch,3134h

p__B54:	rcall	p__5D4					; entry from: 2964h,2990h,29D8h
		rcall	p__5D4
		bra		p__5D4
p__B5A:	nop								; entry from: 2374h

p__B5C:	addlw	0xFF					; entry from: 0B60h,2364h
		btfss	STATUS,2
		bra		p__B5C
		return	

p__B64:	clrf	0x90,BANKED				; entry from: 1C54h,1DC6h
		clrf	0x8F,BANKED
		movlw	0x46
		movwf	0x41
		clrf	0x42
p__B6E:	rcall	p__5D4					; entry from: 0B86h
p__B70:	movf	PORTB,W					; entry from: 0B82h
		andlw	8
		xorwf	0x8F,W,BANKED
		bz		p__B90
		xorwf	0x8F,f,BANKED
		infsnz	0x90,f,BANKED
		retlw	6
p__B7E:	nop								; entry from: 0B90h
		decfsz	0x42
		bra		p__B70
		decfsz	0x41
		bra		p__B6E
		movlw	0x20
		cpfsgt	0x90,BANKED
		bra		p__B92
		retlw	6
p__B90:	bra		p__B7E					; entry from: 0B76h
p__B92:	clrf	0x90,BANKED				; entry from: 0B8Ch
		clrf	0x92,BANKED
		movlw	0x67
		movwf	0x41
		clrf	0x42
p__B9C:	rcall	p__5D4					; entry from: 0BBEh
		setf	0x91,BANKED
p__BA0:	btfss	PORTC,0					; entry from: 0BBAh
		bra		p__BC8
		btfsc	0x92,0,BANKED
		bra		p__BCC
		infsnz	0x90,f,BANKED
		retlw	2
		movlw	0x6C
		btfsc	0x92,3,BANKED
		cpfslt	0x91,BANKED
		decf	0x90,f,BANKED
		bsf		0x92,0,BANKED
		clrf	0x91,BANKED
p__BB8:	decfsz	0x42						; entry from: 0BD6h
		bra		p__BA0
		decfsz	0x41
		bra		p__B9C
		movlw	0xA
		cpfsgt	0x90,BANKED
		bra		p__BD8
		retlw	2
p__BC8:	bcf		0x92,0,BANKED			; entry from: 0BA2h
		bcf		0x92,3,BANKED
p__BCC:	incfsz	0x91,W,BANKED			; entry from: 0BA6h
		incf	0x91,f,BANKED
		movlw	0xF
		cpfslt	0x91,BANKED
		bsf		0x92,3,BANKED
		bra		p__BB8
p__BD8:	clrf	0x90,BANKED				; entry from: 0BC4h
		clrf	0x92,BANKED
		movlw	0x67
		movwf	0x41
		clrf	0x42
p__BE2:	rcall	p__5D4					; entry from: 0C04h
		setf	0x91,BANKED
p__BE6:	btfsc	PORTC,2					; entry from: 0C00h
		bra		p__C0E
		btfss	0x92,2,BANKED
		bra		p__C12
		infsnz	0x90,f,BANKED
		retlw	1
		movlw	9
		btfsc	0x92,4,BANKED
		cpfslt	0x91,BANKED
		decf	0x90,f,BANKED
		bcf		0x92,2,BANKED
		clrf	0x91,BANKED
p__BFE:	decfsz	0x42						; entry from: 0C1Ch
		bra		p__BE6
		decfsz	0x41
		bra		p__BE2
		movlw	0x16
		cpfsgt	0x90,BANKED
		retlw	5
		retlw	1
p__C0E:	bsf		0x92,2,BANKED			; entry from: 0BE8h
		bcf		0x92,4,BANKED
p__C12:	incfsz	0x91,W,BANKED			; entry from: 0BECh
		incf	0x91,f,BANKED
		movlw	3
		cpfslt	0x91,BANKED
		bsf		0x92,4,BANKED
		bra		p__BFE
p__C1E:	movlw	0x31						; entry from: 204h
		cpfseq	0x65,BANKED
		bra		p__C2A
		bcf		0x18,3
p__C26:	bsf		0x18,4					; entry from: 0C32h
		bra		p__E9E
p__C2A:	movlw	0x32						; entry from: 0C22h
		cpfseq	0x65,BANKED
		bra		p__C34
		bsf		0x18,3
		bra		p__C26
p__C34:	tstfsz	0x78,BANKED				; entry from: 0C2Eh
		bra		p__EAC
		bcf		0x18,4
		bra		p__E9E
p__C3C:	movlw	0x24						; entry from: 1F0h
		goto	p_182C
p__C42:	rcall	p__C58					; entry from: 1F6h
		btfsc	STATUS,2
		bra		p__EAC
p__C48:	movf	0x42,W					; entry from: 0C54h
		rcall	p__838
		call	p__7F2
		incf	0x42
		decfsz	0x41
		bra		p__C48
		bra		p__E9A

p__C58:	movlw	0xC						; entry from: 0C42h,0C72h
		movwf	0x41
		movlw	0x6C
		movwf	0x42
p__C60:	movf	0x42,W					; entry from: 0C6Eh
		rcall	p__838
		btfsc	STATUS,2
		return	
		incf	0x42
		movlw	0x70
		cpfseq	0x42
		bra		p__C60
		return	
#if SW_VERSION != 0
p__C72:	movlw	0
		movwf	FSR0H
		movlw	0x65
		movwf	FSR0L
		movf	POSTINC0,W
		xorlw	"E"
		bz		eep_chk
		xorlw	"E"
		xorlw	"B"
		bnz		cmd_err
		movf	POSTINC0,W
		xorlw	"L"
		bnz		cmd_err
		reset
eep_chk:	movf	POSTINC0,W
		xorlw	"E"
		bnz		cmd_err
		call		eep_init
		bra		p__E9E	    ; print OK
cmd_err:	bra		p__EAC	    ; print ?
#else
p__C72:	rcall	p__C58					; entry from: 31Eh
		btfss	STATUS,2
		bra		p__EAC
		movlw	0
		movwf	FSR0H
		movlw	0x65
		movwf	FSR0L
		movlw	0x70
		movwf	EEADR
		bsf		EECON1,2
p__C86:	movf	POSTINC0,W				; entry from: 0C8Ch
		rcall	p__A00
		decfsz	0x41
		bra		p__C86
		movlw	0x6C
		movwf	EEADR
		movlw	0xFF
		rcall	p__A00
		rcall	p__A00
		rcall	p__A00
		rcall	p__A00
		bcf		EECON1,2
		bra		p__E9E
#endif
p__CA0:	bcf		0x10,4					; entry from: 11Eh
		bcf		0x34,6
		bcf		0x34,5
		bsf		0x34,4
		bra		p__E9E
p__CAA:	movlw	0						; entry from: 124h
		movwf	FSR0H
		movlw	0
		movwf	FSR0L
		movlw	0xD
		movwf	0x41
p__CB6:	movf	POSTINC0,W				; entry from: 0CC0h
		call	p__74C
		rcall	p__5D4
		decfsz	0x41
		bra		p__CB6
		bra		p__E9A
p__CC4:	movf	0x3D						; entry from: 12Ah
		btfsc	STATUS,2
		bra		p__EAC
		bsf		0x19,6
		call	p_1E76
		movwf	0x3F
		tstfsz	0x3F
		bra		p__66A
		bra		p__E9E
p__CD8:	call	p_113A					; entry from: 29Eh
		movwf	0x41
		movlw	0x54
		cpfseq	0x65,BANKED
		bra		p__CEA
		movff	0x41,0xCE
		bra		p__E9E
p__CEA:	movlw	0x44						; entry from: 0CE2h
		btfsc	0x72,4,BANKED
		cpfseq	0x65,BANKED
		bra		p__EAC
		movlw	7
		cpfsgt	0x41
		bra		p__EAC
		movff	0xF7D,0x6B
		movff	SPBRG1,0x6C
		setf	0x6D,BANKED
		movlw	0xB4
p__D04:	call	p__730					; entry from: 0D4Eh
		rcall	p_1030
		call	p__658
		movff	0xCE,0x43
		clrf	0x42

p__D14:	call	p__5D4					; entry from: 0D3Ah,0D3Eh
		btfss	PIR1,5
		bra		p__D30
		clrf	0x20
		bcf		LATB,4
		call	p__65A
		btfss	0x6D,0,BANKED
		cpfseq	0x8B,BANKED
		bra		p__D30
		movff	0x41,0xCF
		bra		p__E9E

p__D30:	call	p__5D4					; entry from: 0D1Ah,0D28h
		call	p__5D4
		decfsz	0x42
		bra		p__D14
		decfsz	0x43
		bra		p__D14
		btfss	0x6D,0,BANKED
		bra		p__D50
		clrf	SPBRGH1
		decf	0x41,W
		movwf	SPBRG1
		movlw	0x82
		clrf	0x6D,BANKED
		bra		p__D04
p__D50:	movff	0x6B,0xF7D				; entry from: 0D42h
		movff	0x6C,SPBRG1
		goto	p_1834
p__D5C:	movlw	0x46						; entry from: 242h
		cpfseq	0x65,BANKED
		bra		p__D70
		movlw	0x30
		cpfseq	0x66,BANKED
		bra		p__D6C
		bcf		0x17,0
		bra		p__E9E
p__D6C:	movlw	0x31						; entry from: 0D66h
		cpfseq	0x66,BANKED
p__D70:	bra		p__EAC					; entry from: 0D60h
		bsf		0x17,0
		bra		p__E9E
p__D76:	movlw	0x41						; entry from: 20Ah
		cpfseq	0x65,BANKED
		bra		p__EAC
		bcf		0x18,1
		bra		p__E9E
p__D80:	rcall	p__EA4					; entry from: 2A4h
		bcf		0x35,4
		clrf	0xA2,BANKED
		clrf	0xA3,BANKED
		rcall	p__E48
		andlw	7
		movwf	0xA4,BANKED
		movf	0x79,W,BANKED
		bra		p__DA4
p__D92:	rcall	p__EA4					; entry from: 308h
		bsf		0x35,4
		andlw	0x1F
		movwf	0xA2,BANKED
		movff	0x79,0xA3
		movff	0x7A,0xA4
		movf	0x7B,W,BANKED

p__DA4:	movwf	0xA5,BANKED				; entry from: 0D90h,0E22h
		bsf		0x34,5
		bsf		0x34,4
		bra		p__E9E
p__DAC:	movlw	0x43						; entry from: 248h
		cpfseq	0x65,BANKED
		bra		p__DC0
		movlw	0x30
		cpfseq	0x66,BANKED
		bra		p__DBC
		bcf		0x18,7
		bra		p__E9E
p__DBC:	movlw	0x31						; entry from: 0DB6h
		cpfseq	0x66,BANKED
p__DC0:	bra		p__EAC					; entry from: 0DB0h
		bsf		0x18,7
		bra		p__E9E
p__DC6:	rcall	p__EA4					; entry from: 2AAh
		bcf		0x35,4
		clrf	0x9E,BANKED
		clrf	0x9F,BANKED
		rcall	p__E48
		andlw	7
		movwf	0xA0,BANKED
		movf	0x79,W,BANKED
		bra		p__DEA
p__DD8:	rcall	p__EA4					; entry from: 30Eh
		bsf		0x35,4
		andlw	0x1F
		movwf	0x9E,BANKED
		movff	0x79,0x9F
		movff	0x7A,0xA0
		movf	0x7B,W,BANKED
p__DEA:	movwf	0xA1,BANKED				; entry from: 0DD6h
		bsf		0x34,6
		bsf		0x34,4
		bra		p__E9E
p__DF2:	rcall	p__EA4					; entry from: 24Eh
		movlw	0x1F
		andwf	0x78,f,BANKED
		movff	0x78,0x2E
		bsf		0x34,7
		bsf		0x34,4
		bra		p__E9E
p__E02:	rcall	p__EA4					; entry from: 2C4h
		andlw	0xF0
		xorlw	0xA0
		bnz		p__EAC
		bcf		0x35,4
		clrf	0x9E,BANKED
		clrf	0xA2,BANKED
		clrf	0x9F,BANKED
		clrf	0xA3,BANKED
		movlw	7
		movwf	0xA0,BANKED
		andwf	0x78,W,BANKED
		movwf	0xA4,BANKED
		setf	0xA1,BANKED
		movf	0x79,W,BANKED
p__E20:	bsf		0x34,6					; entry from: 0E46h
		bra		p__DA4
p__E24:	rcall	p__EA4					; entry from: 316h
		rcall	p__E48
		xorlw	0xA
		bnz		p__EAC
		bsf		0x35,4
		movlw	0x1F
		movwf	0x9E,BANKED
		setf	0x9F,BANKED
		setf	0xA0,BANKED
		setf	0xA1,BANKED
		andwf	0x79,W,BANKED
		movwf	0xA2,BANKED
		movff	0x7A,0xA3
		movff	0x7B,0xA4
		movf	0x7C,W,BANKED
		bra		p__E20

p__E48:	movlw	4						; entry from: 0D88h,0DCEh,0E26h,155Ch
		movwf	0x41
p__E4C:	bcf		STATUS,0				; entry from: 0E5Ah
		rrcf	0x78,f,BANKED
		rrcf	0x79,f,BANKED
		rrcf	0x7A,f,BANKED
		rrcf	0x7B,f,BANKED
		rrcf	0x7C,f,BANKED
		decfsz	0x41
		bra		p__E4C
		movf	0x78,W,BANKED
		return	
p__E60:	movf	CANSTAT,W					; entry from: 130h
		andlw	0xE0
		bnz		p__E6E
		movff	TXERRCNT,0xB1
		movff	RXERRCNT,0xB2
p__E6E:	movlw	0x54						; entry from: 0E64h
		call	p__834
		movf	0xB1,W,BANKED
		btfss	COMSTAT,5
		bra		p__E82
		movlw	0x4F
		call	p__7F2
		movlw	0xFF
p__E82:	call	p__74C					; entry from: 0E78h
		call	p__5D4
		movlw	0x52
		call	p__834
		movf	0xB2,W,BANKED
		call	p__74C
		clrf	0xB1,BANKED
		clrf	0xB2,BANKED

p__E9A:	goto	p_1830					; entry from: 0C56h,0CC2h,0F9Ah,1018h,102Eh

; print OK
p__E9E:	movlw	0xB4					; entry from: 302h,0C28h,0C3Ah,0C9Eh,0CA8h,0CD6h,0CE8h,0D2Eh,0D6Ah,0D74h,0D7Eh,0DAAh,0DBAh,0DC4h,0DF0h,0E00h,0F20h,1076h,109Eh,10B8h,1102h,1462h
		goto	p_182C

p__EA4:	movf	0x78,W,BANKED			; entry from: 0D80h,0D92h,0DC6h,0DD8h,0DF2h,0E02h,0E24h,0EB2h,1466h
		btfsc	0x72,4,BANKED
		return	
		pop

; print ?
p__EAC:	movlw	0x8E						; entry from: 0C36h,0C46h,0C76h,0CC8h,0CF0h,0CF6h,0D70h,0D7Ah,0DC0h,0E08h,0E2Ah,0F26h,0F38h,146Ah,1AFAh
		goto	p_182C
p__EB2:	rcall	p__EA4					; entry from: 2CAh
		bnz		p__ECC
		tstfsz	0x79,BANKED
		bra		p__ECC
		movlw	6
		movwf	0xC8,BANKED
		movlw	0xAE
		movwf	0xC9,BANKED
		movlw	2
		movwf	0xCA,BANKED
		movlw	0x6A
		movwf	0xCB,BANKED
		bra		p__F00

p__ECC:	call	p__99E					; entry from: 0EB4h,0EB8h
		movff	ADRESH,0xCA
		movff	ADRESL,0xCB
		swapf	0x79,W,BANKED
		andlw	0xF
		mullw	0xA
		movlw	0xF
		andwf	0x79,W,BANKED
		addwf	PRODL,W
		movwf	0xC9,BANKED
		swapf	0x78,W,BANKED
		andlw	0xF
		mullw	0xA
		movlw	0xF
		andwf	0x78,W,BANKED
		addwf	PRODL,W
		mullw	0x64
		movf	PRODL,W
		addwf	0xC9,f,BANKED
		movf	PRODH,W
		btfsc	STATUS,0
		addlw	1
		movwf	0xC8,BANKED
p__F00:	bsf		EECON1,2					; entry from: 0ECAh
		movlw	8
		movwf	EEADR
		movf	0xC8,W,BANKED
		call	p__A00
		movf	0xC9,W,BANKED
		call	p__A00
		movf	0xCA,W,BANKED
		call	p__A00
		movf	0xCB,W,BANKED
		call	p__A00
		bcf		EECON1,2
		bra		p__E9E
p__F22:	movlw	0x31						; entry from: 210h
		cpfseq	0x65,BANKED
		bra		p__EAC
		movlw	0xFE
		movwf	0x39
		movlw	0xCA
		movwf	0x3A
p__F30:	clrf	0x38						; entry from: 12AAh
p__F32:	call	p_2220					; entry from: 12B8h
		btfss	0x97,2,BANKED
		bra		p__EAC
		clrf	0x11
		goto	p_1D68
p__F40:	movlw	high(TABLE_OFFSET) + 3						; entry from: 142h
		movwf	TBLPTRH
		movlw	low(text_table2)
		movf	0x3D
		bz		p__F94
		btfss	0x3E,7
		bra		p__F5C
		call	p__730
		movlw	0x2C
		call	p__7F2
		call	p__7F0
p__F5C:	movff	0x3D,0x41					; entry from: 0F4Ch
		movlw	6
		cpfslt	0x3D
		bra		p__F7C
		dcfsnz	0x41
		movlw	0x2A
		dcfsnz	0x41
		movlw	0x38
		dcfsnz	0x41
		movlw	0x46
		dcfsnz	0x41
		movlw	0x52
		dcfsnz	0x41
		movlw	0x6A
		bra		p__F94
p__F7C:	movlw	9						; entry from: 0F64h
		cpfsgt	0x3D
		bra		p__F9C
		subwf	0x41
		movlw	0xAC
		dcfsnz	0x41
		movlw	0x8E
		dcfsnz	0x41
		movlw	0x98
		dcfsnz	0x41
		movlw	0x9E
		bra		p__F9E

p__F94:	call	p__730					; entry from: 0F48h,0F7Ah
#if DATA_OFFSET == 0
		clrf	TBLPTRH
#else
		movlw	high(DATA_OFFSET) + 0
		movwf	TBLPTRH
#endif
		bra		p__E9A
p__F9C:	movlw	0x82						; entry from: 0F80h
p__F9E:	call	p__730					; entry from: 0F92h
		movlw	0xA4
		call	p__730
#if DATA_OFFSET == 0
		clrf	TBLPTRH
#else
		movlw	high(DATA_OFFSET) + 0
		movwf	TBLPTRH
#endif
		call	p_2220
		btfss	0x37,7
		bra		p__FBC
		movlw	0x31
		call	p__7F2
		movlw	0x31
		bra		p__FC4
p__FBC:	movlw	0x32						; entry from: 0FB0h
		call	p__7F2
		movlw	0x39
p__FC4:	call	p__7F2					; entry from: 0FBAh
		movlw	0x2F
		call	p__7F2
		clrf	0x2F
		clrf	0x30
		bcf		STATUS,0
		rrcf	0x96,W,BANKED
		addlw	0xF4
		movwf	0x32
		clrf	0x31
		movlw	1
		addwfc	0x31
		clrf	0x7A,BANKED
		movff	0x96,0x7B
		call	p__AE2
		movff	0x7C,0x31
		movff	0x7D,0x32
		call	p__3C6
		tstfsz	0x30
		bra		p_1000
		tstfsz	0x31
		bra		p_1006
		bra		p_100C
p_1000:	movf	0x30,W					; entry from: 0FF8h
		call	p__71C
p_1006:	movf	0x31,W					; entry from: 0FFCh
		call	p__71C
p_100C:	movf	0x32,W					; entry from: 0FFEh
		call	p__71C
		movlw	0x29
		call	p__7F2
		bra		p__E9A
p_101A:	movlw	0x4E						; entry from: 216h
		cpfseq	0x65,BANKED
		bra		p_146A
		movlw	0x41
		btfsc	0x3E,7
		call	p__7F2
		movf	0x3D,W
		call	p__71C
		bra		p__E9A

p_1030:	call	p__724					; entry from: 0D08h,1202h,121Ch

p_1034:	call	p__5D4					; entry from: 818h,103Ch,1400h,1A38h,1A3Eh
		movf	FSR1L,W
		cpfseq	FSR2L
		bra		p_1034
		return	
p_1040:	btfss	0x73,7,BANKED			; entry from: 1A76h
		bra		p_146A
		movlw	0x4D
		cpfseq	0x66,BANKED
		bra		p_1078
		movlw	7
		cpfseq	0x60,BANKED
		bra		p_146A
		swapf	0x79,W,BANKED
		andlw	0xF
		movwf	0x41
		bz		p_1072
		movlw	1
		cpfseq	0x41
		bra		p_1068
		btfss	0x35,7
		bra		p_146A
		btfss	0x35,6
		bra		p_146A
		bra		p_1072
p_1068:	movlw	2						; entry from: 105Ch
		cpfseq	0x41
		bra		p_146A
		btfss	0x35,6
		bra		p_146A

p_1072:	movff	0x41,0xA6				; entry from: 1056h,1066h
		bra		p__E9E
p_1078:	movlw	0x48						; entry from: 1048h
		cpfseq	0x66,BANKED
		bra		p_10BA
		movlw	9
		cpfseq	0x60,BANKED
		bra		p_10A0
		clrf	0xA7,BANKED
		clrf	0xA8,BANKED
		swapf	0x79,W,BANKED
		andlw	7
		movwf	0xA9,BANKED
		movlw	0xF0
		andwf	0x7A,f,BANKED
		movlw	0xF
		andwf	0x79,W,BANKED
		iorwf	0x7A,W,BANKED
		movwf	0xAA,BANKED
		swapf	0xAA,f,BANKED
		bsf		0x35,7
		bra		p__E9E
p_10A0:	movlw	0xE						; entry from: 1082h
		cpfseq	0x60,BANKED
		bra		p_146A
		movff	0x79,0xA7
		movff	0x7A,0xA8
		movff	0x7B,0xA9
		movff	0x7C,0xAA
		bsf		0x35,7
		bra		p__E9E
p_10BA:	movlw	0x44						; entry from: 107Ch
		cpfseq	0x66,BANKED
		bra		p_146A
		btfsc	0x60,0,BANKED
		bra		p_146A
		movlw	7
		cpfsgt	0x60,BANKED
		bra		p_146A
		movf	0x95,W,BANKED
		movwf	0xAC,BANKED
		movwf	0xAD,BANKED
		movwf	0xAE,BANKED
		movwf	0xAF,BANKED
		movlw	0xF
		cpfslt	0x60,BANKED
		movff	0x7D,0xAF
		movlw	0xD
		cpfslt	0x60,BANKED
		movff	0x7C,0xAE
		movlw	0xB
		cpfslt	0x60,BANKED
		movff	0x7B,0xAD
		movlw	9
		cpfslt	0x60,BANKED
		movff	0x7A,0xAC
		movff	0x79,0xAB
		movlw	6
		subwf	0x60,W,BANKED
		movwf	0xB0,BANKED
		rrncf	0xB0,f,BANKED
		bsf		0x35,6
		bra		p__E9E
p_1104:	movlw	5						; entry from: 15Ah
		cpfseq	0x3D
		bra		p_146A
		bra		p_157A
p_110C:	movlw	0x48						; entry from: 254h
		cpfseq	0x78,BANKED
		bra		p_1116
		bsf		0x2D,0
		bra		p_111E
p_1116:	movlw	0x96						; entry from: 1110h
		cpfseq	0x78,BANKED
		bra		p_1122
		bcf		0x2D,0
p_111E:	bsf		0x2D,1					; entry from: 1114h
		bra		p_1462
p_1122:	movlw	0x10						; entry from: 111Ah
		cpfseq	0x78,BANKED
		bra		p_146A
		bcf		0x2D,1
		bra		p_1462
p_112C:	movlw	0x41						; entry from: 2B6h
		cpfseq	0x65,BANKED
		bra		p_146A
		rcall	p_1466
		rcall	p_113A
		movwf	0xBF,BANKED
		bra		p_1462

p_113A:	swapf	0x78,W,BANKED			; entry from: 0CD8h,1134h,153Eh
		andlw	0xF0
		movwf	0x45
		swapf	0x79,W,BANKED
		andlw	0xF
		iorwf	0x45,W
		return	
p_1148:	movlw	0x52						; entry from: 25Ah
		cpfseq	0x65,BANKED
		bra		p_146A
		movlw	0x31
		cpfseq	0x66,BANKED
		bra		p_1158
		bcf		0x10,1
		bra		p_1462
p_1158:	movlw	0x30						; entry from: 1152h
		cpfseq	0x66,BANKED
		bra		p_1164
		bsf		0x10,1
		bcf		0x10,0
		bra		p_1462
p_1164:	movlw	0x32						; entry from: 115Ch
		cpfseq	0x66,BANKED
		bra		p_1170
		bsf		0x10,1
		bsf		0x10,0
		bra		p_1462
p_1170:	movlw	0x48						; entry from: 1168h
		cpfseq	0x66,BANKED
		bra		p_117A
		bcf		0x10,2
		bra		p_1462
p_117A:	movlw	0x53						; entry from: 1174h
		cpfseq	0x66,BANKED
		bra		p_146A
		bsf		0x10,2
		bra		p_1462
p_1184:	movlw	0x4E						; entry from: 21Ch
		cpfseq	0x65,BANKED
		bra		p_146A
		movlw	0x4F
		call	p__7F2
		btfss	PORTC,4
		bra		p_119C
		movlw	0x4E
		call	p__7F2
		bra		p_11F4
p_119C:	movlw	0x46						; entry from: 1192h
		call	p__7F2
		movlw	0x46
		call	p__7F2
		bra		p_11F4
p_11AA:	movlw	0x31						; entry from: 222h
		cpfseq	0x65,BANKED
		bra		p_11B4
		bcf		0x2D,6
		bra		p_1462
p_11B4:	tstfsz	0x78,BANKED				; entry from: 11AEh
		bra		p_146A
		bsf		0x2D,6
		bra		p_1462
p_11BC:	movlw	0x31						; entry from: 178h
		call	p__834
		btfss	0x8D,0,BANKED
		bra		p_11DA
		movf	0xC0,W,BANKED
		call	p__74C
		movlw	0x32
		call	p__834
		movf	0xC1,W,BANKED
		call	p__74C
		bra		p_11F4
p_11DA:	call	p_1528					; entry from: 11C4h
		call	p_1528
		call	p__7F0
		movlw	0x32
		call	p__834
		call	p_1528
		call	p_1528

p_11F4:	goto	p_1830					; entry from: 119Ah,11A8h,11D8h
p_11F8:	btfss	0x1C,7					; entry from: 18Ah
		bra		p_146A
		movlw	0xB4
		call	p__730
		rcall	p_1030
		bra		p_1222

p_1206:	call	p__724					; entry from: 18B8h,18F4h
		movlw	0x4C
		call	p__7F2
		movlw	0x50
		call	p__7F2
		movlw	0x1D
		call	p__730
		rcall	p_1030
		call	p__B1A
p_1222:	call	p__B1A					; entry from: 1204h
		call	p_1FEA
		bcf		LATA,1
		movlw	0xF4
		movwf	PORTB
		call	p__658
		movlw	0x82
		btfsc	PIR1,5
		goto	p__6CC
		bsf		BAUDCON1,1
		bsf		PIE1,5
		bsf		INTCON,7
		movlw	2
		movwf	OSCCON
		bsf		LATC,5
		btfsc	0x1C,6
		bcf		LATC,5
		btfss	PORTC,4
		bra		p_1258
		btfsc	0x19,3
		bra		p_126A
p_1254:	btfsc	PORTC,4					; entry from: 1256h
		bra		p_1254
p_1258:	clrf	0x41						; entry from: 124Eh
p_125A:	btfsc	PORTC,4					; entry from: 1264h
		clrf	0x41
		incf	0x41
		movlw	0x49
		cpfsgt	0x41
		bra		p_125A
p_1266:	btfss	PORTC,4					; entry from: 1268h
		bra		p_1266

p_126A:	clrf	0x41						; entry from: 1252h,1276h
		movlw	0x16
		btfss	0x1C,1
		movlw	5
		movwf	0x42
p_1274:	btfss	PORTC,4					; entry from: 127Eh
		bra		p_126A
		decfsz	0x41
		bra		p_127E
		decfsz	0x42
p_127E:	bra		p_1274					; entry from: 127Ah
p_1280:	bcf		INTCON,7				; entry from: 0Ah
		bcf		PIE1,5
		bcf		BAUDCON1,1
		clrf	OSCCON
p_1288:	btfss	OSCCON,3				; entry from: 128Ah
		bra		p_1288
		bcf		LATC,5
		btfsc	0x1C,6
		bsf		LATC,5
		call	p__B1A
		goto	p_1650

p_129A:	clrf	0x11						; entry from: 19Ch,1826h
		bsf		0x11,4
		goto	p_1D68
p_12A2:	rcall	p_1466					; entry from: 2D0h
		movwf	0x39
		movff	0x79,0x3A
		bra		p__F30
p_12AC:	rcall	p_1466					; entry from: 2ECh
		movwf	0x38
		movff	0x79,0x39
		movff	0x7A,0x3A
		bra		p__F32
p_12BA:	rcall	p_1466					; entry from: 260h
		movwf	0x15
		movlw	0x40
		bra		p_12C8
p_12C2:	rcall	p_1466					; entry from: 266h
		movwf	0x15
		movlw	0x20
p_12C8:	movwf	0x11						; entry from: 12C0h
		goto	p_1D68
p_12CE:	rcall	p_1466					; entry from: 2D6h
		movlw	0x41
		cpfslt	0x79,BANKED
		bra		p_146A
		movff	0x78,0x9A
		movff	0x79,0x9B
		bra		p_1462
p_12E0:	call	p_1FEA					; entry from: 1A8h
		bra		p_1462
p_12E6:	rcall	p_13C2					; entry from: 2DCh
		bsf		0xCC,0,BANKED
		movlw	0x4F
		cpfseq	0x67,BANKED
		bra		p_13F2
		movlw	0x4E
		cpfseq	0x68,BANKED
		bra		p_13F2
		clrf	0xCD,BANKED
		bra		p_1378
p_12FA:	rcall	p_13C2					; entry from: 2E4h
		bsf		0xCC,0,BANKED
		movlw	0x4F
		cpfseq	0x67,BANKED
		bra		p_13F2
		movlw	0x46
		cpfseq	0x68,BANKED
		bra		p_13F2
		cpfseq	0x69,BANKED
		bra		p_13F2
		setf	0xCD,BANKED
		bra		p_1378
p_1312:	rcall	p_13C2					; entry from: 2F2h
		infsnz	0xCC,W,BANKED
		bra		p_13F2
		movlw	0x53
		cpfseq	0x67,BANKED
		bra		p_13F2
		movlw	0x56
		cpfseq	0x68,BANKED
		bra		p_13F2
		movf	0x69,W,BANKED
		call	p_13E2
		movwf	0xCD,BANKED
		swapf	0xCD,f,BANKED
		movf	0x6A,W,BANKED
		call	p_13E2
		iorwf	0xCD,f,BANKED
		rrncf	0xCC,W,BANKED
		xorlw	4
		bnz		p_1340
		movlw	3
		bra		p_1374
p_1340:	rrncf	0xCC,W,BANKED			; entry from: 133Ah
		xorlw	0xC
		bnz		p_134E
		movlw	7
		cpfsgt	0xCD,BANKED
		bra		p_13F2
		bra		p_1378
p_134E:	rrncf	0xCC,W,BANKED			; entry from: 1344h
		xorlw	0x2B
		bz		p_1360
		rrncf	0xCC,W,BANKED
		xorlw	0x2D
		bz		p_1360
		rrncf	0xCC,W,BANKED
		xorlw	0x2F
		bnz		p_1368

p_1360:	movf	0xCD,W,BANKED			; entry from: 1352h,1358h
		bz		p_13F2
		movlw	0x41
		bra		p_1374
p_1368:	rrncf	0xCC,W,BANKED			; entry from: 135Eh
		xorlw	7
		bnz		p_1378
		movf	0xCD,W,BANKED
		bz		p_13F2
		movlw	0xD

p_1374:	cpfslt	0xCD,BANKED				; entry from: 133Eh,1366h
		bra		p_13F2

p_1378:	incf	0xCC,W,BANKED			; entry from: 12F8h,1310h,134Ch,136Ch
		bz		p_1398
		movlw	0xC
		addwf	0xCC,W,BANKED
p_1380:	movwf	0xCC,BANKED				; entry from: 1534h
		call	p__838
		xorwf	0xCD,W,BANKED
		btfsc	STATUS,2
		bra		p_1462
		bsf		EECON1,2
		movf	0xCD,W,BANKED
		call	p__A00
		bcf		EECON1,2
		bra		p_1462
p_1398:	movlw	0x30						; entry from: 137Ah
		movwf	0x41
		movlw	0xC
		iorlw	1
		movwf	EEADR
		bsf		EECON1,2
p_13A4:	call	p__838					; entry from: 13BCh
		xorwf	0xCD,W,BANKED
		bz		p_13B4
		movf	0xCD,W,BANKED
		call	p__A00
		bra		p_13B6
p_13B4:	incf	EEADR						; entry from: 13AAh
p_13B6:	incf	EEADR,W					; entry from: 13B2h
		movwf	EEADR
		decfsz	0x41
		bra		p_13A4
		bcf		EECON1,2
		bra		p_1462

p_13C2:	movf	0x65,W,BANKED			; entry from: 12E6h,12FAh,1312h
		call	p_13E2
		movwf	0xCC,BANKED
		swapf	0xCC,f,BANKED
		movf	0x66,W,BANKED
		call	p_13E2
		iorwf	0xCC,f,BANKED
		infsnz	0xCC,W,BANKED
		return	
		movlw	0x30
		cpfslt	0xCC,BANKED
		bra		p_13F0
		rlncf	0xCC,f,BANKED
		return	

p_13E2:	call	p__52A					; entry from: 1326h,1330h,13C4h,13CEh
		movwf	0x42
		infsnz	0x42,W
		bra		p_13F0
		movf	0x42,W
		return	

p_13F0:	clrf	STKPTR					; entry from: 13DCh,13EAh

p_13F2:	bra		p_146A					; entry from: 12EEh,12F4h,1302h,1308h,130Ch,1316h,131Ch,1322h,134Ah,1362h,1370h,1376h
p_13F4:	movlw	0x53						; entry from: 228h
		cpfseq	0x65,BANKED
		bra		p_146A
		movlw	0xC
		movwf	0xCC,BANKED
		clrf	0x41
p_1400:	rcall	p_1034					; entry from: 1460h
		movlw	4
		movwf	0x42
p_1406:	movlw	4						; entry from: 145Eh
		movwf	0x43
p_140A:	movlw	0x30						; entry from: 144Eh
		cpfslt	0x41
		bra		p_1450
		swapf	0x41,W
		call	p__71C
		movf	0x41,W
		call	p__71C
		call	p__720
		movf	0xCC,W,BANKED
		call	p__838
		call	p__74C
		incf	0xCC,f,BANKED
		movf	0xCC,W,BANKED
		call	p__838
		btfss	STATUS,2
		movlw	0x46
		btfsc	STATUS,2
		movlw	0x4E
		call	p__7F2
		incf	0xCC,f,BANKED
		incf	0x41
		decf	0x43
		bz		p_1450
		call	p__7F0
		call	p__7F0
		bra		p_140A

p_1450:	call	p__724					; entry from: 140Eh,1444h
		movlw	0x30
		cpfslt	0x41
		goto	p_1834
		decfsz	0x42
		bra		p_1406
		bra		p_1400

p_1462:	goto	p__E9E					; entry from: 1120h,112Ah,1138h,1156h,1162h,116Eh,1178h,1182h,11B2h,11BAh,12DEh,12E4h,138Ah,1396h,13C0h,1544h,1556h,156Ch,1584h,1590h,1598h,15A4h,15ACh,15CEh,15DCh,164Eh

p_1466:	goto	p__EA4					; entry from: 1132h,12A2h,12ACh,12BAh,12C2h,12CEh,152Eh,1536h,1546h,1558h,157Eh,1586h,1592h,15A6h,15AEh,15DEh

p_146A:	goto	p__EAC					; entry from: 101Eh,1042h,104Eh,1060h,1064h,106Ch,1070h,10A4h,10BEh,10C2h,10C8h,1108h,1126h,1130h,114Ch,117Eh,1188h,11B6h,11FAh,12D4h,13F2h,13F8h,147Eh,1484h,153Ch,1572h,1578h,15B6h,15F2h,15FEh,1610h
p_146E:	movlw	0x7C						; entry from: 1BAh
		call	p__838
		call	p__7AA
		bra		p_1830
p_147A:	movlw	0x52						; entry from: 22Eh
		cpfseq	0x65,BANKED
		bra		p_146A
		btfsc	0x3E,6
		btfss	0x3E,0
		bra		p_146A
		clrf	0x41
		bra		p_1A02
p_148A:	call	p__99E					; entry from: 1C0h
		bcf		STATUS,0
		rrcf	0xCA,W,BANKED
		movwf	0x31
		rrcf	0xCB,W,BANKED
		movwf	0x32
		clrf	0x2F
		clrf	0x30
		movf	ADRESL,W
		mulwf	0xC9,BANKED
		movf	PRODL,W
		addwf	0x32
		movf	PRODH,W
		addwfc	0x31
		movf	ADRESH,W
		mulwf	0xC9,BANKED
		movf	PRODL,W
		addwf	0x31
		movf	PRODH,W
		addwfc	0x30
		movf	ADRESL,W
		mulwf	0xC8,BANKED
		movf	PRODL,W
		addwf	0x31
		movf	PRODH,W
		addwfc	0x30
		movf	ADRESH,W
		mulwf	0xC8,BANKED
		movf	PRODL,W
		addwf	0x30
		movf	PRODH,W
		addwfc	0x2F
		movff	0xCA,0x7A
		movff	0xCB,0x7B
		call	p__AE2
		iorlw	0
		bnz		p_151A
		movlw	0x27
		cpfslt	0x7C,BANKED
		bra		p_151A
		movlw	5
		addwf	0x7D,W,BANKED
		movwf	0x32
		movlw	0
		addwfc	0x7C,W,BANKED
		movwf	0x31
		call	p__3C6
		iorlw	0
		bnz		p_151A
		movf	0x2F,W
		tstfsz	0x2F
		call	p__71C
		movf	0x30,W
		call	p__71C
		movlw	0x2E
		call	p__7F2
		movf	0x31,W
		call	p__71C
p_1510:	movlw	0x56						; entry from: 1526h
		call	p__7F2
		goto	p_1830

p_151A:	rcall	p_1528					; entry from: 14DAh,14E0h,14F4h
		rcall	p_1528
		movlw	0x2E
		call	p__7F2
		rcall	p_1528
		bra		p_1510

p_1528:	movlw	0x2D						; entry from: 11DAh,11DEh,11ECh,11F0h,151Ah,151Ch,1524h
		goto	p__7F2
p_152E:	rcall	p_1466					; entry from: 284h
		movwf	0xCD,BANKED
		movlw	0x7C
		bra		p_1380
p_1536:	rcall	p_1466					; entry from: 2B0h
		movlw	0x41
		cpfseq	0x65,BANKED
		bra		p_146A
		rcall	p_113A
		movwf	0x16
		bsf		0x18,1
		bra		p_1462
p_1546:	rcall	p_1466					; entry from: 2F8h
		movwf	0x12
		movff	0x79,0x13
		movff	0x7A,0x14
		bsf		0xF,4
		bsf		0x34,4
		bra		p_1462
p_1558:	rcall	p_1466					; entry from: 2BCh
		clrf	0x12
		call	p__E48
		andlw	7
		movwf	0x13
		movff	0x79,0x14
		bsf		0xF,4
		bsf		0x34,4
		bra		p_1462
p_156E:	movlw	5						; entry from: 1D2h
		cpfslt	0x3D
		bra		p_146A
		movlw	2
		cpfsgt	0x3D
		bra		p_146A
p_157A:	bsf		0x2D,7					; entry from: 110Ah
		bra		p_1AFE

p_157E:	rcall	p_1466					; entry from: 26Ch,28Ah
		movwf	0x93,BANKED
		bsf		0x10,4
		bra		p_1462
p_1586:	rcall	p_1466					; entry from: 290h
		btfsc	STATUS,2
		movf	0x80,W,BANKED
		movwf	0x7E,BANKED
		bsf		0xF,6
		bra		p_1462
p_1592:	rcall	p_1466					; entry from: 296h
		bnz		p_159A
		bsf		0x2C,4
		bra		p_1462
p_159A:	movwf	0x86,BANKED				; entry from: 1594h
		movlw	0xF5
		cpfslt	0x86,BANKED
		movwf	0x86,BANKED
		bcf		0x2C,4
		bra		p_1462
p_15A6:	rcall	p_1466					; entry from: 278h
		movwf	0x94,BANKED
		movwf	0xBE,BANKED
		bra		p_1462

p_15AE:	rcall	p_1466					; entry from: 234h,23Ah
		swapf	0x78,W,BANKED
		sublw	0xC
		btfss	STATUS,0
		bra		p_146A
		bsf		0x3E,7
		movf	0x78,f,BANKED
		bz		p_15C8
		bcf		0x3E,7

p_15C0:	movlw	0x53						; entry from: 1604h,160Ah
		xorwf	0x63,W,BANKED
		btfsc	STATUS,2
		bsf		0x3E,2

p_15C8:	swapf	0x78,W,BANKED			; entry from: 15BCh,1606h
		xorwf	0x3D,W
		btfsc	STATUS,2
		bra		p_1462
		call	p_1FEA
		swapf	0x78,W,BANKED
		movwf	0x3D
		call	p_26DE
		bra		p_1462

p_15DE:	rcall	p_1466					; entry from: 272h,27Eh
		bz		p_1608
		andlw	0xF
		xorlw	0xA
		bz		p_15F4
		swapf	0x78,W,BANKED
		movwf	0x78,BANKED
		andlw	0xF
		xorlw	0xA
		bz		p_15F4
		bra		p_146A

p_15F4:	movlw	0xF0					; entry from: 15E6h,15F0h
		andwf	0x78,f,BANKED
		swapf	0x78,W,BANKED
		sublw	0xC
		btfss	STATUS,0
		bra		p_146A
		bsf		0x3E,7
		tstfsz	0x78,BANKED
		bra		p_15C0
		bra		p_15C8
p_1608:	bsf		0x3E,7					; entry from: 15E0h
		bra		p_15C0
p_160C:	btfsc	0x72,4,BANKED			; entry from: 1A66h
		btfsc	0x60,0,BANKED
		bra		p_146A
		bsf		0x2C,5
		movlw	4
		subwf	0x60,W,BANKED
		movwf	0x22
		bcf		STATUS,0
		rrcf	0x22
		movff	0x78,0x23
		movff	0x79,0x24
		movff	0x7A,0x25
		movff	0x7B,0x26
		movff	0x7C,0x27
		movff	0x7D,0x28
		movlw	0
		movwf	FSR0H
		movlw	0x23
		movwf	FSR0L
		movlw	0
		movff	0x22,0x41
p_1644:	addwf	POSTINC0,W				; entry from: 1648h
		decfsz	0x41
		bra		p_1644
		movwf	INDF0
		incf	0x22
		bra		p_1462

p_1650:	movlw	0x20						; entry from: 1EAh,1296h
		bra		p_1666
p_1654:	clrf	OSCCON					; entry from: 2
#if SW_VERSION != 0
		call	p_restart
#endif
		comf	RCON,W
#if WDT_RESET
		movwf	FSR0L
		movlw   0xFD	; keep POR bit
		iorwf	RCON
		movf    FSR0L,W
#else
		setf	RCON
#endif
		clrwdt
		andlw	0x1B
		btfsc	STKPTR,7
		iorlw	0xA0
		btfsc	STKPTR,6
		iorlw	0x60
p_1666:	movlb	0						; entry from: 1652h
		movwf	0xD0,BANKED
		clrf	STKPTR
#if EEPROM_PAGE != 0
		movlw	EEPROM_PAGE
		movwf	EEADRH
#endif
#if SW_VERSION != 0
		call	eep_copy
#endif
		btfsc	0xD0,1,BANKED
		bcf		0xD0,0,BANKED
		btfss	0xD0,4,BANKED
		bra		p_1678
		tstfsz	0xD1,BANKED
		bsf		0xD0,5,BANKED
p_1678:	clrf	0x8E,BANKED				; entry from: 1672h
		movlw	0xFF
		movwf	0xCD,BANKED
		movlw	0x30
		call	p__98C
		addlw	1
		bz		p_168A
		bsf		0x8E,1,BANKED
p_168A:	movlw	0xF9					; entry from: 1686h
		xorwf	0x8E,W,BANKED
		movwf	PORTA
		movlw	0xF9
		movwf	TRISA
		call	p_4028
		movlw	0
		movwf	ADCON1
		movlw	0x86
		movwf	ADCON2
		movlw	0xF4
		movwf	PORTB
		btfss	0xD0,1,BANKED
		bra		p_16B0
		movlw	0x7F
		movwf	INTCON2
		movlw	0xE8
		bra		p_16B2
p_16B0:	movlw	8						; entry from: 16A6h
p_16B2:	movwf	TRISB					; entry from: 16AEh
		movlw	0
		movwf	CCP1CON
		movlw	0x9A
		movwf	0xCD,BANKED
		movlw	0x28
		call	p__98C
		movwf	0x1C
		movlw	0xF0
		btfss	0x1C,7
		bra		p_16CE
		btfss	0x1C,6
		andlw	0xDF
p_16CE:	movwf	PORTC					; entry from: 16C8h
		movlw	0x97
		movwf	TRISC
		btfsc	0xD0,5,BANKED
		bra		p_16DE
		rlncf	PORTA,W
		movwf	0x8C,BANKED
		rlncf	0x8C,f,BANKED
p_16DE:	lfsr	1,0x200					; entry from: 16D6h
		lfsr	2,0x200
		lfsr	0,0
#if DATA_OFFSET == 0
		clrf	TBLPTRH
#else
		movlw	high(DATA_OFFSET) + 0
		movwf	TBLPTRH
#endif
		movlw	0x80
		movwf	EECON1
		bsf		CANCON,7
		clrf	0x97,BANKED
		btfss	0xD0,1,BANKED
		bra		p_1706
		clrf	0xD2,BANKED
		call	p__492
		movlw	0xFF
		movwf	INTCON2
		movlw	8
		movwf	TRISB
p_1706:	clrf	0x3C						; entry from: 16F6h
		movlw	0xD
		movwf	0xCD,BANKED
		movlw	0x2C
		call	p__98C
		movwf	0x3B
		btfss	0xD0,5,BANKED
		bra		p_171C
		movwf	0x3C
		bra		p_1730
p_171C:	movlw	0x7C						; entry from: 1716h
p_171E:	movwf	LATB					; entry from: 172Ch
		call	p__B26
		bsf		STATUS,0
		rrcf	WREG,W
		andlw	0xFC
		btfsc	WREG,3
		bra		p_171E
		movwf	LATB
p_1730:	btfss	TXSTA1,1				; entry from: 171Ah
		call	p__B2A
		clrf	SPBRGH1
		nop
		nop
		movf	0xCF,W,BANKED
		btfss	0xD0,5,BANKED
		call	p__900
		movwf	0xCF,BANKED
		movlw	8
		cpfsgt	0xCF,BANKED
		movwf	0xCF,BANKED
		decf	0xCF,W,BANKED
		bra		p_1754
		incf	SPBRGH1
		movlw	0xA0
p_1754:	movwf	SPBRG1					; entry from: 174Eh
		movlw	8
		movwf	BAUDCON1
		movlw	0x25
		movwf	TXSTA1
		movlw	0x90
		movwf	RCSTA1
		call	p__658
		clrf	0x8D,BANKED
		call	p__8E2
		movwf	0x94,BANKED
		call	p__8F2
		btfsc	STATUS,2
		bsf		0x8D,6,BANKED
		call	p__8F8
		movwf	0x8A,BANKED
		call	p__908
		movwf	0x8B,BANKED
		call	p__926
		btfsc	STATUS,2
		bsf		0x8D,7,BANKED
		call	p__964
		movwf	0x99,BANKED
		movlw	0x40
		cpfslt	0x99,BANKED
		movwf	0x99,BANKED
		movlw	0xF9
		movwf	0xBE,BANKED
		call	p__96C
		movwf	0x9A,BANKED
		call	p__974
		movwf	0x9B,BANKED
		movlw	0x40
		cpfslt	0x9B,BANKED
		movwf	0x9B,BANKED
		call	p__97C
		movwf	0x9C,BANKED
		call	p__984
		movwf	0x9D,BANKED
		movlw	0x40
		cpfslt	0x9D,BANKED
		movwf	0x9D,BANKED
		movlw	0xC5
		movwf	T0CON
		clrf	TMR0L
		clrf	0x2C
		clrf	0x1A
		clrf	0x3E
		clrf	0x60,BANKED
		clrf	0x72,BANKED
		clrf	0xB1,BANKED
		clrf	0xB2,BANKED
		call	p__A1E
		movlw	0
		btfsc	0xD0,3,BANKED
		movlw	0x91
		btfsc	0xD0,6,BANKED
		movlw	0x92
		btfsc	0xD0,7,BANKED
		movlw	0x93
		btfsc	0xD0,4,BANKED
		movf	0xD1,W,BANKED
		iorlw	0
		btfss	STATUS,2
		goto	p__6CC
		call	p__724
		call	p__724
		btfss	0xD0,0,BANKED
		bra		p_1802
		bsf		0xD2,6,BANKED
		movlw	0xE2
		bra		p_182C
p_1802:	bcf		0xD2,6,BANKED			; entry from: 17FAh
		call	p__8C0
		bnz		boot_reason
		movlw	0x82
		call	p__730
		call	p__724
		call	p__724
		movlw	0xD0
		call	p__730
		call	p__724
		call	p__658
		goto	p_129A

boot_reason:
#if WDT_RESET
		btfss   _POR_				; check for power on reset
		bra     p_1830
#endif
p_182A:	movlw	0x82						; entry from: 10Ah,1808h

p_182C:	call	p__730					; entry from: 6C2h,82Ah,0C3Eh,0EA0h,0EAEh,1800h,1840h,1D38h

p_1830:	call	p__724					; entry from: 700h,0E9Ah,11F4h,1478h,1516h

p_1834:	btfss	0x19,4					; entry from: 672h,6BCh,0D58h,1458h,1B58h,1BC8h,1C36h,1CACh,1D2Eh,1D88h
		bra		p_1842
		bcf		0x19,4
		btfss	0x19,7
		bra		p_1842
		movlw	0xC8
		bra		p_182C

p_1842:	call	p__724					; entry from: 1836h,183Ch
		call	p__5D4
		btfsc	0x3E,2
		call	p__9C4
		bcf		0x3E,2
		bcf		0x19,6
		bcf		0x2D,7
		btfss	PIR1,5
		bra		p_185E
		clrf	0x20
		bcf		LATB,4
p_185E:	bcf		0x34,3					; entry from: 1858h
		clrf	0xB3,BANKED
		btfsc	0x3E,0
		btfsc	CANSTAT,7
		bra		p_187E
		movf	CANSTAT,W
		andlw	0xE0
		bnz		p_1876
		btfsc	COMSTAT,5
		bra		p_1876
		btfss	COMSTAT,0
		bra		p_187E

p_1876:	call	p_3CCE					; entry from: 186Ch,1870h
		call	p_3B7C

p_187E:	call	p__658					; entry from: 1866h,1874h
		btfss	0x1C,7
		bcf		LATC,5
		movlw	0xB8
		call	p__730
		movlw	0x70
		andwf	0x72,f,BANKED
		movlw	0x61
		movwf	0x74,BANKED
		movff	0x61,0x71
		bcf		0x1C,0
		bcf		0x19,3
		clrf	0x88,BANKED
		clrf	0x89,BANKED
		movlw	0x10
		call	p__B12
p_18A6:	btfsc	0x1C,2					; entry from: 192Ch
		btfsc	PORTC,4
		clrf	0x21
		call	p__40C
		btfsc	0x1C,7
		btfss	0xF,1
		bra		p_18BA
		bsf		0x19,3
		bra		p_1206
p_18BA:	btfsc	0x2C,7					; entry from: 18B4h
		btfss	0x2C,6
		bra		p_18C4
		call	p_1E2A
p_18C4:	btfsc	LATB,4					; entry from: 18BEh
		bra		p_18D0
		bcf		0x72,2,BANKED
		bcf		0x1C,0
		clrf	0x88,BANKED
		clrf	0x89,BANKED
p_18D0:	btfsc	0x20,7					; entry from: 18C6h
		bsf		0x72,2,BANKED
		btfss	0x20,7
		btfss	0x72,2,BANKED
		bra		p_192A
		bcf		0x72,2,BANKED
		incf	0x88,f,BANKED
		movlw	0x13
		cpfsgt	0x88,BANKED
		bra		p_18E8
		btfsc	0x72,7,BANKED
		bra		p_1AFA
p_18E8:	call	p__40C					; entry from: 18E2h
		movlw	0x39
		cpfsgt	0x88,BANKED
		bra		p_192A
		btfsc	0x1C,0
		bra		p_1206
		incf	0x89,f,BANKED
		clrf	0x88,BANKED
		btfsc	0x1C,7
		btfss	0x1C,5
		bra		p_192A
		movlw	4
		btfsc	0x1C,4
		movlw	0x13
		btfsc	LATB,4
		cpfseq	0x89,BANKED
		bra		p_192A
		bsf		0x1C,0
		btfss	0x1C,3
		bra		p_192A
		call	p__724
		movlw	0x1A
		call	p__730
		call	p__724
		call	p__724
		movlw	0x3E
		call	p__7F2

p_192A:	btfss	0x72,0,BANKED			; entry from: 18D8h,18F0h,18FEh,190Ah,1910h
		bra		p_18A6
		bsf		0x72,1,BANKED
		movlw	0x61
		movwf	FSR0L
		subwf	0x74,W,BANKED
		bnz		p_193E
		movff	0x71,0x61
		bra		p_194E
p_193E:	movwf	0x60,BANKED				; entry from: 1936h
		movwf	0x41
p_1942:	bcf		INDF0,7					; entry from: 194Ch
		btfsc	INDF0,6
		bcf		INDF0,5
		incf	FSR0L
		decfsz	0x41
		bra		p_1942
p_194E:	incf	SPBRG1,W				; entry from: 193Ch
		movwf	0x41
		rrcf	0x41
		bcf		0x41,7
p_1956:	call	p__B4A					; entry from: 1972h
		btfss	PIR1,5
		bra		p_1964
		movf	RCREG1,W
		xorwf	0x8A,W,BANKED
		bz		p_1974
p_1964:	movf	RCSTA1,W				; entry from: 195Ch
		andlw	6
		btfss	STATUS,2
		bcf		RCSTA1,4
		btfss	STATUS,2
		bsf		RCSTA1,4
		decfsz	0x41
		bra		p_1956
p_1974:	btfsc	0x17,2					; entry from: 1962h
		btfss	0x17,7
		bra		p_1980
		movf	0x8A,W,BANKED
		call	p__7F2
p_1980:	incf	SPBRG1,W				; entry from: 1978h
		movwf	0x41
p_1984:	call	p__5D4					; entry from: 198Ah
		decfsz	0x41
		bra		p_1984
		bcf		0x19,7
		call	p__658
		bsf		0x73,7,BANKED
		movlw	0
		movwf	FSR0H
		movlw	0x60
		movwf	FSR0L
		movf	0x60,W,BANKED
		btfss	STATUS,2
		btfsc	0x72,6,BANKED
		bra		p_1AFA
		movwf	0x41
		movlw	0x76
		movwf	0x43
p_19AA:	incf	FSR0L					; entry from: 19F6h
		movf	INDF0,W
		call	p__52A
		movwf	0x42
		incfsz	0x42,W
		bra		p_19C8
		bcf		0x72,5,BANKED
		movlw	0x65
		cpfslt	FSR0L
		bcf		0x72,4,BANKED
		movlw	0x67
		cpfslt	FSR0L
		bcf		0x73,7,BANKED
		clrf	0x42
p_19C8:	movff	FSR0L,0x44				; entry from: 19B6h
		movff	0x43,FSR0L
		movlw	0x61
		andlw	1
		bnz		p_19DC
		btfss	0x44,0
		bra		p_19E0
		bra		p_19E6
p_19DC:	btfss	0x44,0					; entry from: 19D4h
		bra		p_19E6
p_19E0:	swapf	0x42,W					; entry from: 19D8h
		movwf	INDF0
		bra		p_19EC

p_19E6:	movf	0x42,W					; entry from: 19DAh,19DEh
		iorwf	INDF0
		incf	FSR0L
p_19EC:	movff	FSR0L,0x43				; entry from: 19E4h
		movff	0x44,FSR0L
		decfsz	0x41
		bra		p_19AA
		btfsc	0x72,5,BANKED
		dcfsnz	0x60,W,BANKED
		bra		p_1A3E
		movff	0x60,0x41
p_1A02:	setf	0x81,BANKED				; entry from: 1488h
		btfss	0x41,0
		bra		p_1A0E
		movf	0x42,W
		movwf	0x81,BANKED
		decf	0x41
p_1A0E:	movlw	0xF						; entry from: 1A06h
		btfss	0x3E,0
		bra		p_1A2A
		btfsc	0x97,2,BANKED
		bra		p_1A32
		btfsc	0x97,1,BANKED
		btfss	0x17,0
		bra		p_1A24
		btfsc	0x18,1
		movlw	0xD
		bra		p_1A2E
p_1A24:	btfss	0x18,1					; entry from: 1A1Ch
		bra		p_1A32
		bra		p_1A2E
p_1A2A:	btfsc	0x17,4					; entry from: 1A12h
		bra		p_1A32

p_1A2E:	cpfslt	0x41						; entry from: 1A22h,1A28h
		bra		p_1AFA

p_1A32:	rrncf	0x41,W					; entry from: 1A16h,1A26h,1A2Ch
		movwf	0x75,BANKED
		btfsc	0x18,0
		call	p_1034
		bra		p_1AFE
p_1A3E:	call	p_1034					; entry from: 19FCh
		movlw	2
		cpfsgt	0x60,BANKED
		bra		p_1AFA
		movlw	0x41
		cpfseq	0x61,BANKED
		bra		p_1AFA
		movlw	0x54
		cpfseq	0x62,BANKED
		bra		p_1AFA
		movlw	5
		cpfsgt	0x60,BANKED
		bra		p_1A6A
		movlw	0x57
		cpfseq	0x63,BANKED
		bra		p_1A6A
		movf	0x64,W,BANKED
		xorlw	0x4D
		btfsc	STATUS,2
		goto	p_160C

p_1A6A:	movlw	0xFC					; entry from: 1A58h,1A5Eh
		cpfseq	0x77,BANKED
		bra		p_1A7A
		movf	0x65,W,BANKED
		xorlw	0x53
		btfsc	STATUS,2
		goto	p_1040
p_1A7A:	movlw	high(TABLE_OFFSET) + 1						; entry from: 1A6Eh
		movwf	TBLPTRH
		movlw	3
		subwf	0x60,W,BANKED
		movwf	0x41
		btfsc	STATUS,2
		movlw	2
		dcfsnz	0x41
		movlw	0x16
		tstfsz	0x41
		btfsc	0x41,7
		bra		p_1ACC
		incf	TBLPTRH
		dcfsnz	0x41
		movlw	2
		dcfsnz	0x41
		movlw	0x40
		dcfsnz	0x41
		movlw	0x9C
		dcfsnz	0x41
		movlw	0xC2
		dcfsnz	0x41
		movlw	0xE2
		dcfsnz	0x41
		movlw	0xEA
		tstfsz	0x41
		btfsc	0x41,7
		bra		p_1ACC
		incf	TBLPTRH
		dcfsnz	0x41
		movlw	0x22
		dcfsnz	0x41
		movlw	6
		dcfsnz	0x41
		movlw	0x14
		dcfsnz	0x41
		movlw	0x22
		dcfsnz	0x41
		movlw	0x22
		dcfsnz	0x41
		movlw	0x1C

p_1ACC:	movwf	TBLPTRL					; entry from: 1A90h,1AB0h
p_1ACE:	tblrd*+							; entry from: 1AF6h
		movf	TABLAT,W
		bz		p_1AF8
		cpfseq	0x63,BANKED
		bra		p_1AF0
		tblrd*+
		movf	TABLAT,W
		bz		p_1AE2
		cpfseq	0x64,BANKED
		bra		p_1AF2
p_1AE2:	movff	TBLPTRH,PCLATH			; entry from: 1ADCh
#if DATA_OFFSET == 0
		clrf	TBLPTRH
#else
		movlw	high(DATA_OFFSET) + 0
		movwf	TBLPTRH
#endif
		movlw	2
		subwf	TBLPTRL,W
		goto	p__100
p_1AF0:	incf	TBLPTRL					; entry from: 1AD6h
p_1AF2:	movlw	4						; entry from: 1AE0h
		addwf	TBLPTRL
		bra		p_1ACE
#if DATA_OFFSET == 0
p_1AF8:	clrf	TBLPTRH					; entry from: 1AD2h
#else
p_1AF8:	movlw	high(DATA_OFFSET) + 0			; entry from: 1AD2h
		movwf	TBLPTRH
#endif

p_1AFA:	goto	p__EAC					; entry from: 18E6h,19A2h,1A30h,1A46h,1A4Ch,1A52h

p_1AFE:	bcf		0x3E,4					; entry from: 157Ch,1A3Ch,1C34h,1CB4h,1D28h,1D32h
		bsf		0x19,4
		movff	0x75,0
		movff	0x76,4
		movff	0x77,5
		movff	0x78,6
		movff	0x79,7
		movff	0x7A,8
		movff	0x7B,9
		movff	0x7C,0xA
		movff	0x7D,0xB
		bcf		0xF,5
		movlw	2
		cpfseq	0
		bra		p_1B3A
		tstfsz	5
		bra		p_1B3A
		dcfsnz	4,W
		bsf		0xF,5
		btfsc	0xF,4
		bcf		0xF,5

p_1B3A:	btfss	0x2D,7					; entry from: 1B2Ch,1B30h
		btfss	0x3E,6
		bra		p_1B9A
		rcall	p_1EC4
		call	p__404
		rcall	p_1EDC
		call	p__404
		clrf	0x11
		bsf		0x11,6
p_1B50:	call	p__5D4					; entry from: 1B98h
		tstfsz	0x81,BANKED
		btfss	0x17,3
		goto	p_1834
		rcall	p_1F12
		call	p__404
p_1B62:	btfss	0x81,7,BANKED			; entry from: 1BEAh
		decf	0x81,f,BANKED
		call	p__5D4
		tstfsz	0x83,BANKED
		bra		p_1B76
		movlw	5
		movwf	0x83,BANKED
		clrf	0x84,BANKED
		clrf	0x85,BANKED
p_1B76:	btfss	0x84,5,BANKED			; entry from: 1B6Ch
		incf	0x84,f,BANKED
		incf	0x85,f,BANKED
		movlw	0xFF
		btfsc	0x18,3
		movlw	0x3F
		andwf	0x85,W,BANKED
		bnz		p_1B8E
		decf	0x83,f,BANKED
		movlw	5
		cpfsgt	0x83,BANKED
		movwf	0x83,BANKED
p_1B8E:	movf	0x82,W,BANKED			; entry from: 1B84h
		btfsc	STATUS,2
		movlw	0xFF
		cpfsgt	0x83,BANKED
		movwf	0x83,BANKED
		bra		p_1B50
p_1B9A:	movff	0x3D,0xC4				; entry from: 1B3Eh
		bcf		0x2D,5
		setf	0xC6,BANKED
		movf	0x3D
		bz		p_1C44
		movlw	2
		cpfsgt	0x3D
		bra		p_1BB2
		movlw	5
		cpfsgt	0x3D
		bra		p_1C26
p_1BB2:	bsf		0x19,6					; entry from: 1BAAh
		rcall	p_1E76
		bcf		0x19,6
		bcf		0x3E,6
		rcall	p_1BEC
		rcall	p_1EC4
		rcall	p_1BEC
		rcall	p_1EDC
		rcall	p_1BEC
		tstfsz	0x81,BANKED
		btfss	0x17,3
		goto	p_1834
		clrf	0x98,BANKED
		movlw	0x19
		movff	0x3D,0x41
		dcfsnz	0x41
		movlw	0x19
		dcfsnz	0x41
		movlw	0x19
		btfss	0xF,6
		bra		p_1BE4
		cpfslt	0x7E,BANKED
		movf	0x7E,W,BANKED
p_1BE4:	rcall	p_1F5A					; entry from: 1BDEh
		rcall	p_1BEC
		bsf		0x3E,6
		bra		p_1B62

p_1BEC:	movwf	0x3F						; entry from: 1BBAh,1BBEh,1BC2h,1BE6h
		movf	0x3F
		btfsc	STATUS,2
		return	
		pop
p_1BF6:	bcf		0x3E,2					; entry from: 1C3Ch
		rcall	p_1FEA
		btfss	0x2D,7
		btfss	0x3E,7
		bra		p_1C06
		btfsc	0x19,7
		bra		p_1D34
		bra		p_1C44
p_1C06:	movlw	8						; entry from: 1BFEh
		cpfseq	0x3F
		bra		p_1C1E
		movlw	2
		cpfsgt	0x3D
		bra		p_1C20
		movlw	6
		cpfslt	0x3D
		bra		p_1C20
		movlw	0xFF
		movwf	0x3F
		bra		p_1C20
p_1C1E:	btfss	0x19,7					; entry from: 1C0Ah

p_1C20:	goto	p__66A					; entry from: 1C10h,1C16h,1C1Ch
		bra		p_1D34
p_1C26:	btfsc	0x3E,6					; entry from: 1BB0h
		rcall	p_1FEA
		rcall	p_1E76
		movwf	0x3F
		xorlw	0
		bnz		p_1C3A
		btfss	0x2D,7
		bra		p_1AFE
		goto	p_1834
p_1C3A:	btfss	0x19,7					; entry from: 1C30h
		bra		p_1BF6
		call	p_1FEA
		bra		p_1D34

p_1C44:	bsf		0x3E,4					; entry from: 1BA4h,1C04h
		call	p__844
		rcall	p_1D3C
		call	p__84A
		movlw	1
		btfss	0x10,5
		call	p__B64
		movwf	0x3D
		cpfslt	0xC3,BANKED
		bra		p_1C64
		movlw	1
		movwf	0x3D
		bra		p_1C6A
p_1C64:	decf	0x3D,W					; entry from: 1C5Ch
		movwf	0xC5,BANKED
		bnz		p_1C6E
p_1C6A:	movff	0xC3,0xC5				; entry from: 1C62h
p_1C6E:	movlw	2						; entry from: 1C68h
		cpfseq	0x3D
		bra		p_1C82
		decf	0xC4,W,BANKED
		bnz		p_1C9E
		call	p__916
		addlw	0xDB
		bc		p_1C92
		bra		p_1C9E
p_1C82:	movlw	5						; entry from: 1C72h
		btfsc	0x2D,5
		cpfseq	0x3D
		bra		p_1C9E
		call	p__934
		addlw	0xC3
		bnc		p_1C9E

p_1C92:	call	p__B12					; entry from: 1C7Eh,1CF2h,1D00h
p_1C96:	call	p__5D4					; entry from: 1C9Ch
		btfss	0xF,1
		bra		p_1C96

p_1C9E:	clrf	0x98,BANKED				; entry from: 1C76h,1C80h,1C88h,1C90h,1CF0h,1CF8h,1CFEh
		rcall	p_1E76
		movwf	0x3F
		tstfsz	0x3F
		bra		p_1CB6
		bcf		0x3E,4
		btfss	0x11,3
		goto	p_1834
		btfsc	0x19,7
		bra		p_1D34
		bra		p_1AFE
p_1CB6:	rcall	p_1FEA					; entry from: 1CA6h
		btfss	0x19,7
		bra		p_1CC2
		movff	0xC4,0x3D
		bra		p_1D34

p_1CC2:	movf	0x3D,W					; entry from: 1CBAh,1CDEh,1CE4h
		xorwf	0xC5,W,BANKED
		bz		p_1D02
		rcall	p_1D3C
		incf	0x3D
		movf	0xC3,W,BANKED
		cpfsgt	0x3D
		bra		p_1CD6
		movlw	1
		movwf	0x3D
p_1CD6:	btfss	0x2D,5					; entry from: 1CD0h
		bra		p_1CE6
		movlw	3
		xorwf	0x3D,W
		bz		p_1CC2
		movlw	4
		xorwf	0x3D,W
		bz		p_1CC2
p_1CE6:	movlw	2						; entry from: 1CD8h
		cpfseq	0x3D
		bra		p_1CF4
		call	p__916
		bz		p_1C9E
		bra		p_1C92
p_1CF4:	movlw	5						; entry from: 1CEAh
		cpfseq	0x3D
		bra		p_1C9E
		call	p__934
		bz		p_1C9E
		bra		p_1C92
p_1D02:	bcf		0x3E,4					; entry from: 1CC6h
		incfsz	0xC6,W,BANKED
		bra		p_1D18
p_1D08:	movff	0xC4,0x3D				; entry from: 1D24h
		call	p_26DE
		movlw	2
		movwf	0x3F
		goto	p__66A
p_1D18:	movf	0xC6,W,BANKED			; entry from: 1D06h
		movwf	0x3D
		bsf		0x19,6
		rcall	p_1E76
		bcf		0x19,6
		btfss	0x3E,6
		bra		p_1D08
		btfss	0xF,5
		bra		p_1AFE
		btfsc	0x97,1,BANKED
		btfss	0x17,0
		goto	p_1834
		bra		p_1AFE

p_1D34:	bcf		0x19,4					; entry from: 1C02h,1C24h,1C42h,1CB2h,1CC0h,1D5Ch,1E28h
		movlw	0xC8
		goto	p_182C

p_1D3C:	movlw	5						; entry from: 1C4Ah,1CC8h
		xorwf	0x3D,W
		btfsc	STATUS,2
		btfsc	0x2D,5
p_1D44:	return							; entry from: 1D4Eh
		movlw	5
		movwf	0x45
		call	p__93A
		bz		p_1D44
		movwf	0x7F,BANKED
p_1D52:	clrf	0x21						; entry from: 1D64h
		bcf		0xF,1
p_1D56:	call	p__5D4					; entry from: 1D60h
		btfsc	0x19,7
		bra		p_1D34
		btfss	0xF,1
		bra		p_1D56
		decfsz	0x45
		bra		p_1D52
		return	

p_1D68:	bsf		0x11,2					; entry from: 0F3Ch,129Eh,12CAh
		bcf		0x3E,4
		bsf		0x34,3
		clrf	0x98,BANKED
		bcf		0x35,0
		bcf		0x2D,4
		bsf		0x19,4
		call	p__84A
		btfss	0x3E,3
		bra		p_1DA2

p_1D7E:	rcall	p_1F12					; entry from: 1D92h,1DA0h
		movwf	0x3F
p_1D82:	call	p__5D4					; entry from: 1E04h
		btfsc	0x19,7
		goto	p_1834
		call	p__63E
		movf	0x3F,W
		bz		p_1D7E
		xorlw	8
		bz		p_1D9C
		goto	p__66A
p_1D9C:	clrf	0xB3,BANKED				; entry from: 1D96h
		bsf		0xB3,1,BANKED
		bra		p_1D7E
p_1DA2:	movf	0x3D,W					; entry from: 1D7Ch
		movwf	0xC4,BANKED
		btfsc	STATUS,2
		incf	0x3D
		btfss	0x3E,6
		bra		p_1DB8
		rcall	p_1FEA
p_1DB0:	rcall	p_2096					; entry from: 1DBAh
		call	p__404
		bra		p_1E00
p_1DB8:	btfss	0x3E,7					; entry from: 1DACh
		bra		p_1DB0
		bsf		0x3E,4
		call	p__844
		tstfsz	0xC4,BANKED
		bra		p_1DD4
		call	p__B64
		movwf	0x3D
		xorlw	5
		movlw	3
		btfsc	STATUS,2
		movwf	0x3D
p_1DD4:	movf	0x3D,W					; entry from: 1DC4h
		cpfslt	0xC3,BANKED
		bra		p_1DDE
		movlw	1
		movwf	0x3D

p_1DDE:	rcall	p_2096					; entry from: 1DD8h,1E22h
		iorlw	0
		bnz		p_1DEA
		clrf	0x98,BANKED
		movlw	0x32
		rcall	p_1F5A
p_1DEA:	movwf	0x3F						; entry from: 1DE2h
		bcf		0x72,3,BANKED
		xorlw	0
		bnz		p_1E06
		btfss	0x3E,0
		bra		p_1DFA
		btfsc	0x11,1
		bra		p_1E06
p_1DFA:	bcf		0x3E,4					; entry from: 1DF4h
		btfsc	0x17,5
		bsf		0x3E,2
p_1E00:	bsf		0x3E,3					; entry from: 1DB6h
		clrf	0x3F
		bra		p_1D82

p_1E06:	rcall	p_1FEA					; entry from: 1DF0h,1DF8h
		movf	0x3D,W
		xorlw	3
		btfsc	STATUS,2
		incf	0x3D
		movf	0x3D,W
		xorlw	4
		btfsc	STATUS,2
		incf	0x3D
		movf	0xC3,W,BANKED
		cpfslt	0x3D
		clrf	0x3D
		incf	0x3D
		btfss	0x19,7
		bra		p_1DDE
		movff	0xC4,0x3D
		bra		p_1D34
p_1E2A:	btfss	0x2C,4					; entry from: 18C0h
		bra		p_1E34
		bcf		0x2C,7
		bcf		0x2C,4
		retlw	0
p_1E34:	movff	0x22,0					; entry from: 1E2Ch
		movff	0x23,1
		movff	0x24,2
		movff	0x25,3
		movff	0x26,4
		movff	0x27,5
		call	p__40C
		movff	0x28,6
		movff	0x29,7
		rcall	p_1EDC
		call	p__40C
		btfss	0x2C,6
		retlw	0
		movf	0x86,W,BANKED
		cpfslt	0x2A
		retlw	0
		rcall	p_1FEA
		call	p__724
		movlw	5
		movwf	0x3F
		goto	p__66A

p_1E76:	bcf		0x3E,3					; entry from: 0CCCh,1BB4h,1C2Ah,1CA0h,1D1Eh
		bcf		0x11,2
		bcf		0x2C,7
		bcf		0x11,7
		bcf		0x11,0
		clrf	0xC7,BANKED
		rcall	p_204A
		btfss	0x3E,6
		return	
		btfsc	0x17,5
		bsf		0x3E,2
		retlw	0

p_1E8E:	cpfslt	0x7E,BANKED				; entry from: 22C0h,275Ch,3356h
		movf	0x7E,W,BANKED
		movwf	0x8F,BANKED
p_1E94:	btfsc	0x11,0					; entry from: 1EB8h
		bra		p_1EAC
		btfsc	0xF,0
		bra		p_1EA2
		bsf		0x11,0
		bsf		0x3E,6
		bra		p_1EAC
p_1EA2:	movf	0x3D,W					; entry from: 1E9Ah
		movwf	0xC6,BANKED
		incf	0xC7,f,BANKED
		btfsc	0xC7,3,BANKED
		retlw	8

p_1EAC:	movf	0x8F,W,BANKED			; entry from: 1E96h,1EA0h
		call	p_1FE2
		bcf		0x72,3,BANKED
		movwf	0x3F
		movf	0x3F
		bz		p_1E94
		btfss	0x3E,6
		return	
		btfss	0xF,5
		bsf		0x11,3
		retlw	0

p_1EC4:	movlw	0						; entry from: 1B40h,1BBCh,229Ah,2736h,30FAh
		movwf	FSR0H
		movlw	0
		movwf	FSR0L
		movff	POSTINC0,0x42
		movlw	0xC
		addlw	0xFD
		cpfslt	0x42
		retlw	0x75
		rcall	p_20E4
		return	

p_1EDC:	movlw	0						; entry from: 1B46h,1BC0h,1E58h,22A2h,273Eh
		movwf	FSR0H
		movlw	1
		movwf	FSR0L
		clrf	0x11
		bsf		0x11,6
		movf	0x93,W,BANKED
		btfsc	0x10,4
		bra		p_1EF8
		movf	3,W
		btfsc	0x3E,1
		bra		p_1EF8
		btfss	1,2
		incf	2,W

p_1EF8:	movwf	0x15						; entry from: 1EECh,1EF2h
		call	p__40C
		rcall	p_2130
		call	p__ADA
		call	p__40C
		clrf	0x2A
		bcf		0x2C,6
		movlw	5
		movwf	0x2B
		retlw	0

p_1F12:	btfsc	0x11,7					; entry from: 1B5Ch,1D7Eh
		btfsc	0x3E,3
		bra		p_1F58
		btfsc	0x18,4
		btfss	0x3E,6
		bra		p_1F58
		movf	0x83,W,BANKED
		bz		p_1F58
		movlw	4
		cpfsgt	0x84,BANKED
		bra		p_1F58
		movf	0x83,W,BANKED
		movwf	0x7F,BANKED
		bcf		STATUS,0
		btfsc	0x84,5,BANKED
		bra		p_1F40
		movlw	0x10
		cpfslt	0x84,BANKED
		bra		p_1F42
		btfss	0x18,3
		rlcf	0x7F,f,BANKED
		bnc		p_1F48
		bra		p_1F58
p_1F40:	rrcf	0x7F,f,BANKED			; entry from: 1F30h
p_1F42:	bcf		STATUS,0				; entry from: 1F36h
		btfsc	0x18,3
		rrcf	0x7F,f,BANKED
p_1F48:	movf	0x83,W,BANKED			; entry from: 1F3Ch
		addwf	0x7F,f,BANKED
		bc		p_1F58
		movlw	0xF
		cpfsgt	0x83,BANKED
		incf	0x7F,f,BANKED
		movf	0x7F,W,BANKED
		cpfsgt	0x7E,BANKED

p_1F58:	movf	0x7E,W,BANKED			; entry from: 1F16h,1F1Ch,1F20h,1F26h,1F3Eh,1F4Ch

p_1F5A:	btfss	0x97,2,BANKED			; entry from: 1BE4h,1DE8h,1FE8h,315Eh
		bra		p_1F76
		movf	0x7E,W,BANKED
		btfss	0x3E,6
		movlw	0x3E
		cpfslt	0x7E,BANKED
		movf	0x7E,W,BANKED
		movwf	0x41
		movlw	0xFF
		btfsc	0xB3,3,BANKED
		btfsc	0xB3,4,BANKED
		movf	0x41,W
		btfsc	0xB3,1,BANKED
		movf	0x41,W
p_1F76:	movwf	0x7F,BANKED				; entry from: 1F5Ch
		call	p__63E
		clrf	0x21
		bcf		0xF,1
		bcf		0x10,7
		rcall	p_2186
		call	p__ADA
		call	p__63E
		bsf		0x11,7
		btfss	0x1B,4
		bra		p_1F98
		btfsc	0x17,1
		call	p__7BE
p_1F98:	movlw	3						; entry from: 1F90h
		cpfsgt	0
		bsf		0xF,0
		btfsc	0x72,3,BANKED
		bra		p_1FBE
		btfss	0xF,0
		bra		p_1FB6
		call	p__63E
		movlw	0xD8
		btfsc	0x3E,0
		btfss	0x11,1
		movlw	0x76
		call	p__730
p_1FB6:	call	p__63E					; entry from: 1FA4h
		call	p__724
p_1FBE:	btfsc	0xF,0					; entry from: 1FA0h
		bra		p_1FCA
		clrf	0x2A
		bcf		0x2C,6
		movlw	5
		movwf	0x2B
p_1FCA:	btfss	0x10,7					; entry from: 1FC0h
		bra		p_1FDC
		call	p_3CC4
		btfsc	RXB0CON,3
		call	p_3E1E
		call	p_3CB0
p_1FDC:	btfss	0x19,7					; entry from: 1FCCh
		retlw	0
		retlw	1

p_1FE2:	btfsc	0x17,3					; entry from: 1EAEh,22B4h,2750h,334Ah
		btfss	0xF,5
		bsf		0x72,3,BANKED
		bra		p_1F5A

p_1FEA:	tstfsz	0x3D						; entry from: 0AACh,1226h,12E0h,15D0h,1BF8h,1C28h,1C3Eh,1CB6h,1DAEh,1E06h,1E68h
		call	p_21D2
p_1FF0:	movlw	0xB5					; entry from: 3D40h
		andwf	0x3E
		bcf		0x2C,7
		bcf		0x2C,4
		clrf	0x97,BANKED
		clrf	0x83,BANKED
		retlw	0
p_1FFE:	movff	0x3D,0x41					; entry from: 0AC0h
		dcfsnz	0x41
		bra		p_2266
		dcfsnz	0x41
		goto	p_2702
		dcfsnz	0x41
		goto	p_2B54
		dcfsnz	0x41
		goto	p_306E
		dcfsnz	0x41
		goto	p_306E
		dcfsnz	0x41
		goto	p_320C
		dcfsnz	0x41
		goto	p_320C
		dcfsnz	0x41
		goto	p_320C
		dcfsnz	0x41
		goto	p_320C
		dcfsnz	0x41
		goto	p_320C
		dcfsnz	0x41
		goto	p_320C
		dcfsnz	0x41
		goto	p_320C
		retlw	0x74
p_204A:	movff	0x3D,0x41					; entry from: 1E82h
		dcfsnz	0x41
		bra		p_2274
		dcfsnz	0x41
		goto	p_2710
		dcfsnz	0x41
		goto	p_2B4E
		dcfsnz	0x41
		goto	p_3068
		dcfsnz	0x41
		goto	p_30DE
		dcfsnz	0x41
		goto	p_31F8
		dcfsnz	0x41
		goto	p_3EAC
		dcfsnz	0x41
		goto	p_3EC2
		dcfsnz	0x41
		goto	p_3ED8
		dcfsnz	0x41
		goto	p_3EEE
		dcfsnz	0x41
		goto	p_3F04
		dcfsnz	0x41
		goto	p_3F1A
		retlw	0x74

p_2096:	bcf		0x2D,4					; entry from: 1DB0h,1DDEh
		movff	0x3D,0x41
		dcfsnz	0x41
		bra		p_22C2
		dcfsnz	0x41
		goto	p_2760
		dcfsnz	0x41
		goto	p_2E98
		dcfsnz	0x41
		goto	p_2E98
		dcfsnz	0x41
		goto	p_2E98
		dcfsnz	0x41
		goto	p_31FC
		dcfsnz	0x41
		goto	p_3EB2
		dcfsnz	0x41
		goto	p_3EC8
		dcfsnz	0x41
		goto	p_3EDE
		dcfsnz	0x41
		goto	p_3EF4
		dcfsnz	0x41
		goto	p_3F0A
		dcfsnz	0x41
		goto	p_3F20
		retlw	0x74
p_20E4:	movff	0x3D,0x41					; entry from: 1ED8h
		dcfsnz	0x41
		bra		p_22C8
		dcfsnz	0x41
		goto	p_2764
		dcfsnz	0x41
		goto	p_2D7A
		dcfsnz	0x41
		goto	p_31BA
		dcfsnz	0x41
		goto	p_31BA
		dcfsnz	0x41
		goto	p_3368
		dcfsnz	0x41
		goto	p_3368
		dcfsnz	0x41
		goto	p_3368
		dcfsnz	0x41
		goto	p_3368
		dcfsnz	0x41
		goto	p_3368
		dcfsnz	0x41
		goto	p_3368
		dcfsnz	0x41
		goto	p_3368
		retlw	0x74
p_2130:	movff	0x3D,0x41					; entry from: 1EFEh
		movlw	7
		cpfsgt	0x41
		bra		p_215C
		subwf	0x41
		dcfsnz	0x41
		goto	p_3474
		dcfsnz	0x41
		goto	p_3474
		dcfsnz	0x41
		goto	p_3474
		dcfsnz	0x41
		goto	p_3474
		dcfsnz	0x41
		goto	p_3474
		retlw	0x74
p_215C:	dcfsnz	0x41						; entry from: 2138h
		bra		p_230C
		dcfsnz	0x41
		goto	p_2790
		dcfsnz	0x41
		goto	p_2DA6
		dcfsnz	0x41
		goto	p_2DA6
		dcfsnz	0x41
		goto	p_2DA6
		dcfsnz	0x41
		goto	p_3474
		dcfsnz	0x41
		goto	p_3474
		retlw	0x74
p_2186:	movff	0x3D,0x41					; entry from: 1F82h
		dcfsnz	0x41
		bra		p_2420
		dcfsnz	0x41
		goto	p_28D4
		dcfsnz	0x41
		goto	p_2DD2
		dcfsnz	0x41
		goto	p_2DD2
		dcfsnz	0x41
		goto	p_2DD2
		dcfsnz	0x41
		goto	p_3544
		dcfsnz	0x41
		goto	p_3544
		dcfsnz	0x41
		goto	p_3544
		dcfsnz	0x41
		goto	p_3544
		dcfsnz	0x41
		goto	p_3544
		dcfsnz	0x41
		goto	p_3544
		dcfsnz	0x41
		goto	p_3544
		retlw	0x74
p_21D2:	movff	0x3D,0x41					; entry from: 1FECh
		dcfsnz	0x41
		goto	p_260A
		dcfsnz	0x41
		goto	p_2B18
		dcfsnz	0x41
		goto	p_2E98
		dcfsnz	0x41
		goto	p_2E98
		dcfsnz	0x41
		goto	p_2E98
		dcfsnz	0x41
		goto	p_3B64
		dcfsnz	0x41
		goto	p_3B64
		dcfsnz	0x41
		goto	p_3B64
		dcfsnz	0x41
		goto	p_3B64
		dcfsnz	0x41
		goto	p_3B64
		dcfsnz	0x41
		goto	p_3B64
		dcfsnz	0x41
		goto	p_3B64
		retlw	0x74

p_2220:	movff	0x3D,0x41					; entry from: 0F32h,0FAAh
		tstfsz	0x41
		dcfsnz	0x41
		return	
		dcfsnz	0x41
		return	
		dcfsnz	0x41
		return	
		dcfsnz	0x41
		return	
		dcfsnz	0x41
		return	
		dcfsnz	0x41
		goto	p_3202
		dcfsnz	0x41
		goto	p_3EB8
		dcfsnz	0x41
		goto	p_3ECE
		dcfsnz	0x41
		goto	p_3EE4
		dcfsnz	0x41
		goto	p_3EFA
		dcfsnz	0x41
		goto	p_3F10
		dcfsnz	0x41
		goto	p_3F26
		retlw	0x74

p_2266:	movlw	0x61						; entry from: 2004h,2284h
		movwf	0x12
		movlw	0x6A
		movwf	0x13
		movf	0x94,W,BANKED
		movwf	0x14
		return	
p_2274:	rcall	p_22C2					; entry from: 2050h
p_2276:	btfsc	0x19,7					; entry from: 2280h
		retlw	1
		call	p__5D4
		tstfsz	0x3C
		bra		p_2276
		btfss	0xF,4
		call	p_2266
		btfss	0x19,6
		bra		p_2290
		bsf		0x3E,6
		retlw	0
p_2290:	movlw	1						; entry from: 228Ah
		movwf	4
		clrf	5
		movlw	2
		movwf	0
		call	p_1EC4
		call	p__ADA
		call	p_1EDC
		call	p__ADA
		movlw	0x19
		cpfslt	0x7E,BANKED
		movf	0x7E,W,BANKED
		btfss	0xF,6
		movlw	0x19
		call	p_1FE2
		bcf		0x72,3,BANKED
		call	p__ADA
		movlw	0x19
		bra		p_1E8E

p_22C2:	bcf		LATA,2					; entry from: 209Eh,2274h
		bcf		LATC,3
		bra		p_26E8
p_22C8:	movlw	0xFF					; entry from: 20EAh
		movwf	0x40
		movlw	0x61
		btfss	0x3E,4
		movf	0x12,W
		movwf	POSTINC0
		call	p_264E
		movlw	0x6A
		btfss	0x3E,4
		movf	0x13,W
		movwf	POSTINC0
		call	p_264E
		movf	0x94,W,BANKED
		btfss	0x3E,4
		movf	0x14,W
		movwf	POSTINC0
		call	p_264E

p_22F0:	movf	POSTINC0,W				; entry from: 22FCh,278Ch
		call	p_264E
		call	p__5D4
		decfsz	0x42
		bra		p_22F0
		comf	0x40,W
		movwf	INDF0
		movlw	4
		addwf	0
		btfsc	0x19,7
		retlw	1
		retlw	0
p_230C:	movlw	0x7A						; entry from: 215Eh
		call	p__B12
		movlw	4
		movwf	0x92,BANKED
p_2316:	btfsc	0xF,1					; entry from: 23D4h
		retlw	4
		movlw	0
		movwf	FSR0H
		movlw	1
		movwf	FSR0L
		btfsc	PORTC,2
		bra		p_233E
p_2326:	movlw	9						; entry from: 2344h
		movwf	0x41
p_232A:	call	p__5D4					; entry from: 233Ch
		btfsc	0xF,1
		retlw	4
		btfss	0x18,6
		clrf	0x41
		dcfsnz	0x41
		retlw	5
		btfss	PORTC,2
		bra		p_232A
p_233E:	movlw	0x3A						; entry from: 2324h
		movwf	0x41
p_2342:	btfss	PORTC,2					; entry from: 2348h
		bra		p_2326
		decfsz	0x41
		bra		p_2342
		btfsc	0x19,7
		retlw	1
		movlw	0x10
		movwf	0x41
p_2352:	dcfsnz	0x41						; entry from: 2358h
		bra		p_235A
		btfsc	PORTC,2
		bra		p_2352
p_235A:	bsf		LATA,2					; entry from: 2354h
		bsf		LATC,3
		bcf		LATB,7
		clrf	0x1D
		movlw	8
		call	p__B5C
		btfss	PORTC,2
		bra		p_2372
		bcf		LATA,2
		bcf		LATC,3
		retlw	3
p_2372:	movlw	0x14						; entry from: 236Ah
		call	p__B5A
		bcf		LATA,2
		bcf		LATC,3
		nop
		call	p__B40
		movf	0,W
		movwf	0x42
		call	p__5D4
		call	p__B46
p_238E:	call	p__B42					; entry from: 23A0h
		movf	POSTINC0,W
		call	p_23D8
		movwf	0x3F
		tstfsz	0x3F
		bra		p_23A4
		decfsz	0x42
		bra		p_238E
		retlw	0
p_23A4:	clrf	0x8F,BANKED				; entry from: 239Ch
		clrf	0x90,BANKED
		clrf	0x44
p_23AA:	movf	PORTC,W					; entry from: 23BAh
		andlw	4
		xorwf	0x8F,W,BANKED
		bz		p_23D6
		xorwf	0x8F,f,BANKED
		incfsz	0x90,W,BANKED
		incf	0x90,f,BANKED
p_23B8:	decfsz	0x44						; entry from: 23D6h
		bra		p_23AA
		movlw	5
		cpfslt	0x90,BANKED
		bra		p_23C6
		dcfsnz	0x92,f,BANKED
		retlw	5
p_23C6:	clrf	0x44						; entry from: 23C0h
p_23C8:	call	p__B4E					; entry from: 23D2h
		btfsc	0x19,7
		retlw	1
		decfsz	0x44
		bra		p_23C8
		bra		p_2316
p_23D6:	bra		p_23B8					; entry from: 23B0h

p_23D8:	bsf		LATA,2					; entry from: 2394h,25FEh
		bsf		LATC,3
		movwf	0x43
		movlw	8
		movwf	0x44
p_23E2:	call	p__5D4					; entry from: 241Eh
		rlcf	0x43
		btfss	STATUS,0
		bra		p_23FE
		bcf		LATA,2
		bcf		LATC,3
		call	p__5D4
		btfss	PORTC,2
		retlw	9
		call	p__B46
		bra		p_2410
p_23FE:	call	p__5D4					; entry from: 23EAh
		call	p__B44
		nop
		bcf		LATA,2
		bcf		LATC,3
		call	p__B48
p_2410:	dcfsnz	0x44						; entry from: 23FCh
		retlw	0
		call	p__5D4
		bsf		LATA,2
		bsf		LATC,3
		nop
		bra		p_23E2

p_2420:	movlw	0x80						; entry from: 218Ch,2510h,25DEh,264Ah
		movwf	0x1B
		movlw	0xFF
		movwf	0x40
		movlw	0
		movwf	FSR0H
		movlw	1
		movwf	FSR0L
		bcf		0xF,2

p_2432:	btfss	PORTC,2					; entry from: 24A0h,24B0h,24C4h,24E0h,24F6h
		bra		p_24B2
		btfsc	PORTC,4
		btfsc	PIR1,5
		bsf		0x19,7
		btfsc	0x19,7
		retlw	1
		btfsc	0xF,1
		bra		p_2508
		btfss	PORTC,2
		bra		p_24B2
		btfsc	TMR0L,7
		setf	0x1A
		tstfsz	0x1A
		btfsc	TMR0L,7
		bra		p_2496
		btfss	PORTC,2
		bra		p_24B2
		decfsz	0x2B
		bra		p_2460
		movlw	5
		movwf	0x2B
		incf	0x2A
p_2460:	movf	0x86,W,BANKED			; entry from: 2458h
		btfss	PORTC,2
		bra		p_24B2
		cpfslt	0x2A
		bsf		0x2C,6
		incf	0x21
		movf	0x7F,W,BANKED
		cpfslt	0x21
		bsf		0xF,1
		bcf		0x1A,7
		btfss	PORTC,2
		bra		p_24B2
		incf	0x1D
		btfsc	0x1D,3
		bsf		LATB,7
		incf	0x1E
		btfsc	0x1E,3
		bsf		LATB,6
		btfss	PORTC,2
		bra		p_24B2
		incf	0x1F
		btfsc	0x1F,3
		bsf		LATB,5
		incf	0x20
		btfsc	0x20,3
		bsf		LATB,4
		clrf	0x1A
p_2496:	btfss	PORTC,2					; entry from: 2450h
		bra		p_24B2
		movf	FSR1L,W
		cpfseq	FSR2L
		btfss	PIR1,4
		bra		p_2432
		btfss	PORTC,2
		bra		p_24B2
		clrf	0x1F
		bcf		LATB,5
		movf	POSTINC2,W
		bcf		FSR2H,0
		movwf	TXREG1
		bra		p_2432

p_24B2:	movf	0x86,W,BANKED			; entry from: 2434h,2446h,2454h,2464h,2476h,2486h,2498h,24A4h
		cpfslt	0x2A
		bsf		0x2C,6
		clrf	0x1E
		bcf		LATB,6
		bra		p_24BE
p_24BE:	movlw	0x11						; entry from: 24BCh
		movwf	0x41
p_24C2:	btfsc	PORTC,2					; entry from: 24C8h
		bra		p_2432
		decfsz	0x41
		bra		p_24C2
		movlw	0xB
		movwf	0x41
p_24CE:	btfsc	PORTC,2					; entry from: 24D4h
		bra		p_2512
		decfsz	0x41
		bra		p_24CE
		call	p__5D4
		call	p__5D4
		btfsc	PORTC,2
		bra		p_2432
		btfss	0x18,6
		bra		p_24F4
		btfss	0x3E,3
		retlw	5
		movlw	0x56
		call	p__730
		call	p__724

p_24F4:	btfsc	PORTC,2					; entry from: 24E4h,2506h
		bra		p_2432
		clrf	0x1E
		call	p__5D4
		btfsc	0x19,7
		retlw	1
		btfss	0x3E,3
		btfss	0xF,1
		bra		p_24F4
p_2508:	btfss	0x3E,3					; entry from: 2442h
		retlw	8
		clrf	0x21
		bcf		0xF,1
		bra		p_2420
p_2512:	incf	0x21,W					; entry from: 24D0h
		movwf	0x82,BANKED
		call	p__B3E
		call	p__5D4
		clrf	0
p_2520:	rcall	p_262E					; entry from: 25D2h
		call	p__54E
		call	p__B3E
		rcall	p_2610
		call	p__B3A
		rcall	p_262E
		call	p__7AE
		rcall	p_2610
		call	p__B3A
		rcall	p_262E
		call	p__7AE
		rcall	p_2610
		call	p__B3A
		rcall	p_262E
		call	p__7AE
		rcall	p_2610
		call	p__B3A
		rcall	p_262E
		call	p__5D4
		call	p__B3E
		rcall	p_2610
		call	p__B3A
		rcall	p_262E
		call	p__5D4
		call	p__B3E
		rcall	p_2610
		call	p__B3A
		rcall	p_262E
		call	p__5D4
		call	p__B3E
		rcall	p_2610
		call	p__B3A
		rcall	p_262E
		call	p__5D4
		incf	0
		bsf		0x1B,4
		bcf		0xF,2
		movlw	0xD
		cpfslt	FSR0L
		bsf		0xF,2
		bsf		0xF,3
		btfsc	0x17,4
		bra		p_25A8
		movlw	0xB
		btfsc	0x11,2
		movlw	0xC
		cpfseq	0
		bcf		0xF,3
		bra		p_25B4
p_25A8:	btfss	0x19,7					; entry from: 259Ah
		bcf		0xF,3
		movf	0
		btfss	STATUS,2
		bcf		0xF,3
		nop
p_25B4:	call	p__B48					; entry from: 25A6h
		rcall	p_2610
		call	p__B44
		nop
		bcf		0xF,0
		movlw	0xC4
		cpfseq	0x40
		bsf		0xF,0
		movf	0x71,W,BANKED
		btfss	0xF,2
		movwf	POSTINC0
		movwf	0xE
		btfss	0xF,3
		bra		p_2520
		call	p__5D4
		call	p__5D4
p_25DC:	btfsc	0x1B,7					; entry from: 2646h
		bra		p_2420
		btfsc	0x11,2
		retlw	0
		btfsc	0x10,1
		bra		p_2604
		btfsc	0xF,0
		retlw	0
		btfsc	1,3
		retlw	0
p_25F0:	call	p__5D4					; entry from: 2606h
		call	p__5D4
		movf	0x94,W,BANKED
		btfss	0x10,2
		movf	0x14,W
		call	p_23D8
		retlw	0
p_2604:	btfsc	0x10,0					; entry from: 25E6h
		bra		p_25F0
		retlw	0

p_260A:	bcf		LATA,2					; entry from: 21D8h,2B18h
		bcf		LATC,3
		bra		p_26E8

p_2610:	bsf		0x71,7,BANKED			; entry from: 252Ah,2536h,2542h,254Eh,255Eh,256Eh,257Eh,25B8h
		rlncf	0x71,f,BANKED
		btfss	PORTC,2
		bcf		0x71,0,BANKED
		movlw	0x1D
		bcf		STATUS,0
		rlcf	0x40
		bc		p_2628
		btfsc	0x71,0,BANKED
		xorwf	0x40
		nop
		retlw	0
p_2628:	btfss	0x71,0,BANKED			; entry from: 261Eh
		xorwf	0x40
		retlw	0

p_262E:	movlw	5						; entry from: 2520h,2530h,253Ch,2548h,2554h,2564h,2574h,2584h
		movwf	0x41
p_2632:	btfss	PORTC,2					; entry from: 263Eh
		retlw	0
		dcfsnz	0x41
		bra		p_2640
		btfss	PORTC,2
		retlw	0
		bra		p_2632
p_2640:	pop								; entry from: 2638h
		bra		p_2644
p_2644:	tstfsz	0						; entry from: 2642h
		goto	p_25DC
		goto	p_2420

p_264E:	movwf	0x44						; entry from: 22D4h,22E0h,22ECh,22F2h,2770h,277Ch,2788h,2B48h
		movlw	0x1D
		bcf		STATUS,0
		rlcf	0x40
		bc		p_265E
		btfsc	0x44,7
		xorwf	0x40
		bra		p_2664
p_265E:	btfss	0x44,7					; entry from: 2656h
		xorwf	0x40
		bcf		STATUS,0
p_2664:	rlcf	0x40						; entry from: 265Ch
		bc		p_266E
		btfsc	0x44,6
		xorwf	0x40
		bra		p_2674
p_266E:	btfss	0x44,6					; entry from: 2666h
		xorwf	0x40
		bcf		STATUS,0
p_2674:	rlcf	0x40						; entry from: 266Ch
		bc		p_267E
		btfsc	0x44,5
		xorwf	0x40
		bra		p_2684
p_267E:	btfss	0x44,5					; entry from: 2676h
		xorwf	0x40
		bcf		STATUS,0
p_2684:	rlcf	0x40						; entry from: 267Ch
		bc		p_268E
		btfsc	0x44,4
		xorwf	0x40
		bra		p_2694
p_268E:	btfss	0x44,4					; entry from: 2686h
		xorwf	0x40
		bcf		STATUS,0
p_2694:	rlcf	0x40						; entry from: 268Ch
		bc		p_269E
		btfsc	0x44,3
		xorwf	0x40
		bra		p_26A4
p_269E:	btfss	0x44,3					; entry from: 2696h
		xorwf	0x40
		bcf		STATUS,0
p_26A4:	rlcf	0x40						; entry from: 269Ch
		bc		p_26AE
		btfsc	0x44,2
		xorwf	0x40
		bra		p_26B4
p_26AE:	btfss	0x44,2					; entry from: 26A6h
		xorwf	0x40
		bcf		STATUS,0
p_26B4:	rlcf	0x40						; entry from: 26ACh
		bc		p_26BE
		btfsc	0x44,1
		xorwf	0x40
		bra		p_26C4
p_26BE:	btfss	0x44,1					; entry from: 26B6h
		xorwf	0x40
		bcf		STATUS,0
p_26C4:	rlcf	0x40						; entry from: 26BCh
		bc		p_26CE
		btfsc	0x44,0
		xorwf	0x40
		bra		p_26D4
p_26CE:	btfss	0x44,0					; entry from: 26C6h
		xorwf	0x40
		nop
p_26D4:	bcf		0xF,0					; entry from: 26CCh
		movlw	0xC4
		cpfseq	0x40
		bsf		0xF,0
		return	

p_26DE:	movlw	2						; entry from: 0AB6h,15D8h,1D0Ch
		cpfseq	0x3D
		bra		p_26E8
p_26E4:	movlw	2						; entry from: 2762h
		bra		p_26EA

p_26E8:	movlw	0						; entry from: 22C6h,260Eh,26E2h
p_26EA:	xorwf	0x8E,W,BANKED			; entry from: 26E6h
		bnz		p_26F6
		btfss	LATA,1
		retlw	0
		bcf		LATA,1
		bra		p_26FC
p_26F6:	btfsc	LATA,1					; entry from: 26ECh
		retlw	0
		bsf		LATA,1
p_26FC:	movf	0x3B,W					; entry from: 26F4h
		movwf	0x3C
		retlw	0

p_2702:	movlw	0x68						; entry from: 2008h,2720h
		movwf	0x12
		movlw	0x6A
		movwf	0x13
		movf	0x94,W,BANKED
		movwf	0x14
		return	
p_2710:	rcall	p_2760					; entry from: 2054h
p_2712:	btfsc	0x19,7					; entry from: 271Ch
		retlw	1
		call	p__5D4
		tstfsz	0x3C
		bra		p_2712
		btfss	0xF,4
		call	p_2702
		btfss	0x19,6
		bra		p_272C
		bsf		0x3E,6
		retlw	0
p_272C:	movlw	1						; entry from: 2726h
		movwf	4
		clrf	5
		movlw	2
		movwf	0
		call	p_1EC4
		call	p__ADA
		call	p_1EDC
		call	p__ADA
		movlw	0x19
		cpfslt	0x7E,BANKED
		movf	0x7E,W,BANKED
		btfss	0xF,6
		movlw	0x19
		call	p_1FE2
		bcf		0x72,3,BANKED
		call	p__ADA
		movlw	0x19
		goto	p_1E8E

p_2760:	bcf		LATA,2					; entry from: 20A2h,2710h
		bra		p_26E4
p_2764:	movlw	0xFF					; entry from: 20EEh
		movwf	0x40
		movlw	0x68
		btfss	0x3E,4
		movf	0x12,W
		movwf	POSTINC0
		call	p_264E
		movlw	0x6A
		btfss	0x3E,4
		movf	0x13,W
		movwf	POSTINC0
		call	p_264E
		movf	0x94,W,BANKED
		btfss	0x3E,4
		movf	0x14,W
		movwf	POSTINC0
		call	p_264E
		goto	p_22F0
p_2790:	movlw	0x7A						; entry from: 2162h
		call	p__B12
		movlw	4
		movwf	0x92,BANKED
p_279A:	btfsc	0xF,1					; entry from: 2872h
		retlw	4
		movlw	0
		movwf	FSR0H
		movlw	1
		movwf	FSR0L
		btfss	PORTC,0
		bra		p_27D0
p_27AA:	clrf	0x41						; entry from: 27D6h
p_27AC:	call	p__5D4					; entry from: 27CEh
		btfsc	0xF,1
		retlw	4
		btfss	PORTC,0
		bra		p_27D0
		call	p__5D4
		btfss	0x18,6
		clrf	0x41
		dcfsnz	0x41
		retlw	5
		btfss	PORTC,0
		bra		p_27D0
		call	p__5D4
		btfsc	PORTC,0
		bra		p_27AC

p_27D0:	movlw	0xDF					; entry from: 27A8h,27B6h,27C6h
		movwf	0x41
p_27D4:	btfsc	PORTC,0					; entry from: 27DAh
		bra		p_27AA
		decfsz	0x41
		bra		p_27D4
		btfsc	0x19,7
		retlw	1
		movlw	0x10
		movwf	0x41
p_27E4:	dcfsnz	0x41						; entry from: 27EAh
		bra		p_27EC
		btfss	PORTC,0
		bra		p_27E4
p_27EC:	bsf		LATA,2					; entry from: 27E6h
		bcf		LATB,7
		clrf	0x1D
		call	p__B52
		btfsc	PORTC,0
		bra		p_27FE
		bcf		LATA,2
		retlw	3
p_27FE:	movlw	0x17						; entry from: 27F8h
		movwf	0x41
p_2802:	call	p__5D4					; entry from: 280Eh
		nop
		nop
		nop
		decfsz	0x41
		bra		p_2802
		movf	0,W
		movwf	0x42
p_2814:	movf	POSTINC0,W				; entry from: 2834h
		call	p_2876
		movwf	0x3F
		tstfsz	0x3F
		bra		p_283E
		movlw	8
		movwf	0x41
p_2824:	call	p__5D4					; entry from: 282Ch
		nop
		decfsz	0x41
		bra		p_2824
		call	p__B42
		decfsz	0x42
		bra		p_2814
		call	p__B48
		bcf		LATA,2
		retlw	0
p_283E:	setf	0x8F,BANKED				; entry from: 281Eh
		clrf	0x91,BANKED
		clrf	0x44
p_2844:	call	p__5D4					; entry from: 2858h
		movf	PORTC,W
		andlw	1
		xorwf	0x8F,W,BANKED
		bz		p_2874
		xorwf	0x8F,f,BANKED
		incfsz	0x91,W,BANKED
		incf	0x91,f,BANKED
p_2856:	decfsz	0x44						; entry from: 2874h
		bra		p_2844
		movlw	5
		cpfslt	0x91,BANKED
		bra		p_2864
		dcfsnz	0x92,f,BANKED
		retlw	5
p_2864:	clrf	0x44						; entry from: 285Eh
p_2866:	call	p__B4E					; entry from: 2870h
		btfsc	0x19,7
		retlw	1
		decfsz	0x44
		bra		p_2866
		bra		p_279A
p_2874:	bra		p_2856					; entry from: 284Eh

p_2876:	bcf		LATA,2					; entry from: 2816h,2ADCh
		movwf	0x43
		movlw	4
		movwf	0x44
p_287E:	nop								; entry from: 28D2h
		call	p__B50
		btfsc	PORTC,0
		retlw	9
		call	p__B50
		rlcf	0x43
		btfss	STATUS,0
		bra		p_28A4
		call	p__B50
		btfsc	PORTC,0
		retlw	9
		call	p__B50
		call	p__B40
		nop
p_28A4:	nop								; entry from: 2890h
		bsf		LATA,2
		rlcf	0x43
		btfsc	STATUS,0
		bra		p_28BC
		call	p__B50
		call	p__B50
		call	p__B3C
		nop
p_28BC:	dcfsnz	0x44						; entry from: 28ACh
		retlw	0
		call	p__B50
		call	p__B50
		call	p__B48
		nop
		bcf		LATA,2
		nop
		bra		p_287E

p_28D4:	movlw	0x80						; entry from: 2190h,295Ah,2AA0h
		movwf	0x1B
		movlw	0xFF
		movwf	0x40
		movlw	0
		movwf	FSR0H
		movlw	1
		movwf	FSR0L

p_28E4:	call	p__5D4					; entry from: 28F2h,28FEh,2912h,291Ah,2922h,2940h
		btfsc	0xF,1
		bra		p_2952
		btfsc	0x19,7
		retlw	1
		btfss	PORTC,0
		bra		p_28E4
		movlw	0x82
		movwf	0x41
		bcf		LATB,6
		clrf	0x1E
p_28FC:	btfss	PORTC,0					; entry from: 2902h
		bra		p_28E4
		decfsz	0x41
		bra		p_28FC
		movlw	0x3D
		call	p_2B2C
		iorlw	0
		bz		p_295C
		clrf	0x41
p_2910:	btfss	PORTC,0					; entry from: 292Ah
		bra		p_28E4
		call	p__5D4
		btfss	PORTC,0
		bra		p_28E4
		call	p__5D4
		btfss	PORTC,0
		bra		p_28E4
		call	p__5D4
		decfsz	0x41
		bra		p_2910
		btfss	0x18,6
		bra		p_293E
		btfss	0x3E,3
		retlw	5
		movlw	0x56
		call	p__730
		call	p__724

p_293E:	btfss	PORTC,0					; entry from: 292Eh,2950h
		bra		p_28E4
		clrf	0x1E
		call	p__5D4
		btfsc	0x19,7
		retlw	1
		btfss	0x3E,3
		btfss	0xF,1
		bra		p_293E
p_2952:	btfss	0x3E,3					; entry from: 28EAh
		retlw	8
		clrf	0x21
		bcf		0xF,1
		bra		p_28D4
p_295C:	incf	0x21,W					; entry from: 290Ch
		movwf	0x82,BANKED
		clrf	0
		clrf	0x71,BANKED
		call	p__B54
		call	p__B46
p_296C:	bcf		0x71,7,BANKED			; entry from: 2A8Eh
		call	p__B3C
		call	p__B3E
		call	p_2B1C
		iorlw	0
		bz		p_298A
		call	p_2B1C
		iorlw	0
		btfss	STATUS,2
		bra		p_2A9E
		bsf		0x71,7,BANKED
p_298A:	bsf		0x71,6,BANKED			; entry from: 297Ch
		call	p__54E
		call	p__B54
		call	p__B3E
		call	p_2B2A
		iorlw	0
		bz		p_29AC
		call	p_2B2A
		iorlw	0
		btfss	STATUS,2
		bra		p_2AF6
		bcf		0x71,6,BANKED
p_29AC:	bcf		0x71,5,BANKED			; entry from: 299Eh
		call	p__7AE
		call	p__7AE
		call	p__B3E
		call	p__B40
		call	p_2B1C
		iorlw	0
		bz		p_29D2
		call	p_2B1C
		iorlw	0
		btfss	STATUS,2
		bra		p_2B00
		bsf		0x71,5,BANKED
p_29D2:	bsf		0x71,4,BANKED			; entry from: 29C4h
		call	p__7AE
		call	p__B54
		call	p_2B2A
		iorlw	0
		bz		p_29F0
		call	p_2B2A
		iorlw	0
		btfss	STATUS,2
		bra		p_2B06
		bcf		0x71,4,BANKED
p_29F0:	bcf		0x71,3,BANKED			; entry from: 29E2h
		call	p__B52
		call	p__B3E
		call	p_2B1C
		iorlw	0
		bz		p_2A0E
		call	p_2B1C
		iorlw	0
		btfss	STATUS,2
		bra		p_2B08
		bsf		0x71,3,BANKED
p_2A0E:	bsf		0x71,2,BANKED			; entry from: 2A00h
		call	p__B52
		call	p__B3E
		call	p_2B2A
		iorlw	0
		bz		p_2A2C
		call	p_2B2A
		iorlw	0
		btfss	STATUS,2
		bra		p_2B0A
		bcf		0x71,2,BANKED
p_2A2C:	bcf		0x71,1,BANKED			; entry from: 2A1Eh
		call	p__B52
		call	p__B3E
		call	p_2B1C
		iorlw	0
		bz		p_2A4A
		call	p_2B1C
		iorlw	0
		btfss	STATUS,2
		bra		p_2B0C
		bsf		0x71,1,BANKED
p_2A4A:	bsf		0x71,0,BANKED			; entry from: 2A3Ch
		call	p__B52
		call	p__B3E
		call	p_2B2A
		iorlw	0
		bz		p_2A68
		call	p_2B2A
		iorlw	0
		btfss	STATUS,2
		bra		p_2B0E
		bcf		0x71,0,BANKED
p_2A68:	call	p_2B38					; entry from: 2A5Ah
		bsf		0xF,3
		btfsc	0x17,4
		bra		p_2A7E
		movlw	0xB
		btfsc	0x11,2
		movlw	0xC
		cpfseq	0
		bcf		0xF,3
		bra		p_2A8A
p_2A7E:	btfss	0x19,7					; entry from: 2A70h
		bcf		0xF,3
		movf	0
		btfss	STATUS,2
		bcf		0xF,3
		nop
p_2A8A:	bra		p_2A8C					; entry from: 2A7Ch
p_2A8C:	btfss	0xF,3					; entry from: 2A8Ah
		bra		p_296C
		movlw	0x15
		movwf	0x41
p_2A94:	call	p__5D4					; entry from: 2A9Ah
		decfsz	0x41
		bra		p_2A94
		nop

p_2A9E:	btfsc	0x1B,7					; entry from: 2986h,2B16h
		bra		p_28D4
		btfsc	0x11,2
		retlw	0
		btfsc	0x10,1
		bra		p_2AF0
		btfsc	0xF,0
		retlw	0
		btfsc	1,3
		retlw	0
p_2AB2:	movlw	4						; entry from: 2AF2h
		movwf	0x41
		bra		p_2AB8

p_2AB8:	call	p__5D4					; entry from: 2AB6h,2AC2h
		call	p__B48
		decfsz	0x41
		bra		p_2AB8
		bsf		LATA,2
		movlw	9
		movwf	0x41
p_2ACA:	call	p__5D4					; entry from: 2AD0h
		decfsz	0x41
		bra		p_2ACA
		call	p__B44
		movf	0x94,W,BANKED
		btfss	0x10,2
		movf	0x14,W
		call	p_2876
		call	p__B50
		call	p__B50
		call	p__B48
		bcf		LATA,2
		retlw	0
p_2AF0:	btfsc	0x10,0					; entry from: 2AA8h
		bra		p_2AB2
		retlw	0
p_2AF6:	bcf		0x71,6,BANKED			; entry from: 29A8h
		call	p__7AE
		call	p__7AE
p_2B00:	bcf		0x71,5,BANKED			; entry from: 29CEh
		call	p__7AE
p_2B06:	bcf		0x71,4,BANKED			; entry from: 29ECh
p_2B08:	bcf		0x71,3,BANKED			; entry from: 2A0Ah
p_2B0A:	bcf		0x71,2,BANKED			; entry from: 2A28h
p_2B0C:	bcf		0x71,1,BANKED			; entry from: 2A46h
p_2B0E:	bcf		0x71,0,BANKED			; entry from: 2A64h
		call	p_2B38
		bsf		0xF,0
		bra		p_2A9E
p_2B18:	goto	p_260A					; entry from: 21DEh

p_2B1C:	movlw	0x34						; entry from: 2976h,297Eh,29BEh,29C6h,29FAh,2A02h,2A36h,2A3Eh
		movwf	0x41
p_2B20:	btfsc	PORTC,0					; entry from: 2B26h
		retlw	0
		decfsz	0x41
		bra		p_2B20
		retlw	0xFF

p_2B2A:	movlw	0x34						; entry from: 2998h,29A0h,29DCh,29E4h,2A18h,2A20h,2A54h,2A5Ch
p_2B2C:	movwf	0x41						; entry from: 2906h
p_2B2E:	btfss	PORTC,0					; entry from: 2B34h
		retlw	0
		decfsz	0x41
		bra		p_2B2E
		retlw	0xFF

p_2B38:	incf	0						; entry from: 2A68h,2B10h
		bsf		0x1B,4
		movlw	0xD
		subwf	FSR0L,W
		movf	0x71,W,BANKED
		btfss	STATUS,0
		movwf	POSTINC0
		movwf	0xE
		call	p_264E
		return	
p_2B4E:	btfsc	0x19,6					; entry from: 205Ah
		bra		p_2D0A
		bra		p_2BC4

p_2B54:	btfsc	0xF,4					; entry from: 200Eh,2D0Ah
		bra		p_2B64
		movlw	0x68
		movwf	0x12
		movlw	0x6A
		movwf	0x13
		movf	0x94,W,BANKED
		movwf	0x14
p_2B64:	btfsc	0x2C,5					; entry from: 2B56h
		return	
		movlw	6
		movwf	0x22
		movlw	0x68
		movwf	0x23
		movwf	0x24
		movlw	2
		cpfsgt	0x22
		return	
		movff	0x24,0x25
		movlw	0x6A
		movwf	0x24
		addwf	0x25
		movlw	3
		cpfsgt	0x22
		return	
		movff	0x25,0x26
		movf	0x94,W,BANKED
		movwf	0x25
		addwf	0x26
		movlw	4
		cpfsgt	0x22
		return	
		movff	0x26,0x27
		movlw	1
		movwf	0x26
		addwf	0x27
		movlw	5
		cpfsgt	0x22
		return	
		movff	0x27,0x28
		movlw	0
		movwf	0x27
		addwf	0x28
		movlw	6
		cpfsgt	0x22
		return	
		movff	0x28,0x29
		movlw	0
		movwf	0x28
		addwf	0x29
		return	

p_2BC4:	movlw	0x60						; entry from: 2B52h,306Ch
		btfss	0x2D,7
		btfss	0x3E,7
		call	p__730
		bcf		0x8D,0,BANKED
		movlw	0x4B
		call	p_2F70
		call	p__ADA
		btfsc	0x19,7
		retlw	1
		movf	0xBF,W,BANKED
		movwf	0xE
		movwf	1
		clrf	0
		incf	0
		movlw	9
		movwf	0x41
		bra		p_2C16
p_2BEE:	btfss	0x2D,7					; entry from: 2C64h
		btfss	0x3E,7
		bra		p_2BFA
		call	p__B3C
		bra		p_2C10
p_2BFA:	dcfsnz	0x41,W					; entry from: 2BF2h
		bra		p_2C0A
		movlw	4
		xorwf	0x41,W
		bz		p_2C0A
		movlw	7
		xorwf	0x41,W
		bnz		p_2C10

p_2C0A:	movlw	0x2E						; entry from: 2BFCh,2C02h
		call	p__7F2

p_2C10:	rrcf	0xE						; entry from: 2BF8h,2C08h
		btfsc	STATUS,0
		bra		p_2C30
p_2C16:	bcf		LATB,7					; entry from: 2BECh
		movlw	3
		iorwf	LATB
		clrf	0x1D
		call	p__B3A
		call	p__B3A
		btfss	PORTC,1
		bra		p_2C38
		movlw	0xFC
		andwf	LATB
		retlw	3
p_2C30:	movlw	0xFC					; entry from: 2C14h
		andwf	LATB
		call	p__652
p_2C38:	movlw	0x64						; entry from: 2C28h
		movwf	0x43
		clrf	0x42
p_2C3E:	call	p__5D4					; entry from: 2C4Ah
		bra		p_2C44
p_2C44:	decfsz	0x42						; entry from: 2C42h
		bra		p_2C4A
		decfsz	0x43
p_2C4A:	bra		p_2C3E					; entry from: 2C46h
		movlw	0xAB
		movwf	0x42
p_2C50:	call	p__652					; entry from: 2C56h
		decfsz	0x42
		bra		p_2C50
		btfss	0x19,7
		bra		p_2C62
		movlw	0xFC
		andwf	LATB
		retlw	1
p_2C62:	decfsz	0x41						; entry from: 2C5Ah
		bra		p_2BEE
		call	p__5D4
		movlw	0xFC
		andwf	LATB
		movlw	0x64
		movwf	0x42
p_2C72:	call	p__5D4					; entry from: 2C7Ch
		decfsz	0x41
		bra		p_2C7C
		decfsz	0x42
p_2C7C:	bra		p_2C72					; entry from: 2C78h
		bsf		0x2D,5
		bsf		0x72,3,BANKED
		movlw	0xE6
		call	p_2EA0
		bcf		0x72,3,BANKED
		call	p__ADA
		movf	0x71,W,BANKED
		movwf	2
		incf	0
		movlw	0x55
		xorwf	2,W
		bz		p_2CA0
		movlw	0xFF
		call	p__ADA
p_2CA0:	bsf		0x72,3,BANKED			; entry from: 2C98h
		movlw	0x2D
		btfsc	0x2D,1
		movlw	0x87
		call	p_2EA0
		bcf		0x72,3,BANKED
		call	p__ADA
		movf	0x71,W,BANKED
		movwf	3
		movwf	0xC0,BANKED
		incf	0
		bsf		0x72,3,BANKED
		movlw	0x2D
		btfsc	0x2D,1
		movlw	0x87
		call	p_2EA0
		bcf		0x72,3,BANKED
		call	p__ADA
		movf	0x71,W,BANKED
		movwf	4
		movwf	0xC1,BANKED
		incf	0
		bsf		0x8D,0,BANKED
		movlw	3
		cpfseq	0x3D
		bra		p_2D38
		btfss	0x2D,7
		btfsc	0x2D,6
		bra		p_2CF4
		movf	3,W
		cpfseq	4
		bra		p_2D34
		movlw	8
		xorwf	3,W
		bz		p_2CF4
		movlw	0x94
		cpfseq	3
		bra		p_2D34

p_2CF4:	rcall	p_2FB6					; entry from: 2CE0h,2CECh,2D70h,2D78h
		call	p__ADA
		movlw	4
		cpfseq	0x3D
		bra		p_2D0A
		btfss	0x3E,4
		call	p__844
		movlw	3
		movwf	0x3D

p_2D0A:	call	p_2B54					; entry from: 2B50h,2CFEh
p_2D0E:	clrf	0x2A						; entry from: 2D60h
		bcf		0x2C,6
		movlw	5
		movwf	0x2B
		bsf		0x3E,6
		bsf		0x2C,7
		btfsc	0x19,6
		retlw	0
		bsf		0x11,3
		btfsc	0x2D,7
		bra		p_2D28
		btfsc	0x3E,7
		retlw	0
p_2D28:	movlw	0xB4					; entry from: 2D22h
		call	p__730
		call	p__724
		retlw	0

p_2D34:	btfss	0x3E,7					; entry from: 2CE6h,2CF2h
		retlw	0xFF
p_2D38:	btfss	0x2D,7					; entry from: 2CDAh
		btfsc	0x2D,6
		bra		p_2D44
		movlw	0x8F
		cpfseq	4
		bra		p_2D62
p_2D44:	rcall	p_2FB6					; entry from: 2D3Ch
		call	p__ADA
		movlw	3
		cpfseq	0x3D
		bra		p_2D5A
		btfss	0x3E,4
		call	p__844
		movlw	4
		movwf	0x3D

p_2D5A:	call	p_306E					; entry from: 2D4Eh,306Ah,319Ah
		bsf		0x3E,1
		bra		p_2D0E
p_2D62:	btfss	0x3E,7					; entry from: 2D42h
		retlw	0xFF
		movf	3,W
		cpfseq	4
		retlw	0xFF
		movlw	8
		xorwf	3,W
		bz		p_2CF4
		movlw	0x94
		cpfseq	3
		retlw	0xFF
		bra		p_2CF4
p_2D7A:	movlw	0x68						; entry from: 20F4h
		btfss	0x3E,4
		movf	0x12,W
		movwf	1
		movlw	0x6A
		btfss	0x3E,4
		movf	0x13,W
		movwf	2
		movf	0x94,W,BANKED
		btfss	0x3E,4
		movf	0x14,W
		movwf	3
p_2D92:	movlw	3						; entry from: 31D6h
		addwf	0x42
		movlw	0
p_2D98:	addwf	POSTINC0,W				; entry from: 2D9Ch
		decfsz	0x42
		bra		p_2D98
		movwf	INDF0
		movlw	4
		addwf	0
		retlw	0

p_2DA6:	call	p__40C					; entry from: 2168h,216Eh,2174h
		rcall	p_2F6E
		call	p__ADA
p_2DB0:	movff	0,0x44					; entry from: 3152h
p_2DB4:	movf	POSTINC0,W				; entry from: 2DD0h
		rcall	p_2FF6
		call	p__ADA
		dcfsnz	0x44
		retlw	0
		movlw	0xF2
		movwf	0x43
p_2DC4:	call	p__40C					; entry from: 2DCEh
		call	p__40C
		decfsz	0x43
		bra		p_2DC4
		bra		p_2DB4

p_2DD2:	movlw	0x80						; entry from: 2196h,219Ch,21A2h,2E94h
		movwf	0x1B
		clrf	0x40
		movlw	0
		movwf	FSR0H
		movlw	1
		movwf	FSR0L
		btfsc	PORTC,1
		bra		p_2DF8

p_2DE4:	call	p__5D4					; entry from: 2DF6h,2E0Eh
		btfsc	0x3E,3
		bra		p_2DF0
		btfsc	0xF,1
		retlw	8
p_2DF0:	btfsc	0x19,7					; entry from: 2DEAh
		retlw	1
		btfss	PORTC,1
		bra		p_2DE4
p_2DF8:	btfsc	0x11,2					; entry from: 2DE2h
		btfsc	0x2D,4
		bra		p_2E1A
		movf	0xC2,W,BANKED
		movwf	0x44
		clrf	0x43
p_2E04:	call	p__5D4					; entry from: 2E16h
		btfsc	0x19,7
		retlw	1
		btfss	PORTC,1
		bra		p_2DE4
		decfsz	0x43
		bra		p_2E16
		decfsz	0x44
p_2E16:	bra		p_2E04					; entry from: 2E12h
		bsf		0x2D,4

p_2E1A:	call	p__5D4					; entry from: 2DFCh,2E2Ch
		btfsc	0x3E,3
		bra		p_2E26
		btfsc	0xF,1
		retlw	8
p_2E26:	btfsc	0x19,7					; entry from: 2E20h
		retlw	1
		btfsc	PORTC,1
		bra		p_2E1A
		clrf	0
		incf	0x21,W
		movwf	0x82,BANKED
p_2E34:	rcall	p_2E9E					; entry from: 2E8Eh
		movwf	0x3F
		xorlw	8
		bz		p_2E92
		incf	0
		bsf		0x1B,4
		bcf		0xF,0
		movf	0x71,W,BANKED
		cpfseq	0x40
		bsf		0xF,0
		addwf	0x40
		movlw	0xD
		subwf	FSR0L,W
		movf	0x71,W,BANKED
		btfss	STATUS,0
		movwf	POSTINC0
		movwf	0xE
		movlw	4
		cpfsgt	0
		bra		p_2E82
		movlw	3
		cpfseq	0x3D
		bra		p_2E74
		movlw	0xB
		btfsc	0x11,2
		movlw	0xC
		btfss	0x17,4
		bra		p_2E86
		btfsc	0x19,7
		tstfsz	0
		bra		p_2E8A
		bra		p_2E92
p_2E74:	movlw	0x3F						; entry from: 2E60h
		andwf	1,W
		bnz		p_2E7C
		incf	4,W
p_2E7C:	addlw	4						; entry from: 2E78h
		xorwf	0,W
		bz		p_2E92
p_2E82:	bsf		0xF,0					; entry from: 2E5Ah
		bra		p_2E8A
p_2E86:	xorwf	0,W						; entry from: 2E6Ah
		bz		p_2E92

p_2E8A:	movf	0x3F,W					; entry from: 2E70h,2E84h
		xorlw	0xFF
		bnz		p_2E34
		bsf		0xF,0

p_2E92:	btfsc	0x1B,7					; entry from: 2E3Ah,2E72h,2E80h,2E88h
		bra		p_2DD2
		retlw	0

p_2E98:	bcf		LATB,0					; entry from: 20A8h,20AEh,20B4h,21E4h,21EAh,21F0h
		bcf		LATB,1
		retlw	0
p_2E9E:	decf	0xC2,W,BANKED			; entry from: 2E34h

p_2EA0:	movwf	0x44						; entry from: 2C84h,2CA8h,2CC2h,2FD6h
		btfss	PORTC,1
		bra		p_2EC8
		clrf	0x43
p_2EA8:	btfss	PORTC,1					; entry from: 2EBEh
		bra		p_2EC4
		call	p__5D4
		btfss	PORTC,1
		bra		p_2EC4
		btfss	PORTC,1
		bra		p_2EC4
		decfsz	0x43
		bra		p_2EBE
		decfsz	0x44
p_2EBE:	bra		p_2EA8					; entry from: 2EBAh
		bsf		0x2D,4
		retlw	8

p_2EC4:	call	p__B3C					; entry from: 2EAAh,2EB2h,2EB6h
p_2EC8:	bcf		LATB,6					; entry from: 2EA4h
		clrf	0x1E
		call	p__54E
		movlw	3
		cpfsgt	0x3D
		bra		p_2EEC
		btfsc	0x17,1
		bra		p_2EEE
		movlw	4
		cpfseq	0
		bra		p_2EF2
		movlw	0x3F
		andwf	1,W
		bnz		p_2EF6
		movlw	0x80
		movwf	0x1B
		bra		p_2EFA
p_2EEC:	bra		p_2EEE					; entry from: 2ED4h

p_2EEE:	nop								; entry from: 2ED8h,2EECh
		bra		p_2EF2

p_2EF2:	nop								; entry from: 2EDEh,2EF0h
		bra		p_2EF6

p_2EF6:	nop								; entry from: 2EE4h,2EF4h
		bra		p_2EFA

p_2EFA:	nop								; entry from: 2EEAh,2EF8h
		call	p__5D4
		movlw	1
		btfss	0x2D,1
		bra		p_2F16
		call	p__B42
		btfss	0x2D,0
		bra		p_2F16
		nop
		call	p__B3A
		movlw	8

p_2F16:	movwf	0x41						; entry from: 2F04h,2F0Ch
p_2F18:	call	p__5D4					; entry from: 2F1Eh
		decfsz	0x41
		bra		p_2F18
		movlw	9
		movwf	0x41
		movlw	6
		movwf	0x42
		bra		p_2F30
p_2F2A:	rrcf	0x71,f,BANKED			; entry from: 2F60h
		movlw	5
		movwf	0x42

p_2F30:	call	p__7AE					; entry from: 2F28h,2F3Ah
		call	p__5D4
		decfsz	0x42
		bra		p_2F30
		call	p__B3A
		btfss	0x2D,1
		bra		p_2F58
		call	p__B38
		btfss	0x2D,0
		bra		p_2F68
		movlw	0x10
		movwf	0x45
p_2F50:	call	p__5D4					; entry from: 2F56h
		decfsz	0x45
		bra		p_2F50

p_2F58:	bsf		STATUS,0				; entry from: 2F42h,2F6Ch
		btfss	PORTC,1
		bcf		STATUS,0
		decfsz	0x41
		bra		p_2F2A
		btfsc	STATUS,0
		retlw	0
		retlw	0xFF
p_2F68:	call	p__B3C					; entry from: 2F4Ah
		bra		p_2F58
p_2F6E:	movlw	0xF						; entry from: 2DAAh

p_2F70:	movwf	0x43						; entry from: 2BD2h,310Ch
		movlw	0xFF
		call	p__B12
		bcf		0x19,5
		btfss	PORTC,1
		bra		p_2F9E
p_2F7E:	movff	0x43,0x44					; entry from: 2FAAh

p_2F82:	call	p__40C					; entry from: 2F90h,2F96h
		btfsc	0xF,1
		retlw	4
		btfss	PORTC,1
		bra		p_2F9E
		btfss	0x19,5
		bra		p_2F82
		bcf		0x19,5
		decfsz	0x44
		bra		p_2F82
		call	p__40C
		retlw	0

p_2F9E:	clrf	0x44						; entry from: 2F7Ch,2F8Ch
p_2FA0:	call	p__40C					; entry from: 2FAEh
		btfsc	0xF,1
		retlw	4
		btfsc	PORTC,1
		bra		p_2F7E
		decfsz	0x44
		bra		p_2FA0
		call	p__40C
		retlw	5

p_2FB6:	movlw	8						; entry from: 2CF4h,2D44h
		movwf	0x41
		bcf		0x19,5

p_2FBC:	call	p__5D4					; entry from: 2FC2h,2FC8h
		btfss	0x19,5
		bra		p_2FBC
		bcf		0x19,5
		decfsz	0x41
		bra		p_2FBC
		comf	4,W
		movwf	5
		incf	0
		rcall	p_2FF6
		bsf		0x72,3,BANKED
		movlw	0x19
		rcall	p_2EA0
		bcf		0x72,3,BANKED
		movwf	0x3F
		iorlw	0
		bz		p_2FE6
		btfsc	0x2D,6
		retlw	0
		return	
p_2FE6:	movf	0x71,W,BANKED			; entry from: 2FDEh
		movwf	6
		incf	0
		comf	0x71,W,BANKED
		xorwf	0xBF,W,BANKED
		btfss	STATUS,2
		retlw	0xFF
		retlw	0

p_2FF6:	movwf	0xE						; entry from: 2DB6h,2FD0h
		bsf		LATB,0
		bcf		LATB,7
		clrf	0x1D
		call	p__40C
		btfss	PORTC,1
		bra		p_300A
		bcf		LATB,0
		retlw	3
p_300A:	movlw	9						; entry from: 3004h
		movwf	0x43
		bra		p_301A

p_3010:	nop								; entry from: 3052h,3056h
		call	p__40C
		call	p__B44
p_301A:	btfss	0x2D,1					; entry from: 300Eh
		bra		p_302C
		call	p__B48
		movlw	5
		btfss	0x2D,0
		bra		p_3032
		movlw	0xB
		bra		p_3032
p_302C:	call	p__40C					; entry from: 301Ch
		movlw	4

p_3032:	movwf	0x45						; entry from: 3026h,302Ah
		call	p__B48
p_3038:	call	p__40C					; entry from: 3042h
		call	p__5D4
		decfsz	0x45
		bra		p_3038
		dcfsnz	0x43
		bra		p_3058
		rrcf	0xE
		btfsc	STATUS,0
		bra		p_3054
		nop
		bsf		LATB,0
		bra		p_3010
p_3054:	bcf		LATB,0					; entry from: 304Ch
		bra		p_3010
p_3058:	nop								; entry from: 3046h
		bra		p_305C
p_305C:	bcf		LATB,0					; entry from: 305Ah
		call	p__40C
		call	p__40C
		retlw	0
p_3068:	btfsc	0x19,6					; entry from: 2060h
		bra		p_2D5A
		bra		p_2BC4

p_306E:	btfsc	0xF,4					; entry from: 2014h,201Ah,2D5Ah,30DEh
		bra		p_307E
		movlw	0xCF
		movwf	0x12
		movlw	0x33
		movwf	0x13
		movf	0x94,W,BANKED
		movwf	0x14
p_307E:	btfsc	0x2C,5					; entry from: 3070h
		return	
		movlw	5
		movwf	0x22
		movlw	0xC1
		movwf	0x23
		movwf	0x24
		movlw	2
		cpfsgt	0x22
		return	
		movff	0x24,0x25
		movlw	0x33
		movwf	0x24
		addwf	0x25
		movlw	3
		cpfsgt	0x22
		return	
		movff	0x25,0x26
		movf	0x94,W,BANKED
		movwf	0x25
		addwf	0x26
		movlw	4
		cpfsgt	0x22
		return	
		movff	0x26,0x27
		movlw	0x3E
		movwf	0x26
		addwf	0x27
		movlw	5
		cpfsgt	0x22
		return	
		movff	0x27,0x28
		movlw	0
		movwf	0x27
		addwf	0x28
		movlw	6
		cpfsgt	0x22
		return	
		movff	0x28,0x29
		movlw	0
		movwf	0x28
		addwf	0x29
		return	
p_30DE:	call	p_306E					; entry from: 2066h
		btfsc	0x19,6
		bra		p_319A
		movlw	0x60
		btfss	0x2D,7
		btfss	0x3E,7
		call	p__730
		bcf		0x8D,0,BANKED
		movlw	0x81
		movwf	4
		movlw	1
		movwf	0
		call	p_1EC4
		call	p__ADA
		movlw	0
		movwf	FSR0H
		movlw	1
		movwf	FSR0L
		movlw	0x4B
		call	p_2F70
		call	p__ADA
		btfsc	0x19,7
		retlw	1
		movlw	3
		iorwf	LATB
		bcf		LATB,7
		clrf	0x1D
		call	p__5D4
		call	p__B3A
		btfss	PORTC,1
		bra		p_3132
		movlw	0xFC
		andwf	LATB
		retlw	3
p_3132:	rcall	p_31A6					; entry from: 312Ah
		call	p__B52
		call	p__B46
		movlw	0xFC
		andwf	LATB
		rcall	p_31A6
		call	p__B3A
		call	p__B50
		clrf	0x11
		bsf		0x11,6
		movff	3,0x15
		call	p_2DB0
		call	p__ADA
p_315A:	movlw	0x7A						; entry from: 31A2h
		bsf		0x72,3,BANKED
		call	p_1F5A
		bcf		0x72,3,BANKED
		call	p__ADA
		movlw	7
		xorwf	0,W
		bnz		p_317C
		movff	5,0xC0
		movff	6,0xC1
		bsf		0x8D,0,BANKED
		movf	4,W
		bra		p_3192
p_317C:	movlw	8						; entry from: 316Ch
		xorwf	0,W
		bz		p_3186
		btfss	0x2D,6
		bra		p_319E
p_3186:	movff	6,0xC0					; entry from: 3180h
		movff	7,0xC1
		bsf		0x8D,0,BANKED
		movf	5,W
p_3192:	xorlw	0xC1					; entry from: 317Ah
		bz		p_319A
		btfss	0x2D,6
		bra		p_319E

p_319A:	goto	p_2D5A					; entry from: 30E4h,3194h

p_319E:	incf	0xC7,f,BANKED			; entry from: 3184h,3198h
		btfss	0xC7,4,BANKED
		bra		p_315A
		retlw	0xFF

p_31A6:	movlw	0xA						; entry from: 3132h,3140h
		movwf	0x42
		clrf	0x41
p_31AC:	call	p__652					; entry from: 31B6h
		decfsz	0x41
		bra		p_31B6
		decfsz	0x42
p_31B6:	bra		p_31AC					; entry from: 31B2h
		return	

p_31BA:	btfss	0x3E,4					; entry from: 20FAh,2100h
		bra		p_31DA
		movlw	0xCF
p_31C0:	andlw	0xC0					; entry from: 31E2h
		iorwf	0x42,W
p_31C4:	movwf	1						; entry from: 31F6h
		movlw	0x33
		btfss	0x3E,4
		movf	0x13,W
		movwf	2
		movf	0x94,W,BANKED
		btfss	0x3E,4
		movf	0x14,W
		movwf	3
		goto	p_2D92
p_31DA:	movf	0x12,W					; entry from: 31BCh
		andlw	0x3F
		bz		p_31E4
		movf	0x12,W
		bra		p_31C0
p_31E4:	rcall	p_344E					; entry from: 31DEh
		movlw	1
		movwf	FSR0L
		movff	0,4
		incf	0
		movff	0,0x42
		movf	0x12,W
		bra		p_31C4
p_31F8:	rcall	p_3202					; entry from: 206Ch
		bra		p_32EC
p_31FC:	rcall	p_3202					; entry from: 20BAh
		goto	p_335A

p_3202:	movlw	1						; entry from: 223Ch,31F8h,31FCh
		movwf	0x96,BANKED
		movlw	0x81
		goto	p_3D4A

p_320C:	clrf	0xB3,BANKED				; entry from: 2020h,2026h,202Ch,2032h,2038h,203Eh,2044h,32F4h,3362h

p_320E:	bcf		0x34,4					; entry from: 3476h,3590h
		bsf		0x34,2
		btfsc	0xF,4
		bra		p_3244
		btfss	0x97,2,BANKED
		bra		p_3228
		movlw	0xEA
		movwf	0x12
		movlw	0xFF
		movwf	0x13
		movf	0xBE,W,BANKED
		movwf	0x14
		bra		p_3244
p_3228:	btfss	0x37,7					; entry from: 3218h
		bra		p_3238
		clrf	0x12
		movlw	7
		movwf	0x13
		movlw	0xDF
		movwf	0x14
		bra		p_3244
p_3238:	movlw	0xDB					; entry from: 322Ah
		movwf	0x12
		movlw	0x33
		movwf	0x13
		movf	0x94,W,BANKED
		movwf	0x14

p_3244:	btfsc	0x34,7					; entry from: 3214h,3226h,3236h
		bra		p_3254
		movlw	0x18
		btfsc	0x37,7
		movlw	0
		btfsc	0x97,2,BANKED
		movlw	0x18
		movwf	0x2E
p_3254:	btfsc	0x34,6					; entry from: 3246h
		bra		p_329E
		btfss	0x37,7
		bra		p_3294
		clrf	0x9E,BANKED
		clrf	0x9F,BANKED
		movlw	7
		andwf	0x13,W
		xorlw	7
		bnz		p_328A
		movlw	0xDF
		xorwf	0x14,W
		bz		p_3280
		movlw	0xF0
		andwf	0x14,W
		xorlw	0xE0
		bnz		p_328A
		movlw	7
		movwf	0xA0,BANKED
		movlw	0xFF
		movwf	0xA1,BANKED
		bra		p_329E
p_3280:	movlw	7						; entry from: 326Ch
		movwf	0xA0,BANKED
		movlw	0xF8
		movwf	0xA1,BANKED
		bra		p_329E

p_328A:	movlw	7						; entry from: 3266h,3274h
		movwf	0xA0,BANKED
		movlw	0xC0
		movwf	0xA1,BANKED
		bra		p_329E
p_3294:	clrf	0x9E,BANKED				; entry from: 325Ah
		clrf	0x9F,BANKED
		movlw	0xFF
		movwf	0xA0,BANKED
		clrf	0xA1,BANKED

p_329E:	btfsc	0x34,5					; entry from: 3256h,327Eh,3288h,3292h
		retlw	0
		btfss	0x37,7
		bra		p_32E0
		clrf	0xA2,BANKED
		clrf	0xA3,BANKED
		movlw	7
		andwf	0x13,W
		xorlw	7
		bnz		p_32D6
		movlw	0xDF
		xorwf	0x14,W
		bz		p_32CC
		movlw	0xF0
		andwf	0x14,W
		xorlw	0xE0
		bnz		p_32D6
		movf	0x13,W
		movwf	0xA4,BANKED
		movf	0x14,W
		movwf	0xA5,BANKED
		bsf		0xA5,3,BANKED
		retlw	0
p_32CC:	movlw	7						; entry from: 32B6h
		movwf	0xA4,BANKED
		movlw	0xE8
		movwf	0xA5,BANKED
		retlw	0

p_32D6:	movf	0x13,W					; entry from: 32B0h,32BEh
		movwf	0xA4,BANKED
		movf	0x14,W
		movwf	0xA5,BANKED
		retlw	0
p_32E0:	clrf	0xA2,BANKED				; entry from: 32A4h
		clrf	0xA3,BANKED
		movf	0x14,W
		movwf	0xA4,BANKED
		clrf	0xA5,BANKED
		retlw	0

p_32EC:	call	p_3B8A					; entry from: 31FAh,3EAEh,3EC4h,3EDAh,3EF0h,3F06h,3F1Ch
		call	p__ADA
		rcall	p_320C
		call	p_3C82
		call	p__ADA
		btfss	0x19,6
		bra		p_3306
		bsf		0x3E,6
		retlw	0
p_3306:	btfss	0x97,2,BANKED			; entry from: 3300h
		bra		p_3318
		movlw	0xEE
		movwf	5
		movlw	0xFE
		movwf	6
		clrf	7
		bcf		0xF,5
		bra		p_332E
p_3318:	movlw	2						; entry from: 3308h
		movwf	5
		movlw	1
		movwf	6
		clrf	7
		movf	0x95,W,BANKED
		movwf	8
		movwf	9
		movwf	0xA
		movwf	0xB
		movwf	0xC
p_332E:	movlw	3						; entry from: 3316h
		movwf	0
		movwf	0x36
		rcall	p_33A0
		call	p__ADA
		rcall	p_34B0
		call	p__ADA
		movlw	0x19
		cpfslt	0x7E,BANKED
		movf	0x7E,W,BANKED
		btfss	0xF,6
		movlw	0x19
		call	p_1FE2
		bcf		0x72,3,BANKED
		call	p__ADA
		movlw	0x19
		goto	p_1E8E

p_335A:	call	p_3B8A					; entry from: 31FEh,3EB4h,3ECAh,3EE0h,3EF6h,3F0Ch,3F22h
		call	p__ADA
		rcall	p_320C
		bsf		0x34,3
		retlw	0

p_3368:	movf	0x42,W					; entry from: 2106h,210Ch,2112h,2118h,211Eh,2124h,212Ah
		movwf	0x36
		bnz		p_3372
		bsf		0x36,6
		bra		p_33A0
p_3372:	rcall	p_344E					; entry from: 336Ch
		movlw	5
		addwf	0,W
		movwf	FSR0L
		movlw	0xD
p_337C:	cpfslt	FSR0L					; entry from: 3384h
		bra		p_3386
		movff	0x95,POSTINC0
		bra		p_337C
p_3386:	btfsc	0x97,2,BANKED			; entry from: 337Eh
		bra		p_33A0
		btfsc	0x97,1,BANKED
		btfss	0x17,0
		bra		p_3396
		rcall	p_344E
		movf	0,W
		rcall	p_3466
p_3396:	btfss	0x18,1					; entry from: 338Eh
		bra		p_33A0
		rcall	p_344E
		movf	0x16,W
		rcall	p_3466

p_33A0:	movlw	4						; entry from: 3334h,3370h,3388h,3398h
		addwf	0
		btfsc	0x3E,4
		bra		p_33BA
		movff	0x2E,1
		movff	0x12,2
		movff	0x13,3
		movff	0x14,4
		retlw	0
p_33BA:	call	p_3CCE					; entry from: 33A6h
		btfsc	0x97,2,BANKED
		bra		p_33F2
		btfss	0x37,7
		bra		p_3410
		clrf	1
		clrf	2
		movlw	7
		movwf	3
		movlw	0xDF
		movwf	4
		clrf	0x2F
		clrf	0x30
		movlw	7
		movwf	0x31
		movlw	0xF8
		movwf	0x32
		rcall	p_3BDC
		call	p_3C64
		clrf	0x2F
		clrf	0x30
		movlw	7
		movwf	0x31
		movlw	0xE8
		movwf	0x32
		bra		p_3446
p_33F2:	movlw	0x18						; entry from: 33C0h
		movwf	1
		movlw	0xEA
		movwf	2
		movlw	0xFF
		movwf	3
		movf	0xBE,W,BANKED
		movwf	4
		clrf	0x38
		movlw	0xFE
		movwf	0x39
		movlw	0xEE
		movwf	0x3A
		goto	p_3E44
p_3410:	movlw	0x18						; entry from: 33C4h
		movwf	1
		movlw	0xDB
		movwf	2
		movlw	0x33
		movwf	3
		movf	0x94,W,BANKED
		movwf	4
		movlw	0
		movwf	0x2F
		movlw	0
		movwf	0x30
		movlw	0xFF
		movwf	0x31
		movlw	0
		movwf	0x32
		rcall	p_3BDC
		call	p_3C64
		movlw	0
		movwf	0x2F
		movlw	0
		movwf	0x30
		movf	0x94,W,BANKED
		movwf	0x31
		movlw	0
		movwf	0x32
p_3446:	rcall	p_3BDC					; entry from: 33F0h
		rcall	p_3C1A
		bsf		0x34,2
		retlw	0

p_344E:	movlw	4						; entry from: 31E4h,3372h,3390h,339Ah
		movwf	FSR0L
		movlw	8
		movwf	0x42
		movf	POSTINC0,W
p_3458:	movff	INDF0,0x41				; entry from: 3462h
		movwf	POSTINC0
		movf	0x41,W
		decfsz	0x42
		bra		p_3458
		return	

p_3466:	movwf	5						; entry from: 3394h,339Eh
		movlw	8
		cpfslt	0
		return	
		incf	0
		incf	0x36
		return	

p_3474:	btfsc	0x34,4					; entry from: 213Eh,2144h,214Ah,2150h,2156h,217Ah,2180h
		rcall	p_320E
		movlw	7
		btfsc	0x97,2,BANKED
		cpfseq	0
		bra		p_34A4
		btfsc	0x35,2
		bra		p_348C
		movf	7,W
		movff	5,7
		movwf	5
p_348C:	movff	5,0x3A					; entry from: 3482h
		movff	6,0x39
		movff	7,0x38
		bcf		0x35,0
		call	p_3E44
		call	p__ADA
		bra		p_34B0
p_34A4:	bsf		0x35,0					; entry from: 347Eh
		btfss	0x34,2
		bra		p_34B0
		rcall	p_3C82
		call	p__ADA

p_34B0:	movff	1,0x2F					; entry from: 333Ah,34A2h,34A8h
		movff	2,0x30
		movff	3,0x31
		movff	4,0x32
		rcall	p_3BDC
		btfss	0x97,2,BANKED
		btfss	0x37,7
		bsf		0x30,3
		movf	CANSTAT,W
		andlw	0xE0
		bnz		p_34D8
		btfsc	COMSTAT,5
		bra		p_34D6
		btfss	COMSTAT,0
		bra		p_34DC
p_34D6:	rcall	p_3CCE					; entry from: 34D0h
p_34D8:	movlw	0						; entry from: 34CCh
		rcall	p_3CD0
p_34DC:	call	p_3CC4					; entry from: 34D4h
		btfsc	RXB0CON,3
		call	p_3E1E
		btfsc	0x36,6
		bra		p_34F0
		movlw	8
		btfss	0x35,5
		btfsc	0x37,6
p_34F0:	movf	0x36,W					; entry from: 34E8h
		movwf	RXB0DLC
		addlw	4
		movwf	0
		bcf		0,6
		movff	0x2F,0xF61
		movff	0x30,0xF62
		movff	0x31,0xF63
		movff	0x32,0xF64
		movff	5,0xF66
		movff	6,0xF67
		movff	7,0xF68
		movff	8,0xF69
		movff	9,0xF6A
		movff	0xA,0xF6B
		movff	0xB,0xF6C
		movff	0xC,0xF6D
		call	p_3B7C
		clrf	PIR3
p_3530:	bcf		LATB,7					; entry from: 3B62h
		clrf	0x1D
		call	p_3CC4
		movlw	8
		movwf	RXB0CON
		call	p_3E1E
		goto	p_3CB0

p_3544:	bsf		0xB3,5,BANKED			; entry from: 21A8h,21AEh,21B4h,21BAh,21C0h,21C6h,21CCh
		movf	CANSTAT,W
		andlw	0xE0
		bnz		p_3552
		bcf		0xB3,5,BANKED
		btfss	0x35,0
		bcf		0x11,6
p_3552:	btfss	0xB3,1,BANKED			; entry from: 354Ah
		bra		p_3574
		movlw	0x70
		andwf	0x11,W
		bnz		p_3572
		call	p__63E
		call	p_3CCE
		movlb	0xE
		call	p_3E7C
		btfsc	0xB3,5,BANKED
		bra		p_3572
		movlw	0
		rcall	p_3CD0

p_3572:	clrf	0xB3,BANKED				; entry from: 355Ah,356Ch
p_3574:	bcf		0x1B,4					; entry from: 3554h
		bcf		0x34,1
		bcf		PIR5,5
		call	p__63E
		movf	0x98,f,BANKED
		btfsc	STATUS,2
		bcf		0x35,1
		movf	CANSTAT,W
		andlw	0xE0
		bz		p_35AA
		btfss	0x34,3
		bra		p_359A
		bcf		0x34,3
		rcall	p_320E
		rcall	p_3D5C
		call	p__ADA
		bra		p_35A8
p_359A:	movlw	0x60						; entry from: 358Ch
		xorwf	CANSTAT,W
		andlw	0xE0
		bz		p_35AA
p_35A2:	movlw	0x60						; entry from: 35F0h
		rcall	p_3CD0
		rcall	p_3B7C
p_35A8:	clrf	PIR3					; entry from: 3598h

p_35AA:	rcall	p_3CB0					; entry from: 3588h,35A0h,35D4h,35E8h,35FCh,3676h,3692h,36A4h,36C2h,36D0h,37E6h,38F2h,39ECh,39FCh,3A02h,3A08h,3A16h,3A1Ch,3A22h,3A32h,3AC2h,3ACCh,3AD2h
		call	p__5D4
		btfsc	0xF,1
		bra		p_35F2
		btfsc	0x19,7
		retlw	1
		btfsc	COMSTAT,0
		bcf		PIR5,5
		btfsc	0x34,1
		bra		p_35C8
		btfss	RXB0CON,7
		bra		p_35C8
		bsf		0x34,1
		bra		p_35FE

p_35C8:	bcf		0x34,1					; entry from: 35BEh,35C2h
		rcall	p_3CBA
		btfsc	RXB0CON,7
		bra		p_35FE
		btfsc	PIR5,7
		btfsc	0x3E,6
		bra		p_35AA
		incfsz	0x98,W,BANKED
		incf	0x98,f,BANKED
		bcf		PIR5,7
		call	p__63E
		btfss	0x3E,3
		bra		p_3672
		movlw	0x40
		cpfsgt	0x98,BANKED
		bra		p_35AA
		rcall	p_3CCE
		clrf	0x98,BANKED
		incf	0x98,f,BANKED
		bra		p_35A2

p_35F2:	btfss	0xB3,0,BANKED			; entry from: 35B2h,3678h
		btfss	0x3E,3
		retlw	8
		clrf	0x21
		bcf		0xF,1
		bra		p_35AA

p_35FE:	movff	RXB0SIDH,1					; entry from: 35C6h,35CEh
		movff	RXB0SIDL,2
		movff	RXB0EIDH,3
		movff	RXB0EIDL,4
		movff	RXB0D0,5
		movff	RXB0D1,6
		movff	RXB0D2,7
		movff	RXB0D3,8
		movff	RXB0D4,9
		movff	RXB0D5,0xA
		movff	RXB0D6,0xB
		movff	RXB0D7,0xC
		movff	RXB0DLC,0x36
		movff	PIR3,0x44
		bcf		RXB0CON,7
		clrf	PIR3
		bcf		LATB,6
		clrf	0x1E
		call	p__63E
		rcall	p_3CB0
		incf	0x21,W
		movwf	0x82,BANKED
		movlw	8
		btfsc	0x36,3
		movwf	0x36
		bcf		0xF,0
		bcf		0x11,1
		movf	COMSTAT,W
		andlw	0x7F
		bnz		p_3660
		btfsc	0x44,7
		bra		p_3660
		clrf	0x98,BANKED
		bra		p_36A6

p_3660:	clrf	COMSTAT						; entry from: 3656h,365Ah
		bsf		0xF,0
		bsf		0x11,1
		incfsz	0x98,W,BANKED
		incf	0x98,f,BANKED
		btfsc	0x3E,6
		bra		p_36A6
		btfsc	0x3E,3
		bra		p_367A
p_3672:	movlw	5						; entry from: 35E2h
		cpfsgt	0x98,BANKED
		bra		p_35AA
		bra		p_35F2
p_367A:	btfsc	COMSTAT,0					; entry from: 3670h
		bra		p_3684
		movlw	0x40
		cpfsgt	0x98,BANKED
		bra		p_3690
p_3684:	rcall	p_3CCE					; entry from: 367Ch
		clrf	0x98,BANKED
		incf	0x98,f,BANKED
		movlw	0x60
		rcall	p_3CD0
		rcall	p_3B7C
p_3690:	btfsc	0x35,1					; entry from: 3682h
		bra		p_35AA
		bsf		0x35,1
		btfsc	0x44,7
		bra		p_36A6
		movlw	0x6C
		call	p__730
		call	p__724
		bra		p_35AA

p_36A6:	bcf		0x34,0					; entry from: 365Eh,366Ch,3698h
		btfsc	0x36,6
		bsf		0x34,0
		movlw	0xF
		andwf	0x36
		btfsc	STATUS,2
		bsf		0x34,0
		call	p__63E
		btfss	0x34,0
		bra		p_36C4
		btfss	0x97,2,BANKED
		btfss	0x18,1
		bra		p_37C4
		bra		p_35AA
p_36C4:	btfsc	0x97,2,BANKED			; entry from: 36BAh
		bra		p_39BC
		btfss	0x18,1
		bra		p_36D2
		movf	0x94,W,BANKED
		cpfseq	5
		bra		p_35AA
p_36D2:	movf	5,W						; entry from: 36CAh
		btfsc	0x18,1
		movf	6,W
		movwf	0x45
		btfss	0x97,1,BANKED
		bra		p_37C4
		btfsc	0x18,7
		btfsc	0x11,2
		bra		p_37C4
		movf	0x45,W
		andlw	0xF0
		xorlw	0x10
		bnz		p_37C4
		movlw	8
		cpfslt	0x36
		bra		p_3704
		btfsc	0x35,5
		bra		p_36FA
		btfsc	0x33,7
		bra		p_37C4
p_36FA:	movlw	1						; entry from: 36F4h
		btfsc	0x18,1
		movlw	2
		cpfsgt	0x36
		bra		p_37C4
p_3704:	movf	0x45,W					; entry from: 36F0h
		xorlw	0x10
		bnz		p_3714
		movf	6,W
		btfsc	0x18,1
		movf	7,W
		sublw	6
		bc		p_37C4
p_3714:	decfsz	0xA6,W,BANKED			; entry from: 3708h
		bra		p_3730
		movff	0xA7,0x2F
		movff	0xA8,0x30
		movff	0xA9,0x31
		movff	0xAA,0x32
		call	p__63E
		rcall	p_3BDC
		bra		p_3756
p_3730:	movff	1,0x2F					; entry from: 3716h
		movff	2,0x30
		movff	3,0x31
		movff	4,0x32
		bcf		0x30,4
		tstfsz	0xA6,BANKED
		bra		p_3756
		btfss	0x37,7
		bra		p_374E
		bcf		0x2F,0
		bra		p_3756
p_374E:	movff	3,0x32					; entry from: 3748h
		movff	4,0x31

p_3756:	call	p__63E					; entry from: 372Eh,3744h,374Ch
		bcf		0x30,3
		btfss	0x37,7
		bsf		0x30,3
		rcall	p_3CC4
		btfsc	RXB0CON,3
		rcall	p_3E1E
		movlw	8
		btfss	0x35,5
		btfsc	0x37,6
		movlw	3
		movwf	RXB0DLC
		movff	0x2F,0xF61
		movff	0x30,0xF62
		movff	0x31,0xF63
		movff	0x32,0xF64
		movlw	0x30
		movwf	RXB0D0
		clrf	RXB0D1
		clrf	RXB0D2
		movf	0x95,W,BANKED
		movwf	RXB0D3
		movwf	RXB0D4
		movwf	RXB0D5
		movwf	RXB0D6
		movwf	RXB0D7
		movf	0xA6,f,BANKED
		bz		p_37B8
		movf	0xB0,W,BANKED
		btfss	0x35,5
		btfsc	0x37,6
		movwf	RXB0DLC
		movff	0xAB,0xF66
		movff	0xAC,0xF67
		movff	0xAD,0xF68
		movff	0xAE,0xF69
		movff	0xAF,0xF6A
		call	p__63E
p_37B8:	movlw	8						; entry from: 3796h
		movwf	RXB0CON
		bsf		0x10,7
		rcall	p_3CB0
		bra		p_37C4

p_37C2:	bsf		0xB3,1,BANKED			; entry from: 39DAh,39DEh,39E6h,3A0Ah

p_37C4:	rcall	p_3DDA					; entry from: 36C0h,36DCh,36E2h,36EAh,36F8h,3702h,3712h,37C0h,3AECh,3AF2h,3B16h,3B26h
		call	p__63E
		movlw	4
		addwf	0x36,W
		movwf	0
		movlw	0
		movwf	FSR0H
		movlw	6
		btfss	0x97,2,BANKED
		btfss	0x18,1
		movlw	5
		movwf	FSR0L
		btfss	0x97,0,BANKED
		btfss	0x17,0
		bra		p_37F0
		btfsc	0x34,0
		bra		p_35AA
		btfsc	0x97,1,BANKED
		bra		p_3834
		btfsc	0x97,2,BANKED
		bra		p_390C
p_37F0:	btfsc	0x72,3,BANKED			; entry from: 37E2h
		retlw	0
		btfsc	0x17,1
		rcall	p_396E
		btfss	0x34,0
		bra		p_3818
		movlw	0x52
		call	p__7F2
		call	p__63E
		movlw	0x54
		call	p__7F2
		movlw	0x52
		call	p__7F2
		call	p__82E
		retlw	0

p_3818:	movlw	8						; entry from: 37FAh,387Ah,38A0h,390Ah,391Ah,394Eh,3952h,396Ch
p_381A:	cpfsgt	0x36						; entry from: 38E8h
		movf	0x36,W
		addlw	5
		movwf	0x41
p_3822:	call	p__63E					; entry from: 3832h
		movf	0x41,W
		cpfslt	FSR0L
		retlw	0
		movf	POSTINC0,W
		call	p__7AA
		bra		p_3822
p_3834:	movf	0x45,W					; entry from: 37EAh
		bz		p_38FA
		andlw	0xF0
		movwf	0x42
		bnz		p_3842
		btfsc	0x45,3
		bra		p_38EA
p_3842:	sublw	0x3F						; entry from: 383Ch
		bnc		p_38EA
		movlw	0x10
		cpfseq	0x42
		bra		p_3856
		movlw	2
		btfsc	0x18,1
		movlw	3
		cpfsgt	0x36
		bra		p_38EA
p_3856:	btfss	0x35,5					; entry from: 384Ah
		btfss	0x33,7
		bra		p_3862
		movlw	8
		cpfseq	0x36
		bra		p_38EA
p_3862:	btfsc	0x72,3,BANKED			; entry from: 385Ah
		retlw	0
		btfss	0x17,1
		bra		p_387E
		rcall	p_396E
		call	p__63E
		incf	0x45,W
		movf	0x42
		bz		p_38E4
		movlw	0x30
		cpfseq	0x42
		bra		p_3818
		bra		p_38E2
p_387E:	tstfsz	0x42						; entry from: 3868h
		bra		p_3888
		movf	POSTINC0,W
		incf	0x45,W
		bra		p_38E4
p_3888:	call	p__63E					; entry from: 3880h
		movlw	0x20
		cpfseq	0x42
		bra		p_38A2
		movf	POSTINC0,W
		call	p__712
p_3898:	call	p__834					; entry from: 38C2h
		call	p__82E
		bra		p_3818
p_38A2:	movlw	0x10						; entry from: 3890h
		cpfseq	0x42
		bra		p_38C4
		movf	POSTINC0,W
		call	p__71C
		call	p__63E
		movf	POSTINC0,W
		call	p__7AA
		call	p__724
		call	p__63E
		movlw	0x30
		bra		p_3898
p_38C4:	movlw	0x46						; entry from: 38A6h
		call	p__7F2
		movlw	0x43
		call	p__834
		call	p__63E
		call	p__82E
		movf	POSTINC0,W
		call	p__71C
		call	p__82E
p_38E2:	movlw	3						; entry from: 387Ch

p_38E4:	btfsc	0x18,1					; entry from: 3874h,3886h
		addlw	1
		bra		p_381A

p_38EA:	btfsc	0x3E,3					; entry from: 3840h,3844h,3854h,3860h
		bra		p_3900
p_38EE:	btfss	0x3E,6					; entry from: 38FCh
		btfss	0x3E,4
		bra		p_35AA
		bsf		0xF,0
		bsf		0x72,3,BANKED
		retlw	0
p_38FA:	btfss	0x3E,3					; entry from: 3836h
		bra		p_38EE
		btfsc	0x33,6
p_3900:	bsf		0xF,0					; entry from: 38ECh
		btfsc	0x17,1
		rcall	p_396E
		call	p__63E
		bra		p_3818
p_390C:	btfsc	0x72,3,BANKED			; entry from: 37EEh
		retlw	0
		btfss	0x17,1
		bra		p_3950
		btfsc	0x43,3
		bra		p_391C
		rcall	p_3972
		bra		p_3818
p_391C:	rrcf	1,W						; entry from: 3916h
		movwf	0x42
		rrcf	0x42
		bcf		0x42,3
		movf	0x42,W
		call	p__71C
		call	p__63E
		call	p__82E
		movlw	0
		btfsc	1,0
		movlw	1
		call	p__71C
		swapf	2,W
		call	p__71C
		call	p__63E
		movf	2,W
		call	p__71C
		rcall	p_398A
		bra		p_3818
p_3950:	btfss	0xB3,3,BANKED			; entry from: 3912h
		bra		p_3818
		swapf	5,W
		call	p__71C
		call	p__63E
		movf	POSTINC0,W
		call	p__71C
		call	p__720
		call	p__82E
		bra		p_3818

p_396E:	btfsc	0x43,3					; entry from: 37F6h,386Ah,3904h
		bra		p_397A
p_3972:	movf	3,W						; entry from: 3918h
		call	p__71C
		bra		p_3990
p_397A:	movf	1,W						; entry from: 3970h
		call	p__7AA
		call	p__63E
		movf	2,W
		call	p__7AA
p_398A:	movf	3,W						; entry from: 394Ch
		call	p__7AA
p_3990:	call	p__63E					; entry from: 3978h
		movf	4,W
		call	p__7AA
		btfss	0x18,5
		bra		p_39AC
		call	p__63E
		movf	0x36,W
		call	p__71C
		call	p__82E
p_39AC:	btfss	0x97,2,BANKED			; entry from: 399Ch
		btfss	0x18,1
		return	
		call	p__63E
		movf	0x94,W,BANKED
		goto	p__7AA
p_39BC:	call	p__63E					; entry from: 36C6h
		swapf	2,W
		andlw	0x30
		movwf	0xB4,BANKED
		rlncf	2,W
		andlw	0xC1
		iorwf	0xB4,f,BANKED
		rlncf	1,W
		andlw	0xE
		iorwf	0xB4,f,BANKED
		swapf	0xB4,f,BANKED
		movlw	0x70
		andwf	0x11,W
		btfss	STATUS,2
		bra		p_37C2
		btfsc	0x35,0
		bra		p_37C2
		movf	0x39,W
		xorwf	0xB4,W,BANKED
		btfsc	STATUS,2
		bra		p_37C2
		movlw	8
		cpfseq	0x36
		bra		p_35AA
		call	p__63E
		movlw	0xE8
		cpfseq	0xB4,BANKED
		bra		p_3A0C
		movf	0x3A,W
		cpfseq	0xA
		bra		p_35AA
		movf	0x39,W
		cpfseq	0xB
		bra		p_35AA
		movf	0x38,W
		cpfseq	0xC
		bra		p_35AA
		bra		p_37C2
p_3A0C:	movlw	0xEC					; entry from: 39F6h
		cpfseq	0xB4,BANKED
		bra		p_3ACE
		movf	0x3A,W
		cpfseq	0xA
		bra		p_35AA
		movf	0x39,W
		cpfseq	0xB
		bra		p_35AA
		movf	0x38,W
		cpfseq	0xC
		bra		p_35AA
		movf	9,W
		movwf	0xBB,BANKED
		movlw	0x10
		xorwf	5,W
		bz		p_3A3A
		movlw	0x20
		cpfseq	5
		bra		p_35AA
		bsf		0xB3,4,BANKED
		movlw	0xFF
		movwf	0xBB,BANKED
p_3A3A:	bsf		0xB3,3,BANKED			; entry from: 3A2Ch
		movff	4,0xB5
		movff	6,0xB6
		movff	7,0xB7
		movf	8,W
		movwf	0xB8,BANKED
		movwf	0xBA,BANKED
		clrf	0xB9,BANKED
		incf	0xB9,f,BANKED
		clrf	0xBC,BANKED
		clrf	0xBD,BANKED
		movlw	0x10
		cpfslt	0xBB,BANKED
		movwf	0xBB,BANKED
		movf	8,W
		cpfslt	0xBB,BANKED
		movwf	0xBB,BANKED
		bsf		0xB3,5,BANKED
		movf	CANSTAT,W
		andlw	0xE0
		btfsc	STATUS,2
		bcf		0xB3,5,BANKED
		call	p__63E
		rcall	p_3CCE
		movlb	0xE
		setf	0xFE,BANKED
		setf	0xFF,BANKED
		movlw	0x4B
		movwf	0xE9,BANKED
		movwf	0xED,BANKED
		movwf	0xF1,BANKED
		movwf	0xF5,BANKED
		movf	3,W
		movwf	0xEA,BANKED
		movwf	0xEE,BANKED
		movwf	0xF2,BANKED
		movwf	0xF6,BANKED
		movf	4,W
		movwf	0xEB,BANKED
		movwf	0xEF,BANKED
		movwf	0xF3,BANKED
		movwf	0xF7,BANKED
		movlb	0
		bsf		0xB3,0,BANKED
		movlw	0x60
		btfss	0xB3,5,BANKED
		movlw	0
		rcall	p_3CD0
		btfss	0x17,0
		bra		p_3ABE
		movf	0xB7,W,BANKED
		call	p__71C
		call	p__63E
		movf	0xB6,W,BANKED
		call	p__7AA
		call	p__63E
		call	p__724
p_3ABE:	btfss	0xB3,4,BANKED			; entry from: 3AA4h
		btfsc	0x11,2
		bra		p_35AA
		call	p_3B28
		call	p__ADA
		bra		p_35AA
p_3ACE:	movlw	0xEB					; entry from: 3A10h
		cpfseq	0xB4,BANKED
		bra		p_35AA
		btfss	0xB3,4,BANKED
		btfsc	0x11,2
		bsf		0xB3,2,BANKED
		incf	0xBC,f,BANKED
		incf	0xBD,f,BANKED
		call	p__63E
		btfss	0xB3,2,BANKED
		bra		p_3AEE
		movf	0xB8,W,BANKED
		cpfslt	0xBC,BANKED
		bsf		0xB3,1,BANKED
		bra		p_37C4
p_3AEE:	movf	0xBD,W,BANKED			; entry from: 3AE4h
		cpfseq	0xBB,BANKED
		bra		p_37C4
		addwf	0xB9,f,BANKED
		subwf	0xBA,f,BANKED
		tstfsz	0xBA,BANKED
		bra		p_3B18
		bsf		0xB3,1,BANKED
		rcall	p_3CC4
		btfsc	RXB0CON,3
		rcall	p_3E1E
		movlw	0x13
		movwf	RXB0D0
		movf	0xB6,W,BANKED
		movwf	RXB0D1
		movf	0xB7,W,BANKED
		movwf	RXB0D2
		movf	0xB8,W,BANKED
		movwf	RXB0D3
		rcall	p_3B3C
		bra		p_37C4
p_3B18:	clrf	0xBD,BANKED				; entry from: 3AFAh
		movf	0xBA,W,BANKED
		cpfslt	0xBB,BANKED
		movwf	0xBB,BANKED
		rcall	p_3B28
		call	p__ADA
		bra		p_37C4

p_3B28:	rcall	p_3CC4					; entry from: 3AC4h,3B20h
		btfsc	RXB0CON,3
		rcall	p_3E1E
		movlw	0x11
		movwf	RXB0D0
		movff	0xBB,0xF67
		movff	0xB9,0xF68
		setf	RXB0D3
p_3B3C:	setf	RXB0D4						; entry from: 3B14h
		movff	0x3A,RXB0D5
		movff	0x39,RXB0D6
		movff	0x38,RXB0D7
		movlw	0xE7
		movwf	RXB0SIDH
		movlw	0x68
		movwf	RXB0SIDL
		movff	0xB5,RXB0EIDH
		movff	0x14,RXB0EIDL
		movlw	8
		movwf	RXB0DLC
		call	p__63E
		bra		p_3530

p_3B64:	bsf		CANCON,4					; entry from: 21F6h,21FCh,2202h,2208h,220Eh,2214h,221Ah
		movlw	0x30
		xorwf	CANSTAT,W
		andlw	0xE0
		btfss	STATUS,2
		rcall	p_3CCE
		bsf		LATB,2
		bcf		0x3E,0
		bcf		0x34,3
		clrf	0xB1,BANKED
		clrf	0xB2,BANKED
		retlw	0

p_3B7C:	rcall	p_3CBA					; entry from: 187Ah,352Ah,35A6h,368Eh,3DD8h
		bcf		RXB0CON,7
		call	p_3CB0
		bcf		RXB0CON,7
		clrf	COMSTAT
		retlw	0

p_3B8A:	movf	0x96,W,BANKED			; entry from: 32ECh,335Ah
		movwf	0x42
		clrf	0x41
		btfsc	0x33,3
p_3B92:	btfsc	PORTB,3					; entry from: 3B9Ah
		bra		p_3B9E
		dcfsnz	0x41
		decfsz	0x42
		bra		p_3B92
		retlw	6
p_3B9E:	rcall	p_3CCE					; entry from: 3B94h
		clrf	PIE3
		movlw	0x20
		movwf	CIOCON
		goto	p_4000
		nop

#if SW_VERSION == 0
		ORG BASE_ADDR + 0x3BC2
#endif
p_3BC2:	bsf		0x3E,0					; entry from: 4024h
		rcall	p_3CBA
		movlw	0
		movwf	RXB0CON
		rcall	p_3CB0
		movlw	4
		movwf	RXB0CON
		retlw	0

p_3BD2:	btfss	0x37,5					; entry from: 3C94h,3CA8h,3D88h,3DB0h,3DCEh
		bra		p_3BDC
		btfss	0x34,6
		btfsc	0x34,5
		bra		p_3C14

p_3BDC:	btfss	0x37,7					; entry from: 33DEh,3430h,3446h,34C0h,372Ch,3BD4h
		bra		p_3BF8
p_3BE0:	rlcf	0x32						; entry from: 3C18h
		rlcf	0x31
		rlcf	0x32
		rlcf	0x31
		movlw	0x1F
		andwf	0x31,W
		movwf	0x2F
		movlw	0xFC
		andwf	0x32,W
		movwf	0x30
		clrf	0x31
		clrf	0x32

p_3BF8:	movlw	3						; entry from: 3BDEh,3C16h,3E52h
		andwf	0x30,W
		movwf	0x41
		rlcf	0x30
		rlcf	0x2F
		rlcf	0x30
		rlcf	0x2F
		rlcf	0x30
		rlcf	0x2F
		movlw	0xE0
		andwf	0x30,W
		iorwf	0x41,W
		movwf	0x30
		retlw	0
p_3C14:	btfsc	0x35,4					; entry from: 3BDAh
		bra		p_3BF8
		bra		p_3BE0

p_3C1A:	movlb	0xE						; entry from: 3448h,3CAAh,3DD0h
		movf	0x2F,W
		movwf	0xE0,BANKED
		movwf	0xE4,BANKED
		movwf	0xE8,BANKED
		movwf	0xEC,BANKED
		movwf	0xF0,BANKED
		movwf	0xF4,BANKED
		bsf		0x30,3
		btfss	0x37,7
		btfsc	0x37,5
		bcf		0x30,3
		movf	0x30,W
		movwf	0xE5,BANKED
		movwf	0xED,BANKED
		movwf	0xF5,BANKED
		btfsc	0x37,5
		iorlw	8
		movwf	0xE1,BANKED
		movwf	0xE9,BANKED
		movwf	0xF1,BANKED
		movf	0x31,W
		movwf	0xE2,BANKED
		movwf	0xE6,BANKED
		movwf	0xEA,BANKED
		movwf	0xEE,BANKED
		movwf	0xF2,BANKED
		movwf	0xF6,BANKED
		movf	0x32,W
		movwf	0xE3,BANKED
		movwf	0xE7,BANKED
		movwf	0xEB,BANKED
		movwf	0xEF,BANKED
		movwf	0xF3,BANKED
		movwf	0xF7,BANKED
		movlb	0
		retlw	0

p_3C64:	movlb	0xE						; entry from: 33E0h,3432h,3C96h,3D8Ah,3DB2h
		movf	0x2F,W
		movwf	0xF8,BANKED
		movwf	0xFC,BANKED
		movf	0x30,W
		movwf	0xF9,BANKED
		movwf	0xFD,BANKED
		movf	0x31,W
		movwf	0xFA,BANKED
		movwf	0xFE,BANKED
		movf	0x32,W
		movwf	0xFB,BANKED
		movwf	0xFF,BANKED
		movlb	0
		retlw	0

p_3C82:	rcall	p_3CCE					; entry from: 32F6h,34AAh
		movff	0x9E,0x2F
		movff	0x9F,0x30
		movff	0xA0,0x31
		movff	0xA1,0x32
		rcall	p_3BD2
		rcall	p_3C64
		movff	0xA2,0x2F
		movff	0xA3,0x30
		movff	0xA4,0x31
		movff	0xA5,0x32
		rcall	p_3BD2
		rcall	p_3C1A
		bcf		0x34,2
		retlw	0

p_3CB0:	movf	CANCON,W					; entry from: 1FD8h,3540h,35AAh,3642h,37BEh,3B80h,3BCAh,3E3Ch
		andlw	0xE1
		iorlw	0
		movwf	CANCON
		retlw	0

p_3CBA:	movf	CANCON,W					; entry from: 35CAh,3B7Ch,3BC4h
		andlw	0xE1
		iorlw	0xA
		movwf	CANCON
		retlw	0

p_3CC4:	movf	CANCON,W					; entry from: 1FCEh,34DCh,3534h,3760h,3AFEh,3B28h
		andlw	0xE1
		iorlw	8
		movwf	CANCON
		retlw	0

p_3CCE:	movlw	0x80						; entry from: 814h,1876h,33BAh,34D6h,3560h,35EAh,3684h,3A70h,3B6Eh,3B9Eh,3C82h,3D5Ch,3E44h

p_3CD0:	movwf	0x43						; entry from: 34DAh,3570h,35A4h,368Ch,3AA0h,3DD6h
		xorwf	CANSTAT,W
		andlw	0xE0
		btfsc	STATUS,2
		retlw	0
		movff	TXERRCNT,0xB1
		movff	RXERRCNT,0xB2
		movf	CANSTAT,W
		andlw	0xE0
		xorlw	0x60
		bnz		p_3CEE
		movlw	0
		rcall	p_3CF4
p_3CEE:	movf	0x43,W					; entry from: 3CE8h
		rcall	p_3CF4
		retlw	0

p_3CF4:	movwf	CANCON						; entry from: 3CECh,3CF0h
		rlncf	0x96,W,BANKED
		movwf	0x42
		clrf	0x41
p_3CFC:	call	p__5D4					; entry from: 3D0Eh
		movf	CANSTAT,W
		xorwf	CANCON,W
		andlw	0xE0
		btfsc	STATUS,2
		retlw	0
		dcfsnz	0x41
		decfsz	0x42
		bra		p_3CFC
		pop
		bsf		LATB,2
		movlw	0x30
		movwf	CANCON
		rlncf	0x96,W,BANKED
		addlw	0x60
		movwf	0x42
p_3D1E:	call	p__5D4					; entry from: 3D2Eh
		movlw	0x30
		xorwf	CANSTAT,W
		andlw	0xE0
		bz		p_3D38
		dcfsnz	0x41
		decfsz	0x42
		bra		p_3D1E
p_3D30:	bsf		0xD2,7,BANKED			; entry from: 3D3Ah
		movlw	0x94
		movwf	0xD1,BANKED
		reset
p_3D38:	btfss	PORTB,2					; entry from: 3D28h
		bra		p_3D30
		bcf		0x3E,0
		bcf		0x34,3
		call	p_1FF0
		tstfsz	STKPTR
		pop
		retlw	6

p_3D4A:	movwf	0x37						; entry from: 3208h,3EBEh,3ED4h,3EEAh,3F00h,3F16h,3F2Ch
		andlw	7
		addlw	1
		clrf	0x97,BANKED
		bsf		0x97,7,BANKED
p_3D54:	rlncf	0x97,f,BANKED			; entry from: 3D58h
		addlw	0xFF
		bnz		p_3D54
		return
p_3D5C:	rcall	p_3CCE					; entry from: 3592h
		btfss	0x34,6
		btfsc	0x34,5
		bra		p_3D96
		btfss	0x97,2,BANKED
		bra		p_3D76
		movlw	0x70
		andwf	0x11,W
		bnz		p_3D76
		rcall	p_3E44
		call	p__ADA
		bra		p_3DD4

p_3D76:	clrf	0x2F						; entry from: 3D66h,3D6Ch
		clrf	0x30
		clrf	0x31
		clrf	0x32
		movlw	0xFF
		btfsc	0x11,6
		movwf	0x31
		btfsc	0x11,5
		movwf	0x32
		rcall	p_3BD2
		rcall	p_3C64
		clrf	0x2F
		clrf	0x30
		clrf	0x31
		clrf	0x32
		bra		p_3DC4
p_3D96:	movff	0x9E,0x2F					; entry from: 3D62h
		movff	0x9F,0x30
		movff	0xA0,0x31
		movff	0xA1,0x32
		movlw	0xFF
		btfsc	0x11,6
		movwf	0x31
		btfsc	0x11,5
		movwf	0x32
		rcall	p_3BD2
		rcall	p_3C64
		movff	0xA2,0x2F
		movff	0xA3,0x30
		movff	0xA4,0x31
		movff	0xA5,0x32
p_3DC4:	movf	0x15,W					; entry from: 3D94h
		btfsc	0x11,6
		movwf	0x31
		btfsc	0x11,5
		movwf	0x32
		rcall	p_3BD2
		rcall	p_3C1A
		bsf		0x34,2
p_3DD4:	movlw	0x60						; entry from: 3D74h
		rcall	p_3CD0
		bra		p_3B7C
p_3DDA:	clrf	0x43						; entry from: 37C4h
		movlw	0xB
		andwf	2,W
		movwf	0x41
		rrcf	1
		rrcf	2
		swapf	1
		swapf	2
		movlw	0xF
		andwf	2
		movlw	0xF0
		andwf	1,W
		iorwf	2
		movlw	7
		andwf	1
		btfsc	0x41,3
		bra		p_3E0A
		movff	1,3
		movff	2,4
		clrf	1
		clrf	2
		retlw	0
p_3E0A:	bcf		STATUS,0				; entry from: 3DFAh
		rlcf	2
		rlcf	1
		rlcf	2
		rlcf	1
		movf	0x41,W
		andlw	3
		iorwf	2
		setf	0x43
		retlw	0

p_3E1E:	rlncf	0x96,W,BANKED			; entry from: 1FD4h,34E2h,353Ch,3764h,3B02h,3B2Ch
		addwf	0x96,W,BANKED
		movwf	0x41
		clrf	0x42
p_3E26:	call	p__5D4					; entry from: 3E36h
		btfss	RXB0CON,3
		retlw	0
		btfsc	COMSTAT,0
		bra		p_3E38
		dcfsnz	0x42
		decfsz	0x41
		bra		p_3E26
p_3E38:	bcf		RXB0CON,3					; entry from: 3E30h
		bcf		PIR5,5
		call	p_3CB0
		pop
		retlw	6

p_3E44:	rcall	p_3CCE					; entry from: 340Ch,349Ah,3D6Eh
		clrf	0xB3,BANKED
		bsf		0x34,2
		movff	0x38,0x2F
		movff	0x39,0x30
		call	p_3BF8
		movlb	0xE
		movf	0x2F,W
		movwf	0xE0,BANKED
		movwf	0xE4,BANKED
		movf	0x30,W
		iorlw	8
		movwf	0xE1,BANKED
		movwf	0xE5,BANKED
		movlw	0xF0
		cpfslt	0x39
		bra		p_3E76
		movlw	0xFF
		movwf	0xE2,BANKED
		movf	0x14,W
		movwf	0xE6,BANKED
		bra		p_3E7C
p_3E76:	movf	0x3A,W					; entry from: 3E6Ah
		movwf	0xE2,BANKED
		movwf	0xE6,BANKED

p_3E7C:	movlw	0xF						; entry from: 3566h,3E74h
		movwf	0xF8,BANKED
		movwf	0xFC,BANKED
		movlw	0xE3
		movwf	0xF9,BANKED
		movwf	0xFD,BANKED
		setf	0xFA,BANKED
		clrf	0xFE,BANKED
		clrf	0xFB,BANKED
		clrf	0xFF,BANKED
		movlw	7
		movwf	0xE8,BANKED
		movwf	0xEC,BANKED
		movwf	0xF0,BANKED
		movwf	0xF4,BANKED
		movlw	0x68
		movwf	0xE9,BANKED
		movwf	0xED,BANKED
		btfss	0x11,2
		movlw	0x48
		movwf	0xF1,BANKED
		movwf	0xF5,BANKED
		movlb	0
		retlw	0
p_3EAC:	rcall	p_3EB8					; entry from: 2072h
		goto	p_32EC
p_3EB2:	rcall	p_3EB8					; entry from: 20C0h
		goto	p_335A

p_3EB8:	movlw	1						; entry from: 2242h,3EACh,3EB2h
		movwf	0x96,BANKED
		movlw	1
		goto	p_3D4A
p_3EC2:	rcall	p_3ECE					; entry from: 2078h
		goto	p_32EC
p_3EC8:	rcall	p_3ECE					; entry from: 20C6h
		goto	p_335A

p_3ECE:	movlw	2						; entry from: 2248h,3EC2h,3EC8h
		movwf	0x96,BANKED
		movlw	0x81
		goto	p_3D4A
p_3ED8:	rcall	p_3EE4					; entry from: 207Eh
		goto	p_32EC
p_3EDE:	rcall	p_3EE4					; entry from: 20CCh
		goto	p_335A

p_3EE4:	movlw	2						; entry from: 224Eh,3ED8h,3EDEh
		movwf	0x96,BANKED
		movlw	1
		goto	p_3D4A
p_3EEE:	rcall	p_3EFA					; entry from: 2084h
		goto	p_32EC
p_3EF4:	rcall	p_3EFA					; entry from: 20D2h
		goto	p_335A

p_3EFA:	movff	0x99,0x96					; entry from: 2254h,3EEEh,3EF4h
		movlw	0x42
		goto	p_3D4A
p_3F04:	rcall	p_3F10					; entry from: 208Ah
		goto	p_32EC
p_3F0A:	rcall	p_3F10					; entry from: 20D8h
		goto	p_335A

p_3F10:	movff	0x9B,0x96					; entry from: 225Ah,3F04h,3F0Ah
		movf	0x9A,W,BANKED
		goto	p_3D4A
p_3F1A:	rcall	p_3F26					; entry from: 2090h
		goto	p_32EC
p_3F20:	rcall	p_3F26					; entry from: 20DEh
		goto	p_335A

p_3F26:	movff	0x9D,0x96					; entry from: 2260h,3F1Ah,3F20h
		movf	0x9C,W,BANKED
		goto	p_3D4A
		nop

#if WDT_RESET
p_reset:	bsf     _POR_
	    	bsf     _RI_
	    	bsf     _SWDTEN_
reset_loop:	bra	reset_loop
#endif

		ORG BASE_ADDR + 0x4000
p_4000:	decf	0x96,W,BANKED			; entry from: 3BA6h
		btfss	0x97,2,BANKED
		iorlw	0x80
		movff	WREG,0xE43
		movlw	0xBC
		btfss	0x97,2,BANKED
		movlw	0xBB
		movlb	0xE
		movwf	0x44,BANKED
		btfsc	0x37,4
		bcf		0x44,4,BANKED
		movlb	0
		movlw	1
		btfss	0x97,2,BANKED
		movlw	2
		movff	WREG,0xE45
		goto	p_3BC2
p_4028:	movlw	1						; entry from: 1694h
		movff	WREG,0xF5D
		movlw	0
		movff	WREG,0xF5C
		movlw	0
		movwf	ADCON0
		call	p__99E
		return	
p_403E:	btfss	ADRESH,7				; entry from: 9BEh
		bra		p_4046
		clrf	ADRESH
		clrf	ADRESL
p_4046:	rrcf	ADRESH					; entry from: 4040h
		rrcf	ADRESL
		rrcf	ADRESH
		rrcf	ADRESL
		movlw	3
		andwf	ADRESH
		retlw	0
		movf	SPBRGH1,W
		rcall	p_405E
		movf	SPBRG1,W
		rcall	p_405E
p_405C:	bra		p_405C					; entry from: 405Ch

p_405E:	btfss	PIR1,4					; entry from: 4056h,405Ah,4060h
		bra		p_405E
		movwf	TXREG1
		return	
	END END_LABEL
