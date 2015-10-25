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

#include "Crc.h"
#include "BootPackets.h"

const int BootPacket::headerSize = 7;
const int BootPacket::footerSize = 2; // 16-bit CRC causes 2 byte footer
BootPacket::BootPacket()
{
}

void BootPacket::appendEscaped(QByteArray& packet, char byte) const
{
    if(byte == STX || byte == ETX || byte == DLE)
    {
        packet.append(DLE);
    }
    packet.append(byte);
}

/**
 * Converts data into a fully formatted bootloader packet.
 * A 16-bit CRC of the data payload is calculated.
 * Control characters STX, ETX, and DLE are escaped.
 *
 * The end result is a fully formed bootloader command packet in the form:
 * <STX>[<DATA>...]<CRCL><CRCH><ETX>
 */
void BootPacket::FramePacket(QByteArray& sendPacket) const
{
    char byte;
    Crc crc;
    int dataSize = size();

    sendPacket.clear();
    sendPacket.append(STX);
    for(int i = 0; i < dataSize; i++)
    {
        byte = (*this)[i];
        appendEscaped(sendPacket, byte);
        crc.Add(byte);
    }

    appendEscaped(sendPacket, crc.LSB());
    appendEscaped(sendPacket, crc.MSB());
    sendPacket.append(ETX);
}

void BootPacket::setAddress(unsigned int address)
{
    (*this)[1] = (address & 0xFF);
    (*this)[2] = ((address & 0xFF00) >> 8);
    (*this)[3] = ((address & 0xFF0000) >> 16);
    (*this)[4] = ((address & 0xFF000000) >> 24);
}

// ----------------------------------------------------------------------

/**
 * Instructs the bootloader to erase one or more blocks of FLASH program memory.
 * The bootloader will erase blocks in reverse address order, so the highest numbered
 * block address must be passed in this packet, unlike all other commands.
 *
 * @param address Last address to begin erasing.
 * @param blocks Number of blocks to erase downwards.
 */
EraseFlashPacket::EraseFlashPacket()
{
    resize(6);
    (*this)[0] = EraseFlash;
}

/**
 * @param blocks Number of flashEraseBlockSize sized blocks for the
 *  bootloader to iterate and erase.
 */
void EraseFlashPacket::setBlocks(unsigned char blocks)
{
    (*this)[5] = blocks;
}

// ----------------------------------------------------------------------

ReadFlashPacket::ReadFlashPacket()
{
    resize(7);
    (*this)[0] = ReadFlash;
}

/**
 * @param blocks Number of flashReadBlockSize sized blocks for the
 *  bootloader to iterate and read.
 */
void ReadFlashPacket::setBytes(unsigned short blocks)
{
    (*this)[5] = blocks & 0xFF;
    (*this)[6] = (blocks >> 8) & 0xFF;
}

// ----------------------------------------------------------------------
// In:   <STX>[<0x02><ADDRL><ADDRH><ADDRU><0x00><BLOCKSL><BLOCKSH>]<CRCL><CRCH><ETX>
// Out:  <STX>[<CRCL1><CRCH1>...<CRCLn><CRCHn>]<ETX>
ReadFlashCrcPacket::ReadFlashCrcPacket()
{
    resize(7);
    (*this)[0] = ReadFlashCrc;
}

/**
 * @param blocks Number of flashEraseBlockSize sized blocks for the
 *  bootloader to iterate and calculate a CRC for.
 */
void ReadFlashCrcPacket::setBlocks(unsigned short blocks)
{
    (*this)[5] = blocks & 0xFF;
    (*this)[6] = (blocks >> 8) & 0xFF;
}
// ----------------------------------------------------------------------

BootloaderInfoPacket::BootloaderInfoPacket()
{
    resize(1);
    (*this)[0] = BootloaderInfo;
}

/*!
 * Not implemented for ReadBootloaderInfo packets because the bootloader info is
 * provided by the bootloader without requiring an address.
 */
void BootloaderInfoPacket::setAddress(unsigned int address)
{
    qWarning("setAddress is not supported by ReadBootloaderInfo packets.");
}

BulkEraseFlashPacket::BulkEraseFlashPacket()
{
    resize(1);
    (*this)[0] = BulkEraseFlash;
}

// ----------------------------------------------------------------------
const int WriteFlashPacket::headerSize = 6;
WriteFlashPacket::WriteFlashPacket()
{
    resize(headerSize);
    (*this)[0] = WriteFlash;
}

int WriteFlashPacket::payloadSize(void)
{
    return count() - headerSize;
}

unsigned char WriteFlashPacket::blocks(void)
{
    return (*this)[5];
}

/**
 * @param blocks Number of flashWriteBlockSize sized blocks for the
 *  bootloader to write to FLASH.
 */
void WriteFlashPacket::setBlocks(unsigned char blocks)
{
    (*this)[5] = blocks;
}
// ----------------------------------------------------------------------

// ----------------------------------------------------------------------
ReadEepromPacket::ReadEepromPacket()
{
    resize(7);
    (*this)[0] = ReadEeprom;
}

void ReadEepromPacket::setBytes(unsigned short bytes)
{
    (*this)[5] = bytes & 0xFF;
    (*this)[6] = (bytes >> 8) & 0xFF;
}
// ----------------------------------------------------------------------

// ----------------------------------------------------------------------
WriteEepromPacket::WriteEepromPacket()
{
    resize(7);
    (*this)[0] = WriteEeprom;
}

void WriteEepromPacket::setBytes(unsigned short bytes)
{
    (*this)[5] = bytes & 0xFF;
    (*this)[6] = (bytes >> 8) & 0xFF;
}
// ----------------------------------------------------------------------

// ----------------------------------------------------------------------
const int WriteConfigPacket::headerSize = 6;
WriteConfigPacket::WriteConfigPacket()
{
    resize(headerSize);
    (*this)[0] = WriteConfig;
}

void WriteConfigPacket::setBytes(unsigned char bytes)
{
    (*this)[5] = bytes;
}

void WriteConfigPacket::FramePacket(QByteArray& sendPacket)
{
    setBytes(size() - headerSize);
    BootPacket::FramePacket(sendPacket);
}
// ----------------------------------------------------------------------

RunApplicationPacket::RunApplicationPacket()
{
    resize(1);
    (*this)[0] = RunApplication;
}

SetNoncePacket::SetNoncePacket()
{
    resize(5);
    (*this)[0] = SetNonce;
}

void SetNoncePacket::setNonce(unsigned int nonce)
{
    (*this)[1] = nonce & 0xFF;
    (*this)[2] = (nonce >> 8) & 0xFF;
    (*this)[3] = (nonce >> 16) & 0xFF;
    (*this)[4] = (nonce >> 24) & 0xFF;
}
