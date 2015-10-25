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

#ifndef DEVICE_H
#define DEVICE_H

#include <QString>
#include <QVariant>
#include <QLinkedList>
#include <QList>

#define CFG_MAX 8

/*!
 * Provides microcontroller device specific parameters, address calculations, and
 * assembly code tools.
 */
class Device
{
public:
    enum Families
    {
        Unknown = 0, Baseline = 1, PIC16, PIC17, PIC18, PIC24, dsPIC30, dsPIC33, PIC32
    };

    Device();

    void setUnknown(void);

    static int toInt(const QVariant& value);
    static unsigned int toUInt(const QVariant& value);

    int id;
    Families family;    
    unsigned int commandMask;
    QString name;

    int bytesPerWordEEPROM;
    int bytesPerWordFLASH;
    int bytesPerAddressFLASH;   // number of bytes per address
    unsigned int eraseBlockSizeFLASH;    // number of bytes erased at a time
    int writeBlockSizeFLASH;    // number of bytes written at a time
    unsigned int flashWordMask;
    unsigned int blankValue;

    unsigned int startFLASH;
    unsigned int endFLASH;

    unsigned int startBootloader;
    unsigned int endBootloader;

    // Interrupt Vector Table variables -- only for PIC24
    unsigned int startIVT;
    unsigned int endIVT;
    unsigned int startAIVT;
    unsigned int endAIVT;
    unsigned int* IVT;
    unsigned int* AIVT;


    bool hasEeprom(void);
    bool hasUserMemory(void);
    bool hasConfigAsFlash(void);
    bool hasConfigAsFuses(void);
    bool hasConfig(void);
    bool hasConfigReadCommand(void);
    bool hasEraseFlashCommand(void);
    bool hasEncryption(void);
    int maxPacketSize(void);

    unsigned int startDeviceId;
    unsigned int endDeviceId;
    unsigned int deviceIdMask;
    unsigned int configWordMask;

    unsigned int startEEPROM;
    unsigned int endEEPROM;

    unsigned int startUser;
    unsigned int endUser;

    unsigned int startConfig;
    unsigned int endConfig;

    unsigned int startGPR;
    unsigned int endGPR;

    struct MemoryRange
    {
        unsigned int start;
        unsigned int end;
    };

    struct ConfigFieldSetting
    {
        QString name;
        QString cname;
        QString description;
        unsigned int bitMask;
        unsigned int bitValue;
    };

    struct ConfigField
    {
        unsigned int rowId;
        QString name;
        QString cname;
        QString description;
        unsigned int mask;
        unsigned int width;
        bool hidden;
        QList<ConfigFieldSetting> settings;
    };

    struct ConfigWord
    {
        unsigned int rowId;
        QString name;
        unsigned int address;
        unsigned int defaultValue;
        unsigned int implementedBits;
        QList<ConfigField> fields;
    };
    QList<ConfigWord> configWords;

    struct SFR
    {
        QString name;
        unsigned int address;
        QString access;
        QString POR;
        QString MCLR;
        QString WDT;
    };
    QList<SFR> SFRs;

    struct Trap
    {
        QString name;
        QString description;
        unsigned int number;
    };
    QList<Trap> Traps;

    struct IRQ
    {
        QString name;
        QString description;
        unsigned int number;
    };
    QList<IRQ> IRQs;

    unsigned int FromHexAddress(unsigned int hexAddress, bool& error);
    unsigned int* eepromPointer(unsigned int address, unsigned int* data) const;
    unsigned int* flashPointer(unsigned int address, unsigned int* data) const;
    void IncrementFlashAddressByInstructionWord(unsigned int& address) const;
    void IncrementFlashAddressByBytes(unsigned int& address, unsigned int bytes) const;
    unsigned int FlashBytes(unsigned int startAddress, unsigned int endAddress) const;
    void RemapResetVector(unsigned int* memory) const;
    bool HasValidResetVector(unsigned int* data) const;
    bool ResetVectorJumpsToBootloader(unsigned int* data) const;
    ConfigWord ConfigWordByAddress(unsigned int address);

protected:
    int findInstruction(unsigned int* data, unsigned int opcode, unsigned int opcodeMask, unsigned int endAddress) const;
};

#endif // DEVICE_H
