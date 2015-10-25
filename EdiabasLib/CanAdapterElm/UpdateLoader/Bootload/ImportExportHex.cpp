/************************************************************************
* Copyright (c) 2005-2011,  Microchip Technology Inc.
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
* E. Schlunder  2009/04/29  Code ported from PicKit2 pk2cmd source code.
*************************************************************************/

#include <QFile>
#include "ImportExportHex.h"
#include "Device.h"

HexImporter::HexImporter(void)
{
}

HexImporter::~HexImporter(void)
{
}

/*
    PIC16Fxx parts use only one address for each FLASH program word. Address 0 has 14 bits of data, Address 1 has
    14 bits of data, etc. However, the PIC16Fxx HEX file addresses each byte of data with a unique address number.
    As a result, you basically have to take the HEX file address and divide by 2 to figure out the actual
    PIC16Fxx FLASH memory address that the byte belongs to.

    Example: PIC16F886 has 8K program words, word addressed as 0 to 0x2000.
        A full HEX file for this part would have 16Kbytes of FLASH data. The HEX file bytes would
        be addressed from 0 to 0x4000.

    This presents a predicament for EEPROM data. Instead of starting from HEX file address 0x2100 as
    the EDC device database might indicate, the HEX file has to start EEPROM data at 0x2000 + 0x2100 = 0x4100,
    to avoid overlapping with the HEX file's FLASH addresses.
*/
HexImporter::ErrorCode HexImporter::ImportHexFile(QString fileName, DeviceData* data, Device* device)
{
    QFile hexfile(fileName);

    hasEndOfFileRecord = false;
    fileExceedsFlash = false;

    if (!hexfile.open(QIODevice::ReadOnly | QIODevice::Text))
    {
        return CouldNotOpenFile;
    }

    bool lineExceedsFlash = true;
    unsigned int eepromBytesPerWord = 1;
    unsigned int cfgBytesPerWord = 2;

    if(device->family == Device::PIC32)
    {
        cfgBytesPerWord = 4;
    }

    unsigned int configWords = device->endConfig - device->startConfig;
    data->ClearAllData();
    bool error, ok;
    unsigned int segmentAddress = 0;
    unsigned int byteCount;
    unsigned int lineAddress;
    unsigned int arrayAddress;
    unsigned int byteAddress;
    unsigned int deviceAddress;

    int recordType;

    unsigned int bytePosition;
    QString hexByte;
    unsigned int wordByte;
    unsigned int shift;
    unsigned int lineByte;
    QString lineString;
    QByteArray mac;

    Device::MemoryRange range, rawRange;
    range.start = 0;
    range.end = 0;
    rawRange.start = 0;
    rawRange.end = 0;
    ranges.clear();
    rawimport.clear();
    hasConfigBits = false;
    data->Encrypted = false;
    data->Nonce = 0;
    data->mac.clear();
    data->mac.resize((device->endFLASH - device->startFLASH) / device->bytesPerWordFLASH + 1);

    while (!hexfile.atEnd())
    {
        QByteArray line = hexfile.readLine();
        if ((line[0] != ':') || (line.size() < 11))
        {
            // skip line if not hex line entry,or not minimum length ":BBAAAATTCC"
            continue;
        }

        byteCount = line.mid(1, 2).toInt(&ok, 16);
        lineAddress = segmentAddress + line.mid(3, 4).toInt(&ok, 16);
        recordType = line.mid(7, 2).toInt(&ok, 16);

        if (recordType == 1)                        // end of file record
        {
            hasEndOfFileRecord = true;
            break;
        }
        else if (recordType == 0x43)                // nonce value record
        {
            data->Encrypted = true;

            // skip if line isn't long enough for bytecount.
            if((unsigned int)line.size() >= (11 + (2 * byteCount)))
            {
                lineString = line.mid(9, 8);
                data->Nonce = lineString.toInt(&ok, 16);
            }
        }
        else if (recordType == 0x40)                // MAC data record
        {
            data->Encrypted = true;

            // skip if line isn't long enough for bytecount.
            if((unsigned int)line.size() >= (11 + (2 * byteCount)))
            {
                lineString = line.mid(9, 16*2);
                for(int x = 0; x < 16; x++)
                {
                    mac[x] = lineString.mid(x*2, 2).toUInt(&ok, 16);
                }
                data->mac[lineAddress / device->writeBlockSizeFLASH] = mac;
            }
        }
        else if ((recordType == 2) || (recordType == 4)) // Segment address
        {
            // skip if line isn't long enough for bytecount.
            if((unsigned int)line.size() >= (11 + (2 * byteCount)))
            {
                segmentAddress = line.mid(9, 4).toInt(&ok, 16);
            }

            if (recordType == 2)
            {
                segmentAddress <<= 4;
            }
            else
            {
                segmentAddress <<= 16;
            }
        } // end if ((recordType == 2) || (recordType == 4))
        else if (recordType == 0)                        // Data Record
        {
            if ((unsigned int)line.size() < (11 + (2 * byteCount)))
            {
                // skip if line isn't long enough for bytecount.
                continue;
            }

            rawRange.start = lineAddress;
            rawRange.end = lineAddress + byteCount;
            rawimport.append(rawRange);

            deviceAddress = device->FromHexAddress(lineAddress, error);
            if(error)
            {
                // don't do anything here, this address is outside of device memory space.
            }
            else if(range.start == 0 && range.end == 0)
            {
                range.start = deviceAddress;
                range.end = device->FromHexAddress(lineAddress + byteCount, ok);
                ranges.append(range);
            }
            else if(ranges.length() && ranges.last().end == deviceAddress)
            {
                ranges.last().end = device->FromHexAddress(lineAddress + byteCount, ok);
            }
            else
            {
                range.start = deviceAddress;
                range.end = device->FromHexAddress(lineAddress + byteCount, ok);
                ranges.append(range);
            }

            if(device->hasConfigAsFlash())
            {
                if((range.start <= device->startConfig && range.end >= device->endConfig) ||
                   (range.start >= device->startConfig && range.start < device->endConfig) ||
                   (range.end > device->startConfig && range.end <= device->endConfig))
                {
                    hasConfigBits = true;
                }
            }

            for (lineByte = 0; lineByte < byteCount; lineByte++)
            {
                byteAddress = lineAddress + lineByte;
                if(device->family == Device::PIC24)
                {
                    // compute byte position within memory word
                    bytePosition = byteAddress % 4;
                }
                else
                {
                    // compute byte position within memory word
                    bytePosition = byteAddress % device->bytesPerWordFLASH;
                }

                // get the byte value from hex file
                hexByte = line.mid(9 + (2 * lineByte), 2);
                wordByte = 0xFFFFFF00 | hexByte.toInt(&ok, 16);
                // shift the byte into its proper position in the word.
                for (shift = 0; shift < bytePosition; shift++)
                {
                    wordByte <<= 8;
                    wordByte |= 0xFF; // shift in ones.
                }

                lineExceedsFlash = true; // if not in any memory section, then error

                // program memory section --------------------------------------------------
                if (((byteAddress / device->bytesPerAddressFLASH) < device->endFLASH) &&
                    ((byteAddress / device->bytesPerAddressFLASH) >= device->startFLASH))
                {
                    // compute array address from hex file address # bytes per memory location
                    if(device->family == Device::PIC24)
                    {
                        arrayAddress = (byteAddress - device->startFLASH) / 4;
                    }
                    else
                    {
                        arrayAddress = (byteAddress - device->startFLASH) / device->bytesPerWordFLASH;
                    }

                    data->ProgramMemory[arrayAddress] &= wordByte; // add byte.
                    lineExceedsFlash = false;
                    //NOTE: program memory locations containing config words may get modified
                    // by the config section below that applies the config masks.
                }

                // EE data section ---------------------------------------------------------
                if(device->family == Device::PIC16)
                {
                    byteAddress >>= 1;
                }

                if (device->hasEeprom() && byteAddress >= device->startEEPROM)
                {
                    unsigned int eeAddress;

                    switch(device->family)
                    {
                        case Device::PIC24:
                            eeAddress = (byteAddress >> 1) - device->startEEPROM;
                            if(eeAddress < device->endEEPROM - device->startEEPROM)
                            {
                                data->EEPromMemory[eeAddress >> 1] &= wordByte;

                                lineExceedsFlash = false;
                            }
                            break;

                        case Device::PIC16:
                            if(byteAddress < device->endEEPROM)
                            {
                                eeAddress = (byteAddress - device->startEEPROM) / eepromBytesPerWord;
                                data->EEPromMemory[eeAddress] &= wordByte; // add byte.
                                lineExceedsFlash = false;
                            }
                            break;

                        default:
                        case Device::PIC18:
                            if(byteAddress < device->endEEPROM)
                            {
                                eeAddress = (byteAddress - device->startEEPROM) / eepromBytesPerWord;
                                int eeshift = (bytePosition / eepromBytesPerWord) * eepromBytesPerWord;
                                for (int reshift = 0; reshift < eeshift; reshift++)
                                { // shift byte into proper position
                                    wordByte >>= 8;
                                }
                                data->EEPromMemory[eeAddress] &= wordByte; // add byte.
                                lineExceedsFlash = false;
                            }
                            break;
                    }
                }

                // Config words section ----------------------------------------------------
                if ((byteAddress >= device->startConfig) && (configWords > 0))
                {
                    unsigned int configNum = (byteAddress - (device->startConfig)) / cfgBytesPerWord;
                    if (configNum < configWords)
                    {
                        lineExceedsFlash = false;
                        hasConfigBits = true;
                        if(cfgBytesPerWord == 4)
                        {
                            data->ConfigWords[configNum] &= (wordByte & 0xFFFFFFFF);
                        }
                        else
                        {
                            data->ConfigWords[configNum] &= (wordByte & 0xFFFF);
                        }
                    }
                }
            } // end for (lineByte = 0; lineByte < byteCount; lineByte++)

            if (lineExceedsFlash)
            {
                fileExceedsFlash = true;
            }
        } // end if (recordType == 0)
    }

    if(hexfile.isOpen())
    {
        hexfile.close();
    }
/*
    qDebug(QString("Device FLASH: [" + QString::number(device->startFLASH, 16).toUpper() + " - " +
                   QString::number(device->endFLASH, 16).toUpper() +")").toLatin1());
    qDebug(QString("Device EEPROM: [" + QString::number(device->startEEPROM, 16).toUpper() + " - " +
                   QString::number(device->endEEPROM, 16).toUpper() +")").toLatin1());
    qDebug(QString("Device Config: [" + QString::number(device->startConfig, 16).toUpper() + " - " +
                   QString::number(device->endConfig, 16).toUpper() +")").toLatin1());
    qDebug(QString("Device User Id: [" + QString::number(device->startUser, 16).toUpper() + " - " +
                   QString::number(device->endUser, 16).toUpper() +")").toLatin1());
*/
/*    qDebug("Raw Hex Import Address Ranges:");
    for(int i = 0; i < rawimport.count(); i++)
    {
        qDebug(QString("  [" + QString::number(rawimport[i].start, 16).toUpper() + " - " +
                       QString::number(rawimport[i].end, 16).toUpper() +")").toLatin1());
    }
*/
    qDebug("Hex Import Address Ranges:");
    for(int i = 0; i < ranges.count(); i++)
    {
        qDebug(QString("  [" + QString::number(ranges[i].start, 16).toUpper() + " - " +
                       QString::number(ranges[i].end, 16).toUpper() +")").toLatin1());
    }

    return Success;
}

unsigned char HexImporter::computeChecksum(char* fileLine)
{
    unsigned int byteCount = ParseHex(&fileLine[1], 2);
    if (strlen(fileLine) >= (9 + (2* byteCount)))
    { // skip if line isn't long enough for bytecount.
        int checksum = byteCount;
        for(unsigned int i = 0; i < (3 + byteCount); i++)
        {
            checksum += ParseHex(fileLine + 3 + (2*i), 2);
        }
        checksum = 0 - checksum;
        return (unsigned char)(checksum & 0xFF);
    }

    return 0;
}

int HexImporter::ParseHex(char* characters, int length)
{
	int integer = 0;
	
	for (int i = 0; i < length; i++)
	{
		integer *= 16;
        switch(*(characters + i))
		{
			case '1':
				integer += 1;
				break;

			case '2':
				integer += 2;
				break;

			case '3':
				integer += 3;
				break;

			case '4':
				integer += 4;
				break;

			case '5':
				integer += 5;
				break;

			case '6':
				integer += 6;
				break;

			case '7':
				integer += 7;
				break;

			case '8':
				integer += 8;
				break;

			case '9':
				integer += 9;
				break;

			case 'A':
			case 'a':
				integer += 10;
				break;

			case 'B':
			case 'b':
				integer += 11;
				break;

			case 'C':
			case 'c':
				integer += 12;
				break;

			case 'D':
			case 'd':
				integer += 13;
				break;

			case 'E':
			case 'e':
				integer += 14;
				break;

			case 'F':
			case 'f':
				integer += 15;
				break;
		}
	}
	return integer;
}

bool HexImporter::importedAddress(unsigned int address)
{
    Device::MemoryRange range;
    foreach(range, ranges)
    {
        if(range.start <= address && range.end > address)
        {
            return true;
        }
    }

    return false;
}

