;-------------------------------------------------------------------------
;				Receive commands
;-------------------------------------------------------------------------
;00	get bootloader revision
;01	get buffer size
;02	get target signature
;03	get user flash size
;04	program flash
;05	start application
;06	check crc
;07	verify flash
;-------------------------------------------------------------------------
main_ok:
	ldi	a0, SUCCESS
_cex1:
	rcall	putchar
_cex2:
	rcall	getchar
	brne	_cex2			; ignore until COMMAND
_cex3:
	rcall	getchar
	breq	_cex3			; ignore further COMMAND
#if FLASHEND > 0x7FFF
	ldi	zx, 0
#endif
	movw	zl, zerol      ; Z = 0x0000,
	clt				; T = 0 (for program, verify)
	cpi	a0, 4
	brcs	SendMessage		; command 0 ... 3
	breq	program			; command 4
	cpi	a0, 5
	breq	timeout			; command 5
	cpi	a0, 7
#if VERIFY
	breq	VerifyFlash		; command 7
#endif
	ldi	a0, BADCOMMAND
;-------------------------------------------------------------------------
#if CRC
	brcc	_cex1			; command >7
;-------------------------------------------------------------------------
CheckCRC:				; command 6
	rcall	getchar			; read CRC low
	rcall	getchar			; read CRC high
	or	crcl, crch		; now CRC = 0x0000 ?
	breq	main_ok			; yes
	movw	crcl, zerol	; clear CRC
#else
	rjmp	_cex1
#endif
;-------------------------------------------------------------------------
main_error:
	ldi	a0, FAIL
	rjmp	_cex1
;-------------------------------------------------------------------------
timeout:				; command 5
	out	STX_DDR, zerol
	out	STX_PORT, zerol
#if SRX_PORT != STX_PORT
	out	SRX_PORT, zerol
#endif
#if FlashEnd > 0x0fff
        jmp Application
#else
	rjmp	Application		; run application
#endif
;-------------------------------------------------------------------------
#include "message.inc" // command 0 ... 3
;-------------------------------------------------------------------------
#if VERIFY
#include "verify.inc" // command 7
#endif
;-------------------------------------------------------------------------
#ifdef FirstBootStart
#include "progmega.inc" // mega with bootstart fuse set
#else
#include "progtiny.inc" // tiny, mega without RWW section
#endif
;-------------------------------------------------------------------------
