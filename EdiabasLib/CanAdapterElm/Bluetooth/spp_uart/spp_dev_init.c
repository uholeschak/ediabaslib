/****************************************************************************
Copyright (C) Cambridge Silicon Radio Limited 2004-2009
Part of BlueLab 4.1-Release

FILE NAME
    spp_dev_init.h
    
DESCRIPTION
    Handles initialisation procedures of spp dev A application
    
*/

/****************************************************************************
    Header files
*/
#include "spp_dev_init.h"
#include "spp_dev_private.h"

#include <spp.h>

/****************************************************************************
NAME    
    sppDevInit
    
DESCRIPTION
    Initialisation of Spp profile

RETURNS
    void
*/
void sppDevInit()
{
    spp_init_params init;

    init.client_recipe = 0;
    init.size_service_record = 0;
	init.service_record = 0;
	init.no_service_record = 0;
	
    /* Initialise the spp profile lib, stating that this is device B */ 
    SppInitLazy(getAppTask(), getAppTask(), &init);
}
