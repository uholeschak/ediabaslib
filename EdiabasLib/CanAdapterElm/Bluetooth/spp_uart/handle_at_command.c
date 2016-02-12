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

void spp_handleUnrecognised(const uint8 *data, uint16 length, Task task)
{
 	Sink lUart = StreamUartSink();
	uint16 lUsed = 0;
    PRINT(("spp_handleUnrecognised\n"));
	/* Send result to host */
	lUsed = addATStr(lUart, pbapATRespId_Fail);
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

void handleATGetAddr(Task pTask)
{
    sppTaskData* app = (sppTaskData*) pTask;
    Sink lUart = StreamUartSink();
	uint16 lUsed = 0;

	/* Send result to host */
	lUsed = addATStr(lUart, pbapATRespId_Addr);
    lUsed += addATUintHex(lUart, app->bd_addr_local.lap);
	lUsed += addATStr(lUart, pbapATRespId_Colon);
    lUsed += addATUintHex(lUart, app->bd_addr_local.uap);
	lUsed += addATStr(lUart, pbapATRespId_Colon);
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
    lUsed += addATUint8(lUart, VER_H);
	lUsed += addATStr(lUart, pbapATRespId_Dot);
    lUsed += addATUint8(lUart, VER_L);
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

    initAppData();
    ConnectionChangeLocalName(app->name_length, app->name);

	/* Send result to host */
    lUsed = addATStr(lUart, pbapATRespId_Ok);
	addATCrLfandSend(lUart, lUsed);
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
