/************************************************************************
* Copyright (c) 2009-2010,  Microchip Technology Inc.
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

#ifndef DEVICEREADER_H
#define DEVICEREADER_H

#include <QObject>
#include <QString>

#include "Device.h"
#include "Comm.h"

/*!
 * Reads microcontroller device memories into PC. While no GUI code exists in this
 * class, status update signals are emitted during read processes to allow
 * for connecting GUI displays of read progress.
 */
class DeviceReader : public QObject
{
    Q_OBJECT

public:
    DeviceReader(Device* newDevice, Comm* newComm);
    void setMaxRequest(int size);
    Comm::ErrorCode ReadFlash(unsigned int* memory, unsigned int startAddress, unsigned int endAddress);
    Comm::ErrorCode ReadFlash(unsigned int* memory, QLinkedList<Device::MemoryRange>& readList);

    Comm::ErrorCode ReadEeprom(unsigned int* memory, int startAddress, int endAddress);
    Comm::ErrorCode ReadConfig(unsigned int* memory, unsigned int startAddress, unsigned int endAddress);

    Device* device;
    Comm* comm;

signals:
    void StatusMessage(QString msg);

public slots:
    void AbortOperation(void);

protected:
    int maxReadRequest;
    bool abortOperation;

};

#endif // DEVICEREADER_H
