/************************************************************************
* Copyright (c) 2005-2009,  Microchip Technology Inc.
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
#ifndef IMPORTEXPORTHEX_H
#define IMPORTEXPORTHEX_H

#define MAX_LINE_LEN 80

#include <QString>
#include "DeviceData.h"
#include "Device.h"

/*!
 * Reads HEX files into an in-memory DeviceData object.
 */
class HexImporter
{
public:
    enum ErrorCode { Success = 0, CouldNotOpenFile };

    HexImporter(void);
    ~HexImporter(void);

    ErrorCode ImportHexFile(QString fileName, DeviceData* data, Device* device);
    bool importedAddress(unsigned int address);

    bool hasEndOfFileRecord;    // hex file does have an end of file record
    bool hasConfigBits;         // hex file has config bit settings
    bool fileExceedsFlash;      // hex file records exceed device memory constraints

    QList<Device::MemoryRange> ranges;
    QList<Device::MemoryRange> rawimport;

protected:
    int ParseHex(char* characters, int length);
    unsigned char computeChecksum(char* fileLine);

};

#endif // IMPORTEXPORTHEX_H
