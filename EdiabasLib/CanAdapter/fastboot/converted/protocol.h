//*************************************************************************
//				Protocol definitions
//-------------------------------------------------------------------------
#define	COMMAND		0xA5		// Command sequence start
#define ESCAPE		COMMAND

#define	CONNECT		0xA6		// connection established
#define	BADCOMMAND	0xA7		// command not supported
#define	ANSWER		0xA8		// followed by length byte
#define	CONTINUE	0xA9
#define	SUCCESS		0xAA
#define	FAIL		0xAB

#define	ESC_SHIFT	0x80		// offset escape char
#define	PROGEND		ESC_SHIFT
//-------------------------------------------------------------------------
//				APICALL definitions
//-------------------------------------------------------------------------
#define API_PROG_PAGE	0x81		// copy one Page from SRAM to Flash

#define API_SUCCESS	0x80		// success
#define API_ERR_FUNC	0xF0		// function not supported
#define API_ERR_RANGE	0xF1		// address inside bootloader
#define API_ERR_PAGE	0xF2		// address not page aligned
//-------------------------------------------------------------------------
