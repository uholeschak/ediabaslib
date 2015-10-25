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
* E. Schlunder  2009/05/09  Separating device read code from GUI.
************************************************************************/

#include <QCoreApplication>
#include <QTextStream>

#include "DeviceReader.h"
#include "BootPackets.h"

/*!
 * @param newDevice The currently connected microcontroller device parameters (memory sizes, etc).
 * @param newComm The COM port access object to use for communicating to the microcontroller
 *  bootloader firmware.
 */
DeviceReader::DeviceReader(Device* newDevice, Comm* newComm)
{
    device = newDevice;
    comm = newComm;
    maxReadRequest = 0x8000;
}

/*!
 * Call this function to abort an in-progress read operation.
 */
void DeviceReader::AbortOperation(void)
{
    abortOperation = true;
}

/*!
 * Sets the maximum size of data to request from the microcontroller during each read transaction.
 * Setting this too high for a low speed baud rate can cause the application to appear unresponsive
 * as the status message signals are only emitted at the end of each data packet.
 */
void DeviceReader::setMaxRequest(int size)
{
    if(device->family == Device::PIC32)
    {
        if(size > 65532 * 4)
        {
            maxReadRequest = 65532 * 4;         // must not exceed 16-bit block count, must be even number
        }
        else
        {
            maxReadRequest = (size / 4) * 4;
        }
    }
    else if(device->family == Device::PIC24)
    {
        if(size > 65532 * 3)
        {
            maxReadRequest = 65532 * 3;         // must not exceed 16-bit block count
        }
        else
        {
            maxReadRequest = (size / 3) * 3;    // must be multiple of 3 bytes
        }
    }
    else
    {
        if(size > 65534)
        {
            maxReadRequest = 65534;             // must not exceed 16-bit block count, must be even number
        }
        else
        {
            maxReadRequest = size;
        }
    }
}

/*!
 * Reads regions of FLASH memory into the memory buffer provided.
 */
Comm::ErrorCode DeviceReader::ReadFlash(unsigned int* memory, QLinkedList<Device::MemoryRange>& readList)
{
    Comm::ErrorCode result = Comm::Success;
    QLinkedList<Device::MemoryRange>::iterator it;
    unsigned int* readPointer;

    it = readList.begin();
    while(it != readList.end())
    {
        readPointer = device->flashPointer(it->start, memory);
        result = ReadFlash(readPointer, it->start, it->end);
        if(result != Comm::Success)
        {
            break;
        }
        it++;
    }

    return result;
}

/*!
 * Reads a region of FLASH memory into the memory buffer provided.
 */
Comm::ErrorCode DeviceReader::ReadFlash(unsigned int* memory, unsigned int startAddress, unsigned int endAddress)
{
    QString msg;
    QTextStream stream(&msg);
    unsigned int address;
    unsigned int word;
    unsigned int readLength, waitLength;
    Comm::ErrorCode result;
    QByteArray readData, sendPacket;
    ReadFlashPacket cmd;
    unsigned int i;

    qDebug("Reading [%X to %X) - %d bytes", startAddress, endAddress, device->FlashBytes(startAddress, endAddress));
    abortOperation = false;

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);

    address = startAddress;

    // limit the packet size to maximum read request size in bytes
    readLength = maxReadRequest;

    if(device->family == Device::PIC32)
    {
        if((endAddress - address) <= readLength)
        {
            // limit the packet size to remaining memory address space
            readLength = endAddress - address;
        }
        cmd.setAddress(address | 0x80000000);
        cmd.setBytes(readLength / device->bytesPerWordFLASH);
    }
    else if(device->family == Device::PIC24)
    {
        if(device->FlashBytes(address, endAddress) <= readLength)
        {
            // limit the packet size to remaining memory address space
            readLength = device->FlashBytes(address, endAddress);
        }
        cmd.setBytes(readLength / device->bytesPerWordFLASH);
        cmd.setAddress(address);
    }
    else
    {
        if(((endAddress - address) * device->bytesPerAddressFLASH) <= readLength)
        {
            // limit the packet size to remaining memory address space
            readLength = (endAddress - address) * device->bytesPerAddressFLASH;
        }
        cmd.setAddress(address);
        cmd.setBytes(readLength / device->bytesPerAddressFLASH);
    }
    cmd.FramePacket(sendPacket);

    stream << "Reading address: " << address;
    emit StatusMessage(msg);
    msg.clear();

//    qDebug("Reading address: %X (%d blocks, %d bytes)", address, readLength / device->bytesPerWordFLASH, readLength);
    result = comm->SendPacket(sendPacket);
    if(result)
    {
        emit StatusMessage("Read failure: " + comm->ErrorString(result));
        return result;
    }
    device->IncrementFlashAddressByBytes(address, readLength);
    waitLength = readLength;

    while(address < endAddress)
    {
        if(device->family == Device::PIC32)
        {
            if((endAddress - address) <= readLength)
            {
                // limit the packet size to remaining memory address space
                readLength = endAddress - address;
                cmd.setBytes(readLength / device->bytesPerWordFLASH);
            }
            cmd.setAddress(address | 0x80000000);
        }
        else if(device->family == Device::PIC24)
        {
            if(device->FlashBytes(address, endAddress) <= readLength)
            {
                // limit the packet size to remaining memory address space
                readLength = device->FlashBytes(address, endAddress);
                cmd.setBytes(readLength / device->bytesPerWordFLASH);
            }
            cmd.setAddress(address);
        }
        else
        {
            if(((endAddress - address) * device->bytesPerAddressFLASH) <= readLength)
            {
                // limit the packet size to remaining memory address space
                readLength = (endAddress - address) * device->bytesPerAddressFLASH;
                cmd.setBytes(readLength / device->bytesPerAddressFLASH);
            }
            cmd.setAddress(address);
        }
        cmd.FramePacket(sendPacket);

        result = comm->GetPacket(readData, comm->XferMilliseconds(waitLength * 2) + 500);
        QCoreApplication::processEvents();

        if(abortOperation)
        {
            stream << "Read aborted at: " << address;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }

        if(result != Comm::Success)
        {
            emit StatusMessage("Read failure: " + comm->ErrorString(result));
            return result;
        }

        stream << "Reading address: " << address << "h";
        emit StatusMessage(msg);
        msg.clear();

    //    qDebug("Reading address: %X (%d blocks, %d bytes)", address, readLength / device->bytesPerWordFLASH, readLength);
        result = comm->SendPacket(sendPacket);
        if(result)
        {
            emit StatusMessage("Read failure: " + comm->ErrorString(result));
            return result;
        }

        if((unsigned int)readData.count() != waitLength)
        {
            qWarning("Received %d bytes, expected %d", readData.count(), waitLength);
        }

        for(i = 0; i < waitLength;)
        {
            word  = readData[i++] & 0xFF;
            word |= (readData[i++] & 0xFF) << 8;
            if(device->family == Device::PIC24)
            {
                word |= (readData[i++] & 0xFF) << 16;
            }
            else if(device->family == Device::PIC32)
            {
                word |= (readData[i++] & 0xFF) << 16;
                word |= (readData[i++] & 0xFF) << 24;
            }
            *memory++ = word;
        }

        readData.clear();

        if(abortOperation)
        {
            stream << "Read aborted at: " << address;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }

        device->IncrementFlashAddressByBytes(address, readLength);
        waitLength = readLength;
    }
    waitLength = readLength;
    result = comm->GetPacket(readData, comm->XferMilliseconds(waitLength * 2) + 500);
    if((unsigned int)readData.count() != waitLength)
    {
        qWarning("Received %d bytes, expected %d", readData.count(), waitLength);
    }

    if(result != Comm::Success)
    {
        stream << "Read failure: " << comm->ErrorString(result);
        emit StatusMessage(msg);
        return result;
    }

    for(i = 0; i < waitLength;)
    {
        word  = readData[i++] & 0xFF;
        word |= (readData[i++] & 0xFF) << 8;
        if(device->family == Device::PIC24)
        {
            word |= (readData[i++] & 0xFF) << 16;
        }
        else if(device->family == Device::PIC32)
        {
            word |= (readData[i++] & 0xFF) << 16;
            word |= (readData[i++] & 0xFF) << 24;
        }
        *memory++ = word;
    }

    return Comm::Success;
}

/*!
 * Reads a region of EEPROM memory into the memory buffer provided. This method should not be
 * used on devices that do not provide EEPROM memory.
 */
Comm::ErrorCode DeviceReader::ReadEeprom(unsigned int* memory, int startAddress, int endAddress)
{
    QString msg;
    QTextStream stream(&msg);
    int address;
    int readLength, waitLength;
    Comm::ErrorCode result;
    QByteArray readData, sendPacket;
    int i;

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);

    address = startAddress;
    ReadEepromPacket cmd;
    if((endAddress - address) > maxReadRequest)
    {
        // limit the packet size to maximum read request size in bytes
        readLength = maxReadRequest;
    }
    else
    {
        // limit the packet size to remaining memory address space
        readLength = (endAddress - address);
    }
    cmd.setAddress(address);
    if(device->family == Device::PIC24)
    {
        // on PIC24, we need to send the number of 16-bit Words to read, not bytes
        cmd.setBytes(readLength >> 1);
    }
    else
    {
        cmd.setBytes(readLength);
    }

    cmd.FramePacket(sendPacket);
    address += readLength;

    result = comm->SendPacket(sendPacket);
    if(result)
    {
        emit StatusMessage("Read failure: " + comm->ErrorString(result));
        return result;
    }

    while(address < endAddress)
    {
        stream << "Reading address: " << (address - readLength);
        emit StatusMessage(msg);
        msg.clear();

        waitLength = readLength;
        if((endAddress - address) <= maxReadRequest)
        {
            // limit the packet size to remaining memory address space
            readLength = (endAddress - address);

            if(device->family == Device::PIC24)
            {
                // on PIC24, we need to send the number of 16-bit Words to read, not bytes
                cmd.setBytes(readLength >> 1);
            }
            else
            {
                cmd.setBytes(readLength);
            }
        }
        cmd.setAddress(address);
        cmd.FramePacket(sendPacket);
        address += readLength;

        result = comm->GetPacket(readData, comm->XferMilliseconds(waitLength * 2) + 500);
        QCoreApplication::processEvents();

        if(abortOperation)
        {
            stream << "Read aborted at: " << address;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }

        if(readData.count() != waitLength)
        {
            qWarning("Received %d bytes, expected %d", readData.count(), waitLength);
        }

        if(result != Comm::Success)
        {
            stream << "Read failure at " << (address - readLength) << ": " << comm->ErrorString(result);
            emit StatusMessage(msg);
            return result;
        }

        result = comm->SendPacket(sendPacket);
        if(result)
        {
            emit StatusMessage("Read failure: " + comm->ErrorString(result));
            return result;
        }

        for(i = 0; i < waitLength;)
        {
            if(device->family == Device::PIC24)
            {
                *memory = readData[i++] & 0xFF;
                *memory++ |= (readData[i++] & 0xFF) << 8;
            }
            else
            {
                *memory++ = readData[i++] & 0xFF;
            }
        }
        readData.clear();

        if(abortOperation)
        {
            stream << "Read aborted at: " << (address - readLength);
            emit StatusMessage(msg);
            return Comm::Aborted;
        }
    }
    waitLength = readLength;
    result = comm->GetPacket(readData, comm->XferMilliseconds(waitLength * 2) + 500);
    if(readData.count() != waitLength)
    {
        qWarning("Received %d bytes, expected %d", readData.count(), waitLength);
    }

    if(result != Comm::Success)
    {
        stream << "Read failure at " << (address - readLength) << ": " << comm->ErrorString(result);
        emit StatusMessage(msg);
        return result;
    }
    for(i = 0; i < waitLength;)
    {
        if(device->family == Device::PIC24)
        {
            *memory = readData[i++] & 0xFF;
            *memory++ |= (readData[i++] & 0xFF) << 8;
        }
        else
        {
            *memory++ = readData[i++] & 0xFF;
        }
    }

    return Comm::Success;
}

/*!
 * Reads a region of config fuse memory into the memory buffer provided. This method should not
 * be used on devices that store config bits in FLASH memory space (such as PIC18FxxJxx devices).
 *
 * @param startAddress Address of first config word to read. For example on PIC18F8722, to read
 *  CONFIG1H, pass 0x300000 as startAddress.
 */
Comm::ErrorCode DeviceReader::ReadConfig(unsigned int* memory, unsigned int startAddress, unsigned int endAddress)
{
    QString msg;
    QTextStream stream(&msg);
    unsigned int address, readAddress;
    unsigned int readLength, waitLength;
    Comm::ErrorCode result;
    QByteArray readData, sendData, sendPacket;
    unsigned int maxReadRequest = 250;
    ReadFlashPacket cmd;
    unsigned int i;
    unsigned int implementedBits;

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);

    address = startAddress;

    // limit the packet size to maximum read request size in bytes
    readLength = maxReadRequest;

    if(device->family == Device::PIC32)
    {
        if((endAddress - address) <= readLength)
        {
            // limit the packet size to remaining memory address space
            readLength = endAddress - address;
        }
        cmd.setAddress(address | 0x80000000);
        cmd.setBytes(readLength / device->bytesPerWordFLASH);
    }
    else if(device->family == Device::PIC24)
    {
        if(device->FlashBytes(address, endAddress) <= readLength)
        {
            // limit the packet size to remaining memory address space
            readLength = device->FlashBytes(address, endAddress);
        }
        cmd.setBytes(readLength / device->bytesPerWordFLASH);
        cmd.setAddress(address);
    }
    else
    {
        if(((endAddress - address) * device->bytesPerAddressFLASH) <= readLength)
        {
            // limit the packet size to remaining memory address space
            readLength = (endAddress - address) * device->bytesPerAddressFLASH;
        }
        cmd.setAddress(address);
        cmd.setBytes(readLength / device->bytesPerAddressFLASH);
    }
    cmd.FramePacket(sendPacket);

    readAddress = address;
    device->IncrementFlashAddressByBytes(address, readLength);

    result = comm->SendPacket(sendPacket);
    if(result)
    {
        emit StatusMessage("Read failure: " + comm->ErrorString(result));
        return result;
    }

    while(address < endAddress)
    {
        stream << "Reading address: " << address + device->startConfig;
        emit StatusMessage(msg);
        msg.clear();

        waitLength = readLength;
        if(device->family == Device::PIC32)
        {
            if((endAddress - address) <= readLength)
            {
                // limit the packet size to remaining memory address space
                readLength = endAddress - address;
                cmd.setBytes(readLength / device->bytesPerWordFLASH);
            }
            cmd.setAddress(address | 0x80000000);
        }
        else if(device->family == Device::PIC24)
        {
            if(device->FlashBytes(address, endAddress) <= readLength)
            {
                // limit the packet size to remaining memory address space
                readLength = device->FlashBytes(address, endAddress);
                cmd.setBytes(readLength / device->bytesPerWordFLASH);
            }
            cmd.setAddress(address);
        }
        else
        {
            if(((endAddress - address) * device->bytesPerAddressFLASH) <= readLength)
            {
                // limit the packet size to remaining memory address space
                readLength = (endAddress - address) * device->bytesPerAddressFLASH;
                cmd.setBytes(readLength / device->bytesPerAddressFLASH);
            }
            cmd.setAddress(address);
        }
        cmd.FramePacket(sendPacket);
        device->IncrementFlashAddressByBytes(address, readLength);

        result = comm->GetPacket(readData, comm->XferMilliseconds(waitLength * 2) + 500);
        QCoreApplication::processEvents();

        if(abortOperation)
        {
            stream << "Read aborted at: " << address + device->startConfig;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }

        if((unsigned)readData.count() != waitLength)
        {
            qWarning("Received %d bytes, expected %d", readData.count(), waitLength);
        }

        if(result != Comm::Success)
        {
            stream << "Read failure at " << address + device->startConfig << ": " << comm->ErrorString(result);
            emit StatusMessage(msg);
            return result;
        }

        result = comm->SendPacket(sendPacket);
        if(result)
        {
            emit StatusMessage("Read failure: " + comm->ErrorString(result));
            return result;
        }

        for(i = 0; i < waitLength;)
        {
            implementedBits = device->ConfigWordByAddress(readAddress).implementedBits;

            if(device->family == Device::PIC32)
            {
                *memory = (unsigned int)((unsigned char)readData[i++]);
                *memory |= ((unsigned int)((unsigned char)readData[i++])) << 8;
                *memory |= ((unsigned int)((unsigned char)readData[i++])) << 16;
                *memory |= ((unsigned int)((unsigned char)readData[i++])) << 24;
                //*memory &= implementedBits;
                *memory++;
                readAddress += 4;
            }
            else if(device->family == Device::PIC24)
            {
                *memory = (unsigned int)((unsigned char)readData[i++]);
                *memory |= ((unsigned int)((unsigned char)readData[i++])) << 8;
                *memory |= ((unsigned int)((unsigned char)readData[i++])) << 16;
//                *memory &= implementedBits;
                *memory++;
                readAddress += 2;
            }
            else
            {
                if(readAddress & 1)
                {
                    *memory++ |= ((readData[i++] & implementedBits) << 8) ;
                }
                else
                {
                    *memory = readData[i++] & implementedBits;
                }
                readAddress++;
            }
        }
        readData.clear();

        if(abortOperation)
        {
            stream << "Read aborted at: " << address + device->startConfig;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }
    }
    waitLength = readLength;
    result = comm->GetPacket(readData, comm->XferMilliseconds(waitLength * 2) + 500);
    if((unsigned)readData.count() != waitLength)
    {
        qWarning("Received %d bytes, expected %d", readData.count(), waitLength);
    }

    if(result != Comm::Success)
    {
        stream << "Read failure at " << address + device->startConfig << ": " << comm->ErrorString(result);
        emit StatusMessage(msg);
        return result;
    }
    for(i = 0; i < waitLength;)
    {
        implementedBits = device->ConfigWordByAddress(readAddress).implementedBits;

        if(device->family == Device::PIC32)
        {
            *memory = (unsigned int)((unsigned char)readData[i++]);
            *memory |= ((unsigned int)((unsigned char)readData[i++])) << 8;
            *memory |= ((unsigned int)((unsigned char)readData[i++])) << 16;
            *memory |= ((unsigned int)((unsigned char)readData[i++])) << 24;
            //*memory &= implementedBits;
            *memory++;
            readAddress += 4;
        }
        else if(device->family == Device::PIC24)
        {
            *memory = (unsigned int)((unsigned char)readData[i++]);
            *memory |= ((unsigned int)((unsigned char)readData[i++])) << 8;
            *memory |= ((unsigned int)((unsigned char)readData[i++])) << 16;
//            *memory &= implementedBits;
            *memory++;
            readAddress += 2;
        }
        else
        {
            if(readAddress & 1)
            {
                *memory++ |= ((readData[i++] & implementedBits) << 8) ;
            }
            else
            {
                *memory = readData[i++] & implementedBits;
            }
            readAddress++;
        }
    }

    return Comm::Success;
}

