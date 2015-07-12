;------------------------------	check, if watchdog active ----------------
	wdr
	xin	a0, WDTCSR
	ori	a0, 1<<WDCE				; change enable
	ldi	a1, 1<<WDE^1<<WDP2^1<<WDP1^1<<WDP0	; 2s
	xout	WDTCSR, a0
	sbrc	a0, WDE
	xout	WDTCSR, a1
;-------------------------------------------------------------------------

