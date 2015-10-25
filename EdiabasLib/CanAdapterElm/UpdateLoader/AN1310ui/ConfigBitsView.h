/************************************************************************
* Copyright (c) 2010,  Microchip Technology Inc.
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

#ifndef CONFIGBITSVIEW_H
#define CONFIGBITSVIEW_H

#include <QTreeView>
#include <QStandardItemModel>

#include "Bootload/Device.h"
#include "Bootload/DeviceData.h"

class ConfigBitsView : public QTreeView
{
Q_OBJECT
public:
    ConfigBitsView(QWidget *parent = 0);

    void setVerifyData(DeviceData* newVerifyData);
    void SetDevice(Device* newDevice, DeviceData* newDeviceData);

    bool hasVerifyData(void);
    int columnSplit;

signals:

public slots:

protected:
    QStandardItemModel model;

    Device* device;
    DeviceData* deviceData;
    DeviceData* verifyData;
};

#endif // CONFIGBITSVIEW_H
