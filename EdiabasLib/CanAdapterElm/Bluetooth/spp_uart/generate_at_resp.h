/****************************************************************************
Copyright (C) Cambridge Silicon Radio Limited 2006-2009
Part of BlueLab 4.1-Release

DESCRIPTION
    Interface for AT Response generation functionality
	
FILE
	generate_at_resp.h
*/


#ifndef GENERATE_AT_RESP_H
#define GENERATE_AT_RESP_H

#include <stream.h>

typedef enum
{
	pbapATRespId_CrLf,
	pbapATRespId_Ok,
	pbapATRespId_Colon,
	pbapATRespId_Dot,
	pbapATRespId_Fail,
	pbapATRespId_Pin,
	pbapATRespId_Name,
	pbapATRespId_Addr,
	pbapATRespId_Ver,
	
	pbapATRespId_eol
} pbapATRespId;

uint16 addATStr(Sink pSink, pbapATRespId pId);

void addATCrLfandSend(Sink pSink, uint16 pUsed);

uint16 addATBuffer8(Sink pSink, const uint8 *pBuffer, uint16 pBufLen);

uint16 addATByte(Sink pSink, uint8 pByte);

uint16 addATUint8(Sink pSink, uint8 pValue);

uint16 addATUintHex(Sink pSink, uint32 pValue);

#endif /* GENERATE_AT_RESP_H */
