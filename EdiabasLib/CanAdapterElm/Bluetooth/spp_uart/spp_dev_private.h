/****************************************************************************
Copyright (C) Cambridge Silicon Radio Limited 2004-2009
Part of BlueLab 4.1-Release

FILE NAME
    spp_dev_private.h
    
DESCRIPTION
    
*/

#ifndef _SPP_DEV_PRIVATE_H_
#define _SPP_DEV_PRIVATE_H_

#include <spp.h>
#include <message.h>
#include <stream.h>
#include <app/message/system_message.h>

#define VER_H            1
#define VER_L            0

#define PSKEY_USR_PIN    0
#define PSKEY_USR_NAME   1
#define PSKEY_USR_UART   2

#define FRAME_SIZE		0

#define SPP_MSG_BASE    (0x0)

enum
{
    SPP_DEV_RESET = SPP_MSG_BASE,
    SPP_DEV_CONFIG_UART
};

typedef enum
{
    sppDataModeInit,
    sppDataModeConfig,
    sppDataModeDataReq,
    sppDataModeData
} sppDataMode;

typedef enum
{
    sppDevInitialising,
    sppDevReady,
    sppDevPairable,
    sppDevConnecting,
    sppDevConnected
} sppDevState;

typedef struct
{
    vm_uart_rate        baud_rate;
    vm_uart_stop        stop_bits;
    vm_uart_parity      parity;
} uartData;

typedef struct 
{
    TaskData            task;
    uint16              boot_mode;
    SPP*                spp;
    Sink                spp_sink;
    Sink                sink;
    uartData            uart_data;
    bdaddr              bd_addr;
    bdaddr              bd_addr_local;
    uint8               pin[16];
    uint16              pin_length;
    uint8               name[31];
    uint16              name_length;
    sppDevState         spp_state;
    sppDataMode         spp_mode;
} sppTaskData;


/*************************************************************************
NAME    
    setSppState
    
DESCRIPTION
    Set the SPP State to the specified state

RETURNS
    
*/
void setSppState(const sppDevState state);


/****************************************************************************
NAME    
    sppAppTask
    
DESCRIPTION
  Returns the spp app's main task.

RETURNS
    Task
*/
Task getAppTask(void);


/*************************************************************************
NAME    
    initAppData
    
DESCRIPTION
  Init the application data

RETURNS
    
*/
void initAppData(void);
        
#endif /* _SPP_DEV_PRIVATE_H_ */

