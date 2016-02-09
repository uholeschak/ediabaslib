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
void sppDevHandlePinCodeRequest(const CL_SM_PIN_CODE_IND_T* ind)
{
    uint8 pin[16];
	uint16 pin_length = 0;
	
	/* Do we have a fixed pin in PS, if not reject pairing (by setting the length to zero) */ 
    if (((pin_length = PsFullRetrieve(PSKEY_FIXED_PIN, pin, sizeof(pin))) == 0) || (pin_length > sizeof(pin)))
    {
        pin_length = 0; 
    }
	
    ConnectionSmPinCodeResponse(&ind->bd_addr, pin_length, pin);
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
