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
*
* Author        Date        Comment
*************************************************************************
* E. Schlunder  2010/02/13  Implemented new config bits view with full
*                           bit field editing/browsing.
************************************************************************/

#include "ConfigBitsItem.h"

ConfigBitsItem::ConfigBitsItem(Device* newDevice)
{
    word = NULL;
    field = NULL;
    wordValue = NULL;
    wordVerify = NULL;
    device = newDevice;
}

ConfigBitsItem::ConfigBitsItem(Device* newDevice, QString newText)
{
    word = NULL;
    field = NULL;
    wordValue = NULL;
    wordVerify = NULL;
    device = newDevice;

    setText(newText);
}

ConfigBitsItem::ConfigBitsItem(Device* newDevice, unsigned int* newWordValue)
{
    word = NULL;
    field = NULL;
    wordValue = newWordValue;
    wordVerify = NULL;
    device = newDevice;

    setText(ToText(*wordValue));
}

QString ConfigBitsItem::ToText(unsigned int word)
{
    switch(device->family)
    {
        case Device::PIC32:
            return QString::number(word, 16).toUpper() + "h";

        case Device::PIC24:
            return QString::number(word & 0xFFFFFF, 16).toUpper() + "h";

        default:
        case Device::PIC18:
        case Device::PIC16:
            return QString::number(word & 0xFF, 16).toUpper() + "h";
    }
}

ConfigBitsItem::ConfigBitsItem(Device* newDevice, Device::ConfigWord* newWord, unsigned int* newWordValue, unsigned int* newWordVerify)
{
    word = newWord;
    field = NULL;
    wordValue = newWordValue;
    wordVerify = newWordVerify;
    device = newDevice;

    if((*wordValue & word->implementedBits) != (*wordVerify & word->implementedBits))
    {
        setForeground(QBrush(Qt::red));
        setToolTip("Verify read " + ToText(*wordVerify));
    }
    setText(ToText(*wordValue));
}

ConfigBitsItem::ConfigBitsItem(Device* newDevice, Device::ConfigWord* newWord, unsigned int* newWordValue)
{
    field = NULL;
    word = newWord;
    wordValue = newWordValue;
    wordVerify = NULL;
    device = newDevice;

    setText(word->name + " (" + QString::number(word->address, 16).toUpper() + "h)");
}

ConfigBitsItem::ConfigBitsItem(Device* newDevice, Device::ConfigField* newField, Device::ConfigWord* newWord, unsigned int* newWordValue, unsigned int* newWordVerify)
{
    field = newField;
    word = newWord;
    wordValue = newWordValue;
    wordVerify = newWordVerify;
    device = newDevice;

    if(wordVerify)
    {
        for(int i = 0; i < field->settings.count(); i++)
        {
            Device::ConfigFieldSetting* setting = &field->settings[i];
            if((*wordVerify & setting->bitMask) == setting->bitValue)
            {
                if((*wordVerify & setting->bitMask) != (*wordValue & setting->bitMask))
                {
                    setForeground(QBrush(Qt::red));
                    setToolTip("Verify read: " + field->settings[i].description);
                    break;
                }
            }
        }
    }

    for(int i = 0; i < field->settings.count(); i++)
    {
        Device::ConfigFieldSetting* setting = &field->settings[i];
        if((*wordValue & setting->bitMask) == setting->bitValue)
        {
            setText(field->settings[i].description);
            break;
        }
    }

}
