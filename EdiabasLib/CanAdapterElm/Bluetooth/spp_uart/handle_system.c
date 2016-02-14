/****************************************************************************
Copyright (C) Cambridge Silicon Radio Limited 2006-2009
Part of BlueLab 4.1-Release

DESCRIPTION
	Implementation for handling system messages and functionality
	
FILE
	handle_system.c
	
*/

/****************************************************************************
    Header files
*/

#include <print.h>
#include <stdlib.h>
#include <stdbool.h>
#include <string.h>
#include <vm.h>
#include <stream.h>

#include "spp_dev_private.h"
#include "handle_system.h"
#include "spp_uart_parse.h"
#include "generate_at_resp.h"

static const uint8 gConfigStr[] = {'A','T','+','C','O','N','F','\r','\n'};

/* Message Handlers */

void handleMoreData(sppTaskData* app, Source pSrc)
{
    Sink pSink = StreamSinkFromSource(pSrc);
    uint16 lLen = SourceSize(pSrc);

	PRINT(("MESSAGE_MORE_DATA\n"));

    if (pSrc == StreamUartSource())
    {
        if (app->spp_state == sppDevConnected)
        {
            SourceDrop(pSrc, lLen);
            return;
        }
    }
    else
    {
        if (app->spp_mode == sppDataModeInit)
        {
            bool connect = true;
            if (lLen == sizeof(gConfigStr))
            {
                const uint8* s = SourceMap(pSrc);
                if (s != NULL)
                {
                    if (memcmp(s, gConfigStr, sizeof(gConfigStr)) == 0)
                    {
                        connect = false;
                    }
                }
            }
            if (connect)
            {
                app->spp_mode = sppDataModeDataReq;
            }
            else
            {
            	uint16 lUsed = addATStr(pSink, pbapATRespId_Ok);
    	        addATCrLfandSend(pSink, lUsed);
                SourceDrop(pSrc, lLen);
                app->spp_mode = sppDataModeConfig;
            }
            lLen = 0;
        }
    }

    while (lLen > 0)
    {
        app->sink = pSink;
        /* Keep parsing while we have data in the buffer */
        if (!spp_parseSource(pSrc, (Task) app))
            break;

        /* Check we have more data to parse */
        lLen = SourceSize(pSrc);
    }

    if (pSrc != StreamUartSource())
    {
        /* bugfix for stream handler: */
        /* if the stream is not empty during disconnect the stream is invalid at next connection */
        if (lLen > 0)
        {
            SourceDrop(pSrc, lLen);
        }

        if (app->spp_mode == sppDataModeDataReq)
        {
            (void) StreamConnect(StreamUartSource(), StreamSinkFromSource(pSrc));
            (void) StreamConnect(pSrc, StreamUartSink());
            app->spp_mode = sppDataModeData;
        }
    }
}

void handleMoreSpace(sppTaskData* app, Sink pSink)
{
	PRINT(("MESSAGE_MORE_SPACE\n"));
}

