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

#ifndef BOOTLOAD_H
#define BOOTLOAD_H

#include <QObject>

#include "Bootload/Comm.h"
#include "Bootload/DeviceData.h"
#include "Bootload/Device.h"
#include "Bootload/DeviceWritePlanner.h"
#include "Bootload/DeviceVerifyPlanner.h"
#include "Bootload/DeviceReader.h"
#include "Bootload/DeviceWriter.h"
#include "Bootload/DeviceVerifier.h"

class Bootload : public QObject
{
    Q_OBJECT

public:
    Bootload();

    void SetPort(QString portName);
    void SetBaudRate(QString baudRate);

    int Connect(void);
    void Disconnect(void);
    int LoadFile(QString fileName);

    QString SelectDevice(unsigned int deviceId, Device::Families familyId);

    int EraseDevice(void);
    int WriteDevice(void);
    int VerifyDevice(bool verifyFlash = true);
    int RunApplication(void);
    int AssertBreak(void);

    bool abortOperation;

    bool writeFlash;
    bool writeEeprom;
    bool writeConfig;

public slots:
    void PrintMessage(const QString& msg);

protected:
    Comm* comm;
    DeviceData* deviceData;
    DeviceData* hexData;
    DeviceData* verifyData;
    Device* device;
    DeviceWritePlanner* writePlan;
    DeviceVerifyPlanner* verifyPlan;
    DeviceReader* deviceReader;
    DeviceWriter* deviceWriter;
    DeviceVerifier* deviceVerifier;

    int WriteDevice(DeviceData* newData, DeviceData* existingData = NULL);
    Comm::ErrorCode RemapInterruptVectors(Device* device, DeviceData* deviceData);
    void countFlashVerifyFailures(int& flashFails, unsigned int& failAddress, Device::MemoryRange range);

private:
    int failed;

};

#endif // BOOTLOAD_H
