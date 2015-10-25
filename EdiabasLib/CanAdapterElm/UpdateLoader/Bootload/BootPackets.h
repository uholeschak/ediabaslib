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

#ifndef BOOTPACKET_H
#define BOOTPACKET_H

#include <QByteArray>

//Packet control characters
#define STX 0x0F
#define ETX 0x04
#define DLE 0x05

/*!
 * Base class for all bootloader command packet objects. This class extends from
 * the QByteArray class, which is the container for the actual command packet bytes
 * once the FramePacket method is called.
 */
class BootPacket : public QByteArray
{
public:
    BootPacket();
    void FramePacket(QByteArray& sendPacket) const;
    void setAddress(unsigned int address);

    enum Commands
    {
        BootloaderInfo = 0,
        ReadFlash, ReadFlashCrc, EraseFlash, WriteFlash,
        ReadEeprom, WriteEeprom,
        WriteConfig,
        RunApplication, Reset, SetNonce,
        BulkEraseFlash = 0x0B
    };

    static const int headerSize;
    static const int footerSize;

protected:
    void appendEscaped(QByteArray& packet, char byte) const;
};

/*!
 * Requests that the bootloader firmware return information about
 * bootloader. This should be used immediately after establishing
 * autobaud communications with the bootloader.
 *
 * Command:   <STX>[<0x00>]<CRCL><CRCH><ETX>
 * Response:  <STX>[<BOOTBYTESL><BOOTBYTESH><VERL><VERH><STARTBOOTL><STARTBOOTH><STARTBOOTU><0x00>]<CRCL><CRCH><ETX>
 */
class BootloaderInfoPacket : public BootPacket
{
public:
    BootloaderInfoPacket();
    void setAddress(unsigned int address); // do not use
};

/*!
 * Reads FLASH memory from the device.
 *
 * Command:   <STX>[<0x01><ADDRL><ADDRH><ADDRU><0x00><BLOCKSL><BLOCKSH>]<CRCL><CRCH><ETX>
 * Response:  <STX>[<DATA>...]<CRCL><CRCH><ETX>
 */
class ReadFlashPacket : public BootPacket
{
public:
    ReadFlashPacket();
    void setBytes(unsigned short blocks);
};

/*!
 * Reads CRC values of FLASH memory blocks (Erase Block size).
 *
 * Command:   <STX>[<0x02><ADDRL><ADDRH><ADDRU><0x00><BLOCKSL><BLOCKSH>]<CRCL><CRCH><ETX>
 * Response:  <STX>[<CRCL1><CRCH1>...<CRCLn><CRCHn>]<ETX>
 */
class ReadFlashCrcPacket : public BootPacket
{
public:
    ReadFlashCrcPacket();
    void setBlocks(unsigned short blocks);
};

/*!
 * Erase FLASH memory in decending address order.
 *
 * Command:   <STX>[<0x03><ADDRL><ADDRH><ADDRU><0x00><PAGESL>]<CRCL><CRCH><ETX>
 * Response:  <STX>[<0x03>]<CRCL><CRCH><ETX>
 */
class EraseFlashPacket : public BootPacket
{
public:
    EraseFlashPacket();
    void setBlocks(unsigned char blocks);
};

/*!
 * Bulk Erases all of FLASH program memory. Currently, only PIC32
 * devices can implement this command, as it has separate Boot FLASH memory
 * that stays intact during bulk erase.
 *
 * Command:   <STX>[<0x0B>]<CRCL><CRCH><ETX>
 * Response:  <STX>[<0x0B>]<CRCL><CRCH><ETX>
 */
class BulkEraseFlashPacket : public BootPacket
{
public:
    BulkEraseFlashPacket();
};

/*!
 * Writes FLASH memory. The data payload must be contiguous FLASH memory, aligned to FLASH Write
 * Block size.
 *
 * Command:   <STX>[<0x04><ADDRL><ADDRH><ADDRU><0x00><BLOCKSL><DATA>...]<CRCL><CRCH><ETX>
 * Response:  <STX>[<0x04>]<CRCL><CRCH><ETX>
 */
class WriteFlashPacket : public BootPacket
{
public:
    WriteFlashPacket();
    int payloadSize(void);
    void setBlocks(unsigned char blocks);
    unsigned char blocks(void);

    static const int headerSize;
};

/*!
 * Reads EEPROM memory.
 *
 * Command:   <STX>[<0x05><ADDRL><ADDRH><0x00><0x00><BYTESL><BYTESH>]<CRCL><CRCH><ETX>
 * Response:  <STX>[<DATA>...]<CRCL><CRCH><ETX>
 */
class ReadEepromPacket : public BootPacket
{
public:
    ReadEepromPacket();
    void setBytes(unsigned short bytes);
};

/*!
 * Writes to EEPROM data memory.
 *
 * Command:   <STX>[<0x06><ADDRL><ADDRH><0x00><0x00><BYTESL><BYTESH><DATA>...]<CRCL><CRCH><ETX>
 * Response:  <STX>[<0x06>]<CRCL><CRCH><ETX>
 */
class WriteEepromPacket : public BootPacket
{
public:
    WriteEepromPacket();
    void setBytes(unsigned short bytes);
};

/*!
 * Writes to Config fuses. Devices that store config bits in FLASH memory
 * do not implement this command.
 *
 * Command:   <STX>[<0x07><ADDRL><ADDRH><ADDRU><0x00><BYTES><DATA>...]<CRCL><CRCH><ETX>
 * Response:  <STX>[<0x07>]<CRCL><CRCH><ETX>
 */
class WriteConfigPacket : public BootPacket
{
public:
    WriteConfigPacket();
    void FramePacket(QByteArray& sendPacket);
    static const int headerSize;

protected:
    void setBytes(unsigned char bytes);
};

/*!
 * Instructs the bootloader firmware to jump to the application firmware.
 *
 * Command:   <STX>[<0x08>]<CRCL><CRCH><ETX>
 * Response:  none, the application firmware takes control immediately
 *            when this command is issued.
 */
class RunApplicationPacket : public BootPacket
{
public:
    RunApplicationPacket();
};

/*!
 * Sets AES Nonce value.
 *
 * Command:   <STX>[<0x0A><NONCEL><NONCEH><NONCEU><NONCEX>]<CRCL><CRCH><ETX>
 * Response:  <STX>[<0x0A>]<CRCL><CRCH><ETX>
 */
class SetNoncePacket : public BootPacket
{
public:
    SetNoncePacket();
    void setNonce(unsigned int nonce);
};


#endif // BOOTPACKET_H
