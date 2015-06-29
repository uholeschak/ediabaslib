/*
 * CanConverter.c
 *
 * Created: 25.06.2015 13:09:26
 *  Author: Ulrich
 */ 

#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <util/atomic.h>
#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include <stdio.h>
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

#define LED_GREEN           0
#define LED_RED             1

#define CAN_RES             4       // CAN reset (low active)
#define CAN_BLOCK_SIZE      0x0F    // 0 is disabled
#define CAN_MIN_SEP_TIME    2       // min separation time (ms)
#define CAN_TIMEOUT         100     // can receive timeout (10ms)

// receiver state machine
typedef enum
{
    rec_state_idle,     // wait
    rec_state_rec,      // receive
    rec_state_done,     // receive complete, ok
    rec_state_error,    // receive error
} rec_states;

volatile uint8_t time_tick;
volatile uint8_t time_tick_10;  // time tick 10 ms
volatile rec_states rec_state;
volatile uint16_t rec_len;
volatile uint8_t rec_timeout;
uint8_t rec_chksum;
volatile uint8_t rec_buffer[260];

uint16_t send_set_idx;
uint16_t send_get_idx;
volatile uint16_t send_len;
volatile uint8_t send_buffer[260];

uint8_t temp_buffer[260];

can_t msg_send;
can_t msg_rec;
// can receiver variables
uint8_t *can_rec_buffer_offset;
uint8_t can_rec_source_addr;
uint8_t can_rec_target_addr;
uint8_t can_rec_block_count;
uint8_t can_rec_fc_count;
uint8_t can_rec_time;
uint16_t can_rec_rec_len;
uint16_t can_rec_data_len;
bool can_rec_tel_valid;

const uint8_t can_filter[] PROGMEM =
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
    MCP2515_FILTER(0),      // Mask 1 (for group 1)
};

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

void update_led()
{
    if (time_tick_10 & 0x20)
    {
        PORTE |= (1<<LED_RED);
    }
    else
    {
        PORTE &= ~(1<<LED_RED);
    }
}

void can_sender(bool new_can_msg)
{
    uint16_t len = uart_receive(temp_buffer);
    if (len > 0)
    {
        uint8_t *data_offset = &temp_buffer[3];
        uint8_t data_len = temp_buffer[0] & 0x3F;
        if (data_len == 0)
        {
            data_len = temp_buffer[3];
            data_offset = &temp_buffer[4];
        }
        if (data_len <= 6)
        {
            memset(&msg_send, 0x00, sizeof(msg_send));
            msg_send.id = 0x600 | temp_buffer[2];    // source address
            msg_send.flags.rtr = 0;
            msg_send.length = 8;
            msg_send.data[0] = temp_buffer[1];       // target address
            msg_send.data[1] = 0x00 | data_len;      // single frame + length
            memcpy(msg_send.data + 2, data_offset, data_len);

            can_send_message(&msg_send);
        }
    }
}

void can_receiver(bool new_can_msg)
{
    if (new_can_msg)
    {
        if (((msg_rec.id & 0xFF00) == 0x0600) && (msg_rec.length == 8))
        {
            uint8_t frame_type = (msg_rec.data[1] >> 4) & 0x0F;
            switch (frame_type)
            {
                case 0:     // single frame
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
                    msg_send.data[2] = CAN_BLOCK_SIZE;       // block size
                    msg_send.data[3] = CAN_MIN_SEP_TIME;     // min sep. time
                    can_rec_fc_count = CAN_BLOCK_SIZE;

                    can_send_message(&msg_send);
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

                        if (can_rec_fc_count > 0)
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
                                msg_send.data[2] = CAN_BLOCK_SIZE;       // block size
                                msg_send.data[3] = CAN_MIN_SEP_TIME;     // min sep. time
                                can_rec_fc_count = CAN_BLOCK_SIZE;

                                can_send_message(&msg_send);
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

            uint8_t sum = 0;
            for (uint16_t i = 0; i < len; i++)
            {
                sum += temp_buffer[i];
            }
            temp_buffer[len++] = sum;
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
    time_tick = 0;
    time_tick_10 = 0;
    rec_state = rec_state_idle;
    rec_len = 0;
    rec_timeout = 0;
    send_set_idx = 0;
    send_get_idx = 0;
    send_len = 0;

    can_rec_tel_valid = false;

    // config ports
    DDRA = 0x7F;
    PORTA = 0x00;   // enable K-line

    DDRD = (1<<CAN_RES);
    PORTD &= ~(1<<CAN_RES);

    DDRE = (1<<LED_RED) | (1<<LED_GREEN);
    PORTE = (1<<LED_GREEN);

    // config timer 0
    TCCR0 = (1<<WGM01); // CTC Modus
    TCCR0 |= (1<<CS01) | (1<<CS00); // prescaler 64
    // ((7380000/64)/1000) = 115 (1ms)
    OCR0 = 115-1;

    // Allow compare interrupt
    TIMSK |= (1<<OCIE0);

    UBRRL = BAUDRATEFACTOR;                    // Set baud rate and enable UART
    UCSRB = (1<<TXEN) | (1<<RXEN);
    UCSRB |= (1<<RXCIE);
    UCSRC = (1<<URSEL) | (1<<UCSZ1) | (1<<UCSZ0);    // 8N1 Async

    sei();

    PORTD |= (1<<CAN_RES);
    DDRB = (1<<PB4);    // set SS as output, otherwise the SPI switches back to slave mode!
    if (!can_init(BITRATE_500_KBPS))
    {
        PORTE |= (1<<LED_RED);
        PORTE &= ~(1<<LED_GREEN);
        for (;;)
        {
        }
    }
    can_static_filter(can_filter);

    for (;;)
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
        update_led();
    }
}

ISR(TIMER0_COMP_vect)
{
    time_tick++;
    if (time_tick >= 10)
    {
        time_tick = 0;
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
    rec_timeout = 2;

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
