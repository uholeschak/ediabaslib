/****************************************************************************
Copyright (C) Cambridge Silicon Radio Limited 2006-2009
Part of BlueLab 4.1-Release

DESCRIPTION
	Interface definition for handling system messages and functionality
	
FILE
	handle_system.h
*/


#ifndef HANDLE_SYSTEM_H
#define HANDLE_SYSTEM_H

#include <message.h>

void handleMoreData(sppTaskData* app, Source pSrc);
void handleMoreSpace(sppTaskData* app, Sink pSink);



#endif /* HANDLE_SYSTEM_H */
