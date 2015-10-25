/************************************************************************
* Copyright (c) 2009,  Microchip Technology Inc.
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

#ifndef DEVICEWRITER_H
#define DEVICEWRITER_H

#include <QObject>

#include "Comm.h"
#include "Device.h"
#include "DeviceData.h"
#include "DeviceWritePlanner.h"

/*!
 * Executes device Erase/Write operations via serial port. This
 * class does not include GUI code, however, it does emit status message
 * signals during the process which can be connected to display on a GUI.
 */
class DeviceWriter : public QObject
{
    Q_OBJECT

public:
    Comm* comm;
    Device* device;
    DeviceWritePlanner* writePlan;

    bool writeConfig;

    DeviceWriter(Device* newDevice, Comm* newComm);
    ~DeviceWriter();

    Comm::ErrorCode EraseFlash(int startAddress, int endAddress);
    Comm::ErrorCode EraseFlash(QLinkedList<Device::MemoryRange> eraseList);
    Comm::ErrorCode WriteFlash(DeviceData* deviceData, unsigned int startAddress, unsigned int endAddress, unsigned int* existingMemory = NULL);
    Comm::ErrorCode WriteFlashMemory(unsigned int* memory, unsigned int startAddress, unsigned int endAddress, QVector<QByteArray>* macData = NULL);
    Comm::ErrorCode WriteEeprom(unsigned int* memory, unsigned int startAddress, unsigned int endAddress);
    Comm::ErrorCode WriteConfigFuses(unsigned int* memory);
    Comm::ErrorCode WriteConfigMemory(unsigned int* memory);

signals:
     void StatusMessage(QString msg);

public slots:
    void AbortOperation(void);

protected:
    bool abortOperation;

};

#endif // DEVICEWRITER_H
