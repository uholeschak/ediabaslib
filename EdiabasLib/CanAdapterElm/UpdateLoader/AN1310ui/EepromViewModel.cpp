/************************************************************************
* Copyright (c) 2009-2011,  Microchip Technology Inc.
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
* E. Schlunder  2009/04/29  Initial code.
************************************************************************/

#include <ctype.h>
#include <QTextStream>
#include <QSize>
#include <QColor>

#include "EepromViewModel.h"

EepromViewModel::EepromViewModel(Device* newDevice, DeviceData* newDeviceData,
                           QObject *parent) : QAbstractTableModel(parent)
{
    device = newDevice;
    deviceData = newDeviceData;
    verifyData = NULL;
}

bool EepromViewModel::hasVerifyData(void)
{
    return(verifyData != NULL);
}

void EepromViewModel::setVerifyData(DeviceData* data)
{
    verifyData = data;
}

int EepromViewModel::rowCount(const QModelIndex &parent) const
{
    int bytes = device->endEEPROM - device->startEEPROM;
    int bytesPerRow = 0x10;
    int rows;

    if(device->endEEPROM == 0 && device->startEEPROM == 0)
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

int EepromViewModel::columnCount(const QModelIndex &parent) const
{
    if(device->endEEPROM == 0 && device->startEEPROM == 0)
    {
        return 0;
    }

    if(device->family == Device::PIC24)
    {
        return 9;
    }
    else
    {
        return 17;  // PIC16/PIC18
    }
}

QVariant EepromViewModel::data(const QModelIndex &index, int role) const
{
    QString data;
    QTextStream out(&data);

    if(verifyData != NULL)
    {
        if(role == Qt::ForegroundRole)
        {
            if(index.column() == columnCount() - 1)
            {
                // maybe add some verification coloring here for ASCII column
            }
            else
            {
                int address = index.row() * 16 + index.column();
                unsigned int word = deviceData->EEPromMemory[address];
                unsigned int suspectWord = verifyData->EEPromMemory[address];
                if((word & 0xFF) != (suspectWord & 0xFF))
                {
                    return QColor(Qt::red);
                }
            }
        }
        else if(role == Qt::ToolTipRole)
        {
            int address = index.row() * 16 + index.column();
            unsigned int word = deviceData->EEPromMemory[address];
            unsigned int suspectWord = verifyData->EEPromMemory[address];
            if((word & 0xFF) != (suspectWord & 0xFF))
            {
                out.setIntegerBase(16);
                out.setNumberFlags(QTextStream::UppercaseDigits);
                out.setPadChar('0');
                out.setFieldAlignment(QTextStream::AlignRight);
                out.setFieldWidth(2);
                out << "Verify read " << (suspectWord & 0xFF);
                return data;
            }
        }
    }

    if (!index.isValid() || role != Qt::DisplayRole)
    {
        return QVariant();
    }

    if(index.column() == columnCount() - 1) // ASCII column
    {
        out.setFieldWidth(0);
        unsigned int address = index.row() * 16;
        unsigned int* memory = device->eepromPointer(address, deviceData->EEPromMemory);
        unsigned int word;
        unsigned int byte;

        if(device->family == Device::PIC24)
        {
            for(int i = 0; i < 8 && address < device->endEEPROM; i++)
            {
                word = *memory++;
                byte = word & 0xFF;
                if(isprint(byte))
                {
                    out << QString(byte);
                }
                else
                {
                    out << '.';
                }

                byte = (word & 0xFF00) >> 8;
                if(isprint(byte))
                {
                    out << QString(byte);
                }
                else
                {
                    out << '.';
                }

                address += 2;

                if(address >= device->endEEPROM)
                {
                    break;
                }

                if(i == 3)
                {
                    out << ' ';
                }
            }
        }
        else
        {
            for(int i = 0; i < 16 && address < device->endEEPROM; i++)
            {
                word = *memory++;
                byte = word & 0xFF;
                if(isprint(byte))
                {
                    out << QString(byte);
                }
                else
                {
                    out << '.';
                }

                address++;
                if(address >= device->endEEPROM)
                {
                    break;
                }

                if(i == 7)
                {
                    out << ' ';
                }
            }
        }

        return data;
    }

    unsigned int address = index.row() * 16 + index.column();
    if(device->family == Device::PIC24)
    {
        address += index.column();
    }

    unsigned int word;
    unsigned int *memory = device->eepromPointer(address, deviceData->EEPromMemory);
    word = *memory;

    if(address >= device->endEEPROM)
    {
        return QVariant();
    }

    out.setIntegerBase(16);
    out.setNumberFlags(QTextStream::UppercaseDigits);
    out.setPadChar('0');
    out.setFieldAlignment(QTextStream::AlignRight);

    if(device->family == Device::PIC24)
    {
        out.setFieldWidth(4);
        out << (word & 0xFFFF);
    }
    else
    {
        out.setFieldWidth(2);
        out << (word & 0xFF);
    }

    return data;
}

QVariant EepromViewModel::headerData(int section, Qt::Orientation orientation, int role) const
{

    QString data;
    QTextStream out(&data);

    if(orientation == Qt::Vertical)
    {
        if (role == Qt::SizeHintRole)
        {
            return QSize(50, 0);
        }

        if (role == Qt::TextAlignmentRole)
        {
            return Qt::AlignRight;
        }

        if (role == Qt::DisplayRole)
        {
            out.setIntegerBase(16);
            out.setNumberFlags(QTextStream::UppercaseDigits);
            out << (section * 0x10);

            return data;
        }
    }
    if (role == Qt::SizeHintRole)
    {
        return QSize(1, 22);
    }
    else if (role == Qt::DisplayRole)
    {
        if(section == columnCount() - 1)
        {
            return "ASCII";
        }

        data.clear();
        out.setIntegerBase(16);
        out.setPadChar('0');
        out.setFieldAlignment(QTextStream::AlignRight);
        out.setFieldWidth(2);
        out.setNumberFlags(QTextStream::UppercaseDigits);
        if(device->family == Device::PIC24)
        {
            out << section * 2;
        }
        else
        {
            out << section;
        }

        return data;
    }
    return QVariant();
}
