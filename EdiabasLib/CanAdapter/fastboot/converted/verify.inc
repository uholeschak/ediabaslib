;-------------------------------------------------------------------------
;		 		Verify User Flash
;-------------------------------------------------------------------------
;
_ver1:
#if FLASHEND > 0x7FFF
	out	RAMPZ, zx
	elpm	r0, z
	adiw	zl, 1
	adc	zx, zerol		; 24 bit addition
#else
	lpm	r0, z+
#endif
#ifndef FirstBootStart
	cpi	zl, 3
	cpc	zh, zerol
	brcs	VerifyFlash		; exclude jump to bootloader
#endif
	cpse	r0, a0
	set
VerifyFlash:
	rcall	getchar
	brne	_ver1			; not COMMAND ?
	rcall	getchar
	subi	a0, ESC_SHIFT
	brne	_ver1			; COMMMAND + not COMMAND = End
	brts	main_error		; error, Flash not equal
	rjmp	main_ok
;-------------------------------------------------------------------------
