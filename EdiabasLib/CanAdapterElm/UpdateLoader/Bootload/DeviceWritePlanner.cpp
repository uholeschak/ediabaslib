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
* E. Schlunder  2009/04/30  Created device write planning code to
*                           attempt maximizing speed and bootloader
                            safety.
************************************************************************/

#include <QtGlobal>

#include "DeviceWritePlanner.h"
#include "BootPackets.h"

DeviceWritePlanner::DeviceWritePlanner(Device* newDevice)
{
    device = newDevice;
}

/**
 * Produces an erase plan for clearing the entire device.
 *
 * Special care is taken to erase the Config Words page last, when writeConfig is
 * turned on and the device uses FLASH for Config Words.
 *
 * When writeConfig is false, care is taken to avoid erasing the Config Words page.
 *
 * This routine makes sure not to erase the Bootloader Block.
 *
 * All erases are listed in reverse address order. This forces the program to
 * erase all application code from the device before finally erasing the crucial
 * address 0 reset vector. With all application code erased, having a "GOTO BootloaderStart"
 * instruction at address 0 is no longer critical, as blank memory will provide "NOP"
 * executions all the way to the Bootloader Block. Thus, the Bootloader will be able to
 * execute when there is no application code programmed.
 */
void DeviceWritePlanner::planFlashErase(QLinkedList<Device::MemoryRange>& eraseList,
                                        unsigned int* existingData)
{
    Device::MemoryRange block;

    block.start = device->startFLASH;
    block.end = device->endFLASH;
    eraseList.clear();
    eraseList.append(block);
    if(device->family == Device::PIC32 && existingData == NULL)
    {
        return;
    }

    if(writeConfig)
    {
        eraseConfigPageLast(eraseList);
    }
    else
    {
        doNotEraseConfigPage(eraseList);
    }

    doNotEraseInterruptVectorTable(eraseList);

    doNotEraseBootBlock(eraseList);

    if(existingData != NULL)
    {
        // For differential bootloading:
        // Scan existingData to see if we can avoid erasing any pages.
        // Not explicitly required, but could occassionally reduce the
        // number of erase transactions.
    }
}

/**
 * Produces FLASH erase and write plans for optimally programming application code into
 * a device.
 *
 * This routine assumes that pages filled with blank or "NOP" instructions (0xFFFF) do
 * not need to be erased nor written. For most applications, this will be sufficient,
 * providing extremely fast programming.
 *
 * To guarantee clean memory, perform a Verify After Write CRC check on each Erase
 * Block. This will find any leftover junk data from older firmware that can be erased.
 */
void DeviceWritePlanner::planFlashWrite(QLinkedList<Device::MemoryRange>& eraseList,
                                        QLinkedList<Device::MemoryRange>& writeList,
                                        unsigned int start, unsigned int end,
                                        unsigned int* data, unsigned int* existingData)
{
    unsigned int address = start;
    Device::MemoryRange block;

    while(address < end)
    {
        address = skipEmptyFlashPages(address, data);
        block.start = address;
        if(address >= end)
        {
            break;
        }

        address = findEndFlashWrite(address, data);
        if(address >= end)
        {
            address = end;
        }
        block.end = address;

        if(device->family == Device::PIC16 && !device->hasEraseFlashCommand())
        {
            // Certain PIC16 devices (such as PIC16F882) have a peculiar automatic erase
            // during write feature. To make that work, writes must be expanded to align
            // with Erase Block boundaries.
            block.start -= (block.start % device->eraseBlockSizeFLASH);
            if(block.end % device->eraseBlockSizeFLASH)
            {
                block.end += device->eraseBlockSizeFLASH - (block.end % device->eraseBlockSizeFLASH);
                address = block.end;
            }
        }
        writeList.append(block);

        address++;
    }

    if(existingData == NULL && device->family == Device::PIC32)
    {
        // Because PIC32 has Bulk Erase available for bootloader use,
        // it's faster to simply erase the entire FLASH memory space
        // than erasing specific erase blocks using an erase plan.
        block.start = device->startFLASH;
        block.end = device->endFLASH;
        eraseList.append(block);
    }
    else
    {
        if(existingData != NULL)
        {
            QLinkedList<Device::MemoryRange>::iterator it;
            for(it = writeList.begin(); it != writeList.end(); ++it)
            {
                qDebug("unpruned write(%X to %X)", it->start, it->end);
            }
            doNotWriteExistingData(writeList, start, end, data, existingData);
            for(it = writeList.begin(); it != writeList.end(); ++it)
            {
                qDebug("pruned write(%X to %X)", it->start, it->end);
            }
        }

        if(!writeList.isEmpty())
        {
            if(device->hasEraseFlashCommand())
            {
                flashEraseList(eraseList, writeList, data, existingData);
            }

            EraseAppCheckFirst(eraseList);
            WriteAppCheckLast(writeList);

            if(writeConfig)
            {
                eraseConfigPageLast(eraseList);
                writeConfigPageFirst(writeList);
                doNotEraseBootBlock(eraseList);     // needed in case boot block resides on config page
            }
            else
            {
                doNotEraseConfigPage(eraseList);
            }

            doNotEraseInterruptVectorTable(eraseList);
        }
    }

    packetSizeWriteList(writeList);
}

/**
 * When sending a packet to the PIC, we must not overflow the PIC's internal RAM
 * storage limit. Therefore, each write must be made small enough for the PIC
 * to buffer it successfully.
 *
 * Additionally, the AN1310 protocol only provides a single byte for the
 * write flash block count. Therefore, write transactions must not exceed 256
 * write blocks in a single transaction.
 */
void DeviceWritePlanner::packetSizeWriteList(QLinkedList<Device::MemoryRange>& writeList)
{
    QLinkedList<Device::MemoryRange>::iterator it = writeList.begin();
    Device::MemoryRange firstHalf;
    int maxWritePacketData = device->maxPacketSize();
    int bytesPerWriteBlock = device->writeBlockSizeFLASH;
    int blocks, bytes;

    maxWritePacketData -= WriteFlashPacket::headerSize;
    maxWritePacketData -= WriteFlashPacket::footerSize;

    if(device->family == Device::PIC16)
    {
        // On PIC12/PIC16, writeBlockSizeFLASH only counts the number of instruction words per block.
        // Each instruction word requires two bytes of data to be transmitted in the command packet,
        // so we need to double bytesPerWriteBlock here.
        bytesPerWriteBlock <<= 1;
    }
    else if(device->family == Device::PIC24)
    {
        // On PIC24, writeBlockSizeFLASH only counts the two least significant bytes of the
        // instruction word, so we need to add 1/2 more here to count that special third byte.
        bytesPerWriteBlock = device->writeBlockSizeFLASH + (device->writeBlockSizeFLASH >> 1);
    }

    if(device->hasEncryption())
    {
        // each write block must be followed by 16 bytes of message authentication code (MAC) data
        bytesPerWriteBlock += 16;
    }

    if(maxWritePacketData / device->writeBlockSizeFLASH >= 256)
    {
        // Do not allow write transactions greater than 256 blocks.
        // AN1310 protocol only provides an 8-bit block count for Write FLASH commands.
        maxWritePacketData = bytesPerWriteBlock * 256;
    }

    qDebug("Max write packet size: %d bytes", maxWritePacketData);
    while(it != writeList.end())
    {
        blocks = (it->end - it->start) / device->writeBlockSizeFLASH;
        bytes  = blocks * bytesPerWriteBlock;

        if(bytes > maxWritePacketData)
        {
            // this write is too big, split it in half and then try again
            firstHalf.start = it->start;
            firstHalf.end = (maxWritePacketData / bytesPerWriteBlock) * device->writeBlockSizeFLASH;
            // Round off packet size to nearest FLASH write block size.
            firstHalf.end -= firstHalf.end % device->writeBlockSizeFLASH;
            firstHalf.end += firstHalf.start;
            it->start = firstHalf.end;
            writeList.insert(it, firstHalf);
            it--;
        }
        else
        {
            it++;
        }
    }
}

/**
 * "J" flash devices store config bits in the last few bytes of the last page of
 * FLASH memory instead of in dedicated config fuse memory.
 *
 * To avoid problems with config bits getting erased but never re-written, we want
 * to make the config page be the very first page to get written back to the device.
 */
void DeviceWritePlanner::writeConfigPageFirst(QLinkedList<Device::MemoryRange>& writeList)
{
    if(!device->hasConfigAsFlash())
    {
        // ABORT: this device does not store config bits in FLASH, so no worries...
        return;
    }

    QLinkedList<Device::MemoryRange>::iterator it = writeList.end();
    it--;

    if(it->end < device->endFLASH - device->writeBlockSizeFLASH)
    {
        // ABORT: user is not planning to write to config bit page.
        return;
    }

    it->end -= device->writeBlockSizeFLASH;
    if(it->end <= it->start)
    {
        // after taking out the config page, this write transaction has nothing left in it.
        writeList.removeLast();
    }

    Device::MemoryRange configWrite;
    configWrite.start = device->endFLASH - device->writeBlockSizeFLASH;
    configWrite.end = device->endFLASH;
    writeList.prepend(configWrite);
}

/**
 * "J" flash devices store config bits in the last few bytes of the last page of
 * FLASH memory instead of in dedicated config fuse memory.
 *
 * To avoid problems with config bits getting erased but never re-written, we want
 * to make the config page be the very last page to get erased. Immediately after
 * being erased, the config page will be written back first thing, minimizing
 * the window of time where something could go wrong.
 */
void DeviceWritePlanner::eraseConfigPageLast(QLinkedList<Device::MemoryRange>& eraseList)
{
    if(doNotEraseConfigPage(eraseList))
    {
        // ABORT: this device does not store config bits in FLASH or
        // the existing eraseList does not intend to erase the config bits page anyway.
        return;
    }

    // make config page erase very last transaction
    Device::MemoryRange configErase;
    configErase.start = device->endFLASH - device->eraseBlockSizeFLASH;
    configErase.end = device->endFLASH;
    eraseList.append(configErase);
}

/**
 * On PIC24 devices, we want to erase the Application Check address first so that
 * the bootloader will not attempt to start an incomplete application firmware image.
 */
void DeviceWritePlanner::EraseAppCheckFirst(QLinkedList<Device::MemoryRange>& eraseList)
{
    if(device->family != Device::PIC24)
    {
        // ABORT: only applies to PIC24
        return;
    }

    QLinkedList<Device::MemoryRange>::iterator it = eraseList.end();
    it--;

    if(it->start >= device->endBootloader + device->eraseBlockSizeFLASH)
    {
        // ABORT: user is not planning to erase the application check page.
        return;
    }

    it->start += device->eraseBlockSizeFLASH;
    if(it->start >= it->end)
    {
        // after taking out the app check page, this erase transaction has nothing left in it.
        eraseList.removeLast();
    }

    Device::MemoryRange appErase;
    appErase.start = device->endBootloader;
    appErase.end = device->endBootloader + device->eraseBlockSizeFLASH;
    eraseList.prepend(appErase);
}

/**
 * On PIC24 devices, we want to write the Application Check address last so that
 * the bootloader will not attempt to start an incomplete application firmware image.
 */
void DeviceWritePlanner::WriteAppCheckLast(QLinkedList<Device::MemoryRange>& writeList)
{
    if(device->family != Device::PIC24)
    {
        // ABORT: only applies to PIC24
        return;
    }

    QLinkedList<Device::MemoryRange>::iterator it = writeList.begin();
    if(it->start >= device->endBootloader + device->writeBlockSizeFLASH)
    {
        // ABORT: not planning to write Application Check row anyway, nothing to do here.
        return;
    }

    it->start += device->writeBlockSizeFLASH;
    if(it->start >= it->end)
    {
        // after taking out the app check row, this write transaction has nothing left in it.
        writeList.removeFirst();
    }

    // make app check row write very last transaction
    Device::MemoryRange appWrite;
    appWrite.start = device->endBootloader;
    appWrite.end = device->endBootloader + device->writeBlockSizeFLASH;
    writeList.append(appWrite);
}

/**
 * This routine modifies an eraseList so that the eraseList will not end up erasing the
 * config bits page on devices that store config bits in FLASH memory.
 *
 * @return True if the device does not has configuration bits as FLASH memory or the passed
 *  eraseList would not have erased the config bits page anyway.
 *  False if the eraseList would have erased the config bits page (eraseList is returned
 *  modified to avoid erasing the config bits page as a side effect).
 */
bool DeviceWritePlanner::doNotEraseConfigPage(QLinkedList<Device::MemoryRange>& eraseList)
{
    if(!device->hasConfigAsFlash())
    {
        // ABORT: this device does not store config bits in FLASH, so no worries...
        return true;
    }

    QLinkedList<Device::MemoryRange>::iterator it = eraseList.begin();
    if(it->end <= device->endFLASH - device->eraseBlockSizeFLASH)
    {
        // ABORT: not planning to erase config bit page anyway, nothing to do here.
        return true;
    }

    it->end -= device->eraseBlockSizeFLASH;
    if(it->end <= it->start)
    {
        // after taking out the config page, this write transaction has nothing left in it.
        eraseList.removeFirst();
    }

    return false;
}

/**
 * This routine modifies an eraseList so that the eraseList will not end up erasing the
 * interrupt vector table on PIC24 devices.
 *
 * @return True if the device is not a PIC24 device or the passed
 *  eraseList would not have erased the interrupt vector table anyway.
 *  False if the eraseList would have erased the interrupt vector table (eraseList is returned
 *  modified to avoid erasing the interrupt vector table page as a side effect).
 */
bool DeviceWritePlanner::doNotEraseInterruptVectorTable(QLinkedList<Device::MemoryRange>& eraseList)
{
    if(device->family != Device::PIC24)
    {
        // ABORT: this device does not have a fixed interrupt vector table in FLASH, so no worries...
        return true;
    }

    QLinkedList<Device::MemoryRange>::iterator it = eraseList.begin();
    unsigned int endIVT = 0x200;
    if(endIVT < device->eraseBlockSizeFLASH)
    {
        endIVT = device->eraseBlockSizeFLASH;
    }

    if(it->start >= endIVT)
    {
        // ABORT: not planning to erase IVT anyway, nothing to do here.
        return true;
    }

    it->start = endIVT;
    if(it->end <= it->start)
    {
        // after taking out the IVT page, this write transaction has nothing left in it.
        eraseList.removeFirst();
    }

    return false;
}

void DeviceWritePlanner::doNotEraseBootBlock(QLinkedList<Device::MemoryRange>& eraseList)
{
    Device::MemoryRange firstHalf;
    QLinkedList<Device::MemoryRange>::iterator it = eraseList.begin();
    while(it != eraseList.end())
    {
        //         v Boot Block area
        // 1. S  E   |        <- Erase transaction is before the boot block, no problem
        // 2.    |   S  E     <- Erase transaction is after the boot block,  no problem
        // 3.    S   E        <- Erase transaction exactly matches the boot block
        // 4.    S   |  E     <- Erase transaction starts on the boot block and extends past the end of it
        // 5. S  |   E        <- Erase transaction starts before the boot block, but doesn't end before hitting it
        // 6. S  |   |  E     <- Erase transaction starts before and ends after the boot block
        if(!((it->start <= device->startBootloader && it->end <= device->startBootloader) ||    // 1. S  E   |
             (it->start >= device->endBootloader && it->end >= device->endBootloader)))         // 2.    |   S  E
        {
            if(it->start == device->startBootloader && it->end == device->endBootloader)
            {
                // 3.    S   E        <- Erase transaction exactly matches the boot block
                it = eraseList.erase(it);   // delete the transaction and continue checking erase plan
                continue;
            }
            if(it->start == device->startBootloader)
            {
                // 4.    S   |  E     <- Erase transaction starts on the boot block and extends past the end of it
                // modify transaction to only erase the data after the boot block
                it->start = device->endBootloader;
            }
            else if(it->end == device->endBootloader)
            {
                // 5. S  |   E        <- Erase transaction starts before the boot block, but doesn't end before hitting it
                // modify transaction to only erase data up to the boot block
                it->end = device->startBootloader;
            }
            else
            {
                // 6. S  |   |  E     <- Erase transaction starts before and ends after the boot block
                // split the transaction up into two transactions.
                // the first transaction will erase data after the boot block
                firstHalf.start = device->endBootloader;
                firstHalf.end = it->end;
                if(firstHalf.start < firstHalf.end)
                {
                    eraseList.insert(it, firstHalf);
                }

                // the other transaction will erase data before the boot block
                it->end = device->startBootloader;
            }
        }
        it++;
    }
}

void DeviceWritePlanner::doNotWriteExistingData(QLinkedList<Device::MemoryRange>& writeList,
                                                unsigned int start, unsigned int end,
                                                unsigned int* data,
                                                unsigned int* existingData)
{
    QLinkedList<Device::MemoryRange> prunedWriteList;
    QLinkedList<Device::MemoryRange>::iterator it;
    unsigned int address = start;
    Device::MemoryRange eraseBlock, writeBlock;

    while(address < end)
    {
        // find beginning and end of Erase Block
        eraseBlock.start = address - (address % device->eraseBlockSizeFLASH);
        eraseBlock.end = eraseBlock.start + device->eraseBlockSizeFLASH;

        if(blockHasChanged(eraseBlock, data, existingData))
        {
            // This Erase Block contains modified data. We're going to
            // have to re-write any Write Blocks overlapping this Erase Block.
            for(it = writeList.begin(); it != writeList.end(); ++it)
            {
                writeBlock = *it;
                if(writeBlock.end <= eraseBlock.start || writeBlock.start >= eraseBlock.end)
                {
                    // write transaction is entirely before or after the erase block, no problem
                    continue;
                }

                // this write transaction overlaps an erase block that is going to need erasing.
                // therefore, we will need to re-write the affected write block range.
                if(writeBlock.start < eraseBlock.start)
                {
                    writeBlock.start = eraseBlock.start;
                }

                if(writeBlock.end > eraseBlock.end)
                {
                    writeBlock.end = eraseBlock.end;
                }

                if(prunedWriteList.isEmpty() ||
                   prunedWriteList.last().end != writeBlock.start)
                {
                    // create a new write transaction in the pruned write list.
                    prunedWriteList.append(writeBlock);
                }
                else
                {
                    // extend the length of the last write to cover this one.
                    prunedWriteList.last().end = writeBlock.end;
                }
            }
        }
        address = eraseBlock.end;
    }

    writeList = prunedWriteList;
}

bool DeviceWritePlanner::blockHasChanged(Device::MemoryRange& block,
                                         unsigned int* data, unsigned int* existingData)
{
    unsigned int word;
    unsigned int existingWord;
    unsigned int address = block.start;

    data = device->flashPointer(address, data);
    existingData = device->flashPointer(address, existingData);
    while(address < block.end)
    {
        word = (*data++) & device->flashWordMask;
        existingWord = (*existingData++) & device->flashWordMask;
        if(word != existingWord)
        {
            // We found some differing data here.
            return true;
        }

        device->IncrementFlashAddressByInstructionWord(address);
    }

    // couldn't find any data that has changed within this block
    return false;
}

void DeviceWritePlanner::flashEraseList(QLinkedList<Device::MemoryRange>& eraseList,
                                        QLinkedList<Device::MemoryRange>& writeList,
                                        unsigned int* data, unsigned int* existingData)
{
    int eraseStart, eraseEnd;
    Device::MemoryRange block;
    int i;
    int pageStart, pageEnd, pages = device->endFLASH / device->eraseBlockSizeFLASH;
    bool flashPageErased[pages + 1];

    for(i = 0; i <= pages; i++)
    {
        flashPageErased[i] = false;
    }

    QLinkedList<Device::MemoryRange>::iterator it;
    for(it = writeList.begin(); it != writeList.end(); ++it)
    {
        block = *it;
        pageStart = block.start / device->eraseBlockSizeFLASH;
        pageEnd = block.end / device->eraseBlockSizeFLASH;
        if(block.end % device->eraseBlockSizeFLASH)
        {
            pageEnd++;
        }
        eraseStart = -1;
        eraseEnd = -1;
        for(i = pageStart; i < pageEnd; i++)
        {
            if(flashPageErased[i] == false)
            {
                if(eraseStart == -1)
                {
                    eraseStart = i * device->eraseBlockSizeFLASH;
                }
                eraseEnd = (i + 1) * device->eraseBlockSizeFLASH;
                flashPageErased[i] = true;
            }
        }
        if(eraseStart != -1)
        {
            block.start = eraseStart;
            block.end = eraseEnd;
            eraseList.prepend(block);
        }
    }

    if(data != NULL && existingData != NULL)
    {
        // For differential bootloading feature:
        // Scan remaining unerased pages to see if existingData contains data in those areas.
        // Not explicitly required, as the "Verify After Write" CRC check should find any additional
        // pages that need to be erased. It just might be a little faster to do it explicitly here.
    }
}

unsigned int DeviceWritePlanner::findEndFlashWrite(unsigned int address, unsigned int* data)
{
    unsigned int checkAddress;

    while(address < device->endFLASH)
    {
        checkAddress = skipEmptyFlashPages(address, data);
        if(checkAddress != address)
        {
            // the next page is empty, we've reached the end of the area we want to write
            return address;
        }

        address += device->writeBlockSizeFLASH;
    }

    return device->endFLASH;
}

unsigned int DeviceWritePlanner::skipBootloaderFlashPages(unsigned int address)
{
    if(address >= device->startBootloader && address < device->endBootloader)
    {
        return device->endBootloader;
    }

    return address;
}

unsigned int DeviceWritePlanner::skipEmptyFlashPages(unsigned int address, unsigned int* data)
{
    unsigned int* readData;
    unsigned int word;
    unsigned int addressSkip;

    readData = device->flashPointer(address, data);
    while(address < device->endFLASH)
    {
        word = *readData++;
        if((word & device->flashWordMask) != (device->blankValue & device->flashWordMask))
        {
            // We found some non-empty data here. Make sure it's not the bootloader.
            addressSkip = skipBootloaderFlashPages(address);
            if(addressSkip != address)
            {
                // skip over the bootloader and continue
                address = addressSkip;
                readData = device->flashPointer(address, data);
                continue;
            }
            else
            {
                // align address to FLASH write page and return result.
                address = address - (address % device->writeBlockSizeFLASH);
                return address;
            }
        }

        device->IncrementFlashAddressByInstructionWord(address);
    }

    // couldn't find any data, return end of flash memory
    return device->endFLASH;
}
