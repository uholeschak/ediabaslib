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
#include <app/message/system_message.h>

#define FRAME_SIZE		0

#define SPP_MSG_BASE    (0x0)

enum
{
    SPP_DEV_INQUIRY_TIMEOUT_IND = SPP_MSG_BASE
};

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
    TaskData            task;
    SPP*                spp;
    bdaddr              bd_addr;
    sppDevState         spp_state;
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


#endif /* _SPP_DEV_PRIVATE_H_ */

