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
