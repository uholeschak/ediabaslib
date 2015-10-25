/************************************************************************
* Copyright (c) 2009,  Microchip Technology Inc.
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
* E. Schlunder  2009/05/07  Created device verify planning code to
*                           avoid verifying bootloader and config pages.
************************************************************************/

#include "DeviceVerifyPlanner.h"

DeviceVerifyPlanner::DeviceVerifyPlanner(Device* newDevice)
{
    device = newDevice;
}

/*!
 * Memory ranges used by the bootloader firmware are excluded from the verify list.
 *
 * If the Write Config option is disabled, the FLASH Erase Block containing config
 * words is excluded from the verify list. (only for devices that store config bits in
 * FLASH memory).
 */
void DeviceVerifyPlanner::planFlashVerify(QLinkedList<Device::MemoryRange>& verifyList, int start, int end)
{
    Device::MemoryRange block;

    block.start = start;
    block.end = end;
    verifyList.clear();
    verifyList.append(block);

    if(!writeConfig)
    {
        doNotVerifyConfigPage(verifyList);
    }

    doNotVerifyBootBlock(verifyList);
}

void DeviceVerifyPlanner::doNotVerifyConfigPage(QLinkedList<Device::MemoryRange>& verifyList)
{
    if(!device->hasConfigAsFlash())
    {
        // ABORT: this device does not store config bits in FLASH, so no worries...
        return;
    }

    QLinkedList<Device::MemoryRange>::iterator it = verifyList.begin();
    if(it->end <= device->endFLASH - device->eraseBlockSizeFLASH)
    {
        // ABORT: user is not planning to verify the config bit page.
        return;
    }

    it->end -= device->eraseBlockSizeFLASH;
    if(it->end <= it->start)
    {
        // after taking out the config page, this transaction has nothing left in it.
        verifyList.removeFirst();
    }
}

void DeviceVerifyPlanner::doNotVerifyBootBlock(QLinkedList<Device::MemoryRange>& verifyList)
{
    Device::MemoryRange firstHalf;
    QLinkedList<Device::MemoryRange>::iterator it = verifyList.begin();
    while(it != verifyList.end())
    {
        //     S   E
        // S   |   E
        //     S   |  E
        // S   |   |  E
        //
        //     |   S  E
        // S   E   |
        if(!((it->start <= device->startBootloader && it->end <= device->startBootloader) ||
             (it->start >= device->endBootloader && it->end >= device->endBootloader)))
        {
            // This transaction would verify over bootloader memory, which may fail if
            // we haven't (or can't) read the device out.
            if(it->start == device->startBootloader && it->end == device->endBootloader)
            {
                it = verifyList.erase(it);
                continue;
            }
            if(it->start == device->startBootloader)
            {
                it->start = device->endBootloader;
            }
            else if(it->end == device->endBootloader)
            {
                it->end = device->startBootloader;
            }
            else
            {
                firstHalf.start = device->endBootloader;
                firstHalf.end = it->end;
                it->end = device->startBootloader;
                if(firstHalf.start < firstHalf.end)
                {
                    verifyList.insert(it, firstHalf);
                }
            }
        }
        it++;
    }
}
