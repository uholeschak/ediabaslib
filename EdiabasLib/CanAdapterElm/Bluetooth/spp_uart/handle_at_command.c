/****************************************************************************
Copyright (C) Cambridge Silicon Radio Limited 2006-2009
Part of BlueLab 4.1-Release

DESCRIPTION
	AT Command handlers
	
FILE
	handle_at_command.c
*/


/****************************************************************************
    Header files
*/

#include <print.h>
#include <panic.h>
#include <ctype.h>
#include <bdaddr.h>
#include <stdlib.h>
#include <stream.h>
#include <sink.h>
#include <source.h>
#include <string.h>
#include <stdbool.h>
#include <ps.h>
#include <boot.h>

#include "spp_dev_private.h"
#include "spp_uart_parse.h"
#include "generate_at_resp.h"

typedef struct
{
    uint32        baud_rate;
    vm_uart_rate  baud_code;
} uartInfo;

const uartInfo uartInfoTable[] =
{
    { 9600, VM_UART_RATE_9K6 },
    { 19200, VM_UART_RATE_19K2 },
    { 38400, VM_UART_RATE_38K4 },
    { 57600, VM_UART_RATE_57K6 },
    { 115200, VM_UART_RATE_115K2 },
    { 230400, VM_UART_RATE_230K4 },
    { 460800, VM_UART_RATE_460K8 },
    { 921600, VM_UART_RATE_921K6 },
    { 1382400, VM_UART_RATE_1382K4 },
};

static uint32 handSeqExtractUint32(const uint8 *pBuffer, uint16 pLen)
{
	uint32 lRes = 0;
	uint16 lC;
	uint16 lVal = 0;
	uint8 lCh;
	
	for (lC=0; lC<pLen; lC++)
	{
		lCh = toupper(pBuffer[lC]);
		if ((lCh>= '0') && (lCh <= '9'))
			lVal = lCh - '0';
		else /* Invalid character */
			return 0;
		lRes = (lRes * 10) + lVal;
	}
	
	return lRes;
}

void spp_handleUnrecognised(const uint8 *data, uint16 length, Task task)
{
 	Sink lUart = StreamUartSink();
	uint16 lUsed = 0;
    PRINT(("spp_handleUnrecognised\n"));
	/* Send result to host */
	lUsed = addATStr(lUart, pbapATRespId_Fail);
	addATCrLfandSend(lUart, lUsed);
}

void handleATEmpty(Task pTask)
{
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;

	/* Send result to host */
	lUsed += addATStr(lUart, pbapATRespId_Ok);
	addATCrLfandSend(lUart, lUsed);
}

void handleATGetPin(Task pTask)
{
    sppTaskData* app = (sppTaskData*) pTask;
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;

	/* Send result to host */
	lUsed = addATStr(lUart, pbapATRespId_Pin);
    lUsed += addATBuffer8(lUart, app->pin, app->pin_length);
	lUsed += addATStr(lUart, pbapATRespId_CrLf);
	lUsed += addATStr(lUart, pbapATRespId_Ok);
	addATCrLfandSend(lUart, lUsed);
}

void handleATSetPin(Task pTask, const struct ATSetPin *pPinReq)
{
    sppTaskData* app = (sppTaskData*) pTask;
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;
    bool valid = true;
    uint16 i;

    if ((pPinReq->pin.length < 4) || (pPinReq->pin.length > sizeof(app->pin)))
    {
        valid = false;
    }
    for (i = 0; i < pPinReq->pin.length; i++)
    {
        if ((pPinReq->pin.data[i] < '0') || (pPinReq->pin.data[i] > '9'))
        {
            valid = false;
            break;
        }
    }
    if (!valid)
    {
    	lUsed = addATStr(lUart, pbapATRespId_Fail);
    }
    else
    {
        memcpy(app->pin, pPinReq->pin.data, pPinReq->pin.length);
        app->pin_length = pPinReq->pin.length;
        if (!PsStore(PSKEY_USR_PIN, app->pin, app->pin_length))
        {
        	lUsed = addATStr(lUart, pbapATRespId_Fail);    
        }
        else
        {
        	lUsed = addATStr(lUart, pbapATRespId_Ok);
        }
    }
	addATCrLfandSend(lUart, lUsed);
}

void handleATGetName(Task pTask)
{
    sppTaskData* app = (sppTaskData*) pTask;
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;

	/* Send result to host */
	lUsed = addATStr(lUart, pbapATRespId_Name);
    lUsed += addATBuffer8(lUart, app->name, app->name_length);
	lUsed += addATStr(lUart, pbapATRespId_CrLf);
	lUsed += addATStr(lUart, pbapATRespId_Ok);
	addATCrLfandSend(lUart, lUsed);
}

void handleATSetName(Task pTask, const struct ATSetName *pNameReq)
{
    sppTaskData* app = (sppTaskData*) pTask;
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;
    bool valid = true;

    if ((pNameReq->name.length < 1) || (pNameReq->name.length > sizeof(app->name)))
    {
        valid = false;
    }
    if (!valid)
    {
    	lUsed = addATStr(lUart, pbapATRespId_Fail);
    }
    else
    {
        memcpy(app->name, pNameReq->name.data, pNameReq->name.length);
        app->name_length = pNameReq->name.length;
        if (!PsStore(PSKEY_USR_NAME, app->name, app->name_length))
        {
        	lUsed = addATStr(lUart, pbapATRespId_Fail);    
        }
        else
        {
            ConnectionChangeLocalName(app->name_length, app->name);
        	lUsed = addATStr(lUart, pbapATRespId_Ok);
        }
    }
	addATCrLfandSend(lUart, lUsed);
}

void handleATGetUart(Task pTask)
{
    sppTaskData* app = (sppTaskData*) pTask;
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;
	uint16 i;
	uint32 baud_rate = 0;

    for (i = 0; i < sizeof(uartInfoTable)/sizeof(uartInfoTable[0]); i++)
    {
        if (app->uart_data.baud_rate == uartInfoTable[i].baud_code)
        {
            baud_rate = uartInfoTable[i].baud_rate;
            break;
        }
    }

	/* Send result to host */
	lUsed = addATStr(lUart, pbapATRespId_Uart);
    lUsed += addATUint(lUart, baud_rate);
	lUsed += addATByte(lUart, ',');
    lUsed += addATUint(lUart, app->uart_data.stop_bits);
	lUsed += addATByte(lUart, ',');
    lUsed += addATUint(lUart, app->uart_data.parity);
	lUsed += addATStr(lUart, pbapATRespId_CrLf);
	lUsed += addATStr(lUart, pbapATRespId_Ok);
	addATCrLfandSend(lUart, lUsed);
}

void handleATSetUart(Task pTask, const struct ATSetUart *pUartReq)
{
    sppTaskData* app = (sppTaskData*) pTask;
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;
	uint16 i;
    uint32 baud_rate;
    vm_uart_rate baud_code = VM_UART_RATE_SAME;
    bool valid = true;

    baud_rate = handSeqExtractUint32(pUartReq->baud.data, pUartReq->baud.length);
    for (i = 0; i < sizeof(uartInfoTable)/sizeof(uartInfoTable[0]); i++)
    {
        if (uartInfoTable[i].baud_rate == baud_rate)
        {
            baud_code = uartInfoTable[i].baud_code;
            break;
        }
    }
    if (baud_code == VM_UART_RATE_SAME)
    {
        valid = false;
    }
    if (pUartReq->stop >= VM_UART_STOP_SAME)
    {
        valid = false;
    }
    if (pUartReq->parity >= VM_UART_PARITY_SAME)
    {
        valid = false;
    }

    if (!valid)
    {
    	lUsed = addATStr(lUart, pbapATRespId_Fail);
    }
    else
    {
        app->uart_data.baud_rate = baud_code;
        app->uart_data.stop_bits = pUartReq->stop;
        app->uart_data.parity = pUartReq->parity;
        if (!PsStore(PSKEY_USR_UART, &app->uart_data, sizeof(app->uart_data)))
        {
        	lUsed = addATStr(lUart, pbapATRespId_Fail);
            valid = false;
        }
        else
        {
        	lUsed = addATStr(lUart, pbapATRespId_Ok);
        }
    }
	addATCrLfandSend(lUart, lUsed);
    if (valid)
    {
        StreamUartConfigure(app->uart_data.baud_rate, app->uart_data.stop_bits, app->uart_data.parity);
    }
}

void handleATGetAddr(Task pTask)
{
    sppTaskData* app = (sppTaskData*) pTask;
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;

	/* Send result to host */
	lUsed = addATStr(lUart, pbapATRespId_Addr);
    lUsed += addATUintHex(lUart, app->bd_addr_local.lap);
	lUsed += addATByte(lUart, ':');
    lUsed += addATUintHex(lUart, app->bd_addr_local.uap);
	lUsed += addATByte(lUart, ':');
    lUsed += addATUintHex(lUart, app->bd_addr_local.nap);
	lUsed += addATStr(lUart, pbapATRespId_CrLf);
	lUsed += addATStr(lUart, pbapATRespId_Ok);
	addATCrLfandSend(lUart, lUsed);
}

void handleATGetVersion(Task pTask)
{
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;

	/* Send result to host */
	lUsed = addATStr(lUart, pbapATRespId_Ver);
    lUsed += addATUint(lUart, VER_H);
	lUsed += addATByte(lUart, '.');
    lUsed += addATUint(lUart, VER_L);
	lUsed += addATStr(lUart, pbapATRespId_CrLf);
	lUsed += addATStr(lUart, pbapATRespId_Ok);
	addATCrLfandSend(lUart, lUsed);
}

void handleATOrgl(Task pTask)
{
    sppTaskData* app = (sppTaskData*) pTask;
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;

    PsStore(PSKEY_USR_PIN, NULL, 0);
    PsStore(PSKEY_USR_NAME, NULL, 0);
    PsStore(PSKEY_USR_UART, NULL, 0);

    initAppData();
    ConnectionChangeLocalName(app->name_length, app->name);

	/* Send result to host */
    lUsed = addATStr(lUart, pbapATRespId_Ok);
	addATCrLfandSend(lUart, lUsed);
    StreamUartConfigure(app->uart_data.baud_rate, app->uart_data.stop_bits, app->uart_data.parity);
}

void handleATReset(Task pTask)
{
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;

	/* Send result to host */
	lUsed = addATStr(lUart, pbapATRespId_Ok);
	addATCrLfandSend(lUart, lUsed);
    BootSetMode(BootGetMode());
}
