;-------------------------------------------------------------------------
;		 		Program User Flash
;-------------------------------------------------------------------------
;
Program:
	ldi	xl, lo8(ProgBuff)
	ldi	xh, hi8(ProgBuff)
	movw	yl, xl
	brts	_pro5
;---------------------- Receive data for one Page ------------------------
_pro1:
	rcall	getchar
	brne	_pro2
	rcall	getchar
	subi	a0, ESC_SHIFT
	set				; end mark received set
	breq	_pro3
	clt
_pro2:
	st	x+, a0
	cpi	xl, lo8(ProgBuffEnd)	; since buffer size below 256
	brne	_pro1
;-------------------------------------------------------------------------
_pro3:
	adiw	zl, 0
	brne	_pro5
;------------------------------ Insert rjmp to boot loader ---------------
	ld	appl, y
	ldd	apph, y+1
	subi	appl, lo8(BootStart - 0x1001)	; new application jump
	sbci	apph, hi8(BootStart - 0x1001)
	ldi	a0, lo8(BootStart-1)
	ldi	a1, hi8(BootStart-1 + 0xC000)  ; = RJMP
	st	y, a0
	std	y+1, a1				; replace by bootloader jump
;-------------------------------------------------------------------------
;               Erase application Flash backward (avoid lock out)
;-------------------------------------------------------------------------
	ldi     zl, (2*BOOTSTART)&0xff
	ldi     zh, BOOTSTART>>7
_pro4:
#if PageSize < 32
	sbiw	zl, PageSize * 2
#else
	subi    zl, lo8(2 * PageSize)
	sbci    zh, hi8(2 * PageSize)
#endif
	ldi	a0, 1<<PGERS^1<<SPMEN
	out     SPMCSR, a0
	SPM                             ; CPU halted until erase done
	brne    _pro4			; until Z = 0x0000
;-------------------------------------------------------------------------
_pro5:
	brtc	_pro6
	std	y+2*PageSize-2, appl
	std	y+2*PageSize-1, apph
;---------------------- Fill page buffer ---------------------------------
_pro6:
	ld	r0, y+
	ld	r1, y+
	ldi	a0, 1<<SPMEN		; fill buffer command
	out	SPMCSR, a0
	SPM
	adiw	zl, 2
	cpi	yl, lo8(ProgBuffEnd)
	brne	_pro6
#if PageSize < 32
	sbiw	zl, 2*PageSize
#else
	subi	zl, lo8(2*PageSize)
	sbci	zh, hi8(2*PageSize)
#endif
;---------------------- Program page -------------------------------------
	ldi	a0, 1<<PGWRT^1<<SPMEN	; write page command
	out	SPMCSR, a0
	SPM
;---------------------- Next Page ----------------------------------------
#if PageSize < 32
	adiw	zl, 2*PageSize
#else
	subi	zl, lo8(-2*PageSize)
	sbci	zh, hi8(-2*PageSize)	; point to next page
#endif
	brts	_pro8
	ldi	a0, CONTINUE
	rcall	putchar
_pro8:
	cpi	zl, lo8( /* 2* */BootStart)
	ldi	a0, BOOTSTART>>7
	cpc	zh, a0                  ; last page reached ?
	brcs	Program
	brts	_pro9
	rjmp	main_error		; error, size exceeded
_pro9:
	rjmp	main_ok
;-------------------------------------------------------------------------
