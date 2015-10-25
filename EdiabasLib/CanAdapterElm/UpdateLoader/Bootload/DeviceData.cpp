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
* E. Schlunder  2009/05/07  Added code for CRC calculation.
* E. Schlunder  2009/04/29  Code ported from PicKit2 pk2cmd source code.
*************************************************************************/

#include "DeviceData.h"
#include "Crc.h"

DeviceData::DeviceData(Device* newDevice)
{
    device = newDevice;
}

DeviceData::~DeviceData()
{
}

/*!
 * Clears FLASH program memory, EEPROM memory, and Config Words in memory (not on
 * the actual microcontroller device). To erase microcontroller memory, use the
 * appropriate methods in the DeviceWriter class.
 */
void DeviceData::ClearAllData(void)
{
    ClearProgramMemory(device->blankValue);
    ClearEEPromMemory(device->blankValue);
    ClearConfigWords();

    //ClearUserIDs(numIDs, idBytes, memBlankVal);
    //OSCCAL = OSCCALInit | 0xFF;
    BandGap = device->blankValue;
}

void DeviceData::ClearProgramMemory(unsigned int memBlankVal)
{
    for (unsigned int i = 0; i < MAX_MEM; i++)
    {
        ProgramMemory[i] = memBlankVal & device->flashWordMask;
    }
}

void DeviceData::CopyProgramMemory(unsigned int* memory)
{
    memcpy(ProgramMemory, memory, MAX_MEM * sizeof(unsigned int));
}

void DeviceData::ClearConfigWords(void)
{
    int address;
    unsigned int* word;
    Device::ConfigWord* config;

    if(device->configWords.count() == 0)
    {
        for(int i = 0; i < MAX_CFG; i++)
        {
            ConfigWords[i] = 0xFFFFFFFF;
        }
        return;
    }

    for(int i = 0; i < device->configWords.count(); i++)
    {
        config = &device->configWords[i];
        address = config->address - device->startConfig;

        if(device->hasConfigAsFlash())
        {
            word = device->flashPointer(config->address, ProgramMemory);
        }
        else
        {
            word = &ConfigWords[address >> 1];
        }

        switch(device->family)
        {
        case Device::PIC24:
            *word = (config->implementedBits & config->defaultValue);
            break;

        default:
            if(address & 1)
            {
                *word = ((*word) & 0xFFFF00FF) | ((config->implementedBits & config->defaultValue) << 8);
            }
            else
            {
                *word = ((*word) & 0xFFFFFF00) | (config->implementedBits & config->defaultValue);
            }
            break;
        }
    }
}

/**
 * @param wordAddress Device address number for the config word to get a pointer for.
 * @return Pointer to the config word's value inside PC memory.
 */
unsigned int* DeviceData::ConfigWordPointer(unsigned int wordAddress)
{
    unsigned char* wordValue;
    if(device->hasConfigAsFuses())
    {
        wordAddress -= device->startConfig;

        if(device->family == Device::PIC32)
        {
            wordValue = (unsigned char*)&ConfigWords[wordAddress >> 2];
        }
        else
        {
            wordValue = (unsigned char*)&ConfigWords[wordAddress >> 1];
        }
    }
    else if(device->hasConfigAsFlash())
    {
        wordValue = (unsigned char*)device->flashPointer(wordAddress, ProgramMemory);
    }
    else
    {
        return NULL;
    }

    if(wordAddress & 1)
    {
        wordValue++;
    }
    return (unsigned int*)wordValue;
}


void DeviceData::ClearUserIDs(unsigned char numIDs, int idBytes, unsigned int memBlankVal)
{
    if (numIDs> 0)
    {
        //init user ids to blank
        unsigned int idBlank = memBlankVal;
        if (idBytes == 1)
        {
            idBlank = 0xFF;
        }
        for (unsigned int i = 0; i < numIDs; i++)
        {
            UserIDs[i] = idBlank;
        }
    }
}

void DeviceData::ClearEEPromMemory(unsigned int memBlankVal)
{
    //init eeprom to blank
    for (unsigned int i = 0; i < MAX_EE; i++)
    {
        EEPromMemory[i] = memBlankVal;                  // 8-bit eeprom will just use 8 LSBs
    }
}

