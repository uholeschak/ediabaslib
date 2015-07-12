;------------------------------------------------------------------------
;		API-Call to program Flash from application 
;------------------------------------------------------------------------
;
;input:	 R22 = command		a0 = r22
;	 X = source
;	 R21, Z = destination	zx = r21
;output: R22 = return code
;use:    R0, R1, R21, R22, R23, X, Z
;
api_call:
	in	a1, SREG
	cli
	cpi	a0, API_PROG_PAGE
	ldi	a0, API_ERR_FUNC
	brne	_apc1
	rcall	prog_page
	ldi	a0, API_SUCCESS
	brcs	_apc1			; CY = 1: o.k.
	ldi	a0, API_ERR_RANGE
_apc1:
	out	SREG, a1
	ret
;-------------------------------------------------------------------------
/* 	.org	Flashend */  ; removed by _conv.awk
/* 	rjmp	api_call */  ; removed by _conv.awk
;-------------------------------------------------------------------------

