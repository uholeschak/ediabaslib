/************************************************************************
* Copyright (c) 2010-2011,  Microchip Technology Inc.
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
*
************************************************************************/

#ifndef HEXEXPORTER_H
#define HEXEXPORTER_H

#include <QFile>

#include "Bootload/Device.h"
#include "Bootload/DeviceData.h"

class HexExporter
{
public:
    HexExporter();
    ~HexExporter();

    enum ErrorCode { Success = 0, CouldNotOpenFile };

    ErrorCode Open(QString fileName);
    void Export(DeviceData* data, Device* device);
    void Close(void);

    QList<Device::MemoryRange> GenerateExportRanges(DeviceData* data, Device* device);
    void ExportFlash(DeviceData* data, Device* device, QList<Device::MemoryRange> ranges);
    void ExportEeprom(DeviceData* data, Device* device, QList<Device::MemoryRange> ranges);
    void ExportConfig(DeviceData* data, Device* device, QList<Device::MemoryRange> ranges);

    void xmitExtendedAddress(unsigned int address);
    void xmit(const QByteArray& data);
    
protected:
    QFile file;

    unsigned int extendedAddress;
};

#endif // HEXEXPORTER_H
