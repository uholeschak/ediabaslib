;-------------------------------------------------------------------------
;				Send Messages
;-------------------------------------------------------------------------
;input: a0 = number of message 0 .. 3
;
SendMessage:
#if FLASHEND > 0x7FFF
	ldi	zx, hh8(/* 2* */Messages)
	out	RAMPZ, zx
#endif
	ldi	zl, lo8(/* 2* */Messages)
	ldi	zh, hi8(/* 2* */Messages)
	ldi	yl, 0
_sme1:
	add	zl, yl			; add offset to next message
	adc	zh, zerol
	XLPM	yl, z
	subi	a0, 1			; count down until message found
	brcc	_sme1
	ldi	a0, ANSWER		; first byte of message
_sme2:
	rcall	putchar
	XLPM	a0, z+
	subi	yl, 1
	brcc	_sme2
	rjmp	main_ok
;-------------------------------------------------------------------------
