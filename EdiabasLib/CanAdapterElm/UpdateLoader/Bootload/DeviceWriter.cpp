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
* E. Schlunder  2009/05/10  Separating device write code from GUI.
************************************************************************/

#include <QCoreApplication>
#include <QTextStream>
#include <QTime>

#include "DeviceWriter.h"
#include "DeviceReader.h"
#include "BootPackets.h"

DeviceWriter::DeviceWriter(Device* newDevice, Comm* newComm)
{
    device = newDevice;
    comm = newComm;
    writePlan = new DeviceWritePlanner(device);
}

DeviceWriter::~DeviceWriter()
{
    delete writePlan;
}

void DeviceWriter::AbortOperation(void)
{
    abortOperation = true;
}

Comm::ErrorCode DeviceWriter::EraseFlash(QLinkedList<Device::MemoryRange> eraseList)
{
    QLinkedList<Device::MemoryRange>::iterator it;
    Device::MemoryRange block;
    QString msg;
    QTextStream stream(&msg);
    int blocks;
    unsigned int address;
    Comm::ErrorCode result;

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);
    abortOperation = false;

    for(it = eraseList.begin(); it != eraseList.end();)
    {
        blocks = 0;
        block = *it++;

        // Re-consolidate consecutive erase address blocks into one request.
        while(it != eraseList.end())
        {
            address = it->start;
            if(address != block.end)
            {
                break;
            }

            block.end = it->end;
            it++;
        }

        qDebug("Erasing (%X to %X]", block.end, block.start);
        result = EraseFlash(block.start, block.end);
        if(result != Comm::Success)
        {
            return result;
        }

        if(abortOperation)
        {
            stream << "Erase aborted at: " << block.start;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }
    }

    return Comm::Success;
}

/*!
 * Erases FLASH memory on the microcontroller for the range specified. This code does
 * not contain any GUI code directly, however, it does emit status messages during the
 * process which could be connected to display on a GUI.
 *
 * On PIC16/PIC18, this method has to step through each FLASH Erase Block size region
 * in the range provided. No Bulk Erase mechanism is available for bootloader use on
 * these devices.
 *
 * On PIC32, this method may issue a Bulk Erase request if the region matches the
 * entire device FLASH program memory range. This provides extremely fast erases
 * of the entire device FLASH program memory. The Bootloader firmware is stored in
 * separate boot FLASH memory, which is unaffected by the bulk erase.
 */
// Warning: do not blindly change these address variables to "unsigned int" yet.
// This code currently breaks on PIC18/16 if you do that.
Comm::ErrorCode DeviceWriter::EraseFlash(int startAddress, int endAddress)
{
    QString msg;
    QTextStream stream(&msg);
    QByteArray sendPacket, receivePacket;
    int address;
    int i, blocks;
    Comm::ErrorCode result;
    int maxEraseBlocks = 4;
    unsigned int word;

    if(device->family == Device::PIC32)
    {
        if(startAddress == device->startFLASH && endAddress == device->endFLASH)
        {
            // simply use a bulk erase to quickly erase the entire flash memory of this device.
            BulkEraseFlashPacket cmd;
            cmd.FramePacket(sendPacket);
            receivePacket.clear();
            result = comm->SendGetPacket(sendPacket, receivePacket, 5);
            switch(result)
            {
                case Comm::Success:
                    break;

                case Comm::NoAcknowledgement:
                    stream << "Target failed acknowledgement during bulk erase commmand.";
                    emit StatusMessage(msg);
                    break;

                default:
                    stream << "Bulk Erase failure: " << comm->ErrorString(result);
                    emit StatusMessage(msg);
                    break;
            }
            return result;
        }
    }

    if(device->hasEraseFlashCommand() == false)
    {
        maxEraseBlocks = (device->maxPacketSize() - WriteFlashPacket::headerSize) / (device->eraseBlockSizeFLASH * device->bytesPerAddressFLASH);
    }

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);
    abortOperation = false;

    for(address = endAddress; address > startAddress; address -= device->eraseBlockSizeFLASH * blocks)
    {
        if(address - (signed)device->eraseBlockSizeFLASH * maxEraseBlocks <= startAddress)
        {
            blocks = (address - startAddress) / device->eraseBlockSizeFLASH;
        }
        else
        {
            blocks = maxEraseBlocks;
        }

        if(blocks)
        {
            stream << "Erasing address: " << address - device->eraseBlockSizeFLASH;
            emit StatusMessage(msg);
            msg.clear();
            QCoreApplication::processEvents();

            if(device->hasEraseFlashCommand())
            {
                EraseFlashPacket cmd;
                cmd.setAddress(address - device->eraseBlockSizeFLASH);
                cmd.setBlocks(blocks);
                cmd.FramePacket(sendPacket);
            }
            else
            {
                WriteFlashPacket cmd;
                i = address - device->eraseBlockSizeFLASH * blocks;
                cmd.setAddress(i);
                cmd.setBlocks(device->eraseBlockSizeFLASH * blocks / device->writeBlockSizeFLASH);
                word = device->blankValue & device->flashWordMask;
                while(i < address)
                {
                    cmd.append(word & 0xFF);
                    cmd.append((word >> 8) & 0xFF);
                    switch(device->bytesPerAddressFLASH)
                    {
                        case 2:
                            i++;
                            break;

                        default:
                        case 1:
                            i += 2;
                            break;
                    }
                }
                cmd.FramePacket(sendPacket);
            }
            receivePacket.clear();
            result = comm->SendGetPacket(sendPacket, receivePacket, 5);
            switch(result)
            {
                case Comm::Success:
                    break;

                case Comm::NoAcknowledgement:
                    stream << "Target failed acknowledgement at " << address << ".";
                    emit StatusMessage(msg);
                    return result;

                default:
                    stream << "Erase failure at " << address << ": " << comm->ErrorString(result);
                    emit StatusMessage(msg);
                    return result;
            }
        }

        if(abortOperation)
        {
            stream << "Erase aborted at: " << address;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }
    }

    return Comm::Success;
}

Comm::ErrorCode DeviceWriter::WriteFlash(DeviceData* deviceData, unsigned int startAddress, unsigned int endAddress, unsigned int* existingMemory)
{
    QString msg;
    QTextStream stream(&msg);
    int bytesPerWriteBlock = device->writeBlockSizeFLASH;
    Comm::ErrorCode result;
    QByteArray writeData;
    QByteArray sendPacket, receivePacket;
    unsigned int* flashMemory;
    QLinkedList<Device::MemoryRange> writeList, eraseList;

    abortOperation = false;

    writePlan->writeConfig = writeConfig;
    writePlan->planFlashWrite(eraseList, writeList, startAddress, endAddress, deviceData->ProgramMemory, existingMemory);

    if(device->hasEncryption())
    {
        // each write block must be followed by 16 bytes of message authentication code (MAC) data
        bytesPerWriteBlock += 16;
    }

    if(writeList.isEmpty() && eraseList.isEmpty())
    {
        // nothing to do, we're done.
        return Comm::Success;
    }

    QLinkedList<Device::MemoryRange>::iterator it;
    qDebug("Write Plan:");
    for(it = eraseList.begin(); it != eraseList.end(); ++it)
    {
        qDebug("  Erase (%X - %X]", it->end, it->start);
    }

    for(it = writeList.begin(); it != writeList.end(); ++it)
    {
        qDebug("  Write [%X - %X) (%d bytes)", it->start, it->end, device->FlashBytes(it->start, it->end));
    }

    if(device->hasEraseFlashCommand())
    {
        // erase prior to writing FLASH memory
        it = eraseList.begin();
        while(it != eraseList.end())
        {
            result = EraseFlash(it->start, it->end);
            if(result != Comm::Success)
            {
                return result;
            }
            it++;
        }
    }

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);

    it = writeList.begin();
    while(it != writeList.end())
    {
        flashMemory = device->flashPointer(it->start, deviceData->ProgramMemory);

        if(abortOperation)
        {
            stream << "Write aborted at: " << it->start;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }

        result = WriteFlashMemory(flashMemory, it->start, it->end, &deviceData->mac);
        switch(result)
        {
            case Comm::Success:
                break;

            case Comm::NoAcknowledgement:
                stream << "Target failed acknowledgement at " << it->start << ".";
                emit StatusMessage(msg);
                return result;

            default:
                stream << "Write failure at " << it->start << ": " << comm->ErrorString(result);
                emit StatusMessage(msg);
                return result;
        }

        it++;
    }

    return Comm::Success;
}

Comm::ErrorCode DeviceWriter::WriteFlashMemory(unsigned int* memory, unsigned int startAddress, unsigned int endAddress, QVector<QByteArray>* macData)
{
    QString msg;
    QTextStream stream(&msg);
    unsigned int word;
    unsigned int j;
    Comm::ErrorCode result;
    QByteArray writeData;
    QByteArray sendPacket, receivePacket;
    QLinkedList<Device::MemoryRange> writeList, eraseList;
    int bytesPerWriteBlock = device->writeBlockSizeFLASH;

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);

    WriteFlashPacket cmd;
    cmd.setAddress(startAddress);
    if(device->family == Device::PIC32)
    {
        // need to word align the data payload for PIC32 by stuffing two dummy bytes
        // at the beginning of each Write Flash packet.
        cmd.append((char)0x00);
        cmd.append((char)0x00);
    }

    for(j = startAddress; j < endAddress;)
    {
        word = *memory++;
        cmd.append(word & 0xFF);
        cmd.append((word >> 8) & 0xFF);
        switch(device->family)
        {
            case Device::PIC32:
                cmd.append(word >> 16 & 0xFF);
                cmd.append((word >> 24) & 0xFF);
                break;

            case Device::PIC24:
                cmd.append((word >> 16) & 0xFF);
                break;

            default:
                break;
        }
        device->IncrementFlashAddressByInstructionWord(j);
        if(device->hasEncryption())
        {
            if(j % device->writeBlockSizeFLASH == 0)
            {
                // end of write block -- append message authentication code (MAC) data
                cmd.append((*macData)[(j / device->writeBlockSizeFLASH) - 1]);
            }
        }
    }

    if(device->family == Device::PIC24)
    {
        cmd.setBlocks((cmd.payloadSize() / device->bytesPerWordFLASH * device->bytesPerAddressFLASH) / bytesPerWriteBlock);
    }
    else
    {
        cmd.setBlocks(cmd.payloadSize() / (bytesPerWriteBlock * device->bytesPerAddressFLASH));
    }
    cmd.FramePacket(sendPacket);

    if(abortOperation)
    {
        stream << "Write aborted at: " << startAddress;
        emit StatusMessage(msg);
        return Comm::Aborted;
    }

    qDebug("Writing [%X - %X) with %d bytes (%d blocks)", startAddress, endAddress,
        cmd.payloadSize(), cmd.blocks());
    stream << "Writing address: " << startAddress;
    emit StatusMessage(msg);
    msg.clear();

    result = comm->SendPacket(sendPacket);
    QCoreApplication::processEvents();
    if(result == Comm::Success)
    {
        receivePacket.clear();

        // At really slow baud rates (19.2Kbps and below), writing an entire
        // write packet to the device might take a really long time, so we
        // don't want to timeout immediately when we don't get an immediate
        // response from the device.
        QTime elapsed;
        elapsed.start();
        while(comm->serial->bytesAvailable() < 2)
        {
            if(elapsed.elapsed() > 1500)
            {
                break;
            }
        }

        result = comm->GetPacket(receivePacket);
    }

    return result;
}

Comm::ErrorCode DeviceWriter::WriteEeprom(unsigned int* memory, unsigned int startAddress, unsigned int endAddress)
{
    QString msg;
    QTextStream stream(&msg);
    unsigned int address, writeAddress;
    int i;
    unsigned int* eepromMemory;

    Comm::ErrorCode result;
    QByteArray sendPacket, receivePacket;
    int maxWrite = device->maxPacketSize() - WriteEepromPacket::headerSize - WriteEepromPacket::footerSize;

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);
    abortOperation = false;

    qDebug("Writing %X to %X (%d bytes)", startAddress, endAddress, endAddress - startAddress);
    qDebug("maxWrite: %d", maxWrite);

    for(address = startAddress; address < endAddress;)
    {
        stream << "Writing address: " << address;
        emit StatusMessage(msg);
        msg.clear();
        QCoreApplication::processEvents();

        if(abortOperation)
        {
            stream << "Write aborted at: " << address;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }

        writeAddress = address;
        eepromMemory = &memory[(address - device->startEEPROM) / device->bytesPerWordEEPROM];

        WriteEepromPacket cmd;
        cmd.setAddress(address);
        for(i = 0; i < maxWrite; i++)
        {
            cmd.append((char)(*eepromMemory++) & 0xFF);
            address++;
            if(address > endAddress)
            {
                break;
            }
        }
        cmd.setBytes(i);
        cmd.FramePacket(sendPacket);

        qDebug("Writing %X with %d bytes", writeAddress, i);
        QCoreApplication::processEvents();
        receivePacket.clear();
        result = comm->SendGetPacket(sendPacket, receivePacket, 1, i * 8);
        switch(result)
        {
            case Comm::Success:
                break;

            case Comm::NoAcknowledgement:
                stream << "Target failed acknowledgement at " << writeAddress << ".";
                emit StatusMessage(msg);
                return result;

            default:
                stream << "Write failure at " << writeAddress << ": " << comm->ErrorString(result);
                emit StatusMessage(msg);
                return result;
        }
    }

    return Comm::Success;
}

/**
 * Writes PIC32 config memory while preserving contents of FLASH memory sharing the same
 * erase block page as the config memory.
 */
Comm::ErrorCode DeviceWriter::WriteConfigMemory(unsigned int* memory)
{
    QString msg;
    QTextStream stream(&msg);
    unsigned int startBlock, writeAddress;
    Comm::ErrorCode result;
    QByteArray sendPacket, receivePacket;
    unsigned int preserveBlock[device->eraseBlockSizeFLASH / 4];

    DeviceReader* deviceReader = new DeviceReader(device, comm);

    startBlock = device->endConfig - device->eraseBlockSizeFLASH;
    // Read pre-existing config memory erase block page contents
    result = deviceReader->ReadFlash(preserveBlock, startBlock, device->endConfig);
    delete deviceReader;
    switch(result)
    {
        case Comm::Success:
            break;

        default:
            stream << "Could not read config words page at " << startBlock << ": " << comm->ErrorString(result);
            emit StatusMessage(msg);
            return result;
    }

    // Compare new config bits to existing device config bits and abort if already matched.
    if(memcmp(&preserveBlock[(device->startConfig - startBlock) / 4],
                memory, device->endConfig - device->startConfig) == 0)
    {
        // ABORT: config bits are already correct, no need to erase and re-write
        return Comm::Success;
    }

    // Merge new config data with preserve memory.
    memcpy(&preserveBlock[(device->startConfig - startBlock) / 4],
                memory, device->endConfig - device->startConfig);

    // Erase config memory erase block.
    result = EraseFlash(startBlock, device->endConfig);
    switch(result)
    {
        case Comm::Success:
            break;

        default:
            stream << "Could not erase config words page at " << startBlock << ": " << comm->ErrorString(result);
            emit StatusMessage(msg);
            return result;
    }

    // Re-write config block page.
    writeAddress = device->endConfig - device->writeBlockSizeFLASH;
    result = WriteFlashMemory(&preserveBlock[(writeAddress - startBlock) / 4],
                        writeAddress, device->endConfig);
    switch(result)
    {
        case Comm::Success:
            break;

        default:
            stream << "Could not write config words at "
                    << (device->endConfig - device->writeBlockSizeFLASH)
                    << ": " << comm->ErrorString(result);
            emit StatusMessage(msg);
            return result;
    }

    // Re-write rest of preserved config words page
    result = WriteFlashMemory(preserveBlock, startBlock, writeAddress);
    switch(result)
    {
        case Comm::Success:
            break;

        default:
            stream << "Could not write back preserved config page at " << startBlock << ": " << comm->ErrorString(result);
            emit StatusMessage(msg);
            return result;
    }


    return result;
}

Comm::ErrorCode DeviceWriter::WriteConfigFuses(unsigned int* memory)
{
    QString msg;
    QTextStream stream(&msg);
    unsigned int address, writeAddress;
    int i;
    unsigned int word;
    Comm::ErrorCode result;
    QByteArray sendPacket, receivePacket;
    int maxWrite = 64;

    if(device->family == Device::PIC32)
    {
        return WriteConfigMemory(memory);
    }

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);
    abortOperation = false;

    qDebug("Writing %X to %X (%d bytes)",
           device->configWords[0].address,
           device->configWords[device->configWords.count() - 1].address,
           device->configWords[device->configWords.count() - 1].address - device->configWords[0].address);

    i = 0;
    while(i < device->configWords.count())
    {
        address = device->configWords[i].address;

        stream << "Writing address: " << address;
        emit StatusMessage(msg);
        msg.clear();
        QCoreApplication::processEvents();

        if(abortOperation)
        {
            stream << "Write aborted at: " << address;
            emit StatusMessage(msg);
            return Comm::Aborted;
        }

        writeAddress = address;
        WriteConfigPacket cmd;
        cmd.setAddress(address);
        while(i < device->configWords.count() && cmd.count() < maxWrite)
        {
            if(device->configWords[i].address != address)
            {
                // ABORT: can't write the next address contiguously
                // with the rest of the writeData, so stop here.
                break;
            }

            if(device->family == Device::PIC24)
            {
                word = memory[(address - device->startConfig) >> 1];
                cmd.append(word & 0xFF);
                cmd.append((word >> 8) & 0xFF);
                cmd.append((word >> 16) & 0xFF);

                i++;
                address += 2;
            }
            else
            {
                word = memory[(address - device->startConfig) >> 1];
                if(address & 1)
                {
                    cmd.append((word >> 8) & 0xFF);
                }
                else
                {
                    cmd.append(word & 0xFF);
                }

                i++;
                address++;
            }
        }
        cmd.FramePacket(sendPacket);

        qWarning("Writing address %X with %d bytes of data", writeAddress, cmd.size() - cmd.headerSize);
        QCoreApplication::processEvents();
        receivePacket.clear();
        result = comm->SendGetPacket(sendPacket, receivePacket, 1);
        switch(result)
        {
            case Comm::Success:
                break;

            case Comm::NoAcknowledgement:
                stream << "Target failed acknowledgement at " << writeAddress << ".";
                emit StatusMessage(msg);
                return result;

            default:
                stream << "Write failure at " << writeAddress << ": " << comm->ErrorString(result);
                emit StatusMessage(msg);
                return result;
        }
    }

    return Comm::Success;
}
