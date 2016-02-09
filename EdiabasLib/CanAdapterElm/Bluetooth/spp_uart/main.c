/* Copyright (C) Cambridge Silicon Radio Limited 2005-2009 */
/* Part of BlueLab 4.1-Release */
#include "spp_dev_private.h"
#include "spp_dev_init.h"
#include "spp_dev_inquire.h"
#include "spp_dev_auth.h"
#include "spp_uart_leds.h"

#include <connection.h>
#include <panic.h>
#include <stdio.h>
#include <stream.h>
#include <pio.h>

#ifdef DEBUG_ENABLED
#define DEBUG(x) {printf x;}
#else
#define DEBUG(x)
#endif

static sppTaskData theSppApp;


/*************************************************************************
NAME    
    sppAppTask
    
DESCRIPTION
  Returns the spp app's main task.

RETURNS
    Task
*/
Task getAppTask(void)
{
    return &theSppApp.task;
}

/*************************************************************************
NAME    
    unhandledSppState
    
DESCRIPTION
    This function is called when a message arrives and the Spp app is
    in an unexpected state.  
    
RETURNS
    
*/
static void unhandledSppState(sppDevState state, MessageId id)
{
    DEBUG(("SPP current state %d message id 0x%x\n", state, id));   
}

/*************************************************************************
NAME    
    setSppState
    
DESCRIPTION
    Set the SPP State to the specified state

RETURNS
    
*/
void setSppState(const sppDevState state)
{
    DEBUG(("SPP State - C=%d N=%d\n",theSppApp.spp_state, state));
    theSppApp.spp_state = state;
	
	/* Update led flash pattern */
    switch (state)
    {
    case sppDevConnected:
        ledsPlay(RED_ON);
        break;
    default:
		ledsPlay(RED_FLASH);
        break;
    }
}


/* Task handler function */
static void app_handler(Task task, MessageId id, Message message)
{
    sppDevState state = theSppApp.spp_state;
    
    switch(id)
    {
    case CL_INIT_CFM:
        DEBUG(("CL_INIT_CFM\n"));
        if(((CL_INIT_CFM_T*)message)->status == success)
            /* Connection Library initialisation was a success */
            sppDevInit();   
        else
            Panic();
        break;
    case CL_DM_LINK_SUPERVISION_TIMEOUT_IND:
        DEBUG(("CL_DM_LINK_SUPERVISION_TIMEOUT_IND\n"));
        break;
    case CL_DM_SNIFF_SUB_RATING_IND:
        DEBUG(("CL_DM_SNIFF_SUB_RATING_IND\n"));
        break;
    case SPP_INIT_CFM:
        DEBUG(("SPP_INIT_CFM\n"));
        switch(state)
        {
        case sppDevInitialising:
            /* Check for spp_init_success. What do we do if it failed? */
            if (((SPP_INIT_CFM_T *) message)->status == spp_init_success)
            {
                setSppState(sppDevReady);
                sppDevInquire(&theSppApp);
            }
            break;
        case sppDevReady:
        case sppDevPairable:
        case sppDevConnecting:
        case sppDevConnected:
        default:
            unhandledSppState(state, id);
            break;
        }
        break;
    case SPP_CONNECT_CFM:
        {
            SPP_CONNECT_CFM_T *cfm = (SPP_CONNECT_CFM_T *) message;

            DEBUG(("SPP_CONNECT_CFM result = %d\n", cfm->status));
         
            switch(state)
            {   
            case sppDevConnecting:
                /* Connect cfm, but must check status as connection may have failed */
                if (cfm->status == rfcomm_connect_success)
                {
                    /* Connection Success */
                    DEBUG(("Device connected...\n"));
                    /* Connect Uart to Rfcomm */
                    (void) StreamConnect(StreamUartSource(), cfm->sink);
                    (void) StreamConnect(StreamSourceFromSink(cfm->sink), StreamUartSink());
					/* (void) StreamConnectDispose(StreamSourceFromSink(cfm->sink)); */

                    theSppApp.spp = cfm->spp;
                    setSppState(sppDevConnected);
                    ConnectionWriteScanEnable(hci_scan_enable_off);
                    /*(void) MessageCancelFirst(&theSppApp.task, SPP_DEV_INQUIRY_TIMEOUT_IND); */
                }
                else
                {
                    /* Connection failed */
                    setSppState(sppDevPairable);
                    DEBUG(("Connection failed\n"));
                }
                break;
            case sppDevPairable:
                /* Connect cfm, but must check status as connection may have failed */
                if (cfm->status == rfcomm_connect_success)
                {
                    /* Device has been reset to pairable mode. Disconnect from current device */
                    SppDisconnect(cfm->spp);
                }
                break;
            case sppDevInitialising:
            case sppDevReady:
            case sppDevConnected:
            default:
                unhandledSppState(state, id);
                break;
            }
        }
        break;
    case SPP_CONNECT_IND:
        DEBUG(("SPP_CONNECT_IND\n"));
        switch(state)
        {   
        case sppDevPairable:
            /* Received command that a device is trying to connect. Send response. */
            sppDevAuthoriseConnectInd(&theSppApp,(SPP_CONNECT_IND_T*)message);
            setSppState(sppDevConnecting);
            break;
        case sppDevInitialising:
        case sppDevConnecting:
        case sppDevReady:
        case sppDevConnected:
        default:
            unhandledSppState(state, id);
            break;
        }
        break;
    case SPP_DISCONNECT_IND:
        DEBUG(("SPP_DISCONNECT_IND\n"));
        /* Disconnect message has arrived */
        switch(state)
        {
        case sppDevConnected:
            sppDevInquire(&theSppApp);
            break;
        case sppDevInitialising:
        case sppDevPairable:
        case sppDevConnecting:
        case sppDevReady:
        default:
            unhandledSppState(state, id);
            break;
        }
        break;
#if 0        
    case SPP_DEV_INQUIRY_TIMEOUT_IND:
        DEBUG(("SPP_DEV_INQUIRY_TIMEOUT_IND\n"));
        switch(state)
        {
        case sppDevPairable:
            /* Inquiry mode timed out */
            ConnectionWriteScanEnable(hci_scan_enable_off);
            setSppState(sppDevReady);
            break;
        case sppDevConnected:
        case sppDevInitialising:
        case sppDevConnecting:
        case sppDevReady:
        default:
            unhandledSppState(state, id);
            break;
        }
        break;
#endif
    case CL_DM_ACL_OPENED_IND:
        DEBUG(("CL_DM_ACL_OPENED_IND\n"));
        break;
    case CL_DM_ACL_CLOSED_IND:
        DEBUG(("CL_DM_ACL_CLOSED_IND\n"));
        break;
    case CL_SM_PIN_CODE_IND:
        DEBUG(("CL_SM_PIN_CODE_IND\n"));
        sppDevHandlePinCodeRequest((CL_SM_PIN_CODE_IND_T *) message);
        break;
    case CL_SM_AUTHORISE_IND:  
        DEBUG(("CL_SM_PIN_CODE_IND\n"));
        sppDevAuthoriseResponse((CL_SM_AUTHORISE_IND_T*) message);
        break;
    case CL_SM_AUTHENTICATE_CFM:
        DEBUG(("CL_SM_AUTHENTICATE_CFM\n"));
        sppDevSetTrustLevel((CL_SM_AUTHENTICATE_CFM_T*)message);    
        break;
    case CL_SM_ENCRYPTION_KEY_REFRESH_IND:
        DEBUG(("CL_SM_ENCRYPTION_KEY_REFRESH_IND\n"));
        break;
    case CL_DM_LINK_POLICY_IND:
        DEBUG(("CL_DM_LINK_POLICY_IND\n"));
        break;
    case CL_SM_IO_CAPABILITY_REQ_IND:
        DEBUG(("CL_SM_IO_CAPABILITY_REQ_IND\n"));
        ConnectionSmIoCapabilityResponse( &theSppApp.bd_addr, 
                                          cl_sm_io_cap_no_input_no_output,
                                          FALSE,
                                          TRUE,
                                          FALSE,
                                          0,
                                          0 );
        break;
 
    case CL_SM_REMOTE_IO_CAPABILITY_IND:
        {
            CL_SM_REMOTE_IO_CAPABILITY_IND_T *csricit = 
                    ( CL_SM_REMOTE_IO_CAPABILITY_IND_T *) message;

            DEBUG(("CL_SM_REMOTE_IO_CAPABILITY_REQ_IND\n"));
            
            DEBUG(("\t Remote Addr: nap %04x uap %02x lap %08lx\n",
                    csricit->bd_addr.nap,
                    csricit->bd_addr.uap,
                    csricit->bd_addr.lap ));
            theSppApp.bd_addr = csricit->bd_addr;
        }
        break;          
    case SPP_MESSAGE_MORE_DATA:
        DEBUG(("SPP_MESSAGE_MORE_DATA\n"));
        break;
    case SPP_MESSAGE_MORE_SPACE:
        DEBUG(("SPP_MESSAGE_MORE_SPACE\n"));
        break;

    default:
        /* An unexpected message has arrived - must handle it */
        DEBUG(("main app - msg type  not yet handled 0x%x\n", id));
        break;
    }
}

int main(void)
{
    DEBUG(("Main Started...\n"));
    
#ifndef NO_UART_CHECK
    /* Make sure Uart has been successfully initialised before running */
    if (StreamUartSource())
#endif
    {
        /* Set up task 1 handler */
        theSppApp.task.handler = app_handler;
        
        setSppState(sppDevInitialising);
        theSppApp.spp = 0;
        
        /* Init the Connection Manager */
        ConnectionInit(&theSppApp.task);

        /* Start the message scheduler loop */
        MessageLoop();
    }
    
    /* Will never get here! */
    DEBUG(("Main Ended!\n"));
    
    return 0;
}

