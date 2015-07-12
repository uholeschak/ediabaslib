;***** THIS IS A MACHINE GENERATED FILE - DO NOT EDIT ********************
;***** Created: 2011-02-09 12:03 ******* Source: ATmega162.xml ***********
;*************************************************************************
;* A P P L I C A T I O N   N O T E   F O R   T H E   A V R   F A M I L Y
;* 
;* Number            : AVR000
;* File Name         : "m162def.inc"
;* Title             : Register/Bit Definitions for the ATmega162
;* Date              : 2011-02-09
;* Version           : 2.35
;* Support E-mail    : avr@atmel.com
;* Target MCU        : ATmega162
;* 
;* DESCRIPTION
;* When including this file in the assembly program file, all I/O register 
;* names and I/O register bit names appearing in the data book can be used.
;* In addition, the six registers forming the three data pointers X, Y and 
;* Z have been assigned names XL - ZH. Highest RAM address for Internal 
;* SRAM is also defined 
;* 
;* The Register names are represented by their hexadecimal address.
;* 
;* The Register Bit names are represented by their bit number (0-7).
;* 
;* Please observe the difference in using the bit names with instructions
;* such as "sbr"/"cbr" (set/clear bit in register) and "sbrs"/"sbrc"
;* (skip if bit in register set/cleared). The following example illustrates
;* this:
;* 
;* in    r16,PORTB             ;read PORTB latch
;* sbr   r16,(1<<PB6)+(1<<PB5) ;set PB6 and PB5 (use masks, not bit#)
;* out   PORTB,r16             ;output to PORTB
;* 
;* in    r16,TIFR              ;read the Timer Interrupt Flag Register
;* sbrc  r16,TOV0              ;test the overflow flag (use bit#)
;* rjmp  TOV0_is_set           ;jump if set
;* ...                         ;otherwise do something else
;*************************************************************************

#ifndef _M162DEF_INC_
#define _M162DEF_INC_


#pragma partinc 0

; ***** SPECIFY DEVICE ***************************************************
.device ATmega162
#pragma AVRPART ADMIN PART_NAME ATmega162
.equ	SIGNATURE_000	= 0x1e
.equ	SIGNATURE_001	= 0x94
.equ	SIGNATURE_002	= 0x04

#pragma AVRPART CORE CORE_VERSION V2E


; ***** I/O REGISTER DEFINITIONS *****************************************
; NOTE:
; Definitions marked "MEMORY MAPPED"are extended I/O ports
; and cannot be used with IN/OUT instructions
.equ	TCCR3A	= 0x8b	; MEMORY MAPPED
.equ	TCCR3B	= 0x8a	; MEMORY MAPPED
.equ	TCNT3L	= 0x88	; MEMORY MAPPED
.equ	TCNT3H	= 0x89	; MEMORY MAPPED
.equ	OCR3AL	= 0x86	; MEMORY MAPPED
.equ	OCR3AH	= 0x87	; MEMORY MAPPED
.equ	OCR3BL	= 0x84	; MEMORY MAPPED
.equ	OCR3BH	= 0x85	; MEMORY MAPPED
.equ	ICR3L	= 0x80	; MEMORY MAPPED
.equ	ICR3H	= 0x81	; MEMORY MAPPED
.equ	ETIMSK	= 0x7d	; MEMORY MAPPED
.equ	ETIFR	= 0x7c	; MEMORY MAPPED
.equ	PCMSK1	= 0x6c	; MEMORY MAPPED
.equ	PCMSK0	= 0x6b	; MEMORY MAPPED
.equ	CLKPR	= 0x61	; MEMORY MAPPED
.equ	SREG	= 0x3f
.equ	SPL	= 0x3d
.equ	SPH	= 0x3e
.equ	UBRR1H	= 0x3c
.equ	UCSR1C	= 0x3c
.equ	GICR	= 0x3b
.equ	GIFR	= 0x3a
.equ	TIMSK	= 0x39
.equ	TIFR	= 0x38
.equ	SPMCR	= 0x37
.equ	EMCUCR	= 0x36
.equ	MCUCR	= 0x35
.equ	MCUCSR	= 0x34
.equ	TCCR0	= 0x33
.equ	TCNT0	= 0x32
.equ	OCR0	= 0x31
.equ	SFIOR	= 0x30
.equ	TCCR1A	= 0x2f
.equ	TCCR1B	= 0x2e
.equ	TCNT1L	= 0x2c
.equ	TCNT1H	= 0x2d
.equ	OCR1AL	= 0x2a
.equ	OCR1AH	= 0x2b
.equ	OCR1BL	= 0x28
.equ	OCR1BH	= 0x29
.equ	TCCR2	= 0x27
.equ	ASSR	= 0x26
.equ	ICR1L	= 0x24
.equ	ICR1H	= 0x25
.equ	TCNT2	= 0x23
.equ	OCR2	= 0x22
.equ	WDTCR	= 0x21
.equ	UBRR0H	= 0x20
.equ	UCSR0C	= 0x20
.equ	EEARL	= 0x1e
.equ	EEARH	= 0x1f
.equ	EEDR	= 0x1d
.equ	EECR	= 0x1c
.equ	PORTA	= 0x1b
.equ	DDRA	= 0x1a
.equ	PINA	= 0x19
.equ	PORTB	= 0x18
.equ	DDRB	= 0x17
.equ	PINB	= 0x16
.equ	PORTC	= 0x15
.equ	DDRC	= 0x14
.equ	PINC	= 0x13
.equ	PORTD	= 0x12
.equ	DDRD	= 0x11
.equ	PIND	= 0x10
.equ	SPDR	= 0x0f
.equ	SPSR	= 0x0e
.equ	SPCR	= 0x0d
.equ	UDR0	= 0x0c
.equ	UCSR0A	= 0x0b
.equ	UCSR0B	= 0x0a
.equ	UBRR0L	= 0x09
.equ	ACSR	= 0x08
.equ	PORTE	= 0x07
.equ	DDRE	= 0x06
.equ	PINE	= 0x05
.equ	OSCCAL	= 0x04
.equ	OCDR	= 0x04
.equ	UDR1	= 0x03
.equ	UCSR1A	= 0x02
.equ	UCSR1B	= 0x01
.equ	UBRR1L	= 0x00


; ***** BIT DEFINITIONS **************************************************

; ***** TIMER_COUNTER_1 **************
; TIMSK - Timer/Counter Interrupt Mask Register
.equ	TICIE1	= 3	; Timer/Counter1 Input Capture Interrupt Enable
.equ	OCIE1B	= 5	; Timer/Counter1 Output CompareB Match Interrupt Enable
.equ	OCIE1A	= 6	; Timer/Counter1 Output CompareA Match Interrupt Enable
.equ	TOIE1	= 7	; Timer/Counter1 Overflow Interrupt Enable

; TIFR - Timer/Counter Interrupt Flag register
.equ	ICF1	= 3	; Input Capture Flag 1
.equ	OCF1B	= 5	; Output Compare Flag 1B
.equ	OCF1A	= 6	; Output Compare Flag 1A
.equ	TOV1	= 7	; Timer/Counter1 Overflow Flag

; TCCR1A - Timer/Counter1 Control Register A
.equ	WGM10	= 0	; Pulse Width Modulator Select Bit 0
.equ	PWM10	= WGM10	; For compatibility
.equ	WGM11	= 1	; Pulse Width Modulator Select Bit 1
.equ	PWM11	= WGM11	; For compatibility
.equ	FOC1B	= 2	; Force Output Compare for Channel B
.equ	FOC1A	= 3	; Force Output Compare for Channel A
.equ	COM1B0	= 4	; Compare Output Mode 1B, bit 0
.equ	COM1B1	= 5	; Compare Output Mode 1B, bit 1
.equ	COM1A0	= 6	; Compare Ouput Mode 1A, bit 0
.equ	COM1A1	= 7	; Compare Output Mode 1A, bit 1

; TCCR1B - Timer/Counter1 Control Register B
.equ	CS10	= 0	; Clock Select1 bit 0
.equ	CS11	= 1	; Clock Select1 bit 1
.equ	CS12	= 2	; Clock Select1 bit 2
.equ	WGM12	= 3	; Pulse Width Modulator Select Bit 2
.equ	CTC10	= WGM12	; For compatibility
.equ	WGM13	= 4	; Pulse Width Modulator Select Bit 3
.equ	CTC11	= WGM13	; For compatibility
.equ	ICES1	= 6	; Input Capture 1 Edge Select
.equ	ICNC1	= 7	; Input Capture 1 Noise Canceler


; ***** TIMER_COUNTER_2 **************
; TCCR2 - Timer/Counter Control Register
.equ	CS20	= 0	; Clock Select
.equ	CS21	= 1	; Clock Select
.equ	CS22	= 2	; Clock Select
.equ	WGM21	= 3	; Pulse Width Modulator Select Bit 1
.equ	CTC2	= WGM21	; For compatibility
.equ	COM20	= 4	; Compare Match Output Mode
.equ	COM21	= 5	; Compare Match Output Mode
.equ	WGM20	= 6	; Pulse Width Modulator Select Bit 0
.equ	PWM2	= WGM20	; For compatibility
.equ	FOC2	= 7	; Forde Output Compare

; TCNT2 - Timer/Counter Register
.equ	TCNT2_0	= 0	; Timer/Counter Register Bit 0
.equ	TCNT2_1	= 1	; Timer/Counter Register Bit 1
.equ	TCNT2_2	= 2	; Timer/Counter Register Bit 2
.equ	TCNT2_3	= 3	; Timer/Counter Register Bit 3
.equ	TCNT2_4	= 4	; Timer/Counter Register Bit 4
.equ	TCNT2_5	= 5	; Timer/Counter Register Bit 5
.equ	TCNT2_6	= 6	; Timer/Counter Register Bit 6
.equ	TCNT2_7	= 7	; Timer/Counter Register Bit 7

; OCR2 - Output Compare Register
.equ	OCR2_0	= 0	; Output Compare Register Bit 0
.equ	OCR2_1	= 1	; Output Compare Register Bit 1
.equ	OCR2_2	= 2	; Output Compare Register Bit 2
.equ	OCR2_3	= 3	; Output Compare Register Bit 3
.equ	OCR2_4	= 4	; Output Compare Register Bit 4
.equ	OCR2_5	= 5	; Output Compare Register Bit 5
.equ	OCR2_6	= 6	; Output Compare Register Bit 6
.equ	OCR2_7	= 7	; Output Compare Register Bit 7

; TIMSK - Timer/Counter Interrupt Mask Register
.equ	TOIE2	= 2	; Timer/Counter2 Overflow Interrupt Enable
.equ	OCIE2	= 4	; Timer/Counter2 Output Compare Match Interrupt Enable

; TIFR - Timer/Counter Interrupt Flag Register
.equ	TOV2	= 2	; Timer/Counter2 Overflow Flag
.equ	OCF2	= 4	; Output Compare Flag 2

; ASSR - Asynchronous Status Register
.equ	TCR2UB	= 0	; Timer/Counter Control Register2 Update Busy
.equ	OCR2UB	= 1	; Output Compare Register2 Update Busy
.equ	TCN2UB	= 2	; Timer/Counter2 Update Busy
.equ	AS2	= 3	; Asynchronous Timer 2


; ***** TIMER_COUNTER_3 **************
; ETIMSK - Extended Timer/Counter Interrupt Mask Register
.equ	TOIE3	= 2	; Timer/Counter3 Overflow Interrupt Enable
.equ	OCIE3B	= 3	; Timer/Counter3 Output CompareB Match Interrupt Enable
.equ	OCIE3A	= 4	; Timer/Counter3 Output CompareA Match Interrupt Enable
.equ	TICIE3	= 5	; Timer/Counter3 Input Capture Interrupt Enable

; ETIFR - Extended Timer/Counter Interrupt Flag register
.equ	TOV3	= 2	; Timer/Counter3 Overflow Flag
.equ	OCF3B	= 3	; Output Compare Flag 3B
.equ	OCF3A	= 4	; Output Compare Flag 3A
.equ	ICF3	= 5	; Input Capture Flag 3

; TCCR3A - Timer/Counter3 Control Register A
.equ	WGM30	= 0	; Pulse Width Modulator Select Bit 0
.equ	WGM31	= 1	; Pulse Width Modulator Select Bit 1
.equ	FOC3B	= 2	; Force Output Compare for Channel B
.equ	FOC3A	= 3	; Force Output Compare for Channel A
.equ	COM3B0	= 4	; Compare Output Mode 3B, bit 0
.equ	COM3B1	= 5	; Compare Output Mode 3B, bit 1
.equ	COM3A0	= 6	; Compare Ouput Mode 3A, bit 0
.equ	COM3A1	= 7	; Compare Output Mode 3A, bit 1

; TCCR3B - Timer/Counter3 Control Register B
.equ	CS30	= 0	; Clock Select3 bit 0
.equ	CS31	= 1	; Clock Select3 bit 1
.equ	CS32	= 2	; Clock Select3 bit 2
.equ	WGM32	= 3	; Pulse Width Modulator Select Bit 2
.equ	WGM33	= 4	; Pulse Width Modulator Select Bit 3
.equ	ICES3	= 6	; Input Capture 3 Edge Select
.equ	ICNC3	= 7	; Input Capture 3 Noise Canceler


; ***** ANALOG_COMPARATOR ************
; ACSR - Analog Comparator Control And Status Register
.equ	ACIS0	= 0	; Analog Comparator Interrupt Mode Select bit 0
.equ	ACIS1	= 1	; Analog Comparator Interrupt Mode Select bit 1
.equ	ACIC	= 2	; Analog Comparator Input Capture Enable
.equ	ACIE	= 3	; Analog Comparator Interrupt Enable
.equ	ACI	= 4	; Analog Comparator Interrupt Flag
.equ	ACO	= 5	; Analog Compare Output
.equ	ACBG	= 6	; Analog Comparator Bandgap Select
.equ	AINBG	= ACBG	; For compatibility
.equ	ACD	= 7	; Analog Comparator Disable


; ***** USART0 ***********************
; UDR0 - USART I/O Data Register
.equ	UDR	= UDR0	; For compatibility
.equ	UDR0_0	= 0	; USART I/O Data Register bit 0
.equ	UDR0_1	= 1	; USART I/O Data Register bit 1
.equ	UDR0_2	= 2	; USART I/O Data Register bit 2
.equ	UDR0_3	= 3	; USART I/O Data Register bit 3
.equ	UDR0_4	= 4	; USART I/O Data Register bit 4
.equ	UDR0_5	= 5	; USART I/O Data Register bit 5
.equ	UDR0_6	= 6	; USART I/O Data Register bit 6
.equ	UDR0_7	= 7	; USART I/O Data Register bit 7

; UCSR0A - USART Control and Status Register A
.equ	USR	= UCSR0A	; For compatibility
.equ	MPCM0	= 0	; Multi-processor Communication Mode
.equ	U2X0	= 1	; Double the USART transmission speed
.equ	U2X	= U2X0	; For compatibility
.equ	UPE0	= 2	; Parity Error
.equ	DOR0	= 3	; Data overRun
.equ	DOR	= DOR0	; For compatibility
.equ	FE0	= 4	; Framing Error
.equ	FE	= FE0	; For compatibility
.equ	UDRE0	= 5	; USART Data Register Empty
.equ	UDRE	= UDRE0	; For compatibility
.equ	TXC0	= 6	; USART Transmitt Complete
.equ	TXC	= TXC0	; For compatibility
.equ	RXC0	= 7	; USART Receive Complete
.equ	RXC	= RXC0	; For compatibility

; UCSR0B - USART Control and Status Register B
.equ	UCR	= UCSR0B	; For compatibility
.equ	TXB80	= 0	; Transmit Data Bit 8
.equ	TXB8	= TXB80	; For compatibility
.equ	RXB80	= 1	; Receive Data Bit 8
.equ	RXB8	= RXB80	; For compatibility
.equ	UCSZ02	= 2	; Character Size
.equ	UCSZ2	= UCSZ02	; For compatibility
.equ	TXEN0	= 3	; Transmitter Enable
.equ	TXEN	= TXEN0	; For compatibility
.equ	RXEN0	= 4	; Receiver Enable
.equ	RXEN	= RXEN0	; For compatibility
.equ	UDRIE0	= 5	; USART Data register Empty Interrupt Enable
.equ	UDRIE	= UDRIE0	; For compatibility
.equ	TXCIE0	= 6	; TX Complete Interrupt Enable
.equ	TXCIE	= TXCIE0	; For compatibility
.equ	RXCIE0	= 7	; RX Complete Interrupt Enable
.equ	RXCIE	= RXCIE0	; For compatibility

; UCSR0C - USART Control and Status Register C
.equ	UBRRHI	= UCSR0C	; For compatibility
.equ	UCPOL0	= 0	; Clock Polarity
.equ	UCSZ00	= 1	; Character Size
.equ	UCSZ01	= 2	; Character Size
.equ	USBS0	= 3	; Stop Bit Select
.equ	UPM00	= 4	; Parity Mode Bit 0
.equ	UPM01	= 5	; Parity Mode Bit 1
.equ	UMSEL0	= 6	; USART Mode Select
.equ	URSEL0	= 7	; Register Select

; UBRR0H - USART Baud Rate Register Hight Byte
.equ	UBRR8	= 0	; USART Baud Rate Register bit 8
.equ	UBRR9	= 1	; USART Baud Rate Register bit 9
.equ	UBRR10	= 2	; USART Baud Rate Register bit 10
.equ	UBRR11	= 3	; USART Baud Rate Register bit 11
;.equ	URSEL0	= 7	; Register Select

; UBRR0L - USART Baud Rate Register Low Byte
.equ	UBRR0	= UBRR0L	; For compatibility
.equ	UBRR	= UBRR0L	; For compatibility
;.equ	UBRR0	= 0	; USART Baud Rate Register bit 0
.equ	UBRR1	= 1	; USART Baud Rate Register bit 1
.equ	UBRR2	= 2	; USART Baud Rate Register bit 2
.equ	UBRR3	= 3	; USART Baud Rate Register bit 3
.equ	UBRR4	= 4	; USART Baud Rate Register bit 4
.equ	UBRR5	= 5	; USART Baud Rate Register bit 5
.equ	UBRR6	= 6	; USART Baud Rate Register bit 6
.equ	UBRR7	= 7	; USART Baud Rate Register bit 7


; ***** USART1 ***********************
; UDR1 - USART I/O Data Register
.equ	UDR1_0	= 0	; USART1 I/O Data Register bit 0
.equ	UDR1_1	= 1	; USART1 I/O Data Register bit 1
.equ	UDR1_2	= 2	; USART1 I/O Data Register bit 2
.equ	UDR1_3	= 3	; USART1 I/O Data Register bit 3
.equ	UDR1_4	= 4	; USART1 I/O Data Register bit 4
.equ	UDR1_5	= 5	; USART1 I/O Data Register bit 5
.equ	UDR1_6	= 6	; USART1 I/O Data Register bit 6
.equ	UDR1_7	= 7	; USART1 I/O Data Register bit 7

; UCSR1A - USART Control and Status Register A
.equ	MPCM1	= 0	; Multi-processor Communication Mode
.equ	U2X1	= 1	; Double the USART transmission speed
.equ	UPE1	= 2	; Parity Error
.equ	DOR1	= 3	; Data overRun
.equ	FE1	= 4	; Framing Error
.equ	UDRE1	= 5	; USART Data Register Empty
.equ	TXC1	= 6	; USART Transmitt Complete
.equ	RXC1	= 7	; USART Receive Complete

; UCSR1B - USART Control and Status Register B
.equ	TXB81	= 0	; Transmit Data Bit 8
.equ	RXB81	= 1	; Receive Data Bit 8
.equ	UCSZ12	= 2	; Character Size
.equ	CHR91	= UCSZ12	; For compatibility
.equ	TXEN1	= 3	; Transmitter Enable
.equ	RXEN1	= 4	; Receiver Enable
.equ	UDRIE1	= 5	; USART Data register Empty Interrupt Enable
.equ	TXCIE1	= 6	; TX Complete Interrupt Enable
.equ	RXCIE1	= 7	; RX Complete Interrupt Enable

; UCSR1C - USART Control and Status Register C
.equ	UCPOL1	= 0	; Clock Polarity
.equ	UCSZ10	= 1	; Character Size
.equ	UCSZ11	= 2	; Character Size
.equ	USBS1	= 3	; Stop Bit Select
.equ	UPM10	= 4	; Parity Mode Bit 0
.equ	UPM11	= 5	; Parity Mode Bit 1
.equ	UMSEL1	= 6	; USART Mode Select
.equ	URSEL1	= 7	; Register Select

; UBRR1H - USART Baud Rate Register Highg Byte
;.equ	UBRR8	= 0	; USART Baud Rate Register bit 8
;.equ	UBRR9	= 1	; USART Baud Rate Register bit 9
;.equ	UBRR10	= 2	; USART Baud Rate Register bit 10
;.equ	UBRR11	= 3	; USART Baud Rate Register bit 11

;.equ	UBRR1	= UBRR1L	; For compatibility

; ***** SPI **************************
; SPDR - SPI Data Register
.equ	SPDR0	= 0	; SPI Data Register bit 0
.equ	SPDR1	= 1	; SPI Data Register bit 1
.equ	SPDR2	= 2	; SPI Data Register bit 2
.equ	SPDR3	= 3	; SPI Data Register bit 3
.equ	SPDR4	= 4	; SPI Data Register bit 4
.equ	SPDR5	= 5	; SPI Data Register bit 5
.equ	SPDR6	= 6	; SPI Data Register bit 6
.equ	SPDR7	= 7	; SPI Data Register bit 7

; SPSR - SPI Status Register
.equ	SPI2X	= 0	; Double SPI Speed Bit
.equ	WCOL	= 6	; Write Collision Flag
.equ	SPIF	= 7	; SPI Interrupt Flag

; SPCR - SPI Control Register
.equ	SPR0	= 0	; SPI Clock Rate Select 0
.equ	SPR1	= 1	; SPI Clock Rate Select 1
.equ	CPHA	= 2	; Clock Phase
.equ	CPOL	= 3	; Clock polarity
.equ	MSTR	= 4	; Master/Slave Select
.equ	DORD	= 5	; Data Order
.equ	SPE	= 6	; SPI Enable
.equ	SPIE	= 7	; SPI Interrupt Enable


; ***** CPU **************************
; SREG - Status Register
.equ	SREG_C	= 0	; Carry Flag
.equ	SREG_Z	= 1	; Zero Flag
.equ	SREG_N	= 2	; Negative Flag
.equ	SREG_V	= 3	; Two's Complement Overflow Flag
.equ	SREG_S	= 4	; Sign Bit
.equ	SREG_H	= 5	; Half Carry Flag
.equ	SREG_T	= 6	; Bit Copy Storage
.equ	SREG_I	= 7	; Global Interrupt Enable

; MCUCR - MCU Control Register
.equ	ISC00	= 0	; Interrupt Sense Control 0 bit 0
.equ	ISC01	= 1	; Interrupt Sense Control 0 bit 1
.equ	ISC10	= 2	; Interrupt Sense Control 1 bit 1
.equ	ISC11	= 3	; Interrupt Sense Control 1 bit 1
.equ	SM1	= 4	; Sleep Mode Select
.equ	SM	= SM1	; For compatibility
.equ	SE	= 5	; Sleep Enable
.equ	SRW10	= 6	; External SRAM Wait State Select
.equ	SRW	= SRW10	; For compatibility
.equ	SRE	= 7	; External SRAM Enable

; MCUCSR - MCU Control And Status Register
.equ	MCUSR	= MCUCSR	; For compatibility
.equ	PORF	= 0	; Power-on reset flag
.equ	EXTRF	= 1	; External Reset Flag
.equ	BORF	= 2	; Brown-out Reset Flag
.equ	WDRF	= 3	; Watchdog Reset Flag
.equ	JTRF	= 4	; JTAG Reset Flag
.equ	SM2	= 5	; Sleep Mode Select Bit 2
.equ	JDT	= 7	; JTAG Interface Disable

; EMCUCR - Extended MCU Control Register
.equ	ISC2	= 0	; Interrupt Sense Control 2
.equ	SRW11	= 1	; Wait State Select Bit 1 for Upper Sector
.equ	SRW00	= 2	; Wait State Select Bit 0 for Lower Sector
.equ	SRW01	= 3	; Wait State Select Bit 1 for Lower Sector
.equ	SRL0	= 4	; Wait State Sector Limit Bit 0
.equ	SRL1	= 5	; Wait State Sector Limit Bit 1
.equ	SRL2	= 6	; Wait State Sector Limit Bit 2
.equ	SM0	= 7	; Sleep mode Select Bit 0

; OSCCAL - Oscillator Calibration Value
.equ	CAL0	= 0	; Oscillator Calibration Value Bit0
.equ	CAL1	= 1	; Oscillator Calibration Value Bit1
.equ	CAL2	= 2	; Oscillator Calibration Value Bit2
.equ	CAL3	= 3	; Oscillator Calibration Value Bit3
.equ	CAL4	= 4	; Oscillator Calibration Value Bit4
.equ	CAL5	= 5	; Oscillator Calibration Value Bit5
.equ	CAL6	= 6	; Oscillator Calibration Value Bit6

; CLKPR - Clock prescale register
.equ	CLKPS0	= 0	; Clock Prescaler Select Bit 0
.equ	CLKPS1	= 1	; Clock Prescaler Select Bit 1
.equ	CLKPS2	= 2	; Clock Prescaler Select Bit 2
.equ	CLKPS3	= 3	; Clock Prescaler Select Bit 3
.equ	CLKPCE	= 7	; Clock Prescaler Change Enable

; SFIOR - Special Function IO Register
.equ	PSR310	= 0	; Prescaler Reset Timer/Counter3, Timer/Counter1 and Timer/Counter0
.equ	PSR10	= PSR310	; For compatibility
.equ	PSR0	= PSR310	; For compatibility
.equ	PSR1	= PSR310	; For compatibility
.equ	PSR2	= 1	; Prescaler Reset Timer/Counter2
.equ	PUD	= 2	; Pull-up Disable
.equ	XMM0	= 3	; External Memory High Mask Bit 0
.equ	XMM1	= 4	; External Memory High Mask Bit 1
.equ	XMM2	= 5	; External Memory High Mask Bit 2
.equ	XMBK	= 6	; External Memory Bus Keeper Enable
.equ	TSM	= 7	; Timer/Counter Synchronization Mode


; ***** JTAG *************************
; OCDR - On-Chip Debug Related Register in I/O Memory
.equ	OCDR0	= 0	; On-Chip Debug Register Bit 0
.equ	OCDR1	= 1	; On-Chip Debug Register Bit 1
.equ	OCDR2	= 2	; On-Chip Debug Register Bit 2
.equ	OCDR3	= 3	; On-Chip Debug Register Bit 3
.equ	OCDR4	= 4	; On-Chip Debug Register Bit 4
.equ	OCDR5	= 5	; On-Chip Debug Register Bit 5
.equ	OCDR6	= 6	; On-Chip Debug Register Bit 6
.equ	OCDR7	= 7	; On-Chip Debug Register Bit 7
.equ	IDRD	= OCDR7	; For compatibility

; MCUCSR - MCU Control And Status Register
;.equ	JTRF	= 4	; JTAG Reset Flag
.equ	JTD	= 7	; JTAG Interface Disable


; ***** BOOT_LOAD ********************
; SPMCR - Store Program Memory Control Register
.equ	SPMEN	= 0	; Store Program Memory Enable
.equ	PGERS	= 1	; Page Erase
.equ	PGWRT	= 2	; Page Write
.equ	BLBSET	= 3	; Boot Lock Bit Set
.equ	RWWSRE	= 4	; Read While Write secion read enable
.equ	ASRE	= RWWSRE	; For compatibility
.equ	RWWSB	= 6	; Read While Write Section Busy
.equ	ASB	= RWWSB	; For compatibility
.equ	SPMIE	= 7	; SPM Interrupt Enable


; ***** EEPROM ***********************
; EEDR - EEPROM Data Register
.equ	EEDR0	= 0	; EEPROM Data Register bit 0
.equ	EEDR1	= 1	; EEPROM Data Register bit 1
.equ	EEDR2	= 2	; EEPROM Data Register bit 2
.equ	EEDR3	= 3	; EEPROM Data Register bit 3
.equ	EEDR4	= 4	; EEPROM Data Register bit 4
.equ	EEDR5	= 5	; EEPROM Data Register bit 5
.equ	EEDR6	= 6	; EEPROM Data Register bit 6
.equ	EEDR7	= 7	; EEPROM Data Register bit 7

; EECR - EEPROM Control Register
.equ	EERE	= 0	; EEPROM Read Enable
.equ	EEWE	= 1	; EEPROM Write Enable
.equ	EEMWE	= 2	; EEPROM Master Write Enable
.equ	EEWEE	= EEMWE	; For compatibility
.equ	EERIE	= 3	; EEPROM Ready Interrupt Enable


; ***** PORTA ************************
; PORTA - Port A Data Register
.equ	PORTA0	= 0	; Port A Data Register bit 0
.equ	PA0	= 0	; For compatibility
.equ	PORTA1	= 1	; Port A Data Register bit 1
.equ	PA1	= 1	; For compatibility
.equ	PORTA2	= 2	; Port A Data Register bit 2
.equ	PA2	= 2	; For compatibility
.equ	PORTA3	= 3	; Port A Data Register bit 3
.equ	PA3	= 3	; For compatibility
.equ	PORTA4	= 4	; Port A Data Register bit 4
.equ	PA4	= 4	; For compatibility
.equ	PORTA5	= 5	; Port A Data Register bit 5
.equ	PA5	= 5	; For compatibility
.equ	PORTA6	= 6	; Port A Data Register bit 6
.equ	PA6	= 6	; For compatibility
.equ	PORTA7	= 7	; Port A Data Register bit 7
.equ	PA7	= 7	; For compatibility

; DDRA - Port A Data Direction Register
.equ	DDA0	= 0	; Data Direction Register, Port A, bit 0
.equ	DDA1	= 1	; Data Direction Register, Port A, bit 1
.equ	DDA2	= 2	; Data Direction Register, Port A, bit 2
.equ	DDA3	= 3	; Data Direction Register, Port A, bit 3
.equ	DDA4	= 4	; Data Direction Register, Port A, bit 4
.equ	DDA5	= 5	; Data Direction Register, Port A, bit 5
.equ	DDA6	= 6	; Data Direction Register, Port A, bit 6
.equ	DDA7	= 7	; Data Direction Register, Port A, bit 7

; PINA - Port A Input Pins
.equ	PINA0	= 0	; Input Pins, Port A bit 0
.equ	PINA1	= 1	; Input Pins, Port A bit 1
.equ	PINA2	= 2	; Input Pins, Port A bit 2
.equ	PINA3	= 3	; Input Pins, Port A bit 3
.equ	PINA4	= 4	; Input Pins, Port A bit 4
.equ	PINA5	= 5	; Input Pins, Port A bit 5
.equ	PINA6	= 6	; Input Pins, Port A bit 6
.equ	PINA7	= 7	; Input Pins, Port A bit 7


; ***** PORTB ************************
; PORTB - Port B Data Register
.equ	PORTB0	= 0	; Port B Data Register bit 0
.equ	PB0	= 0	; For compatibility
.equ	PORTB1	= 1	; Port B Data Register bit 1
.equ	PB1	= 1	; For compatibility
.equ	PORTB2	= 2	; Port B Data Register bit 2
.equ	PB2	= 2	; For compatibility
.equ	PORTB3	= 3	; Port B Data Register bit 3
.equ	PB3	= 3	; For compatibility
.equ	PORTB4	= 4	; Port B Data Register bit 4
.equ	PB4	= 4	; For compatibility
.equ	PORTB5	= 5	; Port B Data Register bit 5
.equ	PB5	= 5	; For compatibility
.equ	PORTB6	= 6	; Port B Data Register bit 6
.equ	PB6	= 6	; For compatibility
.equ	PORTB7	= 7	; Port B Data Register bit 7
.equ	PB7	= 7	; For compatibility

; DDRB - Port B Data Direction Register
.equ	DDB0	= 0	; Port B Data Direction Register bit 0
.equ	DDB1	= 1	; Port B Data Direction Register bit 1
.equ	DDB2	= 2	; Port B Data Direction Register bit 2
.equ	DDB3	= 3	; Port B Data Direction Register bit 3
.equ	DDB4	= 4	; Port B Data Direction Register bit 4
.equ	DDB5	= 5	; Port B Data Direction Register bit 5
.equ	DDB6	= 6	; Port B Data Direction Register bit 6
.equ	DDB7	= 7	; Port B Data Direction Register bit 7

; PINB - Port B Input Pins
.equ	PINB0	= 0	; Port B Input Pins bit 0
.equ	PINB1	= 1	; Port B Input Pins bit 1
.equ	PINB2	= 2	; Port B Input Pins bit 2
.equ	PINB3	= 3	; Port B Input Pins bit 3
.equ	PINB4	= 4	; Port B Input Pins bit 4
.equ	PINB5	= 5	; Port B Input Pins bit 5
.equ	PINB6	= 6	; Port B Input Pins bit 6
.equ	PINB7	= 7	; Port B Input Pins bit 7


; ***** PORTC ************************
; PORTC - Port C Data Register
.equ	PORTC0	= 0	; Port C Data Register bit 0
.equ	PC0	= 0	; For compatibility
.equ	PORTC1	= 1	; Port C Data Register bit 1
.equ	PC1	= 1	; For compatibility
.equ	PORTC2	= 2	; Port C Data Register bit 2
.equ	PC2	= 2	; For compatibility
.equ	PORTC3	= 3	; Port C Data Register bit 3
.equ	PC3	= 3	; For compatibility
.equ	PORTC4	= 4	; Port C Data Register bit 4
.equ	PC4	= 4	; For compatibility
.equ	PORTC5	= 5	; Port C Data Register bit 5
.equ	PC5	= 5	; For compatibility
.equ	PORTC6	= 6	; Port C Data Register bit 6
.equ	PC6	= 6	; For compatibility
.equ	PORTC7	= 7	; Port C Data Register bit 7
.equ	PC7	= 7	; For compatibility

; DDRC - Port C Data Direction Register
.equ	DDC0	= 0	; Port C Data Direction Register bit 0
.equ	DDC1	= 1	; Port C Data Direction Register bit 1
.equ	DDC2	= 2	; Port C Data Direction Register bit 2
.equ	DDC3	= 3	; Port C Data Direction Register bit 3
.equ	DDC4	= 4	; Port C Data Direction Register bit 4
.equ	DDC5	= 5	; Port C Data Direction Register bit 5
.equ	DDC6	= 6	; Port C Data Direction Register bit 6
.equ	DDC7	= 7	; Port C Data Direction Register bit 7

; PINC - Port C Input Pins
.equ	PINC0	= 0	; Port C Input Pins bit 0
.equ	PINC1	= 1	; Port C Input Pins bit 1
.equ	PINC2	= 2	; Port C Input Pins bit 2
.equ	PINC3	= 3	; Port C Input Pins bit 3
.equ	PINC4	= 4	; Port C Input Pins bit 4
.equ	PINC5	= 5	; Port C Input Pins bit 5
.equ	PINC6	= 6	; Port C Input Pins bit 6
.equ	PINC7	= 7	; Port C Input Pins bit 7


; ***** PORTD ************************
; PORTD - Port D Data Register
.equ	PORTD0	= 0	; Port D Data Register bit 0
.equ	PD0	= 0	; For compatibility
.equ	PORTD1	= 1	; Port D Data Register bit 1
.equ	PD1	= 1	; For compatibility
.equ	PORTD2	= 2	; Port D Data Register bit 2
.equ	PD2	= 2	; For compatibility
.equ	PORTD3	= 3	; Port D Data Register bit 3
.equ	PD3	= 3	; For compatibility
.equ	PORTD4	= 4	; Port D Data Register bit 4
.equ	PD4	= 4	; For compatibility
.equ	PORTD5	= 5	; Port D Data Register bit 5
.equ	PD5	= 5	; For compatibility
.equ	PORTD6	= 6	; Port D Data Register bit 6
.equ	PD6	= 6	; For compatibility
.equ	PORTD7	= 7	; Port D Data Register bit 7
.equ	PD7	= 7	; For compatibility

; DDRD - Port D Data Direction Register
.equ	DDD0	= 0	; Port D Data Direction Register bit 0
.equ	DDD1	= 1	; Port D Data Direction Register bit 1
.equ	DDD2	= 2	; Port D Data Direction Register bit 2
.equ	DDD3	= 3	; Port D Data Direction Register bit 3
.equ	DDD4	= 4	; Port D Data Direction Register bit 4
.equ	DDD5	= 5	; Port D Data Direction Register bit 5
.equ	DDD6	= 6	; Port D Data Direction Register bit 6
.equ	DDD7	= 7	; Port D Data Direction Register bit 7

; PIND - Port D Input Pins
.equ	PIND0	= 0	; Port D Input Pins bit 0
.equ	PIND1	= 1	; Port D Input Pins bit 1
.equ	PIND2	= 2	; Port D Input Pins bit 2
.equ	PIND3	= 3	; Port D Input Pins bit 3
.equ	PIND4	= 4	; Port D Input Pins bit 4
.equ	PIND5	= 5	; Port D Input Pins bit 5
.equ	PIND6	= 6	; Port D Input Pins bit 6
.equ	PIND7	= 7	; Port D Input Pins bit 7


; ***** TIMER_COUNTER_0 **************
; TCCR0 - Timer/Counter 0 Control Register
.equ	CS00	= 0	; Clock Select 0
.equ	CS01	= 1	; Clock Select 1
.equ	CS02	= 2	; Clock Select 2
.equ	WGM01	= 3	; Waveform Generation Mode 1
.equ	CTC0	= WGM01	; For compatibility
.equ	COM00	= 4	; Compare match Output Mode 0
.equ	COM01	= 5	; Compare Match Output Mode 1
.equ	WGM00	= 6	; Waveform Generation Mode 0
.equ	PWM0	= WGM00	; For compatibility
.equ	FOC0	= 7	; Force Output Compare

; TCNT0 - Timer/Counter 0 Register
.equ	TCNT0_0	= 0	; 
.equ	TCNT0_1	= 1	; 
.equ	TCNT0_2	= 2	; 
.equ	TCNT0_3	= 3	; 
.equ	TCNT0_4	= 4	; 
.equ	TCNT0_5	= 5	; 
.equ	TCNT0_6	= 6	; 
.equ	TCNT0_7	= 7	; 

; OCR0 - Timer/Counter 0 Output Compare Register
.equ	OCR0_0	= 0	; 
.equ	OCR0_1	= 1	; 
.equ	OCR0_2	= 2	; 
.equ	OCR0_3	= 3	; 
.equ	OCR0_4	= 4	; 
.equ	OCR0_5	= 5	; 
.equ	OCR0_6	= 6	; 
.equ	OCR0_7	= 7	; 

; TIMSK - Timer/Counter Interrupt Mask Register
.equ	OCIE0	= 0	; Timer/Counter0 Output Compare Match Interrupt register
.equ	TOIE0	= 1	; Timer/Counter0 Overflow Interrupt Enable

; TIFR - Timer/Counter Interrupt Flag register
.equ	OCF0	= 0	; Output Compare Flag 0
.equ	TOV0	= 1	; Timer/Counter0 Overflow Flag


; ***** WATCHDOG *********************
; WDTCR - Watchdog Timer Control Register
.equ	WDTCSR	= WDTCR	; For compatibility
.equ	WDP0	= 0	; Watch Dog Timer Prescaler bit 0
.equ	WDP1	= 1	; Watch Dog Timer Prescaler bit 1
.equ	WDP2	= 2	; Watch Dog Timer Prescaler bit 2
.equ	WDE	= 3	; Watch Dog Enable
.equ	WDCE	= 4	; Watchdog Change Enable
.equ	WDTOE	= WDCE	; For compatibility


; ***** PORTE ************************
; PORTE - Data Register, Port E
.equ	PORTE0	= 0	; 
.equ	PE0	= 0	; For compatibility
.equ	PORTE1	= 1	; 
.equ	PE1	= 1	; For compatibility
.equ	PORTE2	= 2	; 
.equ	PE2	= 2	; For compatibility

; DDRE - Data Direction Register, Port E
.equ	DDE0	= 0	; 
.equ	DDE1	= 1	; 
.equ	DDE2	= 2	; 

; PINE - Input Pins, Port E
.equ	PINE0	= 0	; 
.equ	PINE1	= 1	; 
.equ	PINE2	= 2	; 
.equ	PINE3	= 3	; 


; ***** EXTERNAL_INTERRUPT ***********
; MCUCR - MCU Control Register
;.equ	ISC00	= 0	; Interrupt Sense Control 0 Bit 0
;.equ	ISC01	= 1	; Interrupt Sense Control 0 Bit 1
;.equ	ISC10	= 2	; Interrupt Sense Control 1 Bit 0
;.equ	ISC11	= 3	; Interrupt Sense Control 1 Bit 1

; EMCUCR - Extended MCU Control Register
;.equ	ISC2	= 0	; Interrupt Sense Control 2

; GICR - General Interrupt Control Register
.equ	EIMSK	= GICR	; For compatibility
.equ	GIMSK	= GICR	; For compatibility
.equ	IVCE	= 0	; Interrupt Vector Change Enable
.equ	IVSEL	= 1	; Interrupt Vector Select
.equ	PCIE0	= 3	; Pin Change Interrupt Enable 0
.equ	PCIE1	= 4	; Pin Change Interrupt Enable 1
.equ	INT2	= 5	; External Interrupt Request 2 Enable
.equ	INT0	= 6	; External Interrupt Request 0 Enable
.equ	INT1	= 7	; External Interrupt Request 1 Enable

; GIFR - General Interrupt Flag Register
.equ	PCIF0	= 3	; Pin Change Interrupt Flag 0
.equ	PCIF1	= 4	; Pin Change Interrupt Flag 1
.equ	INTF2	= 5	; External Interrupt Flag 2
.equ	INTF0	= 6	; External Interrupt Flag 0
.equ	INTF1	= 7	; External Interrupt Flag 1

; PCMSK1 - Pin Change Mask Register 1
.equ	PCINT8	= 0	; Pin Change Enable Mask 8
.equ	PCINT9	= 1	; Pin Change Enable Mask 9
.equ	PCINT10	= 2	; Pin Change Enable Mask 10
.equ	PCINT11	= 3	; Pin Change Enable Mask 11
.equ	PCINT12	= 4	; Pin Change Enable Mask 12
.equ	PCINT13	= 5	; Pin Change Enable Mask 13
.equ	PCINT14	= 6	; Pin Change Enable Mask 14
.equ	PCINT15	= 7	; Pin Change Enable Mask 15

; PCMSK0 - Pin Change Enable Mask
.equ	PCINT0	= 0	; Pin Change Enable Mask 0
.equ	PCINT1	= 1	; Pin Change Enable Mask 1
.equ	PCINT2	= 2	; Pin Change Enable Mask 2
.equ	PCINT3	= 3	; Pin Change Enable Mask 3
.equ	PCINT4	= 4	; Pin Change Enable Mask 4
.equ	PCINT5	= 5	; Pin Change Enable Mask 5
.equ	PCINT6	= 6	; Pin Change Enable Mask 6
.equ	PCINT7	= 7	; Pin Change Enable Mask 7



; ***** LOCKSBITS ********************************************************
.equ	LB1	= 0	; Lock bit
.equ	LB2	= 1	; Lock bit
.equ	BLB01	= 2	; Boot Lock bit
.equ	BLB02	= 3	; Boot Lock bit
.equ	BLB11	= 4	; Boot lock bit
.equ	BLB12	= 5	; Boot lock bit


; ***** FUSES ************************************************************
; LOW fuse bits
.equ	CKSEL0	= 0	; Select Clock Source
.equ	CKSEL1	= 1	; Select Clock Source
.equ	CKSEL2	= 2	; Select Clock Source
.equ	CKSEL3	= 3	; Select Clock Source
.equ	SUT0	= 4	; Select start-up time
.equ	SUT1	= 5	; Select start-up time
.equ	CKOUT	= 6	; Oscillator options
.equ	CKDIV8	= 7	; Divide clock by 8

; HIGH fuse bits
.equ	BOOTRST	= 0	; Select Reset Vector
.equ	BOOTSZ0	= 1	; Select Boot Size
.equ	BOOTSZ1	= 2	; Select Boot Size
.equ	EESAVE	= 3	; EEPROM memory is preserved through chip erase
.equ	WDTON	= 4	; Watchdog timer always on
.equ	SPIEN	= 5	; Enable Serial programming and Data Downloading
.equ	JTAGEN	= 6	; Enable JTAG
.equ	OCDEN	= 7	; Enable OCD

; EXTENDED fuse bits
.equ	BODLEVEL0	= 1	; Brown out detector trigger level
.equ	BODLEVEL1	= 2	; Brown out detector trigger level
.equ	BODLEVEL2	= 3	; Brown out detector trigger level
.equ	M161C	= 4	; ATMega 161 compatibility mode



; ***** CPU REGISTER DEFINITIONS *****************************************
.def	XH	= r27
.def	XL	= r26
.def	YH	= r29
.def	YL	= r28
.def	ZH	= r31
.def	ZL	= r30



; ***** DATA MEMORY DECLARATIONS *****************************************
.equ	FLASHEND	= 0x1fff	; Note: Word address
.equ	IOEND	= 0x00ff
.equ	SRAM_START	= 0x0100
.equ	SRAM_SIZE	= 1024
.equ	RAMEND	= 0x04ff
.equ	XRAMEND	= 0xffff
.equ	E2END	= 0x01ff
.equ	EEPROMEND	= 0x01ff
.equ	EEADRBITS	= 9
#pragma AVRPART MEMORY PROG_FLASH 16384
#pragma AVRPART MEMORY EEPROM 512
#pragma AVRPART MEMORY INT_SRAM SIZE 1024
#pragma AVRPART MEMORY INT_SRAM START_ADDR 0x100



; ***** BOOTLOADER DECLARATIONS ******************************************
.equ	NRWW_START_ADDR	= 0x1c00
.equ	NRWW_STOP_ADDR	= 0x1fff
.equ	RWW_START_ADDR	= 0x0
.equ	RWW_STOP_ADDR	= 0x1bff
.equ	PAGESIZE	= 64
.equ	FIRSTBOOTSTART	= 0x1f80
.equ	SECONDBOOTSTART	= 0x1f00
.equ	THIRDBOOTSTART	= 0x1e00
.equ	FOURTHBOOTSTART	= 0x1c00
.equ	SMALLBOOTSTART	= FIRSTBOOTSTART
.equ	LARGEBOOTSTART	= FOURTHBOOTSTART



; ***** INTERRUPT VECTORS ************************************************
.equ	INT0addr	= 0x0002	; External Interrupt Request 0
.equ	INT1addr	= 0x0004	; External Interrupt Request 1
.equ	INT2addr	= 0x0006	; External Interrupt Request 2
.equ	PCI0addr	= 0x0008	; Pin Change Interrupt Request 0
.equ	PCI1addr	= 0x000a	; Pin Change Interrupt Request 1
.equ	ICP3addr	= 0x000c	; Timer/Counter3 Capture Event
.equ	OC3Aaddr	= 0x000e	; Timer/Counter3 Compare Match A
.equ	OC3Baddr	= 0x0010	; Timer/Counter3 Compare Match B
.equ	OVF3addr	= 0x0012	; Timer/Counter3 Overflow
.equ	OC2addr	= 0x0014	; Timer/Counter2 Compare Match
.equ	OVF2addr	= 0x0016	; Timer/Counter2 Overflow
.equ	ICP1addr	= 0x0018	; Timer/Counter1 Capture Event
.equ	OC1Aaddr	= 0x001a	; Timer/Counter1 Compare Match A
.equ	OC1Baddr	= 0x001c	; Timer/Counter Compare Match B
.equ	OVF1addr	= 0x001e	; Timer/Counter1 Overflow
.equ	OC0addr	= 0x0020	; Timer/Counter0 Compare Match
.equ	OVF0addr	= 0x0022	; Timer/Counter0 Overflow
.equ	SPIaddr	= 0x0024	; SPI Serial Transfer Complete
.equ	URXC0addr	= 0x0026	; USART0, Rx Complete
.equ	URXC1addr	= 0x0028	; USART1, Rx Complete
.equ	UDRE0addr	= 0x002a	; USART0 Data register Empty
.equ	UDRE1addr	= 0x002c	; USART1, Data register Empty
.equ	UTXC0addr	= 0x002e	; USART0, Tx Complete
.equ	UTXC1addr	= 0x0030	; USART1, Tx Complete
.equ	ERDYaddr	= 0x0032	; EEPROM Ready
.equ	ACIaddr	= 0x0034	; Analog Comparator
.equ	SPMRaddr	= 0x0036	; Store Program Memory Read

.equ	INT_VECTORS_SIZE	= 56	; size in words

#endif  /* _M162DEF_INC_ */

; ***** END OF FILE ******************************************************
