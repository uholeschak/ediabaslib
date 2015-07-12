;-------------------------------------------------------------------------
;				Receive Byte
;-------------------------------------------------------------------------
;output: a0 = byte
;used: a1
;
getchar:
#if WDTRIGGER
	wdr	
#endif
	SKIP_RXD_1			; wait for RXD = 1
	rjmp	getchar
_rx1:
	SKIP_RXD_0			; wait for RXD = 0 (start bit)
	rjmp	_rx1
	ldi	a1, 8
	movw	twl, baudl
	lsr	twh
	ror	twl
	rcall	wait_time		; middle of start bit
_rx2:
	rcall	wait_bit_time		;14 + tw
	lsr	a0			;1
	SKIP_RXD_0			;1/2
	ori	a0, 0x80		;1
#if CRC
;------------------------------ CRC --------------------------------------
	sbrc	a0, 7			;1
	eor	crcl, polynoml		;1 crcl.0 ^= a0.7
	lsr	crch			;1
	ror	crcl			;1
	brcc	_rx5			;1
	eor	crcl, polynoml		;1 ^0x01
_rx5:
	brcc	_rx6			;1
	eor	crch, polynomh		;1 ^0xA0
_rx6:
;-------------------------------------------------------------------------
#else
	rjmp .			;2
	rjmp .			;2
#endif
	dec	a1			;1
	brne	_rx2			;2 = 24 + tw
	cpi	a0, COMMAND		; needed several times
	ret
;-------------------------------------------------------------------------
;				transmit byte
;-------------------------------------------------------------------------
;input: a0 = byte
;used: a1
;
putchar:
	rcall	wait_bit_time
	TXD_0
#if ONEWIRE
	rjmp	_tx2
syncputchar:				; start with 1->0 from master
	SKIP_RXD_1
	rjmp	syncputchar
_tx1:
	SKIP_RXD_0
	rjmp	_tx1
_tx2:
#else
	lpm	a1, z			;3
#endif
	ldi	a1, 9			;1
	com	a0			;1 = 5
_tx3:
#if CRC
	rjmp .			;2
	rjmp .			;2
#endif
	rcall	wait_bit_time		;14 + tw
	lsr	a0			;1
	brcc	_tx4			;1/2
	nop				;1
	TXD_0				;2
	rjmp	_tx5			;2
_tx4:
	TXD_1				;2
	rjmp	_tx5			;2
_tx5:
	dec	a1			;1
	brne	_tx3			;2 = 24 + tw
	ret
;-------------------------------------------------------------------------
;	Wait 14 cycle + tw
;
wait_bit_time:
	movw	twl, baudl	;1
wait_time:
	sbiw	twl, 4		;2
	brcc	wait_time		;2/1
	cpi	twl, 0xFD		;1
	brcs	_wt1			;2/1 (2)
	breq	_wt1			;2/1 (3)
	cpi	twl, 0xFF		;1
	breq	_wt1			;2/1 (4/5)
_wt1:
	ret				;4 + 3 (rcall) = 14
;-------------------------------------------------------------------------
