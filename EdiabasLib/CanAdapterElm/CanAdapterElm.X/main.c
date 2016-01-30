/*
 * File:   main.c
 * Author: Ulrich
 *
 * Created on 16. Oktober 2015, 09:58
 */


#include <xc.h>
#include <p18cxxx.h>
#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include "can.h"

#if _HTC_EDITION_ != 2
#error Compiler is not PRO mode, generated code will be too slow
#endif

// #pragma config statements should precede project file includes.
// Use project enums instead of #define for ON and OFF.

// CONFIG1L
#pragma config RETEN = ON       // VREG Sleep Enable bit (Ultra low-power regulator is Enabled (Controlled by SRETEN bit))
#pragma config INTOSCSEL = HIGH // LF-INTOSC Low-power Enable bit (LF-INTOSC in High-power mode during Sleep)
#pragma config SOSCSEL = DIG    // SOSC Power Selection and mode Configuration bits (Digital (SCLKI) mode)
#pragma config XINST = OFF      // Extended Instruction Set (Disabled)

// CONFIG1H
#pragma config FOSC = HS1       // Oscillator (HS oscillator (Medium power, 4 MHz - 16 MHz))
#pragma config PLLCFG = ON      // PLL x4 Enable bit (Enabled)
#pragma config FCMEN = OFF      // Fail-Safe Clock Monitor (Disabled)
#pragma config IESO = OFF       // Internal External Oscillator Switch Over Mode (Disabled)

// CONFIG2L
#pragma config PWRTEN = ON      // Power Up Timer (Enabled)
#pragma config BOREN = SBORDIS  // Brown Out Detect (Enabled in hardware, SBOREN disabled)
#pragma config BORV = 0         // Brown-out Reset Voltage bits (3.0V)
#pragma config BORPWR = HIGH    // BORMV Power level (BORMV set to high power level)

// CONFIG2H
#pragma config WDTEN = ON       // Watchdog Timer (WDT controlled by SWDTEN bit setting)
#pragma config WDTPS = 128      // Watchdog Postscaler (1:128)

// CONFIG3H
#pragma config CANMX = PORTB    // ECAN Mux bit (ECAN TX and RX pins are located on RB2 and RB3, respectively)
#pragma config MSSPMSK = MSK7   // MSSP address masking (7 Bit address masking mode)
#pragma config MCLRE = ON       // Master Clear Enable (MCLR Enabled, RE3 Disabled)

// CONFIG4L
#pragma config STVREN = ON      // Stack Overflow Reset (Enabled)
#pragma config BBSIZ = BB1K     // Boot Block Size (1K word Boot Block size)

// CONFIG5L
#pragma config CP0 = OFF        // Code Protect 00800-01FFF (Disabled)
#pragma config CP1 = OFF        // Code Protect 02000-03FFF (Disabled)
#pragma config CP2 = OFF        // Code Protect 04000-05FFF (Disabled)
#pragma config CP3 = OFF        // Code Protect 06000-07FFF (Disabled)

// CONFIG5H
#pragma config CPB = OFF        // Code Protect Boot (Disabled)
#pragma config CPD = OFF        // Data EE Read Protect (Disabled)

// CONFIG6L
#pragma config WRT0 = OFF       // Table Write Protect 00800-01FFF (Disabled)
#pragma config WRT1 = OFF       // Table Write Protect 02000-03FFF (Disabled)
#pragma config WRT2 = OFF       // Table Write Protect 04000-05FFF (Disabled)
#pragma config WRT3 = OFF       // Table Write Protect 06000-07FFF (Disabled)

// CONFIG6H
#pragma config WRTC = ON        // Config. Write Protect (Enabled)
#pragma config WRTB = ON        // Table Write Protect Boot (Enabled)
#pragma config WRTD = OFF       // Data EE Write Protect (Disabled)

// CONFIG7L
#pragma config EBTR0 = OFF      // Table Read Protect 00800-01FFF (Disabled)
#pragma config EBTR1 = OFF      // Table Read Protect 02000-03FFF (Disabled)
#pragma config EBTR2 = OFF      // Table Read Protect 04000-05FFF (Disabled)
#pragma config EBTR3 = OFF      // Table Read Protect 06000-07FFF (Disabled)

// CONFIG7H
#pragma config EBTRB = OFF      // Table Read Protect Boot (Disabled)

//#pragma config IDLOC0 = 0x2, IDLOC1 = 0x4, IDLOC2 = 0x6, IDLOC3 = 0x8, IDLOC4 = 0xA, IDLOC5 = 0xC, IDLOC6 = 0xE, IDLOC7 = 0x0

#define DEBUG_PIN           0   // enable debug pin
#define ID_LOCATION         0x200000    // location of ID memory

#if ADAPTER_TYPE != 0x02
#define ADAPTER_VERSION     0x0005
#if ADAPTER_TYPE == 0x03
#define REQUIRES_BT_INIT
#define REQUIRES_BT_CRFL
#define BT_COMMAND_PAUSE 50         // bluetooth command pause
#define BT_RESPONSE_TIMEOUT 500
#else
//#define REQUIRES_BT_INIT
//#define REQUIRES_BT_CRFL
#define BT_COMMAND_PAUSE 500        // bluetooth command pause
#define BT_RESPONSE_TIMEOUT 1500    // bluetooth command response timeout
#endif
#else
#define ADAPTER_VERSION     0x0004
#define REQUIRES_BT_CRFL
#endif

#define IGNITION_STATE()    IGNITION

#define KLINE_OUT LATBbits.LATB0
#define LLINE_OUT LATBbits.LATB1
#define LED_RS_RX LATBbits.LATB4
#define LED_RS_TX LATBbits.LATB5
#define LED_OBD_RX LATBbits.LATB6
#define LED_OBD_TX LATBbits.LATB7
#define KLINE_IN PORTCbits.RC1
#define IGNITION PORTCbits.RC4

#define TIMER0_RESOL        15625ul         // 16526 Hz
#define TIMER1_RELOAD       (0x10000-500)   // 1 ms

// K-Line flags
#define KLINEF_PARITY_MASK      0x7
#define KLINEF_PARITY_NONE      0x0
#define KLINEF_PARITY_EVEN      0x1
#define KLINEF_PARITY_ODD       0x2
#define KLINEF_PARITY_MARK      0x3
#define KLINEF_PARITY_SPACE     0x4
#define KLINEF_USE_LLINE        0x08
#define KLINEF_SEND_PULSE       0x10
#define KLINEF_NO_ECHO          0x20
#define KLINEF_FAST_INIT        0x40

#define CAN_MODE            1       // default can mode (1=500kb)
#define CAN_BLOCK_SIZE      0       // 0 is disabled
#define CAN_MIN_SEP_TIME    0       // min separation time (ms)
#define CAN_TIMEOUT         1000    // can receive timeout (1ms)

#define EEP_ADDR_BAUD       0x00    // eeprom address for baud setting (2 bytes)
#define EEP_ADDR_BLOCKSIZE  0x02    // eeprom address for FC block size (2 bytes)
#define EEP_ADDR_SEP_TIME   0x04    // eeprom address for FC separation time (2 bytes)
#define EEP_ADDR_BT_INIT    0x06    // eeprom address for Bluetooth init required (2 bytes)
#define EEP_ADDR_BT_PIN     0x08    // eeprom address for Blutooth pin (16 bytes)
#define EEP_ADDR_BT_NAME    0x18    // eeprom address for Blutooth pin (16 bytes)

#define TEMP_BUF_SIZE       0x0800  // temp buffer size

#if (TEMP_BUF_SIZE & 0xFF) != 0
#error TEMP_BUF_SIZE must be divisible by 256
#endif

// receiver state machine
typedef enum
{
    rec_state_idle,     // wait
    rec_state_rec,      // receive
    rec_state_done,     // receive complete, ok
    rec_state_error,    // receive error
} rec_states;

// constants in rom
static const uint16_t adapter_type @ _ROMSIZE - 4 = ADAPTER_TYPE;
static const uint16_t adapter_version @ _ROMSIZE - 6 = ADAPTER_VERSION;

static volatile bool start_indicator;   // show start indicator
static volatile bool init_failed;       // initialization failed
static uint8_t idle_counter;
#if ADAPTER_TYPE != 0x02
#if defined(REQUIRES_BT_INIT)
static bool init_bt_required;
#endif
static uint8_t pin_buffer[4];
static uint8_t name_buffer[16];
#endif

static volatile rec_states rec_state;
static volatile uint16_t rec_len;
static volatile bool rec_bt_mode;
static uint8_t rec_chksum;
static volatile uint8_t rec_buffer[270];

static uint16_t send_set_idx;
static uint16_t send_get_idx;
static volatile uint16_t send_len;
static volatile uint8_t send_buffer[280];   // larger send buffer for multi responses

static uint32_t kline_baud;     // K-line baud rate, 0=115200 (BMW-FAST))
static uint8_t kline_flags;     // K-line flags
static uint8_t kline_interbyte; // K-line interbyte time [ms]
static uint8_t kline_bit_delay; // K-line read bit delay

static uint8_t temp_buffer[TEMP_BUF_SIZE];
static uint8_t temp_buffer_short[10];

static bool can_enabled;
static uint8_t can_mode;
static uint8_t can_blocksize;
static uint8_t can_sep_time;

// can sender variables
static bool can_send_active;
static bool can_send_wait_for_fc;
static bool can_send_wait_sep_time;
static uint16_t can_send_pos;
static uint8_t can_send_block_count;
static uint8_t can_send_block_size;
static uint8_t can_send_sep_time;
static uint16_t can_send_sep_time_start;
static uint16_t can_send_time;

// can receiver variables
static uint8_t *can_rec_buffer_offset;
static uint8_t can_rec_source_addr;
static uint8_t can_rec_target_addr;
static uint8_t can_rec_block_count;
static uint8_t can_rec_fc_count;
static uint16_t can_rec_time;
static uint16_t can_rec_rec_len;
static uint16_t can_rec_data_len;
static bool can_rec_tel_valid;

void update_led();

inline uint16_t get_systick()
{
    uint8_t low = TMR0L;
    uint8_t high = TMR0H;

    return (((uint16_t)high) << 8) | low;
}

void do_idle()
{
    CLRWDT();
    if (INTCONbits.TMR0IF)
    {
        INTCONbits.TMR0IF = 0;
        idle_counter++;
    }
    if (idle_counter > 2)
    {   // idle
        bool enter_idle = true;
        if (can_mode)
        {
            PIR5 = 0x00;            // clear CAN interrupts
            if (COMSTATbits.FIFOEMPTY)
            {   // CAN message present
                enter_idle = false;
            }
            else
            {
                PIE5bits.RXBnIE = 1;    // enable CAN interrupt for wakeup
            }
        }
        if (enter_idle)
        {
            idle_counter = 0;
            WDTCONbits.SWDTEN = 0;  // disable watchdog
            //LED_OBD_RX = 0;
            SLEEP();
            //LED_OBD_RX = 1;
            WDTCONbits.SWDTEN = 1;  // enable watchdog
            PIE5bits.RXBnIE = 0;
        }
    }
}

void kline_baud_cfg()
{
    // fout = fclk / (4 * prescaler * PR2 * postscaler)
    // PR2 = fclk / (4 * prescaler * fout * postscaler)
    if (kline_baud == 0)
    {   // BMW-FAST
        T2CONbits.TMR2ON = 0;
        T2CONbits.T2CKPS = 0;       // prescaler 1
        T2CONbits.T2OUTPS = 0x0;    // postscaler 1
        TMR2 = 0x00;                // reset timer 2
        // PR2 = 16000000 / (4 * 1 * 115200 * 1) = 34
        PR2 = 34;                   // timer 2 stop value
        kline_bit_delay = 0;
    }
    else
    {
        T2CONbits.TMR2ON = 0;
        T2CONbits.T2CKPS = 1;       // prescaler 4
        T2CONbits.T2OUTPS = 0x0;    // postscaler 1
        TMR2 = 0x00;                // reset timer 2
        PR2 = 16000000ul / 16 / kline_baud;
        kline_bit_delay = 0;
        uint8_t bit_delay = (PR2 >> 1) + 15;
        if (bit_delay < PR2)
        {
            kline_bit_delay = bit_delay;
        }
    }
}

void kline_send(uint8_t *buffer, uint16_t count)
{
    uint8_t *ptr = buffer;

    while (PIE1bits.TXIE)   // uart send active
    {
        update_led();
    }
    update_led();
    if (kline_baud == 0)
    {   // BMW-FAST
        di();
        kline_baud_cfg();
        PIR1bits.TMR2IF = 0;    // clear timer 2 interrupt flag
        T2CONbits.TMR2ON = 1;   // enable timer 2
        LED_OBD_TX = 0;         // on
        LLINE_OUT = 0;          // idle
        for (uint16_t i = 0; i < count; i++)
        {
            CLRWDT();
            while (!PIR1bits.TMR2IF) {}
            PIR1bits.TMR2IF = 0;
            KLINE_OUT = 1;      // start bit

            uint8_t out_data = *ptr++;
            for (uint8_t j = 0; j < 8; j++)
            {
                while (!PIR1bits.TMR2IF) {}
                PIR1bits.TMR2IF = 0;
                if ((out_data & 0x01) != 0)
                {
                    KLINE_OUT = 0;
                }
                else
                {
                    KLINE_OUT = 1;
                }
                out_data >>= 1;
            }
            // 2 stop bits
            while (!PIR1bits.TMR2IF) {}
            PIR1bits.TMR2IF = 0;
            KLINE_OUT = 0;
            while (!PIR1bits.TMR2IF) {}
            PIR1bits.TMR2IF = 0;
        }
        KLINE_OUT = 0;      // idle
        LED_OBD_TX = 1;     // off
        T2CONbits.TMR2ON = 0;
        INTCONbits.TMR0IF = 0;  // clear timer 0 interrupt flag
        idle_counter = 0;
        ei();
        return;
    }
    // dynamic baudrate
    di();
    bool use_lline = false;
    if ((kline_flags & KLINEF_USE_LLINE) != 0)
    {
        use_lline = true;
    }
    if ((kline_flags & KLINEF_SEND_PULSE) != 0)
    {   // send pulse with defined width
        if (count >= 3)
        {
            uint16_t compare_tick = buffer[0] * TIMER0_RESOL / 1000;
            uint8_t bit_count = buffer[1];
            ptr = buffer + 2;
            uint8_t out_data;
            LED_OBD_TX = 0;         // on
            for (uint8_t i = 0; i < bit_count; i++)
            {
                CLRWDT();
                if ((i & 0x07) == 0x00)
                {   // start of new byte
                    out_data = *ptr++;
                }
                if ((out_data & 0x01) != 0)
                {
                    if (use_lline)
                    {
                        LLINE_OUT = 0;
                    }
                    else
                    {
                        KLINE_OUT = 0;
                    }
                }
                else
                {
                    if (use_lline)
                    {
                        LLINE_OUT = 1;
                    }
                    else
                    {
                        KLINE_OUT = 1;
                    }
                }
                out_data >>= 1;
                // wait for pulse width
                uint16_t start_tick = get_systick();
                while ((uint16_t) (get_systick() - start_tick) < compare_tick)
                {
                    CLRWDT();
                }
            }
        }
        KLINE_OUT = 0;      // idle
        LLINE_OUT = 0;      // idle
        LED_OBD_TX = 1;     // off
        idle_counter = 0;
        ei();
        return;
    }

    kline_baud_cfg();
    LED_OBD_TX = 0;         // on
    if ((kline_flags & KLINEF_FAST_INIT) != 0)
    {   // fast init request
        if (use_lline)
        {
            LLINE_OUT = 1;
        }
        else
        {
            KLINE_OUT = 1;
        }
        // pulse with 25 ms
        uint16_t start_tick = get_systick();
        while ((uint16_t) (get_systick() - start_tick) < 25 * TIMER0_RESOL / 1000)
        {
            CLRWDT();
        }
        if (use_lline)
        {
            LLINE_OUT = 0;
        }
        else
        {
            KLINE_OUT = 0;
        }
        // pulse with 25 ms
        uint16_t start_tick = get_systick();
        while ((uint16_t) (get_systick() - start_tick) < 25 * TIMER0_RESOL / 1000)
        {
            CLRWDT();
        }
    }
    PIR1bits.TMR2IF = 0;    // clear timer 2 interrupt flag
    T2CONbits.TMR2ON = 1;   // enable timer 2
    for (uint16_t i = 0; i < count; i++)
    {
        if (kline_interbyte != 0 && i != 0)
        {
            T2CONbits.TMR2ON = 0;   // disable timer 2
            uint16_t start_tick = get_systick();
            uint16_t compare_tick = kline_interbyte * TIMER0_RESOL / 1000;
            while ((uint16_t) (get_systick() - start_tick) < compare_tick)
            {
                CLRWDT();
            }
            PIR1bits.TMR2IF = 0;    // clear timer 2 interrupt flag
            T2CONbits.TMR2ON = 1;   // enable timer 2
        }
        CLRWDT();
        while (!PIR1bits.TMR2IF) {}
        PIR1bits.TMR2IF = 0;
        if (use_lline)
        {
            LLINE_OUT = 1;      // start bit
        }
        else
        {
            KLINE_OUT = 1;      // start bit
        }

        uint8_t out_data = *ptr++;
        uint8_t parity = 0;
        for (uint8_t j = 0; j < 8; j++)
        {
            while (!PIR1bits.TMR2IF) {}
            PIR1bits.TMR2IF = 0;
            if ((out_data & 0x01) != 0)
            {
                if (use_lline)
                {
                    LLINE_OUT = 0;
                }
                else
                {
                    KLINE_OUT = 0;
                }
                parity++;
            }
            else
            {
                if (use_lline)
                {
                    LLINE_OUT = 1;
                }
                else
                {
                    KLINE_OUT = 1;
                }
            }
            out_data >>= 1;
        }
        switch (kline_flags & KLINEF_PARITY_MASK)
        {
            case KLINEF_PARITY_EVEN:
                while (!PIR1bits.TMR2IF) {}
                PIR1bits.TMR2IF = 0;
                if ((parity & 0x01) != 0)
                {
                    if (use_lline)
                    {
                        LLINE_OUT = 0;
                    }
                    else
                    {
                        KLINE_OUT = 0;
                    }
                }
                else
                {
                    if (use_lline)
                    {
                        LLINE_OUT = 1;
                    }
                    else
                    {
                        KLINE_OUT = 1;
                    }
                }
                break;

            case KLINEF_PARITY_ODD:
                while (!PIR1bits.TMR2IF) {}
                PIR1bits.TMR2IF = 0;
                if ((parity & 0x01) != 0)
                {
                    if (use_lline)
                    {
                        LLINE_OUT = 1;
                    }
                    else
                    {
                        KLINE_OUT = 1;
                    }
                }
                else
                {
                    if (use_lline)
                    {
                        LLINE_OUT = 0;
                    }
                    else
                    {
                        KLINE_OUT = 0;
                    }
                }
                break;

            case KLINEF_PARITY_MARK:
                while (!PIR1bits.TMR2IF) {}
                PIR1bits.TMR2IF = 0;
                if (use_lline)
                {
                    LLINE_OUT = 0;
                }
                else
                {
                    KLINE_OUT = 0;
                }
                break;

            case KLINEF_PARITY_SPACE:
                while (!PIR1bits.TMR2IF) {}
                PIR1bits.TMR2IF = 0;
                if (use_lline)
                {
                    LLINE_OUT = 1;
                }
                else
                {
                    KLINE_OUT = 1;
                }
                break;
        }
        // 2 stop bits
        while (!PIR1bits.TMR2IF) {}
        PIR1bits.TMR2IF = 0;
        if (use_lline)
        {
            LLINE_OUT = 0;
        }
        else
        {
            KLINE_OUT = 0;
        }
        while (!PIR1bits.TMR2IF) {}
        PIR1bits.TMR2IF = 0;
    }
    KLINE_OUT = 0;      // idle
    LLINE_OUT = 0;      // idle
    LED_OBD_TX = 1;     // off
    T2CONbits.TMR2ON = 0;
    INTCONbits.TMR0IF = 0;  // clear timer 0 interrupt flag
    idle_counter = 0;
    ei();
}

void inline start_kline_rec_timer()
{
    if (!T2CONbits.TMR2ON && !KLINE_IN)
    {
        if (kline_bit_delay != 0)
        {
            TMR2 = kline_bit_delay;
            T2CONbits.TMR2ON = 1;
            while (!PIR1bits.TMR2IF) {}
            PIR1bits.TMR2IF = 0;
        }
        else
        {
            T2CONbits.TMR2ON = 1;
        }
    }
}

void kline_receive()
{
    uint8_t const temp_bufferh = HIGH_BYTE((uint16_t) temp_buffer);
    uint16_t buffer_len = 0;
    uint8_t *write_ptr = temp_buffer;
    uint8_t *read_ptr = temp_buffer;

    if (kline_baud == 0)
    {   // BMW-FAST
        di();
        kline_baud_cfg();
        PIR1bits.TMR2IF = 0;    // clear timer 2 interrupt flag
        INTCONbits.TMR0IF = 0;  // clear timer 0 interrupt flag
        idle_counter = 0;
        for (;;)
        {
            // wait for start bit
            for (;;)
            {
                CLRWDT();
                if (!KLINE_IN) T2CONbits.TMR2ON = 1;
                if (T2CONbits.TMR2ON)
                {
                    break;
                }
                if (PIR1bits.RCIF)
                {   // start of new UART telegram
                    ei();
                    return;
                }
                if (INTCONbits.TMR0IF)
                {
                    INTCONbits.TMR0IF = 0;
                    idle_counter++;
                    if (idle_counter > 16)  // 60 sec.
                    {   // idle -> leave loop
                        ei();
                        return;
                    }
                }
                if (!KLINE_IN) T2CONbits.TMR2ON = 1;
                if (buffer_len != 0)
                {   // send data back to UART
                    if (TXSTAbits.TRMT)
                    {   // transmitter empty
                        if (!KLINE_IN) T2CONbits.TMR2ON = 1;
                        TXREG = *read_ptr;
                        if (!KLINE_IN) T2CONbits.TMR2ON = 1;
                        read_ptr++;
                        if (!KLINE_IN) T2CONbits.TMR2ON = 1;
                        uint8_t diff = *(((uint8_t *) &read_ptr) + 1) - temp_bufferh;
                        if (!KLINE_IN) T2CONbits.TMR2ON = 1;
                        if (diff == (sizeof(temp_buffer) >> 8))
                        {
                            if (!KLINE_IN) T2CONbits.TMR2ON = 1;
                            read_ptr = temp_buffer;
                        }
                        if (!KLINE_IN) T2CONbits.TMR2ON = 1;
                        buffer_len--;
                    }
                }
            }
            idle_counter = 0;
            LED_OBD_RX = 0; // on
            uint8_t data = 0x00;
            for (uint8_t i = 0; i < 8 ; i++)
            {
                while (!PIR1bits.TMR2IF) {}
                PIR1bits.TMR2IF = 0;
#if DEBUG_PIN
                LED_RS_TX = 1;
#endif
                if (KLINE_IN)
                {
                    data >>= 1;
                    data |= 0x80;
                }
                else
                {
                    data >>= 1;
                }
#if DEBUG_PIN
                LED_RS_TX = 0;
#endif
            }
#if DEBUG_PIN
            LED_RS_TX = 1;
#endif
            if (buffer_len < sizeof(temp_buffer))
            {
                *write_ptr = data;
                write_ptr++;
                buffer_len++;
                uint8_t diff = *(((uint8_t *) &write_ptr) + 1) - temp_bufferh;
                if (diff == (sizeof(temp_buffer) >> 8))
                {
                    write_ptr = temp_buffer;
                }
            }
            T2CONbits.TMR2ON = 0;
            TMR2 = 0;               // reset timer
            PIR1bits.TMR2IF = 0;    // clear timer 2 interrupt flag
            LED_OBD_RX = 1; // off
#if DEBUG_PIN
            LED_RS_TX = 0;
#endif
        }
    }
    // dynamic baudrate
    di();
    kline_baud_cfg();
    PIR1bits.TMR2IF = 0;    // clear timer 2 interrupt flag
    INTCONbits.TMR0IF = 0;  // clear timer 0 interrupt flag
    idle_counter = 0;
    for (;;)
    {
        // wait for start bit
        for (;;)
        {
            CLRWDT();
            start_kline_rec_timer();
            if (T2CONbits.TMR2ON)
            {
                break;
            }
            if (PIR1bits.RCIF)
            {   // start of new UART telegram
                ei();
                return;
            }
            if (INTCONbits.TMR0IF)
            {
                INTCONbits.TMR0IF = 0;
                idle_counter++;
                if (idle_counter > 16)  // 60 sec.
                {   // idle -> leave loop
                    ei();
                    return;
                }
            }
            start_kline_rec_timer();
            if (buffer_len != 0)
            {   // send data back to UART
                if (TXSTAbits.TRMT)
                {   // transmitter empty
                    start_kline_rec_timer();
                    TXREG = *read_ptr;
                    start_kline_rec_timer();
                    read_ptr++;
                    start_kline_rec_timer();
                    uint8_t diff = *(((uint8_t *) &read_ptr) + 1) - temp_bufferh;
                    start_kline_rec_timer();
                    if (diff == (sizeof(temp_buffer) >> 8))
                    {
                        start_kline_rec_timer();
                        read_ptr = temp_buffer;
                    }
                    start_kline_rec_timer();
                    buffer_len--;
                }
            }
        }
        idle_counter = 0;
        LED_OBD_RX = 0; // on
        uint8_t data = 0x00;
        for (uint8_t i = 0; i < 8 ; i++)
        {
            while (!PIR1bits.TMR2IF) {}
            PIR1bits.TMR2IF = 0;
#if DEBUG_PIN
            LED_RS_TX = 1;
#endif
            if (KLINE_IN)
            {
                data >>= 1;
                data |= 0x80;
            }
            else
            {
                data >>= 1;
            }
#if DEBUG_PIN
            LED_RS_TX = 0;
#endif
        }
#if DEBUG_PIN
        LED_RS_TX = 1;
#endif
        if ((kline_flags & KLINEF_PARITY_MASK) != KLINEF_PARITY_NONE)
        {   // read parity bit
            while (!PIR1bits.TMR2IF) {}
            PIR1bits.TMR2IF = 0;
        }
        if (buffer_len < sizeof(temp_buffer))
        {
            *write_ptr = data;
            write_ptr++;
            buffer_len++;
            uint8_t diff = *(((uint8_t *) &write_ptr) + 1) - temp_bufferh;
            if (diff == (sizeof(temp_buffer) >> 8))
            {
                write_ptr = temp_buffer;
            }
        }
        // read stop bit
        while (!PIR1bits.TMR2IF) {}
        PIR1bits.TMR2IF = 0;

        T2CONbits.TMR2ON = 0;
        TMR2 = 0x00;
        PIR1bits.TMR2IF = 0;    // clear timer 2 interrupt flag
        LED_OBD_RX = 1; // off
#if DEBUG_PIN
        LED_RS_TX = 0;
#endif
    }
}

uint8_t calc_checkum(uint8_t *buffer, uint16_t len)
{
    uint8_t sum = 0;
    for (uint16_t i = 0; i < len; i++)
    {
        sum += buffer[i];
    }
    return sum;
}

bool uart_send(uint8_t *buffer, uint16_t count)
{
    uint16_t volatile temp_len;

    if (count == 0)
    {
        return true;
    }
    idle_counter = 0;
    di();
    temp_len = send_len;
    ei();

    if (temp_len + count > sizeof(send_buffer))
    {
        return false;
    }

    for (uint16_t i = 0; i < count; i++)
    {
        send_buffer[send_set_idx++] = buffer[i];
        if (send_set_idx >= sizeof(send_buffer))
        {
            send_set_idx = 0;
        }
    }

    di();
    send_len += count;
    PIE1bits.TXIE = 1;    // enable TX interrupt
    ei();
    return true;
}

uint16_t uart_receive(uint8_t *buffer)
{
    if (rec_state != rec_state_done)
    {
        return 0;
    }
    idle_counter = 0;

    uint16_t data_len;
    if (rec_buffer[0] == 0x00)
    {   // special mode
        // byte 1: telegram type
        // byte 2+3: baud rate (high/low) / 2
        // byte 4: flags
        // byte 5: interbyte time
        // byte 6+7: telegram length (high/low)
        kline_baud = (((uint32_t) rec_buffer[2] << 8) + rec_buffer[3]) << 1;
        if ((kline_baud != 0) && ((kline_baud < 9600) || (kline_baud > 19200)))
        {
            data_len = 0;
        }
        else
        {
            kline_flags = rec_buffer[4];
            kline_interbyte = rec_buffer[5];
            data_len = ((uint16_t) rec_buffer[6] << 8) + rec_buffer[7];
            memcpy(buffer, rec_buffer + 8, data_len);
        }
    }
    else
    {
        kline_baud = 0;
        kline_flags = 0;
        kline_interbyte = 0;
        data_len = rec_len;
        memcpy(buffer, rec_buffer, data_len);
    }
    rec_state = rec_state_idle;
    return data_len;
}

#if ADAPTER_TYPE != 0x02
bool send_bt_config(uint8_t *buffer, uint16_t count, uint8_t retries)
{
    for (uint8_t i = 0; i < retries; i++)
    {
        if (!uart_send(buffer, count))
        {
            return false;
        }
        uint16_t start_tick = get_systick();
        for (;;)
        {
            CLRWDT();
            update_led();
            uint16_t len = uart_receive(temp_buffer);
            if (len > 0)
            {
                // pause after command
                uint16_t start_tick2 = get_systick();
                while ((uint16_t) (get_systick() - start_tick2) < (BT_COMMAND_PAUSE * TIMER0_RESOL / 1000))
                {
                    CLRWDT();
                }
                return true;
            }
            if ((uint16_t) (get_systick() - start_tick) > (BT_RESPONSE_TIMEOUT * TIMER0_RESOL / 1000))
            {
                break;
            }
        }
    }
    return false;
}

#if defined(REQUIRES_BT_INIT)
bool set_bt_default()
{
    static const char bt_default[] = "AT+DEFAULT\r\n";
    return send_bt_config((uint8_t *) bt_default, sizeof(bt_default) - 1, 3);
}

bool set_bt_baud()
{
    static const char bt_baud[] = "AT+BAUD8\r\n";   // 115200

    uint8_t old_baudl = SPBRG1;
    uint8_t old_baudh = SPBRGH1;

    RCSTAbits.SPEN = 0;
    SPBRGH1 = 415 >> 8;
    SPBRG1 = 415;        // 9600 @ 16MHz
    RCSTAbits.SPEN = 1;

    send_bt_config((uint8_t *) bt_baud, sizeof(bt_baud) - 1, 2);

    RCSTAbits.SPEN = 0;
    SPBRGH1 = old_baudh;
    SPBRG1 = old_baudl;
    RCSTAbits.SPEN = 1;

    return send_bt_config((uint8_t *) bt_baud, sizeof(bt_baud) - 1, 2);
}

bool set_bt_init()
{
    static const char * const bt_init[] =
    {
        "AT+ENABLEIND0\r\n",
        "AT+ROLE0\r\n",
    };
    bool result = true;
    for (uint8_t i = 0; i < sizeof(bt_init)/sizeof(bt_init[0]); i++)
    {
        const char *pinit = bt_init[i];
        if (!send_bt_config((uint8_t *) pinit, strlen(pinit), 3))
        {
            result = false;
        }
    }
    return result;
}
#endif

bool set_bt_pin()
{
    temp_buffer[0] = 'A';
    temp_buffer[1] = 'T';
    temp_buffer[2] = '+';
    temp_buffer[3] = 'P';
    temp_buffer[4] = 'I';
    temp_buffer[5] = 'N';

    uint8_t len = 6;
    for (uint8_t i = 0; i < sizeof(pin_buffer); i++)
    {
        uint8_t value = pin_buffer[i];
        if (value == 0)
        {
            break;
        }
        temp_buffer[len++] = value;
    }
#if defined(REQUIRES_BT_CRFL)
    temp_buffer[len++] = '\r';
    temp_buffer[len++] = '\n';
#endif

    return send_bt_config(temp_buffer, len, 3);
}

bool set_bt_name()
{
    temp_buffer[0] = 'A';
    temp_buffer[1] = 'T';
    temp_buffer[2] = '+';
    temp_buffer[3] = 'N';
    temp_buffer[4] = 'A';
    temp_buffer[5] = 'M';
    temp_buffer[6] = 'E';

    uint8_t len = 7;
    for (uint8_t i = 0; i < sizeof(name_buffer); i++)
    {
        uint8_t value = name_buffer[i];
        if (value == 0)
        {
            break;
        }
        temp_buffer[len++] = value;
    }
#if defined(REQUIRES_BT_CRFL)
    temp_buffer[len++] = 0x00;
    temp_buffer[len++] = '\r';
    temp_buffer[len++] = '\n';
#endif

    return send_bt_config(temp_buffer, len, 3);
}

bool init_bt()
{
    if (!RCONbits.RI)
    {   // do nothing after a software reset
        return true;
    }
    di();
    rec_bt_mode = true;
    rec_state = rec_state_idle;
    ei();
    // wait for bt chip init
    uint16_t start_tick = get_systick();
    while ((uint16_t) (get_systick() - start_tick) < (1000 * TIMER0_RESOL / 1000))
    {
        CLRWDT();
        update_led();
    }

    bool result = true;
#if defined(REQUIRES_BT_INIT)
    if (init_bt_required)
    {
        //set_bt_default();   // for testing
        if (!set_bt_baud())
        {
            result = false;
        }
        if (!set_bt_init())
        {
            result = false;
        }
        if (result)
        {
            eeprom_write(EEP_ADDR_BT_INIT, 0x01);
            eeprom_write(EEP_ADDR_BT_INIT + 1, ~0x01);
        }
    }
#endif
    if (!set_bt_pin())
    {
        result = false;
    }
    if (!set_bt_name())
    {
        result = false;
    }

    di();
    rec_bt_mode = false;
    rec_state = rec_state_idle;
    ei();
    return result;
}
#endif

void update_led()
{
    if (start_indicator)
    {
        uint16_t tick = get_systick();
        if (tick < 500 * TIMER0_RESOL / 1000)
        {
            LED_OBD_RX = 1; // off
            LED_OBD_TX = 0; // on
        }
        else if (tick < 1000 * TIMER0_RESOL / 1000)
        {
            LED_OBD_RX = 0; // on
            LED_OBD_TX = 1; // off
        }
        else
        {
            start_indicator = false;
        }
    }
    if (!start_indicator)
    {
        if (rec_state != rec_state_idle)
        {
            LED_OBD_RX = 0; // on
        }
        else
        {
            if (init_failed)
            {
                LED_OBD_RX = 0;     // on
            }
            else
            {
                LED_OBD_RX = 1; // off
            }
        }
        if (send_len > 0)
        {
            init_failed = false;
            LED_OBD_TX = 0; // on
        }
        else
        {
            LED_OBD_TX = 1; // off
        }
    }
}

void can_config()
{
    uint8_t bitrate = 5;
    switch (can_mode)
    {
        case 0:     // can off
            can_enabled = false;
            break;

        default:
        case 1:     // can 500kb
            bitrate = 5;
            can_enabled = true;
            break;

        case 9:     // can 100kb
            bitrate = 1;
            can_enabled = true;
            break;
    }
    if (can_enabled)
    {
        if (bitrate == 1)
        {   // 100 kb
            open_can(SJW_2_TQ, BRP_FOSC_10, PSEG1T_6_TQ, PRGT_7_TQ, PSEG2T_2_TQ,
                0x600, 0x700);
        }
        else
        {   // 500kb
            open_can(SJW_2_TQ, BRP_FOSC_2, PSEG1T_6_TQ, PRGT_7_TQ, PSEG2T_2_TQ,
                0x600, 0x700);
        }
    }
    else
    {
        close_can();
    }
    kline_baud = 0;
    kline_flags = 0;
    kline_interbyte = 0;
}

void read_eeprom()
{
    uint8_t temp_value1;
    uint8_t temp_value2;

      // wait for write to finish
    while(WR) continue;

    temp_value1 = eeprom_read(EEP_ADDR_BAUD);
    temp_value2 = eeprom_read(EEP_ADDR_BAUD + 1);
    can_mode = CAN_MODE;
    if ((~temp_value1 & 0xFF) == temp_value2)
    {
        can_mode = temp_value1;
    }

    temp_value1 = eeprom_read(EEP_ADDR_BLOCKSIZE);
    temp_value2 = eeprom_read(EEP_ADDR_BLOCKSIZE + 1);
    can_blocksize = CAN_BLOCK_SIZE;
    if ((~temp_value1 & 0xFF) == temp_value2)
    {
        can_blocksize = temp_value1;
    }

    temp_value1 = eeprom_read(EEP_ADDR_SEP_TIME);
    temp_value2 = eeprom_read(EEP_ADDR_SEP_TIME + 1);
    can_sep_time = CAN_MIN_SEP_TIME;
    if ((~temp_value1 & 0xFF) == temp_value2)
    {
        can_sep_time = temp_value1;
    }

#if ADAPTER_TYPE != 0x02
#if defined(REQUIRES_BT_INIT)
    temp_value1 = eeprom_read(EEP_ADDR_BT_INIT);
    temp_value2 = eeprom_read(EEP_ADDR_BT_INIT + 1);
    init_bt_required = true;
    if ((~temp_value1 & 0xFF) == temp_value2)
    {
        if (temp_value1 == 0x01)
        {
            init_bt_required = false;
        }
    }
#endif

    uint8_t pin_len = 0;
    for (uint8_t i = 0; i < sizeof(pin_buffer); i++)
    {
        temp_value1 = eeprom_read(EEP_ADDR_BT_PIN + i);
        if (temp_value1 < '0' || temp_value1 > '9')
        {
            temp_value1 = 0;
        }
        else
        {
            pin_len = i + 1;
        }
        pin_buffer[i] = temp_value1;
    }
    if (pin_len < 4)
    {
        static const uint8_t default_pin[] = {'1', '2', '3', '4'};
        memcpy(pin_buffer, default_pin, sizeof(default_pin));
    }

    for (uint8_t i = 0; i < sizeof(name_buffer); i++)
    {
        temp_value1 = eeprom_read(EEP_ADDR_BT_NAME + i);
        if (i == 0 && temp_value1 == 0xFF)
        {
            temp_value1 = 0;
        }
        name_buffer[i] = temp_value1;
    }
    if (name_buffer[0] == 0)
    {
        static const char default_name[] = {"Deep OBD BMW"};
        memcpy(name_buffer, default_name, sizeof(default_name));
    }
#endif
}

bool can_send_message_wait()
{
    uint16_t start_tick = get_systick();
    while (!writeCAN())
    {
        CLRWDT();
        update_led();
        if ((uint16_t) (get_systick() - start_tick) > (250 * TIMER0_RESOL / 1000))
        {
            return false;
        }
    }
    return true;
}

bool internal_telegram(uint16_t len)
{
    if ((len == 5) &&
    (temp_buffer[0] == 0x81) &&
    (temp_buffer[1] == 0x00) &&
    (temp_buffer[2] == 0x00))
    {
        uint8_t cfg_value = temp_buffer[3];
        eeprom_write(EEP_ADDR_BAUD, cfg_value);
        eeprom_write(EEP_ADDR_BAUD + 1, ~cfg_value);
        read_eeprom();
        can_config();
        temp_buffer[3] = ~can_mode;
        temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
        uart_send(temp_buffer, len);
        return true;
    }

    if ((temp_buffer[1] == 0xF1) &&
        (temp_buffer[2] == 0xF1))
    {
        if ((len == 6) && (temp_buffer[3] & 0x7F) == 0x00)
        {      // block size
            if ((temp_buffer[3] & 0x80) == 0x00)
            {   // write
                uint8_t cfg_value = temp_buffer[4];
                eeprom_write(EEP_ADDR_BLOCKSIZE, cfg_value);
                eeprom_write(EEP_ADDR_BLOCKSIZE + 1, ~cfg_value);
                read_eeprom();
            }
            temp_buffer[4] = can_blocksize;
            temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((len == 6) && (temp_buffer[3] & 0x7F) == 0x01)
        {      // separation time
            if ((temp_buffer[3] & 0x80) == 0x00)
            {   // write
                uint8_t cfg_value = temp_buffer[4];
                eeprom_write(EEP_ADDR_SEP_TIME, cfg_value);
                eeprom_write(EEP_ADDR_SEP_TIME + 1, ~cfg_value);
                read_eeprom();
            }
            temp_buffer[4] = can_sep_time;
            temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((len == 6) && (temp_buffer[3] & 0x7F) == 0x02)
        {      // can mode
            if ((temp_buffer[3] & 0x80) == 0x00)
            {   // write
                uint8_t cfg_value = temp_buffer[4];
                eeprom_write(EEP_ADDR_BAUD, cfg_value);
                eeprom_write(EEP_ADDR_BAUD + 1, ~cfg_value);
                read_eeprom();
                can_config();
            }
            temp_buffer[4] = can_mode;
            temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((len >= 6) && (temp_buffer[3] & 0x7F) == 0x04)
        {      // bt pin
#if ADAPTER_TYPE != 0x02
            if ((temp_buffer[3] & 0x80) == 0x00)
            {   // write
                for (uint8_t i = 0; i < sizeof(pin_buffer); i++)
                {
                    uint8_t cfg_value = 0;
                    if (i < (len - 5))
                    {
                        cfg_value = temp_buffer[4 + i];
                    }
                    eeprom_write(EEP_ADDR_BT_PIN + i, cfg_value);
                }
                read_eeprom();
            }
            memcpy(temp_buffer + 4, pin_buffer, sizeof(pin_buffer));
            len = 5 + sizeof(pin_buffer);
#else
            len = 5;    // no pin
#endif
            temp_buffer[0] = 0x80 + len - 4;
            temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((len >= 6) && (temp_buffer[3] & 0x7F) == 0x05)
        {      // bt name
#if ADAPTER_TYPE != 0x02
            if ((temp_buffer[3] & 0x80) == 0x00)
            {   // write
                for (uint8_t i = 0; i < sizeof(name_buffer); i++)
                {
                    uint8_t cfg_value = 0;
                    if (i < (len - 5))
                    {
                        cfg_value = temp_buffer[4 + i];
                    }
                    eeprom_write(EEP_ADDR_BT_NAME + i, cfg_value);
                }
                read_eeprom();
            }
            memcpy(temp_buffer + 4, name_buffer, sizeof(name_buffer));
            len = 5 + sizeof(name_buffer);
#else
            len = 5;    // no name
#endif
            temp_buffer[0] = 0x80 + len - 4;
            temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((len == 6) && (temp_buffer[3] == 0xFB) && (temp_buffer[4] == 0xFB))
        {      // read id location
            temp_buffer[0] = 0x89;
            const uint8_t far *id_loc=(const uint8_t far *) ID_LOCATION;
            for (uint8_t i = 0; i < 8; i++)
            {
                temp_buffer[4 + i] = (*id_loc++);
            }
            len = 13;
            temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((len == 6) && (temp_buffer[3] == 0xFC) && (temp_buffer[4] == 0xFC))
        {      // read Vbat
            ADCON0bits.GODONE = 1;
            while (ADCON0bits.GODONE) {}
            temp_buffer[4] = (((int16_t) ADRES) * 50l * 6l / 4096l); // Voltage*10
            temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((len == 6) && (temp_buffer[3] == 0xFD) && (temp_buffer[4] == 0xFD))
        {      // read adapter type and version
            temp_buffer[0] = 0x85;
            temp_buffer[4] = ADAPTER_TYPE >> 8;
            temp_buffer[5] = ADAPTER_TYPE;
            temp_buffer[6] = ADAPTER_VERSION >> 8;
            temp_buffer[7] = ADAPTER_VERSION;
            len = 9;
            temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((len == 6) && (temp_buffer[3] == 0xFE) && (temp_buffer[4] == 0xFE))
        {      // read ignition state
            temp_buffer[4] = IGNITION_STATE() ? 0x01 : 0x00;
            temp_buffer[4] |= 0x80;     // invalid mark
            temp_buffer[len - 1] = calc_checkum(temp_buffer, len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((temp_buffer[3] == 0xFF) && (temp_buffer[4] == 0xFF))
        {      // reset command
            RESET();
            return true;
        }
        return true;
    }
    return false;
}

void can_sender(bool new_can_msg)
{
    if (!can_enabled)
    {
        return;
    }
    if (!can_send_active && !PIE1bits.TXIE)
    {
        uint16_t len = uart_receive(temp_buffer);
        if (len > 0)
        {
            if ((kline_flags & KLINEF_NO_ECHO) == 0)
            {
                uart_send(temp_buffer, len);
            }
            if (internal_telegram(len))
            {
                return;
            }
            can_send_active = true;
            can_send_wait_sep_time = false;
            can_send_pos = 0;
        }
    }
    if (can_send_active)
    {
        idle_counter = 0;
        uint8_t *data_offset = &temp_buffer[3];
        uint8_t data_len = temp_buffer[0] & 0x3F;
        if (data_len == 0)
        {
            data_len = temp_buffer[3];
            data_offset = &temp_buffer[4];
        }
        uint8_t target_address = temp_buffer[1];
        uint8_t source_address = temp_buffer[2];
        if (can_send_pos == 0)
        {   // start sending
            if (data_len <= 6)
            {   // single frame
                memset(&can_out_msg, 0x00, sizeof(can_out_msg));
                can_out_msg.sid = 0x600 | source_address;    // source address
                can_out_msg.dlc.bits.count = 8;
                can_out_msg.data[0] = target_address;
                can_out_msg.data[1] = 0x00 | data_len;      // single frame + length
                memcpy(can_out_msg.data + 2, data_offset, data_len);

                can_send_message_wait();
                can_send_active = false;
                return;
            }
            // first frame
            memset(&can_out_msg, 0x00, sizeof(can_out_msg));
            can_out_msg.sid = 0x600 | source_address;    // source address
            can_out_msg.dlc.bits.count = 8;
            can_out_msg.data[0] = target_address;
            can_out_msg.data[1] = 0x10 | ((data_len >> 8) & 0xFF);      // first frame + length
            can_out_msg.data[2] = data_len;
            uint8_t len = 5;
            memcpy(can_out_msg.data + 3, data_offset, len);
            can_send_pos += len;

            can_send_message_wait();
            can_send_wait_for_fc = true;
            can_send_block_count = 1;
            can_send_time = get_systick();
            return;
        }
        if (can_send_wait_for_fc)
        {
            if (new_can_msg)
            {
                if (((can_in_msg.sid & 0xFF00) == 0x0600) &&
                    (can_in_msg.dlc.bits.count >= 4) &&
                    ((can_in_msg.sid & 0x00FF) == target_address) &&
                    (can_in_msg.data[0] == source_address) &&
                    ((can_in_msg.data[1] & 0xF0) == 0x30)  // FC
                    )
                {
                    switch (can_in_msg.data[1] & 0x0F)
                    {
                        case 0: // CTS
                            can_send_wait_for_fc = false;
                            break;

                        case 1: // Wait
                            can_send_time = get_systick();
                            break;

                        default:    // invalid
                            break;
                    }
                    can_send_block_size = can_in_msg.data[2];
                    can_send_sep_time = can_in_msg.data[3];
                }
            }
            if (can_send_wait_for_fc)
            {
                if ((uint16_t) (get_systick() - can_send_time) > (CAN_TIMEOUT * TIMER0_RESOL / 1000))
                {   // FC timeout
                    can_send_active = false;
                    return;
                }
            }
            return;
        }
        if (can_send_wait_sep_time)
        {
            if ((uint16_t) (get_systick() - can_send_sep_time_start) <= ((uint16_t) (can_send_sep_time * TIMER0_RESOL / 1000)))
            {
                return;
            }
            can_send_wait_sep_time = false;
        }
        // consecutive frame
        memset(&can_out_msg, 0x00, sizeof(can_out_msg));
        can_out_msg.sid = 0x600 | source_address;    // source address
        can_out_msg.dlc.bits.count = 8;
        can_out_msg.data[0] = target_address;
        can_out_msg.data[1] = 0x20 | (can_send_block_count & 0x0F);      // consecutive frame + block count
        uint8_t len = data_len - can_send_pos;
        if (len > 6)
        {
            len = 6;
        }
        memcpy(can_out_msg.data + 2, data_offset + can_send_pos, len);
        can_send_pos += len;
        can_send_block_count++;

        can_send_message_wait();

        if (can_send_pos >= data_len)
        {   // all blocks transmitted
            can_send_active = false;
            return;
        }
        can_send_wait_for_fc = false;
        if (can_send_block_size > 0)
        {
            if (can_send_block_size == 1)
            {
                can_send_wait_for_fc = true;
                can_send_time = get_systick();
            }
            can_send_block_size--;
        }
        if (!can_send_wait_for_fc && can_send_sep_time > 0)
        {
            can_send_wait_sep_time = true;
            can_send_sep_time_start = get_systick();
        }
    }
}

void can_receiver(bool new_can_msg)
{
    if (!can_enabled)
    {
        return;
    }
    if (new_can_msg)
    {
        idle_counter = 0;
        if (((can_in_msg.sid & 0xFF00) == 0x0600) && (can_in_msg.dlc.bits.count >= 2))
        {
            uint8_t frame_type = (can_in_msg.data[1] >> 4) & 0x0F;
            switch (frame_type)
            {
                case 0:     // single frame
                {
                    uint8_t rec_source_addr = can_in_msg.sid & 0xFF;
                    uint8_t rec_target_addr = can_in_msg.data[0];
                    uint8_t rec_data_len = can_in_msg.data[1] & 0x0F;
                    if (rec_data_len > (can_in_msg.dlc.bits.count - 2))
                    {   // invalid length
                        break;
                    }
                    temp_buffer_short[0] = 0x80 | rec_data_len;
                    temp_buffer_short[1] = rec_target_addr;
                    temp_buffer_short[2] = rec_source_addr;
                    memcpy(temp_buffer_short + 3, can_in_msg.data + 2, rec_data_len);
                    uint8_t len = rec_data_len + 3;

                    temp_buffer_short[len] = calc_checkum(temp_buffer_short, len);
                    len++;
                    uart_send(temp_buffer_short, len);
                    break;
                }

                case 1:     // first frame
                    if (can_rec_tel_valid)
                    {   // ignore new first frames during reception
                        break;
                    }
                    if (can_in_msg.dlc.bits.count < 8)
                    {   // invalid length
                        break;
                    }
                    can_rec_source_addr = can_in_msg.sid & 0xFF;
                    can_rec_target_addr = can_in_msg.data[0];
                    can_rec_data_len = (((uint16_t) can_in_msg.data[1] & 0x0F) << 8) + can_in_msg.data[2];
                    if (can_rec_data_len > 0xFF)
                    {   // too long
                        can_rec_tel_valid = false;
                        break;
                    }
                    if (can_rec_data_len > 0x3F)
                    {
                        can_rec_buffer_offset = temp_buffer + 4;
                    }
                    else
                    {
                        can_rec_buffer_offset = temp_buffer + 3;
                    }
                    memcpy(can_rec_buffer_offset, can_in_msg.data + 3, 5);
                    can_rec_rec_len = 5;
                    can_rec_block_count = 1;

                    memset(&can_out_msg, 0x00, sizeof(can_out_msg));
                    can_out_msg.sid = 0x600 | can_rec_target_addr;
                    can_out_msg.dlc.bits.count = 8;
                    can_out_msg.data[0] = can_rec_source_addr;
                    can_out_msg.data[1] = 0x30;     // FC
                    can_out_msg.data[2] = can_blocksize;       // block size
                    can_out_msg.data[3] = can_sep_time;        // min sep. time
                    can_rec_fc_count = can_blocksize;
                    can_rec_tel_valid = true;

                    // wait for free send buffer
                    for (;;)
                    {
                        uint16_t volatile temp_len;
                        di();
                        temp_len = send_len;
                        ei();
                        if ((sizeof(send_buffer) - temp_len) >= (can_rec_data_len + 5))
                        {
                            break;
                        }
                        do_idle();
                    }
                    can_send_message_wait();
                    can_rec_time = get_systick();
                    break;

                case 2:     // consecutive frame
                    if (can_rec_tel_valid &&
                        (can_rec_source_addr == (can_in_msg.sid & 0xFF)) &&
                        (can_rec_target_addr == can_in_msg.data[0]) &&
                        ((can_in_msg.data[1] & 0x0F) == (can_rec_block_count & 0x0F))
                    )
                    {
                        uint16_t copy_len = can_rec_data_len - can_rec_rec_len;
                        if (copy_len > 6)
                        {
                            copy_len = 6;
                        }
                        if (copy_len > (can_in_msg.dlc.bits.count - 2))
                        {   // invalid length
                            break;
                        }
                        memcpy(can_rec_buffer_offset + can_rec_rec_len, can_in_msg.data + 2, copy_len);
                        can_rec_rec_len += copy_len;
                        can_rec_block_count++;

                        if (can_rec_fc_count > 0 && (can_rec_rec_len < can_rec_data_len))
                        {
                            can_rec_fc_count--;
                            if (can_rec_fc_count == 0)
                            {
                                memset(&can_out_msg, 0x00, sizeof(can_out_msg));
                                can_out_msg.sid = 0x600 | can_rec_target_addr;
                                can_out_msg.dlc.bits.count = 8;
                                can_out_msg.data[0] = can_rec_source_addr;
                                can_out_msg.data[1] = 0x30;     // FC
                                can_out_msg.data[2] = can_blocksize;       // block size
                                can_out_msg.data[3] = can_sep_time;        // min sep. time
                                can_rec_fc_count = can_blocksize;

                                can_send_message_wait();
                            }
                        }
                        can_rec_time = get_systick();
                    }
                    break;
            }
        }

        if (can_rec_tel_valid && can_rec_rec_len >= can_rec_data_len)
        {
            uint16_t len;
            // create BMW-FAST telegram
            if (can_rec_data_len > 0x3F)
            {
                temp_buffer[0] = 0x80;
                temp_buffer[1] = can_rec_target_addr;
                temp_buffer[2] = can_rec_source_addr;
                temp_buffer[3] = can_rec_data_len;
                len = can_rec_data_len + 4;
            }
            else
            {
                temp_buffer[0] = 0x80 | can_rec_data_len;
                temp_buffer[1] = can_rec_target_addr;
                temp_buffer[2] = can_rec_source_addr;
                len = can_rec_data_len + 3;
            }

            temp_buffer[len] = calc_checkum(temp_buffer, len);
            len++;
            if (uart_send(temp_buffer, len))
            {
                can_rec_tel_valid = false;
            }
            else
            {   // send failed, keep message alive
                can_rec_time = get_systick();
            }
        }
    }
    else
    {
        if (can_rec_tel_valid)
        {   // check for timeout
            if ((uint16_t) (get_systick() - can_rec_time) > (CAN_TIMEOUT * TIMER0_RESOL / 1000))
            {
                can_rec_tel_valid = false;
            }
        }
    }
}

void main(void)
{
    start_indicator = true;
    init_failed = false;
    idle_counter = 0;
    rec_state = rec_state_idle;
    rec_len = 0;
    rec_bt_mode = false;
    send_set_idx = 0;
    send_get_idx = 0;
    send_len = 0;

    can_send_active = false;
    can_rec_tel_valid = false;

    RCONbits.IPEN = 1;      // interrupt priority enable

    // port configuration
    TRISAbits.TRISA0 = 1;   // AN0 input
    ANCON0 = 0x01;          // AN0 analog
    ANCON1 = 0x00;
    WPUB = 0x10;            // LED_RS_RX pullup
    INTCON2bits.RBPU = 0;   // port B pull up

    // K/L line
    KLINE_OUT = 0;  // idle
    LLINE_OUT = 0;  // idle
    TRISBbits.TRISB0 = 0;   // K line out
    TRISBbits.TRISB1 = 0;   // L line out
    TRISCbits.TRISC1 = 1;   // K line in

    // J1850
    LATAbits.LATA1 = 0;     // power control
    TRISAbits.TRISA1 = 0;   // out
    LATAbits.LATA2 = 0;
    TRISAbits.TRISA2 = 0;   // J1850+ out
    LATCbits.LATC3 = 0;
    TRISCbits.TRISC3 = 0;   // J1850- out

    // LED off
    LED_RS_RX = 0;          // not present
    LED_RS_TX = 0;          // not present
    LED_OBD_RX = 1;
    LED_OBD_TX = 1;
    // LED as output
    //TRISBbits.TRISB4 = 0; // used by bootloader
    TRISBbits.TRISB5 = 0;
    TRISBbits.TRISB6 = 0;
    TRISBbits.TRISB7 = 0;

    // CAN
    TRISBbits.TRISB3 = 1;   // CAN RX input
    //TRISBbits.TRISB2 = 0;   // CAN TX output (set automatically)
    IPR5 = 0x00;            // CAN interrupt low priority
    BIE0 = 0xFF;            // interrupt for all buffers

    TRISCbits.TRISC4 = 1;   // ignition state (input)

    // analog input
    ADCON0bits.CHS = 0;     // input AN0
    ADCON1bits.CHSN = 0;    // negative input is GND
    ADCON1bits.VNCFG = 0;   // VREF- is GND
    ADCON1bits.VCFG = 1;    // VREF+ 5V
    ADCON2bits.ADFM = 1;    // right justified
    ADCON2bits.ACQT = 0;    // Aquisition 0 Tad
    ADCON2bits.ADCS = 5;    // Fosc/16
    ADCON0bits.ADON = 1;    // enable ADC

    // UART
    TRISCbits.TRISC6 = 0;   // TX output
    TRISCbits.TRISC7 = 1;   // RX input
#if ADAPTER_TYPE == 0x02
    SPBRGH1 = 0;
    SPBRG1 = 103;           // 38400 @ 16MHz
#else
    SPBRGH1 = 0;
    SPBRG1 = 34;            // 115200 @ 16MHz
#endif
    TXSTAbits.TXEN = 1;     // Enable transmit
    TXSTAbits.BRGH = 1;     // Select high baud rate
    TXSTAbits.SYNC = 0;     // async mode
    BAUDCON1bits.BRG16 = 1; // 16 bit counter
    RCSTAbits.CREN = 1;     // Enable continuous reception

    IPR1bits.RCIP = 1;      // UART interrupt high priority
    PIR1bits.RCIF = 0;      // Clear RCIF Interrupt Flag
    PIE1bits.RCIE = 1;      // Set RCIE Interrupt Enable
    PIR1bits.TXIF = 0;      // Clear TXIF Interrupt Flag
    PIE1bits.TXIE = 0;      // Set TXIE Interrupt disable
    RCSTAbits.SPEN = 1;     // Enable Serial Port

    // timer 0
    T0CONbits.TMR0ON = 0;   // stop timer 0
    T0CONbits.T08BIT = 0;   // 16 bit mode
    T0CONbits.T0CS = 0;     // clock internal
    T0CONbits.T0PS = 7;     // prescaler 256 = 15625Hz
    T0CONbits.PSA = 0;      // prescaler enabled
    TMR0H = 0;
    TMR0L = 0;

    INTCON2bits.T0IP = 0;   // low priority
    INTCONbits.TMR0IF = 0;  // clear timer 0 interrupt flag
    //INTCONbits.TMR0IE = 1;  // enable timer 0 interrupt
    T0CONbits.TMR0ON = 1;   // enable timer 0

    // timer 1
    T1CONbits.TMR1ON = 0;   // stop timer
    T1CONbits.RD16 = 1;     // 16 bit access
    T1CONbits.TMR1CS = 0;   // internal clock 4MHz
    T1CONbits.SOSCEN = 0;   // oscillator disabled
    T1CONbits.T1CKPS = 3;   // prescaler 8 = 500kHz

    IPR1bits.TMR1IP = 1;    // timer 1 high prioriy
    PIR1bits.TMR1IF = 0;    // clear timer 1 interrupt flag
    PIE1bits.TMR1IE = 1;    // enable timer 1 interrupt

    // timer 2
    T2CONbits.T2CKPS = 0;   // prescaler 1
    T2CONbits.T2OUTPS = 0x0;// postscaler 1
    TMR2 = 0x00;            // timer 2 start value
    PR2 = 34;               // timer 2 stop value

    IPR1bits.TMR2IP = 0;    // timer 2 low prioriy
    PIR1bits.TMR2IF = 0;    // clear timer 2 interrupt flag
    //PIE1bits.TMR2IE = 1;    // enable timer 2 interrupt
    //T2CONbits.TMR2ON = 1;   // enable timer 2

    INTCONbits.GIEL = 1;    // enable low priority interrupts
    INTCONbits.GIEH = 1;    // enable high priority interrupts

    ADCON0bits.GODONE = 1;  // start first AD conversion

    OSCCONbits.IDLEN = 1;   // enable idle mode
    WDTCONbits.SWDTEN = 1;  // enable watchdog
    WDTCONbits.SRETEN = 1;  // ultra low power mode enabled
    WDTCONbits.REGSLP = 1;  // regulator low power mode
    CLRWDT();

    read_eeprom();
    can_config();
#if ADAPTER_TYPE != 0x02
    if (!init_bt())
    {   // error
        init_failed = true;
    }
#endif

    for (;;)
    {
        if (can_enabled)
        {
            bool new_can_msg = readCAN();
#if DEBUG_PIN
            if (new_can_msg) LED_RS_TX = 1;
#endif
            can_receiver(new_can_msg);
            can_sender(new_can_msg);
#if DEBUG_PIN
            LED_RS_TX = 0;
#endif
        }
        else
        {
            if (!PIE1bits.TXIE) // uart send active
            {
                uint16_t len = uart_receive(temp_buffer);
                if (len > 0)
                {
                    if ((kline_flags & KLINEF_NO_ECHO) == 0)
                    {
                        uart_send(temp_buffer, len);
                    }
                    if (!internal_telegram(len))
                    {
                        kline_send(temp_buffer, len);
                        kline_receive();
                    }
                }
            }
        }
        update_led();
        do_idle();
    }
}

// vector 0x0018
void interrupt low_priority low_isr (void)
{
    if (INTCONbits.TMR0IF)
    {
        INTCONbits.TMR0IF = 0;
        return;
    }
    if (PIR1bits.TMR2IF)
    {
        PIR1bits.TMR2IF = 0;
        return;
    }
    if (PIE5 != 0x00 && PIR5 != 0x00)
    {   // CAN interrupt
        PIE5 = 0x00;    // disable interrupt, only for wakeup
        PIR5 = 0x00;
    }
}

// vector 0x0008
void interrupt high_priority high_isr (void)
{
    if (PIE1bits.RCIE && PIR1bits.RCIF)
    {   // receive interrupt
        if (RCSTA & 0x06)
        {   // receive error -> reset flags
            RCSTAbits.CREN = 0;
            RCSTAbits.CREN = 1;
        }
        else
        {
            uint8_t rec_data = RCREG;
            // restart timeout timer
            TMR1H = TIMER1_RELOAD >> 8;
            TMR1L = TIMER1_RELOAD;
            PIR1bits.TMR1IF = 0;    // clear interrupt flag
            T1CONbits.TMR1ON = 1;   // start timeout timer

            if (rec_bt_mode)
            {
                switch (rec_state)
                {
                    case rec_state_idle:
                        rec_len = 0;
                        rec_buffer[rec_len++] = rec_data;
                        rec_chksum = 0;     // used as state
                        rec_state = rec_state_rec;
                        // no break here

                    case rec_state_rec:
                        if (rec_len < sizeof(rec_buffer))
                        {
                            rec_buffer[rec_len++] = rec_data;
                        }
                        switch (rec_chksum)
                        {
                            case 0:
                                if (rec_data == 'O')
                                {
                                    rec_chksum++;
                                }
                                break;

                            case 1:
                                if (rec_data == 'K')
                                {
#if defined(REQUIRES_BT_CRFL)
                                    rec_chksum++;
#else
                                    T1CONbits.TMR1ON = 0;   // stop timer
                                    PIR1bits.TMR1IF = 0;
                                    rec_state = rec_state_done;
#endif
                                    break;
                                }
                                rec_chksum = 0;
                                break;
#if defined(REQUIRES_BT_CRFL)
                            case 2:
                                if (rec_data == '\r')
                                {
                                    rec_chksum++;
                                    break;
                                }
                                rec_chksum = 0;
                                break;

                            case 3:
                                if (rec_data == '\n')
                                {
                                    T1CONbits.TMR1ON = 0;   // stop timer
                                    PIR1bits.TMR1IF = 0;
                                    rec_state = rec_state_done;
                                    break;
                                }
                                if (rec_data == '\r')
                                {   // can appear multiple times
                                    break;
                                }
                                rec_chksum = 0;
                                break;
#endif
                        }
                        break;
                }
                return;
            }
            switch (rec_state)
            {
                case rec_state_idle:
                    rec_len = 0;
                    rec_buffer[rec_len++] = rec_data;
                    rec_chksum = rec_data;
                    rec_state = rec_state_rec;
                    break;

                case rec_state_rec:
                    if (rec_len < sizeof(rec_buffer))
                    {
                        rec_buffer[rec_len++] = rec_data;
                    }
                    if (rec_len >= 4)
                    {   // header received
                        uint16_t tel_len;
                        if (rec_buffer[0] == 0x00)
                        {   // special mode:
                            // byte 1: telegram type
                            // byte 2+3: baud rate (high/low) / 2
                            // byte 4: flags
                            // byte 5: interbyte time
                            // byte 6+7: telegram length (high/low)
                            if (rec_buffer[1] != 0x00)
                            {   // invalid telegram type
                                rec_state = rec_state_error;
                                break;
                            }
                            if (rec_len >= 8)
                            {
                                tel_len = ((uint16_t) rec_buffer[6] << 8) + rec_buffer[7] + 9;
                            }
                            else
                            {
                                tel_len = 0;
                            }
                        }
                        else
                        {
                            // standard mode
                            tel_len = rec_buffer[0] & 0x3F;
                            if (tel_len == 0)
                            {
                                tel_len = rec_buffer[3] + 5;
                            }
                            else
                            {
                                tel_len += 4;
                            }
                        }
                        if (tel_len != 0 && rec_len >= tel_len)
                        {   // complete tel received
                            if (rec_chksum != rec_data)
                            {   // checksum error
                                rec_state = rec_state_error;
                                break;
                            }
                            T1CONbits.TMR1ON = 0;   // stop timer
                            PIR1bits.TMR1IF = 0;
                            rec_state = rec_state_done;
                            break;
                        }
                    }
                    rec_chksum += rec_data;
                    break;

                default:
                    break;
            }
        }
        return;
    }
    if (PIE1bits.TXIE && PIR1bits.TXIF)
    {
        if (send_len == 0)
        {
            PIE1bits.TXIE = 0;    // disable TX interrupt
        }
        else
        {
            TXREG = send_buffer[send_get_idx++];
            send_len--;
            if (send_get_idx >= sizeof(send_buffer))
            {
                send_get_idx = 0;
            }
        }
        return;
    }
    if (PIE1bits.TMR1IE && PIR1bits.TMR1IF)
    {   // timeout timer
        T1CONbits.TMR1ON = 0;   // stop timer
        PIR1bits.TMR1IF = 0;
        switch (rec_state)
        {
            case rec_state_rec:
            case rec_state_error:
                // receive timeout
                rec_state = rec_state_idle;
                break;

            default:
                break;
        }
        return;
    }
}
