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
*
* Author        Date        Comment
*************************************************************************
* E. Schlunder  2009/05/17  Modified to support PIC16 word addressing.
* E. Schlunder  2009/04/29  Initial code.
************************************************************************/

#include <ctype.h>
#include <QTextStream>
#include <QSize>
#include <QColor>

#include "FlashViewModel.h"

FlashViewModel::FlashViewModel(Device* newDevice, DeviceData* newDeviceData,
                           QObject *parent) : QAbstractTableModel(parent)
{
    device = newDevice;
    deviceData = newDeviceData;
    verifyData = NULL;
}

bool FlashViewModel::hasVerifyData(void)
{
    return(verifyData != NULL);
}

void FlashViewModel::setVerifyData(DeviceData* data)
{
    verifyData = data;
}

int FlashViewModel::rowCount(const QModelIndex &parent) const
{
    int bytes, bytesPerRow, rows;

    if(device->family == Device::PIC24)
    {
        bytes = (device->endFLASH - device->startFLASH) / 2 * device->bytesPerWordFLASH;
        bytesPerRow = 24;
    }
    else
    {
        bytes = (device->endFLASH - device->startFLASH) * device->bytesPerAddressFLASH;
        bytesPerRow = 0x10;
    }

    if(device->endFLASH == 0 && device->startFLASH == 0)
    {
        return 0;
    }

    rows = bytes / bytesPerRow;
    if(bytes % bytesPerRow)
    {
        rows++;
    }
    return rows;
}

int FlashViewModel::columnCount(const QModelIndex &parent) const
{
    if(device->endFLASH == 0 && device->startFLASH == 0)
    {
        return 0;
    }

    if(device->family == Device::PIC32)
    {
        return 5;
    }
    else
    {
        return 9;
    }
}

QVariant FlashViewModel::data(const QModelIndex &index, int role) const
{
    bool asciiColumn = false;
    int dataColumns = 8;
    QString data;
    QTextStream out(&data);

    unsigned int address;
    switch(device->family)
    {
        case Device::PIC16:
            address = index.row() * 8;
            if(index.column() < 8)
            {
                address += index.column();
            }
            else
            {
                asciiColumn = true;
            }
            break;

        case Device::PIC32:
            address = index.row() * 16 + device->startFLASH;
            dataColumns = 4;
            if(index.column() < 4)
            {
                address += index.column() * 4;
            }
            else
            {
                asciiColumn = true;
            }
            break;

        default:
            address = index.row() * 16;
            if(index.column() < 8)
            {
                address += index.column() * 2;
            }
            else
            {
                asciiColumn = true;
            }
    }

    if(role == Qt::BackgroundRole)
    {        
        if(address < 3 && !asciiColumn)
        {
            return QColor(Qt::cyan);
        }
        if(address == 3 && !asciiColumn && device->family != Device::PIC16)
        {
            return QColor(Qt::cyan);
        }
        if(address >= device->startBootloader - 4 && address < device->startBootloader)
        {
            return QColor(0xA0, 0xFF, 0xFF);
        }
        if(device->family == Device::PIC16 && address == device->startBootloader - 5)
        {
            return QColor(Qt::cyan);
        }
        else if(address >= device->startBootloader && address < device->endBootloader && !asciiColumn)
        {
            return QColor(Qt::cyan);
        }
        else if(device->hasConfigAsFlash() &&
                address >= device->startConfig && address < device->endConfig)
        {
            return QColor(Qt::yellow);
        }
        else
        {
            if(index.row() & 1)
            {
                return QColor(0xDD, 0xDD, 0xDD);
            }
            else
            {
                return QColor(Qt::white);
            }
        }
    }

    unsigned int* memory = device->flashPointer(address, deviceData->ProgramMemory);
    unsigned int* verifyMemory = device->flashPointer(address, verifyData->ProgramMemory);

    if(verifyData != NULL)
    {
        if(role == Qt::ForegroundRole)
        {
            if(asciiColumn)
            {
                // maybe add some verification coloring here for ASCII column
            }
            else
            {
                unsigned int word = *memory;
                unsigned int suspectWord = *verifyMemory;
                if((word & device->flashWordMask) != (suspectWord & device->flashWordMask))
                {
                    return QColor(Qt::red);
                }
            }
        }
        else if(role == Qt::ToolTipRole)
        {
            unsigned int word = *memory;
            unsigned int suspectWord = *verifyMemory;
            if((word & device->flashWordMask) != (suspectWord & device->flashWordMask))
            {
                out.setIntegerBase(16);
                out.setNumberFlags(QTextStream::UppercaseDigits);
                out.setPadChar('0');
                out.setFieldAlignment(QTextStream::AlignRight);
                out.setFieldWidth(4);
                out << "Verify read " << (suspectWord & device->flashWordMask);
                return data;
            }
        }
    }

    if (!index.isValid() || role != Qt::DisplayRole)
    {
        return QVariant();
    }

    if(asciiColumn)
    {
        out.setFieldWidth(0);
        unsigned int word;
        unsigned int byte;
        int j;
        for(int i = 0; i < dataColumns && address < device->endFLASH; i++)
        {
            word = *memory++ & device->flashWordMask;
            for(j = 0; j < device->bytesPerWordFLASH; j++)
            {
                byte = word & 0xFF;
                word >>= 8;
                if(isprint(byte))
                {
                    out << QString(byte);
                }
                else
                {
                    out << '.';
                }
            }
            device->IncrementFlashAddressByInstructionWord(address);

            if(device->family == Device::PIC32)
            {
                if(i == 1)
                {
                    out << ' ';
                }
            }
            else
            {
                if(i == 3)
                {
                    out << ' ';
                }
            }
        }
        return data;
    }

    if(address >= device->endFLASH)
    {
        return QVariant();
    }

    unsigned int word = *memory;
    out.setIntegerBase(16);
    out.setNumberFlags(QTextStream::UppercaseDigits);
    out.setPadChar('0');
    out.setFieldAlignment(QTextStream::AlignRight);
    if(device->family == Device::PIC24)
    {
        out.setFieldWidth(6);
    }
    else if(device->family == Device::PIC32)
    {
        out.setFieldWidth(8);
    }
    else
    {
        out.setFieldWidth(4);
    }
    out << (word & device->flashWordMask);

    return data;
}

QVariant FlashViewModel::headerData(int section, Qt::Orientation orientation, int role) const
{

    QString data;
    QTextStream out(&data);

    if(orientation == Qt::Vertical)
    {
        if (role == Qt::SizeHintRole)
        {
            if(device->family == Device::PIC32)
            {
                return QSize(75, 0);
            }
            else
            {
                return QSize(50, 0);
            }
        }

        if (role == Qt::TextAlignmentRole)
        {
            return Qt::AlignRight;
        }

        if (role == Qt::DisplayRole)
        {
            out.setIntegerBase(16);
            out.setNumberFlags(QTextStream::UppercaseDigits);
            if(device->family == Device::PIC24)
            {
                out << (section * 0x10) + device->startFLASH;
            }
            else
            {
                out << (section * 0x10 / device->bytesPerAddressFLASH) + device->startFLASH;
            }
            return data;
        }
    }
    if (role == Qt::SizeHintRole)
    {
        return QSize(1, 22);
    }
    else if (role == Qt::DisplayRole)
    {
        switch(device->family)
        {
            case Device::PIC16:
                switch(section)
                {
                    case 0:
                        return "0|8";
                    case 1:
                        return "1|9";
                    case 2:
                        return "2|A";
                    case 3:
                        return "3|B";
                    case 4:
                        return "4|C";
                    case 5:
                        return "5|D";
                    case 6:
                        return "6|E";
                    case 7:
                        return "7|F";
                    case 8:
                        return "ASCII";
                }
                break;

            case Device::PIC32:
                switch(section)
                {
                    case 0:
                        return "00";
                    case 1:
                        return "04";
                    case 2:
                        return "08";
                    case 3:
                        return "0C";
                    case 4:
                        return "ASCII";
                }
                break;

            default:
                switch(section)
                {
                    case 0:
                        return "00";
                    case 1:
                        return "02";
                    case 2:
                        return "04";
                    case 3:
                        return "06";
                    case 4:
                        return "08";
                    case 5:
                        return "0A";
                    case 6:
                        return "0C";
                    case 7:
                        return "0E";
                    case 8:
                        return "ASCII";
                }
        }
    }
    return QVariant();
}
