/****************************************************************************
Copyright (C) Cambridge Silicon Radio Limited 2004-2009
Part of BlueLab 4.1-Release

FILE NAME
    spp_dev_auth.c
    
DESCRIPTION
    Handles authorisation procedures of spp dev B application
    
*/

/****************************************************************************
    Header files
*/
#include "spp_dev_auth.h"
#include "spp_dev_private.h"

#include <stdio.h>
#include <ps.h>

/****************************************************************************
NAME    
    sppDevHandlePinCodeRequest
    
DESCRIPTION
    Reply to pin code request

RETURNS
    void
*/
void sppDevHandlePinCodeRequest(sppTaskData* app, const CL_SM_PIN_CODE_IND_T* ind)
{
    ConnectionSmPinCodeResponse(&ind->bd_addr, app->pin_length, app->pin);
}

/****************************************************************************
NAME    
    sppDevAuthoriseResponse
    
DESCRIPTION
    Give authorisation to device

RETURNS
    void
*/
void sppDevAuthoriseResponse(const CL_SM_AUTHORISE_IND_T* ind)
{
    ConnectionSmAuthoriseResponse(&ind->bd_addr, 
                                          ind->protocol_id, 
                                          ind->channel, 
                                          ind->incoming, 
                                          TRUE);
}

/****************************************************************************
NAME    
    sppDevAuthoriseConnectInd
    
DESCRIPTION
    Authorise a connect request

RETURNS
    void
*/
void sppDevAuthoriseConnectInd(sppTaskData* app, const SPP_CONNECT_IND_T* ind)
{
    SppConnectResponseLazy(ind->spp, TRUE, &ind->addr, 1, FRAME_SIZE);
}

/****************************************************************************
NAME    
    sppDevSetTrustLevel
    
DESCRIPTION
    Set the trust level of a device

RETURNS
    void
*/
void sppDevSetTrustLevel(const CL_SM_AUTHENTICATE_CFM_T* cfm)
{
    if(cfm->status == auth_status_success)
    {
        /* DEBUG("Pairing success, now set the trust level\n");*/
        ConnectionSmSetTrustLevel(&cfm->bd_addr, TRUE);
    }
    else if(cfm->status == auth_status_fail)
    {
        /* DEBUG("Pairing failed\n");*/
    }
    else
    {
        /* DEBUG("Pairing timeout\n");*/
    }
}
