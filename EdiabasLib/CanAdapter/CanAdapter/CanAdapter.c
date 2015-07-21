/*
 * CanConverter.c
 *
 * Created: 25.06.2015 13:09:26
 *  Author: Ulrich
 */ 

#define F_CPU 7372800

#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/sleep.h>
#include <avr/pgmspace.h>
#include <avr/eeprom.h>
#include <avr/wdt.h>
#include <avr/fuse.h>
#include <util/atomic.h>
#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include "can.h"
#include "spi.h"

/*** UART registers ***/
#define UBRRL           UBRR0L           // UART Baud Rate Register Low
#define UCSRA           UCSR0A           // UART Status Register
#define UCSRB           UCSR0B           // UART Control Register
#define UDR             UDR0             // UART Data Register
#define UCSRC           UCSR0C           // UART Control Register C
#define RXCIE           RXCIE0           // UART Receive interrupt enable
#define TXCIE           TXCIE0           // UART Transmit interrupt enable

#define TXEN            TXEN0            // Transmitter Enable Bit
#define RXEN            RXEN0            // Receiver Enable Bit
#define TXC             TXC0             // Transmit Complete Bit
#define RXC             RXC0             // Receive Complete Bit
#define UDRE            UDRE0            // UART Data Register Empty Bit
#define URSEL           URSEL0
#define UCSZ1           UCSZ01
#define UCSZ0           UCSZ00

#define USART_RXC_vect  USART0_RXC_vect
#define USART_TXC_vect  USART0_TXC_vect

#define BAUDRATEFACTOR      3            // Calculated baud rate factor: 115200bps @ 7.38 MHz
#define TIMER0_PRESCALE     ((1<<CS01) | (1<<CS00))     // prescaler 64
#define TIMER0_RELOAD       (115-1)      // 1ms: ((7380000/64)/1000) = 115

#define IGNITION            PB4
#define LED_GREEN           PE0
#define LED_RED             PE1
#define DSR_OUT             PE2
#define IGNITION_STATE()    ((PINB & (1<<IGNITION)) != 0)
#define LED_GREEN_ON()      { PORTE |= (1<<LED_GREEN); }
#define LED_GREEN_OFF()     { PORTE &= ~(1<<LED_GREEN); }
#define LED_RED_ON()        { PORTE |= (1<<LED_RED); }
#define LED_RED_OFF()       { PORTE &= ~(1<<LED_RED); }
#define DSR_ON()            { PORTE &= ~(1<<DSR_OUT); }     // inverted by FTDI
#define DSR_OFF()           { PORTE |= (1<<DSR_OUT); }      // inverted by FTDI

#define CAN_RES             4       // CAN reset (low active)
#define CAN_MODE            1       // default can mode (1=500kb)
#define CAN_BLOCK_SIZE      0x0F    // 0 is disabled
#define CAN_MIN_SEP_TIME    1       // min separation time (ms)
#define CAN_TIMEOUT         100     // can receive timeout (10ms)
#define SER_REC_TIMEOUT     3       // serial receive timeout (ms) (+1 required for disabled timer in sleep mode)
//#define SER_REC_TIMEOUT     20      // serial receive timeout for bluetooth converter (ms)

#define EEP_ADDR_BAUD       0x00    // eeprom address for baud setting (2 bytes, address is in word steps!)
#define EEP_ADDR_BLOCKSIZE  0x01    // eeprom address for FC block size (2 bytes, address is in word steps!)
#define EEP_ADDR_SEP_TIME   0x02    // eeprom address for FC separation time (2 bytes, address is in word steps!)

FUSES =
{
    .low = (FUSE_CKSEL1),   // 0xFD
    .high = (FUSE_BOOTSZ0 & FUSE_BOOTRST & FUSE_SPIEN), // 0xDC
    .extended = (FUSE_BODLEVEL0 & FUSE_BODLEVEL1),  // 0xF9
};

// receiver state machine
typedef enum
{
    rec_state_idle,     // wait
    rec_state_rec,      // receive
    rec_state_done,     // receive complete, ok
    rec_state_error,    // receive error
} rec_states;

// wait types
typedef enum
{
    wait_off,           // no wait
    wait_1ms,           // wait in 1ms units
    wait_10ms,          // wait in 10ms units
} wait_types;

static volatile uint8_t time_prescaler;
static volatile uint8_t time_tick_1;  // time tick 1 ms
static volatile uint8_t time_tick_10;  // time tick 10 ms
static volatile bool start_indicator;  // show start indicator

static volatile rec_states rec_state;
static volatile uint16_t rec_len;
static volatile uint8_t rec_timeout;
static uint8_t rec_chksum;
static volatile uint8_t rec_buffer[260];

static uint16_t send_set_idx;
static uint16_t send_get_idx;
static volatile uint16_t send_len;
static volatile uint8_t send_buffer[260];

static uint8_t temp_buffer[260];

static bool can_enabled;
static uint8_t can_mode;
static uint8_t can_blocksize;
static uint8_t can_sep_time;
static can_t msg_send;
static can_t msg_rec;

// can sender variables
static bool can_send_active;
static bool can_send_wait_for_fc;
static wait_types can_send_wait_sep_time;
static uint16_t can_send_pos;
static uint8_t can_send_block_count;
static uint8_t can_send_block_size;
static uint8_t can_send_sep_time;
static uint8_t can_send_sep_time_start;
static uint8_t can_send_time;

// can receiver variables
static uint8_t *can_rec_buffer_offset;
static uint8_t can_rec_source_addr;
static uint8_t can_rec_target_addr;
static uint8_t can_rec_block_count;
static uint8_t can_rec_fc_count;
static uint8_t can_rec_time;
static uint16_t can_rec_rec_len;
static uint16_t can_rec_data_len;
static bool can_rec_tel_valid;

static const uint8_t can_filter[] PROGMEM =
{
    // Group 0
    MCP2515_FILTER(0),      // Filter 0
    MCP2515_FILTER(0),      // Filter 1

    // Group 1
    MCP2515_FILTER(0),      // Filter 2
    MCP2515_FILTER(0),      // Filter 3
    MCP2515_FILTER(0),      // Filter 4
    MCP2515_FILTER(0),      // Filter 5

    MCP2515_FILTER(0),      // Mask 0 (for group 0)
    MCP2515_FILTER(0x07FF), // Mask 1 (for group 1), disabled used for overflow
};

void do_idle()
{
    if (can_send_active &&
        (can_send_wait_sep_time == wait_off) &&
        !can_send_wait_for_fc)
    {   // can sending active, don't sleep
        return;
    }
    bool disable_timer = !start_indicator &&
        !can_send_active &&
        !can_rec_tel_valid &&
        (rec_state == rec_state_idle) &&
        (send_len == 0);

    sleep_enable();
    GICR |= (1<<PCIE1);     // CAN pin change interrupt on
    if (disable_timer)
    {
        //TIMSK &= ~(1<<OCIE0);   // disable timer interrupt
        TCCR0 = (1<<WGM01) | (1<<CS02) | (1<<CS00); // prescaler 1024
    }
    if (!can_check_message())
    {
        sleep_cpu();
    }
    if (disable_timer)
    {
        //TIMSK |= (1<<OCIE0);    // enable timer interrupt
        TCCR0 = (1<<WGM01) | TIMER0_PRESCALE; // standard time
    }
    GICR &= ~(1<<PCIE1);    // CAN pin change interrupt off
    sleep_disable();
}

bool uart_send(uint8_t *buffer, uint16_t count)
{
    if (count == 0)
    {
        return true;
    }
    if (send_len + count > sizeof(send_buffer))
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

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        bool start_send = send_len == 0;
        send_len += count;
        if (start_send)
        {
            UDR = send_buffer[send_get_idx++];
            send_len--;
            if (send_get_idx >= sizeof(send_buffer))
            {
                send_get_idx = 0;
            }
            UCSRB |= (1<<TXCIE);
        }
    }
    return true;
}

uint16_t uart_receive(uint8_t *buffer)
{
    if (rec_state != rec_state_done)
    {
        return 0;
    }

    uint16_t data_len = rec_len;
    memcpy(buffer, (void *) rec_buffer, data_len);
    rec_state = rec_state_idle;
    return data_len;
}

void soft_reset()
{
    wdt_enable(WDTO_15MS);
    for (;;)
    {
        sleep_mode();
    }
}

void update_led()
{
#if 0
    start_indicator = false;
    if (time_tick_10 & 0x20)
    {
        LED_RED_ON();
        LED_GREEN_OFF();
    }
    else
    {
        LED_RED_OFF();
        LED_GREEN_OFF();
    }
#else
    if (start_indicator)
    {
        if (time_tick_10 > 50)
        {
            start_indicator = false;
        }
    }
    if (start_indicator)
    {
        LED_RED_OFF();
        LED_GREEN_ON();
    }
    else
    {
        if (rec_state != rec_state_idle)
        {
            LED_RED_ON();
        }
        else
        {
            LED_RED_OFF();
        }
        if (send_len > 0)
        {
            LED_GREEN_ON();
        }
        else
        {
            LED_GREEN_OFF();
        }
    }
#endif
    if (IGNITION_STATE())
    {
        DSR_ON();
    }
    else
    {
        DSR_OFF();
    }
}

uint8_t calc_checkum(uint16_t len)
{
    uint8_t sum = 0;
    for (uint16_t i = 0; i < len; i++)
    {
        sum += temp_buffer[i];
    }
    return sum;
}

void read_eeprom()
{
    uint16_t temp_value;

    temp_value = eeprom_read_word((uint16_t *) EEP_ADDR_BAUD);
    can_mode = CAN_MODE;
    if (((~temp_value >> 8) & 0xFF) == (temp_value & 0xFF))
    {
        can_mode = temp_value;
    }

    temp_value = eeprom_read_word((uint16_t *) EEP_ADDR_BLOCKSIZE);
    can_blocksize = CAN_BLOCK_SIZE;
    if (((~temp_value >> 8) & 0xFF) == (temp_value & 0xFF))
    {
        can_blocksize = temp_value;
    }

    temp_value = eeprom_read_word((uint16_t *) EEP_ADDR_SEP_TIME);
    can_sep_time = CAN_MIN_SEP_TIME;
    if (((~temp_value >> 8) & 0xFF) == (temp_value & 0xFF))
    {
        can_sep_time = temp_value;
    }
}

void can_config()
{
    can_bitrate_t bitrate = BITRATE_500_KBPS;
    switch (can_mode)
    {
        case 0:     // can off
            can_enabled = false;
            break;

        default:
        case 1:     // can 500kb
            bitrate = BITRATE_500_KBPS;
            can_enabled = true;
            break;

        case 9:     // can 125kb
            bitrate = BITRATE_125_KBPS;
            can_enabled = true;
            break;
    }

    can_send_active = false;
    can_rec_tel_valid = false;

    if (can_enabled)
    {
        PORTD |= (1<<CAN_RES);  // end can reset
        if (!can_init(bitrate))
        {
            LED_GREEN_OFF();
            LED_RED_ON();
            cli();
            for (;;)
            {
                sleep_mode();
            }
        }
        can_static_filter(can_filter);
    }
    else
    {
        PORTD &= ~(1<<CAN_RES);     // CAN reset
    }
}

bool can_send_message_wait(const can_t *msg)
{
    uint8_t start_tick = time_tick_10;
    while (can_send_message(msg) == 0)
    {
        update_led();
        if ((uint8_t) (time_tick_10 - start_tick) > 25)
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
        eeprom_update_word(EEP_ADDR_BAUD, cfg_value | (((uint16_t) ~cfg_value << 8) & 0xFF00));
        read_eeprom();
        can_config();
        temp_buffer[3] = ~can_mode;
        temp_buffer[len - 1] = calc_checkum(len - 1);
        uart_send(temp_buffer, len);
        return true;
    }

    if ((len == 6) &&
    (temp_buffer[0] == 0x82) &&
    (temp_buffer[1] == 0x00) &&
    (temp_buffer[2] == 0x00))
    {
        if ((temp_buffer[3] & 0x7F) == 0x00)
        {      // block size
            if ((temp_buffer[3] & 0x80) == 0x00)
            {   // write
                uint8_t cfg_value = temp_buffer[4];
                eeprom_update_word((uint16_t *) EEP_ADDR_BLOCKSIZE, cfg_value | (((uint16_t) ~cfg_value << 8) & 0xFF00));
                read_eeprom();
            }
            temp_buffer[4] = can_blocksize;
            temp_buffer[len - 1] = calc_checkum(len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((temp_buffer[3] & 0x7F) == 0x01)
        {      // separation time
            if ((temp_buffer[3] & 0x80) == 0x00)
            {   // write
                uint8_t cfg_value = temp_buffer[4];
                eeprom_update_word((uint16_t *) EEP_ADDR_SEP_TIME, cfg_value | (((uint16_t) ~cfg_value << 8) & 0xFF00));
                read_eeprom();
            }
            temp_buffer[4] = can_sep_time;
            temp_buffer[len - 1] = calc_checkum(len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((temp_buffer[3] & 0x7F) == 0x02)
        {      // can mode
            if ((temp_buffer[3] & 0x80) == 0x00)
            {   // write
                uint8_t cfg_value = temp_buffer[4];
                eeprom_update_word((uint16_t *) EEP_ADDR_BAUD, cfg_value | (((uint16_t) ~cfg_value << 8) & 0xFF00));
                read_eeprom();
                can_config();
            }
            temp_buffer[4] = can_mode;
            temp_buffer[len - 1] = calc_checkum(len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((temp_buffer[3] == 0xFE) && (temp_buffer[4] == 0xFE))
        {      // read ignition state
            temp_buffer[4] = IGNITION_STATE() ? 0xFF : 0x00;
            temp_buffer[len - 1] = calc_checkum(len - 1);
            uart_send(temp_buffer, len);
            return true;
        }
        if ((temp_buffer[3] == 0xFF) && (temp_buffer[4] == 0xFF))
        {      // reset command
            soft_reset();
            return true;
        }
    }
    return false;
}

void can_sender(bool new_can_msg)
{
    if (!can_enabled)
    {
        return;
    }
    if (!can_send_active)
    {
        uint16_t len = uart_receive(temp_buffer);
        if (len > 0)
        {
            if (internal_telegram(len))
            {
                return;
            }
            can_send_active = true;
            can_send_wait_sep_time = wait_off;
            can_send_pos = 0;
        }
    }
    if (can_send_active)
    {
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
                memset(&msg_send, 0x00, sizeof(msg_send));
                msg_send.id = 0x600 | source_address;    // source address
                msg_send.flags.rtr = 0;
                msg_send.length = 8;
                msg_send.data[0] = target_address;
                msg_send.data[1] = 0x00 | data_len;      // single frame + length
                memcpy(msg_send.data + 2, data_offset, data_len);

                can_send_message_wait(&msg_send);
                can_send_active = false;
                return;
            }
            // first frame
            memset(&msg_send, 0x00, sizeof(msg_send));
            msg_send.id = 0x600 | source_address;    // source address
            msg_send.flags.rtr = 0;
            msg_send.length = 8;
            msg_send.data[0] = target_address;
            msg_send.data[1] = 0x10 | ((data_len >> 8) & 0xFF);      // first frame + length
            msg_send.data[2] = data_len;
            uint8_t len = 5;
            memcpy(msg_send.data + 3, data_offset, len);
            can_send_pos += len;

            can_send_message_wait(&msg_send);
            can_send_wait_for_fc = true;
            can_send_block_count = 1;
            can_send_time = time_tick_10;
            return;
        }
        if (can_send_wait_for_fc)
        {
            if (new_can_msg)
            {
                if (((msg_rec.id & 0xFF00) == 0x0600) &&
                    (msg_rec.length >= 4) &&
                    ((msg_rec.id & 0x00FF) == target_address) &&
                    (msg_rec.data[0] == source_address) &&
                    ((msg_rec.data[1] & 0xF0) == 0x30)  // FC
                    )
                {
                    switch (msg_rec.data[1] & 0x0F)
                    {
                        case 0: // CTS
                            can_send_wait_for_fc = false;
                            break;

                        case 1: // Wait
                            can_send_time = time_tick_10;
                            break;

                        default:    // invalid
                            break;
                    }
                    can_send_block_size = msg_rec.data[2];
                    can_send_sep_time = msg_rec.data[3];
                }
            }
            if (can_send_wait_for_fc)
            {
                if ((uint8_t) (time_tick_10 - can_send_time) > CAN_TIMEOUT)
                {   // FC timeout
                    can_send_active = false;
                    return;
                }
            }
            return;
        }
        if (can_send_wait_sep_time != wait_off)
        {
            if (can_send_wait_sep_time == wait_1ms)
            {
                if ((uint8_t) (time_tick_1 - can_send_sep_time_start) <= can_send_sep_time)
                {
                    return;
                }
            }
            else
            {
                if ((uint8_t) (time_tick_10 - can_send_sep_time_start) <= ((can_send_sep_time + 9) / 10))
                {
                    return;
                }
            }
            can_send_wait_sep_time = wait_off;
        }
        // consecutive frame
        memset(&msg_send, 0x00, sizeof(msg_send));
        msg_send.id = 0x600 | source_address;    // source address
        msg_send.flags.rtr = 0;
        msg_send.length = 8;
        msg_send.data[0] = target_address;
        msg_send.data[1] = 0x20 | (can_send_block_count & 0x0F);      // consecutive frame + block count
        uint8_t len = data_len - can_send_pos;
        if (len > 6)
        {
            len = 6;
        }
        memcpy(msg_send.data + 2, data_offset + can_send_pos, len);
        can_send_pos += len;
        can_send_block_count++;

        can_send_message_wait(&msg_send);

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
                can_send_time = time_tick_10;
            }
            can_send_block_size--;
        }
        if (!can_send_wait_for_fc && can_send_sep_time > 0)
        {
            if (can_send_sep_time < 100)
            {
                can_send_wait_sep_time = wait_1ms;
                can_send_sep_time_start = time_tick_1;
            }
            else
            {
                can_send_wait_sep_time = wait_10ms;
                can_send_sep_time_start = time_tick_10;
            }
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
        if (((msg_rec.id & 0xFF00) == 0x0600) && (msg_rec.length == 8))
        {
            uint8_t frame_type = (msg_rec.data[1] >> 4) & 0x0F;
            switch (frame_type)
            {
                case 0:     // single frame
                    if (can_rec_tel_valid)
                    {   // ignore new first frames during reception
                        break;
                    }
                    can_rec_source_addr = msg_rec.id & 0xFF;
                    can_rec_target_addr = msg_rec.data[0];
                    can_rec_data_len = msg_rec.data[1] & 0x0F;
                    if (can_rec_data_len > 6)
                    {   // invalid length
                        can_rec_tel_valid = false;
                        break;
                    }
                    memcpy(temp_buffer + 3, msg_rec.data + 2, can_rec_data_len);
                    can_rec_tel_valid = true;
                    can_rec_rec_len = can_rec_data_len;
                    can_rec_time = time_tick_10;
                    break;

                case 1:     // first frame
                    if (can_rec_tel_valid)
                    {   // ignore new first frames during reception
                        break;
                    }
                    can_rec_source_addr = msg_rec.id & 0xFF;
                    can_rec_target_addr = msg_rec.data[0];
                    can_rec_data_len = (((uint16_t) msg_rec.data[1] & 0x0F) << 8) + msg_rec.data[2];
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
                    memcpy(can_rec_buffer_offset, msg_rec.data + 3, 5);
                    can_rec_rec_len = 5;
                    can_rec_block_count = 1;

                    memset(&msg_send, 0x00, sizeof(msg_send));
                    msg_send.id = 0x600 | can_rec_target_addr;
                    msg_send.flags.rtr = 0;
                    msg_send.length = 8;
                    msg_send.data[0] = can_rec_source_addr;
                    msg_send.data[1] = 0x30;     // FC
                    msg_send.data[2] = can_blocksize;       // block size
                    msg_send.data[3] = can_sep_time;        // min sep. time
                    can_rec_fc_count = can_blocksize;

                    can_send_message_wait(&msg_send);
                    can_rec_tel_valid = true;
                    can_rec_time = time_tick_10;
                    break;

                case 2:
                    if (can_rec_tel_valid &&
                        (can_rec_source_addr == (msg_rec.id & 0xFF)) &&
                        (can_rec_target_addr == msg_rec.data[0]) &&
                        ((msg_rec.data[1] & 0x0F) == (can_rec_block_count & 0x0F))
                    )
                    {
                        uint16_t copy_len = can_rec_data_len - can_rec_rec_len;
                        if (copy_len > 6)
                        {
                            copy_len = 6;
                        }
                        memcpy(can_rec_buffer_offset + can_rec_rec_len, msg_rec.data + 2, copy_len);
                        can_rec_rec_len += copy_len;
                        can_rec_block_count++;

                        if (can_rec_fc_count > 0 && (can_rec_rec_len < can_rec_data_len))
                        {
                            can_rec_fc_count--;
                            if (can_rec_fc_count == 0)
                            {
                                memset(&msg_send, 0x00, sizeof(msg_send));
                                msg_send.id = 0x600 | can_rec_target_addr;
                                msg_send.flags.rtr = 0;
                                msg_send.length = 8;
                                msg_send.data[0] = can_rec_source_addr;
                                msg_send.data[1] = 0x30;     // FC
                                msg_send.data[2] = can_blocksize;       // block size
                                msg_send.data[3] = can_sep_time;        // min sep. time
                                can_rec_fc_count = can_blocksize;

                                can_send_message_wait(&msg_send);
                            }
                        }
                        can_rec_time = time_tick_10;
                    }
                    break;
            }
        }
        else
        {
            if (can_rec_tel_valid)
            {   // check for timeout
                if ((uint8_t) (time_tick_10 - can_rec_time) > CAN_TIMEOUT)
                {
                    can_rec_tel_valid = false;
                }
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

            temp_buffer[len] = calc_checkum(len);
            len++;
            if (uart_send(temp_buffer, len))
            {
                can_rec_tel_valid = false;
            }
            else
            {   // send failed, keep message alive
                can_rec_time = time_tick_10;
            }
        }
    }
}

int main(void)
{
    time_prescaler = 0;
    time_tick_1 = 0;
    time_tick_10 = 0;
    start_indicator = true;
    rec_state = rec_state_idle;
    rec_len = 0;
    rec_timeout = 0;
    send_set_idx = 0;
    send_get_idx = 0;
    send_len = 0;

    can_send_active = false;
    can_rec_tel_valid = false;

    // disable AD converter (reduces power consumption)
    ACSR |= (1<<ACD);

    // pin change interrupt for CAN
    PCMSK1 |= (1<<PCINT10); // can interrupt

    // config ports
    DDRA = 0x7F;
    PORTA = 0x00;   // enable K-line
    // bit 0: 0=ODBRX->FTDIRX, ODBRX->RDX1
    // bit 1: not used
    // bit 2: 0=FTDITX->TXOUT(PIN16)
    // bit 3: 0=FTDITX->OBDTX
    // bit 4: 0=FTDITX->OBDTX, ODBRX->FTDIRX
    // bit 5: 0=TXD1->TXOUT(PIN16)
    // bit 6: 0=TXD1->OBDTX

    DDRD = (1<<CAN_RES);
    PORTD &= ~(1<<CAN_RES);

    // LED + DSR
    DDRE = (1<<LED_RED) | (1<<LED_GREEN) | (1<<DSR_OUT);
    LED_RED_OFF();
    LED_GREEN_ON();
    DSR_OFF();

    // config timer 0
    TCCR0 = (1<<WGM01) | TIMER0_PRESCALE; // CTC Modus
    OCR0 = TIMER0_RELOAD;

    // Allow compare interrupt
    TIMSK |= (1<<OCIE0);

    UBRRL = BAUDRATEFACTOR;                    // Set baud rate and enable UART
    UCSRB = (1<<TXEN) | (1<<RXEN);
    UCSRB |= (1<<RXCIE);
    UCSRC = (1<<URSEL) | (1<<UCSZ1) | (1<<UCSZ0);    // 8N1 Async

    // port for direct OBD communication, not used at the moment
    UBRR1L = BAUDRATEFACTOR;
    UCSR1B = (1<<TXEN1);
    UCSR1C = (1<<URSEL1) | (1<<UCSZ11) | (1<<UCSZ10);    // 8N1 Async

    set_sleep_mode(SLEEP_MODE_IDLE);
    sei();

    read_eeprom();
    can_config();

    for (;;)
    {
        if (can_enabled)
        {
            bool new_can_msg = false;
            if (can_check_message())
            {
                if (can_get_message(&msg_rec))
                {
                    new_can_msg = true;
                }
            }

            can_receiver(new_can_msg);
            can_sender(new_can_msg);
        }
        else
        {
            uint16_t len = uart_receive(temp_buffer);
            if (len > 0)
            {
                internal_telegram(len);
            }
        }

        update_led();
        do_idle();
    }
}

ISR(TIMER0_COMP_vect)
{
    time_tick_1++;
    time_prescaler++;
    if (time_prescaler >= 10)
    {
        time_prescaler = 0;
        time_tick_10++;
    }
    if (rec_timeout > 0)
    {
        rec_timeout--;
        switch (rec_state)
        {
            case rec_state_rec:
            case rec_state_error:
                if (rec_timeout == 0)
                {   // receive timeout
                    rec_state = rec_state_idle;
                }
                break;

            default:
                break;
        }
    }
}

ISR(USART_RXC_vect)
{
    uint8_t rec_data = UDR;
    rec_timeout = SER_REC_TIMEOUT;

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
                uint16_t tel_len = rec_buffer[0] & 0x3F;
                if (tel_len == 0)
                {
                    tel_len = rec_buffer[3] + 5;
                }
                else
                {
                    tel_len += 4;
                }
                if (rec_len >= tel_len)
                {   // complete tel received
                    if (rec_chksum != rec_data)
                    {   // checksum error
                        rec_state = rec_state_error;
                        break;
                    }
                    rec_timeout = 0;
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

ISR(USART_TXC_vect)
{
    if (send_len == 0)
    {
        UCSRB &= ~(1<<TXCIE);
    }
    else
    {
        UDR = send_buffer[send_get_idx++];
        send_len--;
        if (send_get_idx >= sizeof(send_buffer))
        {
            send_get_idx = 0;
        }
    }
}

ISR(PCINT1_vect)
{
    sleep_disable();
}
