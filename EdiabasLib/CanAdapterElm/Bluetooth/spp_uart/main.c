/* Copyright (C) Cambridge Silicon Radio Limited 2005-2009 */
/* Part of BlueLab 4.1-Release */
#include "spp_dev_private.h"
#include "spp_dev_init.h"
#include "spp_dev_inquire.h"
#include "spp_dev_auth.h"
#include "spp_uart_leds.h"
#include "handle_system.h"

#include <connection.h>
#include <panic.h>
#include <stdio.h>
#include <stream.h>
#include <source.h>
#include <string.h>
#include <pio.h>
#include <ps.h>
#include <boot.h>
#include <app/bluestack/types.h>
#include <app/bluestack/bluetooth.h>
#include <app/bluestack/l2cap_prim.h>
#include <app/bluestack/rfcomm_prim.h>

#ifdef DEBUG_ENABLED
#define DEBUG(x) {printf x;}
#else
#define DEBUG(x)
#endif

#ifdef VOLATILE_PS
#define PSKEY_USR0 0x028a
#define	TRUSTED_DEVICE_INDEX 41
#define	TRUSTED_DEVICE_LIST_BASE 42
#define	TRUSTED_DEVICE_LIST_LENGTH 8
#define PS_ARRAY_BASE TRUSTED_DEVICE_INDEX
#define PS_ARRAY_SIZE (TRUSTED_DEVICE_LIST_LENGTH + 1)

typedef struct
{
    uint8               data[32];
    uint16              length;
} psData;

static psData psDataArray[PS_ARRAY_SIZE];
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
    initAppData
    
DESCRIPTION
  Init the application data

RETURNS
    
*/
void initAppData(void)
{
    theSppApp.pin_length = PsRetrieve(PSKEY_USR_PIN, theSppApp.pin, sizeof(theSppApp.pin));
    if ((theSppApp.pin_length < 4) || (theSppApp.pin_length > sizeof(theSppApp.pin)))
    {
        const uint8 pin_code[] = {'1','2','3','4'};

        memcpy(theSppApp.pin, pin_code, sizeof(pin_code));
        theSppApp.pin_length = sizeof(pin_code);
    }

    theSppApp.name_length = PsRetrieve(PSKEY_USR_NAME, theSppApp.name, sizeof(theSppApp.name));
    if ((theSppApp.name_length < 1) || (theSppApp.name_length > sizeof(theSppApp.name)))
    {
        const uint8 name[] = {'D','e','e','p',' ','O','B','D'};

        memcpy(theSppApp.name, name, sizeof(name));
        theSppApp.name_length = sizeof(name);
    }

    if (PsRetrieve(PSKEY_USR_UART, &theSppApp.uart_data, sizeof(theSppApp.uart_data)) != sizeof(theSppApp.uart_data))
    {
        theSppApp.uart_data.baud_rate = VM_UART_RATE_115K2;
        theSppApp.uart_data.stop_bits = VM_UART_STOP_ONE;
        theSppApp.uart_data.parity = VM_UART_PARITY_NONE;
    }
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
        ledsPlay(LED_CONNECTED);
        break;
    default:
		ledsPlay(LED_DISCONNECTED);
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
        {
            ConnectionChangeLocalName(theSppApp.name_length, theSppApp.name);
            ConnectionReadLocalAddr(task);
            /* Configure Mode4 Security Settings */
            ConnectionSmSecModeConfig(task, cl_sm_wae_always, FALSE, TRUE);
            /* Turn off all SDP security */
            ConnectionSmSetSecurityLevel(protocol_l2cap, 1, ssp_secl4_l0, TRUE, FALSE, FALSE);
            /* Connection Library initialisation was a success */
            sppDevInit();
        }
        else
        {
            Panic();
        }
        break;
    case CL_DM_LOCAL_BD_ADDR_CFM:
        {
            CL_DM_LOCAL_BD_ADDR_CFM_T *bd_addr = (CL_DM_LOCAL_BD_ADDR_CFM_T *) message;
            if (bd_addr->status != success)
            {
                Panic();
            }
            memcpy(&theSppApp.bd_addr_local, &bd_addr->bd_addr, sizeof(theSppApp.bd_addr_local));
        }
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
                    /*
                    (void) StreamConnect(StreamUartSource(), cfm->sink);
                    (void) StreamConnect(StreamSourceFromSink(cfm->sink), StreamUartSink());
                    */
					/* (void) StreamConnectDispose(StreamSourceFromSink(cfm->sink)); */

                    theSppApp.spp = cfm->spp;
                    theSppApp.spp_sink = cfm->sink;
                    theSppApp.spp_mode = sppDataModeInit;

                    /* CONTROL_MODEM_DV_MASK: DCD */
                    /* CONTROL_MODEM_RTR_MASK: CTS */
                    /* CONTROL_MODEM_RTC_MASK: DSR */
                    ConnectionRfcommControlSignalRequest(&theSppApp.task, theSppApp.spp_sink, 0, CONTROL_MODEM_DV_MASK | CONTROL_MODEM_RTR_MASK | CONTROL_MODEM_RTC_MASK);
                    setSppState(sppDevConnected);
                    ConnectionWriteScanEnable(hci_scan_enable_off);
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
    case SPP_DEV_RESET:
        DEBUG(("SPP_DEV_RESET\n"));
        BootSetMode(BOOTMODE_UART);
        break;
    case SPP_DEV_FWUPDATE:
        DEBUG(("SPP_DEV_FWUPDATE\n"));
        BootSetMode(BOOTMODE_UPDATE);
        break;
    case SPP_DEV_CONFIG_UART:
        DEBUG(("SPP_DEV_CONFIG_UART\n"));
        StreamUartConfigure(theSppApp.uart_data.baud_rate, theSppApp.uart_data.stop_bits, theSppApp.uart_data.parity);
        break;
    case CL_DM_ACL_OPENED_IND:
        DEBUG(("CL_DM_ACL_OPENED_IND\n"));
        break;
    case CL_DM_ACL_CLOSED_IND:
        DEBUG(("CL_DM_ACL_CLOSED_IND\n"));
        break;
    case CL_SM_PIN_CODE_IND:
        DEBUG(("CL_SM_PIN_CODE_IND\n"));
        sppDevHandlePinCodeRequest(&theSppApp, (CL_SM_PIN_CODE_IND_T *) message);
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
		handleMoreData(&theSppApp, ((SPP_MESSAGE_MORE_DATA_T*)message)->source);
        break;
    case SPP_MESSAGE_MORE_SPACE:
		handleMoreSpace(&theSppApp, ((SPP_MESSAGE_MORE_SPACE_T*)message)->sink);
        break;
	case MESSAGE_MORE_DATA:
        {
            Source pSrc = ((MessageMoreData*)message)->source;
            if (pSrc == StreamUartSource())
            {
        		handleMoreData(&theSppApp, pSrc);
            }
            else
            {
                SourceDrop(pSrc, SourceSize(pSrc));
            }
        }
		break;
	case MESSAGE_MORE_SPACE:
		handleMoreSpace(&theSppApp, ((MessageMoreSpace*)message)->sink);
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

    theSppApp.boot_mode = BootGetMode();
#ifdef VOLATILE_PS
    memset(&psDataArray, 0x00, sizeof(psDataArray));
#else
    if (PsFreeCount(50) < 4)
    {
        /* storage memory is low, force a defragment */
        PsFlood();
        BootSetMode(theSppApp.boot_mode);
        return 0;
    }
#endif

    if (theSppApp.boot_mode == BOOTMODE_INIT)
    {
        BootSetMode(BOOTMODE_UART);
        return 0;
    }
    /* Make sure Uart has been successfully initialised before running */
    if ((theSppApp.boot_mode != BOOTMODE_UART) || StreamUartSource())
    {
        /* Set up task 1 handler */
        theSppApp.task.handler = app_handler;

        setSppState(sppDevInitialising);
        theSppApp.spp = 0;

        initAppData();
        if (theSppApp.boot_mode == BOOTMODE_UART)
        {
            StreamUartConfigure(theSppApp.uart_data.baud_rate, theSppApp.uart_data.stop_bits, theSppApp.uart_data.parity);

            PanicNotNull(MessageSinkTask(StreamUartSink(), &theSppApp.task));
        }

        /* Init the Connection Manager */
        ConnectionInit(&theSppApp.task);

        /* Start the message scheduler loop */
        MessageLoop();
    }

    /* Will never get here! */
    DEBUG(("Main Ended!\n"));

    return 0;
}

#ifdef VOLATILE_PS
uint16 PsStore(uint16 key, const void *buff, uint16 words)
{
    if (key >= PS_ARRAY_BASE && key < PS_ARRAY_BASE + PS_ARRAY_SIZE)
    {
        uint16 index = key - PS_ARRAY_BASE;
        uint16 length = words;
        if (sizeof(psDataArray[index].data) < length)
        {
            length = sizeof(psDataArray[index].data);
        }

        if (buff != NULL)
        {
            memcpy(&psDataArray[index].data, buff, length);
            psDataArray[index].length = length;
        }
        else
        {
            psDataArray[index].length = 0;
        }
        return length;
    }

    return words;
}

uint16 PsRetrieve(uint16 key, void *buff, uint16 words)
{
    if (key >= PS_ARRAY_BASE && key < PS_ARRAY_BASE + PS_ARRAY_SIZE)
    {
        uint16 index = key - PS_ARRAY_BASE;
        uint16 length = words;
        if (psDataArray[index].length < length)
        {
            length = psDataArray[index].length;
        }

        if (buff != NULL)
        {
            memcpy(buff, &psDataArray[index].data, length);
        }
        return length;
    }

    return PsFullRetrieve(key + PSKEY_USR0, buff, words);
}
#endif
