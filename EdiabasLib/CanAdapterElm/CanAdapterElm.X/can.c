#include <xc.h>
#include <p18cxxx.h>
#include "can.h"

CAN_MSG can_in_msg;
CAN_MSG can_out_msg;

//------------------------------------------------------------------------------
void open_can(uint8_t sjw, uint8_t brp_fosz, uint8_t seg1tm, uint8_t prpgtm, uint8_t seg2tm,
                uint16_t sid, uint16_t mask)
{
    // ensure ECAN is in config mode
    set_can_mode(ECAN_CONFIG_MODE);

    ECANCON = ECAN_SET_FIFO_MODE | 0x03;    // map transmit buffer 0
    BSEL0   = 0x00;     // (6 + 2) receive buffers

    RXB0CON = 0x00;     // use acceptance filter
    RXB1CON = 0x00;     // use acceptance filter

    // setup mask 0
    RXM0SIDH = mask >> 3;
    RXM0SIDL = mask << 5;
    RXM0EIDH = 0x00;
    RXM0EIDL = 0x00;

    RXF0SIDH = sid >> 3;
    RXF0SIDL = sid << 5;

    // Set mask 0 to filter 0
    MSEL0bits.FIL0 = 0b00;

    // Leave all filters set to RXB0.  The filters will apply to
    // all receive buffers.
    RXFBCON0  = 0x00;
    RXFBCON1  = 0x00;
    RXFBCON2  = 0x00;

    // Enable filters 0 Disable the others.
    RXFCON0  = 0x01;
    RXFCON1  = 0x00;

    //---- set baud rate -----
    BRGCON1 = sjw | brp_fosz;
    BRGCON2 = PSEG1TS_FREE | SAMPLE_3 | seg1tm | prpgtm;
    BRGCON3 = WAKDIS_CAN | NWAKFIL_CAN | seg2tm;

    //  1 = CANTX pin will drive VDD when recessive
    //  0 = CANTX pin will be tri-state when recessive
    CIOCONbits.ENDRHI  = 1;

    set_can_mode(ECAN_NORMAL_MODE);
}

/*********************************************************************
set_can_mode

This routine sets the ECAN module to a specific mode.  It requests the
mode, and then waits until the mode is actually set.

Parameters:	unsigned char	ECAN module mode.  Must be either
							ECAN_CONFIG_MODE or ECAN_NORMAL_MODE
Return:		None
*********************************************************************/
void set_can_mode( uint8_t mode )
{
    CANCON = mode;
    while ((CANSTAT & 0xE0) != mode);
}

//------------------------------------------------------------------------------
void set_standard_filter_RXB0(uint16_t sid, uint16_t mask)
{
    set_can_mode(ECAN_CONFIG_MODE);

    RXF0SIDH = sid >> 3;    // filter 0, mask 0 -> RXB0
    RXF0SIDL = sid << 5;

    RXM0SIDH = mask >> 3;
    RXM0SIDL = mask << 5;

    RXFCON0bits.RXF0EN = 1; // filter 0 enable
}

//------------------------------------------------------------------------------
void baudCAN(uint8_t sjw, uint8_t brp_fosz, uint8_t seg1tm, uint8_t prpgtm, uint8_t seg2tm)
{
    set_can_mode(ECAN_CONFIG_MODE);

    BRGCON1 = sjw | brp_fosz;
    BRGCON2 = PSEG1TS_FREE | SAMPLE_3 | seg1tm | prpgtm;
    BRGCON3 = WAKDIS_CAN | NWAKFIL_CAN | seg2tm;
}

//------------------------------------------------------------------------------
bool writeCAN()
{
    // wait for last transmission to finish
    if (TXB0CONbits.TXREQ)
    {
        return false;
    }

    switch(can_out_msg.dlc.bits.count)
    {
        case 8: TXB0D7 = can_out_msg.data[7];
        case 7: TXB0D6 = can_out_msg.data[6];
        case 6: TXB0D5 = can_out_msg.data[5];
        case 5: TXB0D4 = can_out_msg.data[4];
        case 4: TXB0D3 = can_out_msg.data[3];
        case 3: TXB0D2 = can_out_msg.data[2];
        case 2: TXB0D1 = can_out_msg.data[1];
        case 1: TXB0D0 = can_out_msg.data[0];
        case 0: ;
    }
    TXB0DLC = can_out_msg.dlc.byte;       // load data length
//    TXB0DLCbits.TXRTR = 0;          // send dataframe no RTR

    // Load message ID & enable standard ID
    TXB0SIDH = can_out_msg.sid >> 3;
    TXB0SIDL = can_out_msg.sid << 5;

    // send message
    TXB0CONbits.TXREQ = 1;
    return true;
}

//------------------------------------------------------------------------------
bool readCAN()
{
    if (COMSTATbits.FIFOEMPTY)
    {
        // map first fifo buffer to RXB0
        ECANCON = ECAN_SET_FIFO_MODE | ECAN_SELECT_RX_BUFFER | (CANCON & 0x07);
        if (RXB0CONbits.RXB0FUL)
        {
            can_in_msg.data[0] = RXB0D0;
            can_in_msg.data[1] = RXB0D1;
            can_in_msg.data[2] = RXB0D2;
            can_in_msg.data[3] = RXB0D3;
            can_in_msg.data[4] = RXB0D4;
            can_in_msg.data[5] = RXB0D5;
            can_in_msg.data[6] = RXB0D6;
            can_in_msg.data[7] = RXB0D7;

            can_in_msg.dlc.byte = RXB0DLC;
            can_in_msg.sid = (((uint16_t) RXB0SIDH) << 3) | (RXB0SIDL >> 5);
            RXB0CONbits.RXB0FUL = 0;
            return true;
        }
    }
    return false;
}

//------------------------------------------------------------------------------
void close_can (void)
{
    set_can_mode(ECAN_CONFIG_MODE);
    CANSTAT = DISABLE_MODE;
}

//------------------------------------------------------------------------------
