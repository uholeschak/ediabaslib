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
* E. Schlunder  2009/05/20  Initial code.
************************************************************************/

#include <QTextStream>

#include "DeviceVerifier.h"
#include "BootPackets.h"
#include "Crc.h"
#include "DeviceData.h"

DeviceVerifier::DeviceVerifier(Device* newDevice, Comm* newComm)
{
    device = newDevice;
    comm = newComm;
    verifyPlan = new DeviceVerifyPlanner(device);
}

DeviceVerifier::~DeviceVerifier()
{
    delete verifyPlan;
}

void DeviceVerifier::AbortOperation(void)
{
    abortOperation = true;
}


/*!
 * Verifies microcontroller FLASH program memory blocks against the expected memory contents
 * using CRC numbers calculated across FLASH Erase Block size portions of FLASH memory.
 *
 * If failure is due to microcontroller memory simply needing to be erased, the memory
 * range is added to the eraseList member. The Erase List can be executed using the
 * DeviceWriter::EraseFlash method as a final pass to correct the microcontroller memory.
 *
 * Incorrect data causes memory ranges to be added to the failList member. The failList
 * can then be used to read or re-write failed regions only.
 */
Comm::ErrorCode DeviceVerifier::VerifyFlash(unsigned int* memory, int startAddress, int endAddress)
{
    QString msg;
    QTextStream stream(&msg);
    int i, address, errors = 0, expectedBytes;
    unsigned short word1, word2;

    Comm::ErrorCode result;
    QByteArray deviceCrc, sendPacket;
    QByteArray memoryCrc;
    QByteArray emptyCrc;
    QLinkedList<Device::MemoryRange> verifyList;
    QLinkedList<Device::MemoryRange>::iterator it;
    Device::MemoryRange block;
    ReadFlashCrcPacket cmd;
    DeviceData emptyData(device);
    emptyData.ClearAllData();
    eraseList.clear();
    failList.clear();

    verifyPlan->writeConfig = writeConfig;
    verifyPlan->planFlashVerify(verifyList, startAddress, endAddress);

    it = verifyList.begin();
    while(it != verifyList.end())
    {
        qDebug("CRC verify [%X - %X)", it->start, it->end);
        if(device->family == Device::PIC32)
        {
            cmd.setAddress(it->start | 0x80000000);
        }
        else
        {
            cmd.setAddress(it->start);
        }

        // PIC18 device calculating CRC over 129024 bytes of data requires:
        // 6.812s at 2MHz
        // 13.532s at 1MHz
        // 26.984s at 500KHz
        // 107.673s at 125KHz
        // 215.346s at 62.5KHz
        // 410.611s at 32.768KHz
        //
        // Someday, we might want to break our block sizes down to accomodate more
        // GUI updates when verifying a slow running device.
        cmd.setBlocks((it->end - it->start) / device->eraseBlockSizeFLASH);
        cmd.FramePacket(sendPacket);
        result = comm->SendPacket(sendPacket);
        if(result != Comm::Success)
        {
            emit StatusMessage("CRC verify failed: " + comm->ErrorString(result));
            failed = -1;
            return result;
        }

        deviceCrc.clear();

        result = comm->GetCrcData(deviceCrc);
        if(result != Comm::ERROR_BAD_CHKSUM && result != Comm::Success)
        {
            msg.clear();
            stream << "CRC Verify Failure: "
                    << QString::number(it->start, 16).toUpper() << " - "
                    << QString::number(it->end, 16).toUpper() << "h (" << comm->ErrorString(result) << ")";
            emit StatusMessage(msg);
            failed = -1;
            return result;
        }

        expectedBytes = (it->end - it->start) / device->eraseBlockSizeFLASH * 2;
        if(deviceCrc.size() != expectedBytes)
        {
            qWarning("Received %d bytes but expected %d", deviceCrc.size(), expectedBytes);
            failed = -1;
        }

        msg.clear();
        stream.setIntegerBase(16);
        stream.setNumberFlags(QTextStream::UppercaseDigits);

        // now we need to compute CRC's against the HEX file data we have in memory.
        memoryCrc.clear();
        emptyCrc.clear();
        CalculateCrc(memory, memoryCrc, it->start, it->end, &deviceCrc);
        CalculateCrc(emptyData.ProgramMemory, emptyCrc, it->start, it->end, &deviceCrc);

        i = 0;
        while(i < memoryCrc.count() && i < deviceCrc.count())
        {
            address = (it->start + (i / 2 * device->eraseBlockSizeFLASH));
            if((deviceCrc[i] != memoryCrc[i]) || (deviceCrc[i+1] != memoryCrc[i+1]))
            {
                block.start = address;
                block.end = address + device->eraseBlockSizeFLASH;

                word1 = memoryCrc[i+1];
                word1 <<= 8;
                word1 |=  memoryCrc[i];
                word2 = deviceCrc[i+1];
                word2 <<= 8;
                word2 |= deviceCrc[i];

                if(memoryCrc[i] == emptyCrc[i] && memoryCrc[i+1] == emptyCrc[i+1])
                {
                    eraseList.append(block);
                    qDebug("Expected %X but got %X for CRC on block [%X - %X) - Erase Needed", word1, word2, block.start, block.end);
                }
                else
                {
                    failList.append(block);
                    qDebug("Expected %X but got %X for CRC on block [%X - %X) - Re-write Needed", word1, word2, block.start, block.end);
                }

                errors++;

                msg.clear();
                stream << "Verify failed in address range " << block.start << " - " << block.end;
                emit StatusMessage(msg);
            }
            i += 2;
        }

        it++;
    }

    if(errors)
    {
        return Comm::ERROR_BAD_CHKSUM;
    }
    else
    {
        qWarning("CRC verify passed");
    }

    return Comm::Success;
}

void DeviceVerifier::CalculateCrc(unsigned int* memory, QByteArray& result, unsigned int startAddress, unsigned int endAddress, QByteArray* deviceCrc)
{
    unsigned int word;
    unsigned int* ptr = device->flashPointer(startAddress, memory);
    unsigned int i, j;

    i = startAddress;
    j = 0;
    Crc crc;
    while(i < endAddress)
    {
        word = *ptr++ & device->flashWordMask;
        crc.Add(word & 0xFF);
        crc.Add((word >> 8) & 0xFF);
        if(device->family == Device::PIC24)
        {
            crc.Add((word >> 16) & 0xFF);
        }
        else if(device->family == Device::PIC32)
        {
            crc.Add((word >> 16) & 0xFF);
            crc.Add((word >> 24) & 0xFF);
        }

        device->IncrementFlashAddressByInstructionWord(i);

        if((i % device->eraseBlockSizeFLASH) == 0)
        {
            result.append(crc.LSB());
            result.append(crc.MSB());

            if(deviceCrc != NULL)
            {
                word = (*deviceCrc)[j++] & 0xFF;
                word |= ((*deviceCrc)[j++] & 0xFF) << 8;
                crc = Crc(word);
            }
        }
    }
}
