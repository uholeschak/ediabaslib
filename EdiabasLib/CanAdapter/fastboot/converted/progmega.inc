;-------------------------------------------------------------------------
;		 		Program User Flash
;-------------------------------------------------------------------------
_pro1:
	ldi	a0, CONTINUE
	rcall	putchar
program:
	set
	ldi	xl, lo8(ProgBuff)
	ldi	xh, hi8(ProgBuff)
	ldi	yh, hi8(ProgBuffEnd)
;------------------------------ Receive data into buffer -----------------
_pro2:
	rcall	getchar
	brne	_pro3
	rcall	getchar
	subi	a0, ESC_SHIFT
	brne	_pro3			; A5,80 = end mark
	brts	_pro6
	set
	rjmp	_pro4
_pro3:
	clt
	st	x+, a0
	cpi	xl, lo8(ProgBuffEnd)
	cpc	xh, yh
	brne	_pro2
;-------------------------------------------------------------------------
_pro4:
	ldi	xl, lo8(ProgBuff)
	ldi	xh, hi8(ProgBuff)
_pro5:
	rcall	prog_page		; CY = 1: o.k
	brcc	main_error		; error, bootloader reached
	subi	zl, lo8(-2*PageSize)
	sbci	zh, hi8(-2*PageSize)	; point to next page
#if FLASHEND > 0x7FFF
	sbci    zx, hh8(-/* 2* */BufferSize)
#endif
	cpi	xl, lo8(ProgBuffEnd)
	cpc	xh, yh
	brne	_pro5			; until buffer end
	brtc	_pro1
;-------------------------------------------------------------------------
_pro6:
	rjmp	main_ok
;-------------------------------------------------------------------------
;			Program page in Flash
;-------------------------------------------------------------------------
;use:	r0, r1, a0, xl, xh, zl, zh
;
;input:  X  = buffer to RAM
;        Z  = page to program
;output: CY = 0: error, attempt to overwrite itself
;
#define  PAGEMASK (PageSize *2 - 1) & ~1

prog_page:
;------------------------------ Avoid self destruction ! -----------------
	cpi	zl, (2*BOOTSTART)&0xff
	ldi	a0, (BOOTSTART&0x7fff)>>7
	cpc	zh, a0                  	; below bootloader ?
#if FLASHEND > 0x7FFF
	ldi     a0, hh8(2*BootStart)
	cpc     zx, a0
#endif
	brcc	_prp3				; CY = 0: error
;------------------------------ Fill page buffer -------------------------
_prp1:	ld	r0, x+
	ld	r1, x+
	ldi	a0, 1<<SPMEN
	rcall	do_spm
	adiw	zl, 2
	mov	a0, zl
	andi	a0, lo8(PAGEMASK)
	brne	_prp1
;------------------------------ Erase page -------------------------------
	subi	zl, lo8(2*PageSize)
	sbci	zh, hi8(2*PageSize)
	ldi	a0, 1<<PGERS^1<<SPMEN		; erase page command
	rcall	do_spm
;------------------------------ Program page -----------------------------
	ldi	a0, 1<<PGWRT^1<<SPMEN		; write page command
	rcall	do_spm
	ldi	a0, 1<<RWWSRE^1<<SPMEN
do_spm:
	xout	SPMCSR, a0
#if FLASHEND > 0x7FFF
	out     RAMPZ, zx               ; 3 byte Z pointer
	xout    SPMCSR, a0
#endif
	spm
_prp2:
	xin	a0, SPMCSR
	sbrc	a0, SPMEN
	rjmp	_prp2
	sec					; CY = 1: successful
_prp3:
	ret
;-------------------------------------------------------------------------
