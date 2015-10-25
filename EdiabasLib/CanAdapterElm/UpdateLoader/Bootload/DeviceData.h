/************************************************************************
* Copyright (c) 2009-2011,  Microchip Technology Inc.
*
* Microchip licenses this software to you solely for use with Microchip
* products.  The software is owned by Microchip and its licensors, and
* is protected under applicable copyright laws.  All rights reserved.
*
* SOFTWARE IS PROVIDED "AS IS."  MICROCHIP EXPRESSLY DISCLAIMS ANY
* WARRANTY OF ANY KIND, WHETHER EXPRESS OR IMPLIED, INCLUDING BUT
* NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS
* FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.  IN NO EVENT SHALL
* MICROCHIP BE LIABLE FOR ANY INCIDENTAL, SPECIAL, INDIRECT OR
* CONSEQUENTIAL DAMAGES, LOST PROFITS OR LOST DATA, HARM TO YOUR
* EQUIPMENT, COST OF PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY
* OR SERVICES, ANY CLAIMS BY THIRD PARTIES (INCLUDING BUT NOT LIMITED
* TO ANY DEFENSE THEREOF), ANY CLAIMS FOR INDEMNITY OR CONTRIBUTION,
* OR OTHER SIMILAR COSTS.
*
* To the fullest extent allowed by law, Microchip and its licensors
* liability shall not exceed the amount of fees, if any, that you
* have paid directly to Microchip to use this software.
*
* MICROCHIP PROVIDES THIS SOFTWARE CONDITIONALLY UPON YOUR ACCEPTANCE
* OF THESE TERMS.
*/
#ifndef DEVICEDATA_H
#define DEVICEDATA_H

#define	MAX_MEM	131072
#define	MAX_EE	1024
#define	MAX_CFG	16
#define	MAX_UID	8

#include "Device.h"

#include <QVector>
#include <QByteArray>

typedef struct
{
    unsigned int address;
    unsigned char mac[16];
} MAC;

/*!
 * Provides in-memory, PC representation of microcontroller device memory contents.
 */
class DeviceData
{
    public:
        DeviceData(Device* newDevice);
        ~DeviceData();

        void ClearAllData(void);
        void ClearProgramMemory(unsigned int memBlankVal);
        void ClearConfigWords(void);
        unsigned int* ConfigWordPointer(unsigned int wordAddress);

        void ClearUserIDs(unsigned char numIDs, int idBytes, unsigned int memBlankVal);
        void ClearEEPromMemory(unsigned int memBlankVal);
        void CopyProgramMemory(unsigned int* memory);

        /*!
         * FLASH program memory.
         */
        unsigned int ProgramMemory[MAX_MEM];
        unsigned int EEPromMemory[MAX_EE];
        unsigned int ConfigWords[MAX_CFG];
        unsigned int UserIDs[MAX_UID];
        unsigned int OSCCAL;
        unsigned int BandGap;

        QVector<QByteArray> mac;

        bool Encrypted;
        unsigned int Nonce;

    protected:
        Device* device;
};

#endif // DEVICEDATA_H
