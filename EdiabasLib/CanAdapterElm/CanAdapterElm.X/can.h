#ifndef CAN_H
#define CAN_H
#include <stdint.h>
#include <stdbool.h>


#define ECAN_CONFIG_MODE			0x80
#define ECAN_ERROR_INT_ENABLE		0xA0
#define ECAN_NORMAL_MODE			0x00
#define ECAN_RX_INT_ENABLE_FIFO		0x02
#define ECAN_RX_INT_ENABLE_LEGACY	0x03
#define ECAN_SELECT_RX_BUFFER		0x10
#define ECAN_SET_LEGACY_MODE		0x00
#define ECAN_SET_FIFO_MODE			0x80
#define ECAN_TX_INT_ENABLE_LEGACY	0x0C

/** CAN CONTROL *******************************************************/
//- modes of CAN modul -------------------------
#define LEGACY_MODE       0b00000000  //- modes of CAN modul ---------------------
#define E_LEGACY_MODE     0b01000000
#define E_FIFO_MODE       0b10000000

#define NORMAL_MODE       0b00000000  //- modes of CAN-OPERATION -----------------
#define LOOPBACK_MODE     0b01000000
#define DISABLE_MODE      0b00100000
#define LISTEN_ONLY       0b01100000

//----------------------------------------------


/** BAUD RATE CONTROL *******************************************************/
// ****************** BRGCON1 ***********************************
#define SJW_1_TQ        0b00000000 // Synchronization jump width time = 1 x TQ
#define SJW_2_TQ        0b01000000
#define SJW_3_TQ        0b10000000
#define SJW_4_TQ        0b11000000

#define BRP_FOSC_2      0b00000000  // BaudRate Prescaler bits = TQ = (2x5)/FOSC
#define BRP_FOSC_4      0b00000001
#define BRP_FOSC_8      0b00000011
#define BRP_FOSC_10     0b00000100
#define BRP_FOSC_16     0b00000111  // 2x(7+1)
#define BRP_FOSC_32     0b00001111
#define BRP_FOSC_64     0b00011111
#define BRP_FOSC_128    0b00111111

//------------------------------------------------------------------------------

//--------------- BRGCON2 ------------------------------------------------------
#define PSEG1TS_FREE    0b10000000
#define PSEG1TS_MAX     0b00000000

#define SAMPLE_3        0b01000000
#define SAMPLE_1        0b00000000

#define PSEG1T_1_TQ     0b00000000 // Phase Segment 1 time = 1 x TQ
#define PSEG1T_2_TQ     0b00001000
#define PSEG1T_3_TQ     0b00010000
#define PSEG1T_4_TQ     0b00011000
#define PSEG1T_5_TQ     0b00100000
#define PSEG1T_6_TQ     0b00101000
#define PSEG1T_7_TQ     0b00110000
#define PSEG1T_8_TQ     0b00111000

#define PRGT_1_TQ       0b00000000  // Propagation time selection = 1 x TQ
#define PRGT_2_TQ       0b00000001
#define PRGT_3_TQ       0b00000010
#define PRGT_4_TQ       0b00000011
#define PRGT_5_TQ       0b00000100
#define PRGT_6_TQ       0b00000101
#define PRGT_7_TQ       0b00000110
#define PRGT_8_TQ       0b00000111

//--------------- BRGCON3 ------------------------------------------------------
#define WAKDIS_CAN      0b10000000
#define WAKEN_CAN       0b00000000

#define WAKFIL_CAN      0b01000000
#define NWAKFIL_CAN     0b00000000

#define PSEG2T_1_TQ     0b00000000 // Phase Segment 2 time = 1 x TQ
#define PSEG2T_2_TQ     0b00000001
#define PSEG2T_3_TQ     0b00000010
#define PSEG2T_4_TQ     0b00000011
#define PSEG2T_5_TQ     0b00000100
#define PSEG2T_6_TQ     0b00000101
#define PSEG2T_7_TQ     0b00000110
#define PSEG2T_8_TQ     0b00000111

typedef union {
    uint8_t byte;
    struct {
        uint8_t count  : 4;
        uint8_t        : 2;
        uint8_t rtr    : 1;
        uint8_t        : 1;
    } bits;
} CAN_DLC;

typedef struct {
    uint16_t sid;
    CAN_DLC dlc;
    uint8_t data[8];
}CAN_MSG;

//- Function declarations ----------------------
extern void open_can(uint8_t sjw, uint8_t brp_fosz, uint8_t seg1tm, uint8_t prgtm, uint8_t seg2tm,
                uint16_t sid1, uint16_t mask1, uint16_t sid2, uint16_t mask2);

void set_can_mode( uint8_t mode );

extern void baudCAN(uint8_t sjw, uint8_t brp_fosz, uint8_t seg1tm, uint8_t prpgtm, uint8_t seg2tm);

extern bool can_error();

extern void close_can(void);

//---------------------------------------------------------------------------
extern void set_standard_filter_RXB0(uint16_t sid1, uint16_t mask1, uint16_t sid2, uint16_t mask2);
extern bool writeCAN();
extern bool readCAN();
extern CAN_MSG can_in_msg;
extern CAN_MSG can_out_msg;

#endif