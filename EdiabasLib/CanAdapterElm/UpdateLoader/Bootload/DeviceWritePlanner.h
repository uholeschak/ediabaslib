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
*/

#ifndef DEVICEWRITEPLANNER_H
#define DEVICEWRITEPLANNER_H

#include "Device.h"

/*!
 * Produces FLASH Erase and Write Plans for optimally programming application code into
 * a device.
 */
class DeviceWritePlanner
{
public:
    bool writeConfig;

    DeviceWritePlanner(Device* newDevice);

    void planFlashWrite(QLinkedList<Device::MemoryRange>& eraseList,
                        QLinkedList<Device::MemoryRange>& writeList,
                        unsigned int start, unsigned int end,
                        unsigned int* data, unsigned int* existingData = NULL);

    void planFlashErase(QLinkedList<Device::MemoryRange>& eraseList, unsigned int* existingData = NULL);

protected:
    Device* device;

    void flashEraseList(QLinkedList<Device::MemoryRange>& eraseList,
                        QLinkedList<Device::MemoryRange>& writeList,
                        unsigned int* data = NULL, unsigned int* existingData = NULL);
    void calculatePacketSize(QLinkedList<Device::MemoryRange>& writeList, unsigned int* data);
    void packetSizeWriteList(QLinkedList<Device::MemoryRange>& writeList);
    void eraseConfigPageLast(QLinkedList<Device::MemoryRange>& eraseList);
    bool doNotEraseConfigPage(QLinkedList<Device::MemoryRange>& eraseList);
    bool doNotEraseInterruptVectorTable(QLinkedList<Device::MemoryRange>& eraseList);
    void doNotEraseBootBlock(QLinkedList<Device::MemoryRange>& eraseList);
    void doNotWriteExistingData(QLinkedList<Device::MemoryRange>& writeList,
                                                unsigned int start, unsigned int end,
                                                unsigned int* data,
                                                unsigned int* existingData);
    bool blockHasChanged(Device::MemoryRange& block, unsigned int* data, unsigned int* existingData);

    void writeConfigPageFirst(QLinkedList<Device::MemoryRange>& writeList);
    unsigned int findEndFlashWrite(unsigned int address, unsigned int* data);
    unsigned int skipBootloaderFlashPages(unsigned int address);
    unsigned int skipEmptyFlashPages(unsigned int address, unsigned int* data);
    void EraseAppCheckFirst(QLinkedList<Device::MemoryRange>& eraseList);
    void WriteAppCheckLast(QLinkedList<Device::MemoryRange>& writeList);
};

#endif // DEVICEWRITEPLANNER_H
