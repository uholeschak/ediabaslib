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

#ifndef CONFIGBITSITEM_H
#define CONFIGBITSITEM_H

#include <QStandardItem>
#include "Bootload/Device.h"

class ConfigBitsItem : public QStandardItem
{
public:
    ConfigBitsItem(Device* device);
    ConfigBitsItem(Device* device, QString newText);
    ConfigBitsItem(Device* device, Device::ConfigWord* newWord, unsigned int* newWordValue);
    ConfigBitsItem(Device* device, unsigned int* newWordValue);
    ConfigBitsItem(Device* device, Device::ConfigWord* newWord, unsigned int* newWordValue, unsigned int* newWordVerify);
    ConfigBitsItem(Device* device, Device::ConfigField* newField, Device::ConfigWord* newWord, unsigned int* newWordValue, unsigned int* newWordVerify = NULL);

    QString ToText(unsigned int word);

    Device::ConfigWord* word;
    Device::ConfigField* field;
    unsigned int* wordValue;
    unsigned int* wordVerify;

protected:
    Device* device;
};

#endif // CONFIGBITSITEM_H
