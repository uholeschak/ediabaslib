/****************************************************************************
Copyright (C) Cambridge Silicon Radio Limited 2006-2009
Part of BlueLab 4.1-Release

DESCRIPTION
    Implementation of AT Response generation functionality
	
FILE
	generate_at_resp.c
*/

#include <stream.h>
#include <sink.h>
#include <source.h>
#include <string.h>
#include <panic.h>
#include <stdio.h>

#include "generate_at_resp.h"


static const uint8 gCrLfStr[] = {'\r','\n'};
static const uint8 gOkResStr[] = {'O','K'};
static const uint8 gFailResStr[] = {'F','A','I','L'};
static const uint8 gPinResStr[] = {'+','P','S','W','D',':'};
static const uint8 gNameResStr[] = {'+','N','A','M','E',':'};

typedef struct
{
	const uint8 *str;
	uint16 len;
} ATStrType;

#define AT_LIST_ENTRY(i) {(i), sizeof((i))}

static const ATStrType gAtStrList[] = 
							{ 
								AT_LIST_ENTRY(gCrLfStr),
								AT_LIST_ENTRY(gOkResStr),
								AT_LIST_ENTRY(gFailResStr),
								AT_LIST_ENTRY(gPinResStr),
								AT_LIST_ENTRY(gNameResStr),
								{0, 0}
							};


uint16 addATStr(Sink pSink, pbapATRespId pId)
{
	uint16 lLen = gAtStrList[pId].len;
	uint8* lS=SinkMap(pSink);
	uint16 lO;

	lO = SinkClaim(pSink, lLen);
	if (lO == 0xffff)
		Panic(); /* Error */
	lS += lO;

	memcpy(lS, gAtStrList[pId].str, lLen);

	return lLen;
}

void addATCrLfandSend(Sink pSink, uint16 pUsed)
{
	uint16 lLen = sizeof(gCrLfStr);
	uint8* lS=SinkMap(pSink);
	uint16 lO;

	lO = SinkClaim(pSink, lLen);
	if (lO == 0xffff)
		Panic(); /* Error */
	lS += lO;

	memcpy(lS, gCrLfStr, lLen);

	SinkFlush(pSink, lLen+pUsed);
}

uint16 addATBuffer8(Sink pSink, const uint8 *pBuffer, uint16 pBufLen)
{
	uint8* lS=SinkMap(pSink);
	uint16 lO;

	lO = SinkClaim(pSink, pBufLen);
	if (lO == 0xffff)
		Panic(); /* Error */
	lS += lO;

	memcpy(lS, pBuffer, pBufLen);

	return pBufLen;
}

uint16 addATByte(Sink pSink, uint8 pByte)
{
	uint8* lS=SinkMap(pSink);
	uint16 lO;

	lO = SinkClaim(pSink, 1);
	if (lO == 0xffff)
		Panic(); /* Error */
	lS += lO;

	lS[0] = pByte;

	return 1;
}

uint16 addATUint8(Sink pSink, uint8 pValue)
{
	char lTxt[10];
	uint16 lLen;
	uint8* lS=SinkMap(pSink);
	uint16 lO;

	sprintf(lTxt, "%d", pValue & 0xff);
	lLen = strlen(lTxt);

	lO = SinkClaim(pSink, lLen);
	if (lO == 0xffff)
		Panic(); /* Error */
	lS += lO;

	memcpy(lS, lTxt, lLen);

	return lLen;
}
