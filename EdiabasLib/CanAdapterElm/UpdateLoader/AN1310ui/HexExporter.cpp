/************************************************************************
* Copyright (c) 20010-2011,  Microchip Technology Inc.
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
* E. Schlunder  2010/10/25  Initial code.
************************************************************************/

#include <iostream>

#include <QByteArray>
#include <QTextStream>

#include "HexExporter.h"

using namespace std;

HexExporter::HexExporter()
{
}

HexExporter::~HexExporter()
{
    if(file.isOpen())
    {
        Close();
    }
}

void HexExporter::Close(void)
{
    // Write the end of file record and close the file.
    QByteArray msg;
    msg.append((char)0x00);
    msg.append((char)0x00);
    msg.append((char)0x01);   // end of file record
    xmit(msg);
    file.close();
}

void HexExporter::Export(DeviceData* data, Device* device)
{
    QList<Device::MemoryRange> ranges;
    ranges = GenerateExportRanges(data, device);
    ExportFlash(data, device, ranges);
    ExportEeprom(data, device, ranges);
    ExportConfig(data, device, ranges);
}

HexExporter::ErrorCode HexExporter::Open(QString fileName)
{
    ErrorCode result = Success;

    file.setFileName(fileName);
    file.open(QIODevice::WriteOnly);

    extendedAddress = 0xFFFFFFFF;

    return result;
}

QList<Device::MemoryRange> HexExporter::GenerateExportRanges(DeviceData* data, Device* device)
{
    Device::MemoryRange range;
    QList<Device::MemoryRange> exportRanges;

/*    range.start = device->startFLASH;
    range.end = device->endFLASH;
    exportRanges.clear();
    exportRanges.append(range);
    return exportRanges;
*/
    exportRanges.clear();
    unsigned int i = device->startFLASH, word;
    while(i < device->endFLASH)
    {
        while(i < device->endFLASH)
        {
            word = *(device->flashPointer(i, data->ProgramMemory));
            if((word & device->flashWordMask) != device->flashWordMask)
            {
                break;
            }
            device->IncrementFlashAddressByInstructionWord(i);
        }
        range.start = i;

        while(i < device->endFLASH)
        {
            word = *(device->flashPointer(i, data->ProgramMemory));
            if((word & device->flashWordMask) == device->flashWordMask)
            {
                break;
            }
            device->IncrementFlashAddressByInstructionWord(i);
        }
        range.end = i;
        exportRanges.append(range);
    }

    qDebug("Export Ranges:");
    foreach(range, exportRanges)
    {
        qDebug("[%X, %X)", range.start, range.end);
    }

    return exportRanges;
}

void HexExporter::ExportEeprom(DeviceData* data, Device* device, QList<Device::MemoryRange> ranges)
{
    unsigned int address;
    unsigned int word;
    unsigned int* ptr;
    int i, j;
    QString printString;

    QByteArray msg;
    Device::MemoryRange range;
    range.start = 0;
    range.end = 0;

    for(int r = 0; r < ranges.length(); r++)
    {
        if(ranges[r].start < range.end)
        {
            range.start = range.end;
            range.end = ranges[r].end;
        }
        else
        {
            range = ranges[r];
        }
/*        // align block to write flash block boundaries.
        range.start -= (range.start % device->writeBlockSizeFLASH);
        if(range.end % device->writeBlockSizeFLASH)
        {
            range.end += device->writeBlockSizeFLASH - (range.end % device->writeBlockSizeFLASH);
        }
*/
        address = range.start;
        if(range.start < device->startEEPROM || range.start >= device->endEEPROM ||
           range.end <= device->startEEPROM || range.end > device->endEEPROM)
        {
            // ABORT: this address range is beyond the flash memory
            continue;
        }
        ptr = device->flashPointer(address, data->ProgramMemory);
        while(address < range.end)
        {
            if((address >> 16) != extendedAddress)
            {
                xmitExtendedAddress(address);
                extendedAddress = address >> 16;
            }

            msg.clear();
            msg.append((char)((address >> 8) & 0xFF));
            msg.append((char)(address & 0xFF));
            msg.append((char)0x00);   // Data Record type
            for(i = 0; i < 16 / device->bytesPerWordFLASH; i++)
            {
                word = *ptr++;
                for(j = 0; j < device->bytesPerWordFLASH; j++)
                {
                    msg.append(word & 0xFF);
                    word >>= 8;
                }
                device->IncrementFlashAddressByInstructionWord(address);

                if((address >> 16) != extendedAddress)
                {
                    break;
                }

                if(address >= range.end)
                {
                    break;
                }
            }
            xmit(msg);
        }
    }
}

void HexExporter::ExportConfig(DeviceData* data, Device* device, QList<Device::MemoryRange> ranges)
{
}

void HexExporter::ExportFlash(DeviceData* data, Device* device, QList<Device::MemoryRange> ranges)
{
    unsigned int address;
    unsigned int word;
    unsigned int* ptr;
    int i, j;
    QString printString;

    QByteArray msg;
    Device::MemoryRange range;
    range.start = 0;
    range.end = 0;

    for(int r = 0; r < ranges.length(); r++)
    {
        if(ranges[r].start < range.end)
        {
            range.start = range.end;
            range.end = ranges[r].end;
        }
        else
        {
            range = ranges[r];
        }
/*        // align block to write flash block boundaries.
        range.start -= (range.start % device->writeBlockSizeFLASH);
        if(range.end % device->writeBlockSizeFLASH)
        {
            range.end += device->writeBlockSizeFLASH - (range.end % device->writeBlockSizeFLASH);
        }
*/
        address = range.start;
        if(range.start < device->startFLASH || range.start >= device->endFLASH ||
           range.end <= device->startFLASH || range.end > device->endFLASH)
        {
            // ABORT: this address is beyond the flash memory
            continue;
        }
        ptr = device->flashPointer(address, data->ProgramMemory);
        while(address < range.end)
        {
            if((address >> 16) != extendedAddress)
            {
                xmitExtendedAddress(address);
                extendedAddress = address >> 16;
            }

            msg.clear();
            msg.append((char)((address >> 8) & 0xFF));
            msg.append((char)(address & 0xFF));
            msg.append((char)0x00);   // Data Record type
            for(i = 0; i < 16 / device->bytesPerWordFLASH; i++)
            {
                word = *ptr++;
                for(j = 0; j < device->bytesPerWordFLASH; j++)
                {
                    msg.append(word & 0xFF);
                    word >>= 8;
                }
                device->IncrementFlashAddressByInstructionWord(address);

                if((address >> 16) != extendedAddress)
                {
                    break;
                }

                if(address >= range.end)
                {
                    break;
                }
            }
            xmit(msg);
        }
    }
}

void HexExporter::xmitExtendedAddress(unsigned int address)
{
    QByteArray msg;

    msg.append((char)0x00);   // address (already 0000 for Extended Linear Address Record)
    msg.append((char)0x00);
    msg.append((char)0x04);   // Extended Linear Address Record type
    msg.append((address >> 24) & 0xFF);
    msg.append((address >> 16) & 0xFF);

    xmit(msg);
}

void HexExporter::xmit(const QByteArray& data)
{
    QByteArray msg;
    QTextStream stream(&msg);
    unsigned char checksum;

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);
    stream.setPadChar('0');
    stream.setFieldAlignment(QTextStream::AlignRight);

    stream << ":";
    stream.setFieldWidth(2);
    stream << (data.length() - 3);
    checksum = data.length() - 3;

    for(int i = 0; i < data.length(); i++)
    {
        stream << (unsigned char)data[i];
        checksum += (unsigned char)data[i];
    }

    checksum = 0x01 + ~(checksum);
    stream << checksum;
    stream.setFieldWidth(2);
    stream << "\r\n" << flush;
    file.write(msg);
}
