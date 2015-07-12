;-------------------------------------------------------------------------
;				Check password
;-------------------------------------------------------------------------
	ldi	yl, 10			; try it 10 times
#if FLASHEND > 0x7FFF
	ldi	a0, hh8(/* 2* */Password)
	out	RAMPZ, a0
#endif
checkpwd:
	ldi	zl, lo8(/* 2* */Password)
	ldi	zh, hi8(/* 2* */Password)
_cpw2:
	XLPM	r0, z+
	tst	r0
	breq	_cpw3			; end if zero byte
	rcall	getchar
	cp	r0, a0
	breq	_cpw2			; compare next byte
	dec	yl			; count down wrong compares
	breq	timeout
	rjmp	checkpwd		; try again
_cpw3:
;-------------------------------------------------------------------------
