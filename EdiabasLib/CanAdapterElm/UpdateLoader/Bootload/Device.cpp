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
* E. Schlunder  2010/02/01  Loading config bit fields/settings added.
* E. Schlunder  2009/04/14  Initial code ported from VB app.
************************************************************************/

#include "Device.h"

Device::Device()
{
    id = 0;
    family = Unknown;
    IVT = NULL;
    AIVT = NULL;

    setUnknown();
}

void Device::setUnknown(void)
{
    bytesPerWordEEPROM = 1;
    blankValue = 0xFFFFFFFF;
    flashWordMask = 0xFFFFFFFF;
    configWordMask = 0xFFFFFFFF;

    name = "";
    bytesPerAddressFLASH = 1;
    writeBlockSizeFLASH = 64;
    eraseBlockSizeFLASH = 1024;
    startFLASH  = 0;
    endFLASH    = 0;
    startBootloader = 0;
    endBootloader = 0;
    startEEPROM = 0;
    endEEPROM   = 0;
    startUser   = 0;
    endUser     = 0;
    startConfig = 0;
    endConfig   = 0;
    startGPR    = 0;
    endGPR      = 0;

    startIVT    = 0x4;
    endIVT      = 0x100;
    startAIVT   = 0x104;
    endAIVT     = 0x200;
    if(IVT)
    {
        delete IVT;
        IVT = NULL;
    }
    if(AIVT)
    {
        delete AIVT;
        AIVT = NULL;
    }
    Traps.clear();
    IRQs.clear();

    bytesPerWordFLASH = 2;

    foreach(ConfigWord word, configWords)
    {
        foreach(ConfigField field, word.fields)
        {
            field.settings.clear();
        }
        word.fields.clear();
    }

    configWords.clear();
    SFRs.clear();
}

bool Device::hasEeprom(void)
{
    if(endEEPROM > 0)
    {
        return true;
    }

    return false;
}

bool Device::hasUserMemory(void)
{
    if(endUser > 0)
    {
        return true;
    }

    return false;
}

bool Device::hasConfigAsFlash(void)
{
    if(family == Device::PIC16)
    {
        return false;
    }

    if(endConfig > 0)
    {
        if(startConfig < endFLASH && endConfig <= endFLASH)
        {
            return true;
        }
    }

    return false;
}

bool Device::hasConfigAsFuses(void)
{
    if(family == Device::PIC16)
    {
        return false;
    }

    if(endConfig > 0)
    {
        if(startConfig >= endFLASH && endConfig >= endFLASH)
        {
            return true;
        }
    }

    return false;
}

bool Device::hasConfig(void)
{
    return hasConfigAsFuses() || hasConfigAsFlash();
}

bool Device::hasConfigReadCommand(void)
{
    if(hasConfigAsFuses() && family != PIC16)
    {
        return true;
    }

    return false;
}

bool Device::hasEraseFlashCommand(void)
{
    if(family == PIC16)
    {
        if((commandMask & 1) == 0)
        {
            return false;
        }
    }

    return true;
}

bool Device::hasEncryption(void)
{
    if(commandMask & 0x100)
    {
        return true;
    }

    return false;
}

int Device::maxPacketSize(void)
{
    if(family == PIC16)
    {
        // PIC16F bootloader firmware does a dummy write of the ETX character as part
        // of the footer, so we don't actually get to use all 96 bytes of the bank 0 GPR area.

        // Bit Bang serial bootloaders need 5 bytes of GPR memory, so again, we lose some space.
        return 96 - 6;
    }
    else if(family == PIC24 || family == dsPIC30 || family == dsPIC33)
    {
        if((endGPR - startGPR) > 0xFFFF)
        {
            return 0x7000;
        }

        return ((endGPR - startGPR) - 1024);
    }
    else if(family == PIC32)
    {
        // Calculate max packet size based on GPR address range, subtracting off 2K bytes for
        // packet headers and bootloader static variable space. This value of 2K bytes might
        // be smaller in reality.
        return ((endGPR - startGPR) - 2048);
    }
    else
    {
        int overhead = 16;
        if(hasEncryption())
        {
            overhead += 7 + 16 + 16 + 4;
        }

        // Calculate max packet size based on GPR address range, subtracting off 16 bytes for
        // packet headers and bootloader static variable space. This value of 16 bytes might
        // be smaller in reality.
        return ((endGPR - startGPR) - overhead);
    }

}

/**
 * Calculates the correct memory pointer for the given device FLASH memory address and the
 * base memory array data pointer.
 */
unsigned int* Device::eepromPointer(unsigned int address, unsigned int* data) const
{
    switch(family)
    {
        case PIC24:
            return &data[address >> 1];

        default:
            return &data[address];
    }
}

/**
 * Calculates the correct memory pointer for the given device FLASH memory address and the
 * base memory array data pointer.
 */
unsigned int* Device::flashPointer(unsigned int address, unsigned int* data) const
{
    switch(family)
    {
        case PIC16:
            return &data[address];

        case PIC32:
            return &(data[(address - startFLASH) >> 2]);

        default:
        case PIC24:
        case dsPIC30:
        case dsPIC33:
        case PIC18:
            return &(data[address >> 1]);
    }
}

/**
 * Increments the given FLASH memory address by one instruction word.
 */
void Device::IncrementFlashAddressByInstructionWord(unsigned int& address) const
{
    switch(family)
    {
        case PIC16:
            address++;
            break;

        default:
        case PIC24:
        case dsPIC30:
        case dsPIC33:
        case PIC18:
            address += 2;
            break;

        case PIC32:
            address += 4;
            break;
    }
}

void Device::IncrementFlashAddressByBytes(unsigned int& address, unsigned int bytes) const
{
    switch(family)
    {
        case dsPIC30:
        case dsPIC33:
        case PIC24:
            address += (bytes / bytesPerWordFLASH) * 2;
            break;

        default:
            address += bytes / bytesPerAddressFLASH;
            break;
    }
}

/**
 * Calculates the number of bytes that exist for the given FLASH address range.
 */
unsigned int Device::FlashBytes(unsigned int startAddress, unsigned int endAddress) const
{
    switch(family)
    {
        case PIC16:
        case PIC18:
        default:
            return (endAddress - startAddress) * bytesPerAddressFLASH;

        case dsPIC30:
        case dsPIC33:
        case PIC24:
            return ((endAddress - startAddress) / 2) * 3;

        case PIC32:
            return (endAddress - startAddress);
    }
}

/**
 * Converts HEX file addresses to actual device address numbers.
 *
    PIC16 parts use only one address for each FLASH program word. Address 0 has 14 bits of data, Address 1 has
    14 bits of data, etc. However, the PIC16 HEX file addresses each byte of data with a unique address number.
    As a result, you basically have to take the HEX file address and divide by 2 to figure out the actual
    PIC16 FLASH memory address that the byte belongs to.

    Example: PIC16F886 has 8K program words, word addressed as 0 to 0x2000.
        A full HEX file for this part would have 16Kbytes of FLASH data. The HEX file bytes would
        be addressed from 0 to 0x4000.

    This presents a predicament for EEPROM data. Instead of starting from HEX file address 0x2100 as
    the EDC device database might indicate, the HEX file has to start EEPROM data at 0x2000 + 0x2100 = 0x4100,
    to avoid overlapping with the HEX file's FLASH addresses.
 */
unsigned int Device::FromHexAddress(unsigned int hexAddress, bool& error)
{
    unsigned int flashAddress = hexAddress / bytesPerAddressFLASH;
    unsigned int eepromAddress;
    unsigned int configAddress;
    unsigned int userAddress;

    switch(family)
    {
    case PIC16:
        eepromAddress = hexAddress - startFLASH;
        configAddress = hexAddress - startFLASH;
        userAddress = hexAddress - startFLASH;
        break;

    default:
        eepromAddress = hexAddress;
        configAddress = hexAddress;
        userAddress = hexAddress;
        break;
    }

    if(flashAddress >= startFLASH && flashAddress <= endFLASH)
    {
        error = false;
        return flashAddress;
    }

    if(eepromAddress >= startEEPROM && eepromAddress <= endEEPROM)
    {
        error = false;
        return eepromAddress;
    }

    if(configAddress >= startConfig && configAddress <= endConfig)
    {
        error = false;
        return configAddress;
    }

    if(userAddress >= startUser && userAddress <= endUser)
    {
        error = false;
        return userAddress;
    }

    error = true;
    return 0;
}

/**
 * This function moves instructions from the beginning of program memory to
 * just before the bootloader FLASH memory area. It then replaces the beginning
 * of program memory with reset vector code that will jump to running the Bootloader
 * firmware upon start up.
 */
void Device::RemapResetVector(unsigned int* memory) const
{
    unsigned int* dest;
    unsigned int* source;
    unsigned int operand;

    if(ResetVectorJumpsToBootloader(memory))
    {
        // ABORT: The hardware reset vector instructions already jump to
        // the bootloader firmware. Assume that this firmware has already
        // been remapped, don't do it again and foul things up.
        return;
    }

    switch(family)
    {
        case dsPIC30:
        case dsPIC33:
        case PIC24:
            dest = flashPointer(startBootloader - 4, memory);
            source = memory;
            if(HasValidResetVector(memory))
            {
                // Remap the start up GOTO instruction for bootloader application mode entry.
                *dest++ = *source++;
                *dest++ = *source++;
            }
            else
            {
                // force a NOP to keep bootloader from jumping to no-where-land. NOP will
                // drop execution back into the bootloader.
                *dest++ = blankValue & flashWordMask;
                *dest++ = blankValue & flashWordMask;
            }

            // Encode address 0: "GOTO Bootloader"
            operand = startBootloader;
            memory[0] = 0x040000 | (operand & 0xFFFE);
            memory[1] = 0x000000 | ((operand >> 16) & 0x7F);
            break;

        case PIC16:
            dest = flashPointer(startBootloader - 5, memory);
            source = memory;
            if(HasValidResetVector(memory))
            {
                *dest++ = 0x018A;                           // clrf     PCLATH
                // Copy first four instruction words up to the bootloader application entry vector
                *dest++ = *source++;
                *dest++ = *source++;
                *dest++ = *source++;
                *dest++ = *source++;
            }
            else
            {
                // force NOP to keep botloader from jumping to no-where-land. NOP will drop
                // execution back into the bootloader.
                *dest++ = blankValue & flashWordMask;
                *dest++ = blankValue & flashWordMask;
                *dest++ = blankValue & flashWordMask;
                *dest++ = blankValue & flashWordMask;
                *dest++ = blankValue & flashWordMask;
            }

            // Encode address 0:
            //  movlw   high(BootloaderBreakCheck)
            //  movwf   PCLATH
            //  goto    BootloaderBreakCheck
            operand = (startBootloader + 3);
            memory[0] = 0x3000 | ((operand >> 8) & 0xFF);   // movlw   high(BootloaderBreakCheck)
            memory[1] = 0x008A;                             // movwf   PCLATH
            memory[2] = 0x2800 | (operand & 0x7FF);         // goto    BootloaderBreakCheck
            break;

        case PIC18:
        default:
            dest = flashPointer(startBootloader - 4, memory);
            source = memory;

            if(HasValidResetVector(memory))
            {
                // Remap the start up GOTO instruction for bootloader application mode entry.
                *dest++ = *source++;
                *dest++ = *source++;
            }
            else
            {
                // force a NOP to keep bootloader from jumping to no-where-land. NOP will
                // drop execution back into the bootloader.
                *dest++ = blankValue & flashWordMask;
                *dest++ = blankValue & flashWordMask;
            }

            // Encode address 0: "GOTO BootloaderBreakCheck"
            operand = (startBootloader + 2) / bytesPerWordFLASH;
            memory[0] = 0xEF00 | (operand & 0xFF);
            memory[1] = 0xF000 | ((operand >> 8) & 0x0FFF);
            break;
    }
}

/*!
 * Checks to see whether the HEX file loaded includes both application and bootloader firmware
 * combined, with hardware reset vectors already automatically remapped. This is done by checking
 * the hardware reset vector for code that jumps to the bootloader start up vector. If found,
 * it is assumed this HEX file does not need to be remapped again.
 *
 * @return True if the hardware reset vector already jumps to the start of the bootloader firmware.
 *  False otherwise.
 */
bool Device::ResetVectorJumpsToBootloader(unsigned int* data) const
{
    unsigned int address;

    switch(family)
    {
        case PIC16:
            if(findInstruction(data, 0x3000, 0x3F00, 1) != 0)  // movlw    XX
            {
                return false;
            }
            if(findInstruction(data, 0x008A, 0x3FFF, 2) != 1)  // movwf    PCLATH
            {
                return false;
            }
            if(findInstruction(data, 0x2800, 0x3800, 3) != 2)  // goto     XXX
            {
                return false;
            }

            address  = (data[0] & 0xFF) << 8;
            address |= (data[2] & 0x7FF);
            address -= 3;
            if(address != startBootloader)
            {
                return false;
            }
            return true;

        case dsPIC30:
        case dsPIC33:
        case PIC24:
            if(HasValidResetVector(data) == false)
            {
                return false;
            }

            address  = data[0] & 0xFFFE;
            address |= (data[1] & 0x7F) << 16;
            if(address != startBootloader)
            {
                return false;
            }
            return true;

        case PIC18:
            if(HasValidResetVector(data) == false)
            {
                return false;
            }

            address  = data[0] & 0xFF;
            address |= (data[1] & 0xFFF) << 8;
            address  = (address * bytesPerWordFLASH) - 2;
            if(address != startBootloader)
            {
                return false;
            }
            return true;

        default:
            break;
    }

    return false;
}

bool Device::HasValidResetVector(unsigned int* data) const
{
    int addressGOTO;

    switch(family)
    {
        case PIC16:
            // make sure there exists a GOTO within the first four instruction words
            addressGOTO = findInstruction(data, 0x2800, 0x3800, 4);         // goto     XXX
            if(addressGOTO == -1)
            {
                return false;
            }

            return true;

        case dsPIC30:
        case dsPIC33:
        case PIC24:
            if(findInstruction(data, 0x040000, 0xFF0001, 2) == -1)          // goto     XXXX
            {
                return false;
            }
            if(findInstruction(data, 0x000000, 0xFFFF80, 4) != 2)           // second half of GOTO instruction
            {
                return false;
            }
            return true;

        default:
        case PIC18:
            if(findInstruction(data, 0xEF00, 0xFF00, 1) == -1)              // goto     XXXX
            {
                return false;
            }

            if(findInstruction(data, 0xF000, 0xFF00, 4) != 2)               // second half of GOTO instruction
            {
                return false;
            }
            return true;
    }

    return false;
}

/*!
 * Searches program memory for the opcode specified, up to the endAddress.
 *
 * @param opcode The instruction opcode value we are searching for.
 * @param opcodeMask Bit mask to allow for matching instructions while allowing certain bits
 *  to be "don't care" (helpful for matching "GOTO xxx" instruction for example).
 * @return -1 if the opcode was not found. Otherwise returns an address offset number
 *  from 0 to endAddress indicating where the opcode was finally found.
 */
int Device::findInstruction(unsigned int* data,
                            unsigned int opcode, unsigned int opcodeMask,
                            unsigned int endAddress) const
{
    unsigned int address = 0;
    unsigned int word;

    while(address < endAddress)
    {
        word = *data++ & opcodeMask;
        if(word == opcode)
        {
            return address;
        }

        IncrementFlashAddressByInstructionWord(address);
    }

    return -1;
}

/*!
 * Returns the ConfigWord structure for the configuration word
 * occupying the address requested. This makes it easier to
 * look up information about a configuration word than trying
 * to access the configWords list directly.
 */
Device::ConfigWord Device::ConfigWordByAddress(unsigned int address)
{
    int i;
    for(i = 0; i < configWords.size(); i++)
    {
        if(configWords.at(i).address == address)
        {
            return configWords.at(i);
        }
    }

    // couldn't find it, create a dummy structure instead
    ConfigWord result;
    result.address = address;
    result.defaultValue = 0xFFFFFFFF;
    result.implementedBits = 0xFFFFFFFF;
    result.name = "(unknown)";

    return result;
}

int Device::toInt(const QVariant& value)
{
    QString s(value.toString());
    bool ok;
    int result;

    if(s.length() == 0)
    {
        return 0;
    }

    if(s.startsWith("0x"))
    {
        result = s.mid(2).toInt(&ok, 16);
    }
    else if(s.startsWith("0b"))
    {
        result = s.mid(2).toInt(&ok, 2);
    }
    else
    {
        result = s.toInt(&ok, 10);
    }

    if(ok == false)
    {
        result = 0;
    }

    return result;
}

unsigned int Device::toUInt(const QVariant& value)
{
    QString s(value.toString());
    bool ok;
    unsigned int result;

    if(s.length() == 0)
    {
        return 0;
    }

    if(s.startsWith("0x"))
    {
        result = s.mid(2).toUInt(&ok, 16);
    }
    else if(s.startsWith("0b"))
    {
        result = s.mid(2).toUInt(&ok, 2);
    }
    else
    {
        result = s.toUInt(&ok, 10);
    }

    if(ok == false)
    {
        result = 0;
    }

    return result;
}
