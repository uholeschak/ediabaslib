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

#ifndef DEVICEVERIFIER_H
#define DEVICEVERIFIER_H

#include <QObject>

#include "Comm.h"
#include "Device.h"
#include "DeviceVerifyPlanner.h"

/*!
 * Performs FLASH program memory verification using CRC's. While this class contains no
 * GUI code, it does emit status messages which can be connected to display on a GUI.
 */
class DeviceVerifier : public QObject
{
    Q_OBJECT

public:
    Comm* comm;
    Device* device;

    QLinkedList<Device::MemoryRange> eraseList;
    QLinkedList<Device::MemoryRange> failList;

    bool writeConfig;
    bool failed;

    DeviceVerifier(Device* newDevice, Comm* newComm);
    ~DeviceVerifier();

    Comm::ErrorCode VerifyFlash(unsigned int* memory, int startAddress, int endAddress);

signals:
     void StatusMessage(QString msg);

public slots:
    void AbortOperation(void);

protected:
    bool abortOperation;
    DeviceVerifyPlanner* verifyPlan;

    void CalculateCrc(unsigned int* memory, QByteArray& result, unsigned int startAddress, unsigned int endAddress, QByteArray* deviceCrc = NULL);
};

#endif // DEVICEVERIFIER_H
