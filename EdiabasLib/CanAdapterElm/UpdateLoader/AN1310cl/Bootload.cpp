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
* E. Schlunder  2010/08/02  Command line only bootloader app.
************************************************************************/

#include <iostream>
using namespace std;

#include <QTime>
#include <QTextStream>

#include "Bootload.h"
#include "Bootload/ImportExportHex.h"
#include "Bootload/DeviceSqlLoader.h"

Bootload::Bootload()
{
    comm = new Comm();
#ifdef _TTY_POSIX_
    comm->serial->setPortName("/dev/serial");
#else
    comm->serial->setPortName("COM1");
#endif

    writeFlash = true;
    writeConfig = false;
    writeEeprom = false;

    device = new Device();
    writePlan = new DeviceWritePlanner(device);
    verifyPlan = new DeviceVerifyPlanner(device);
    deviceData = new DeviceData(device);
    verifyData = new DeviceData(device);
    hexData = new DeviceData(device);
    deviceReader = new DeviceReader(device, comm);
    connect(deviceReader, SIGNAL(StatusMessage(QString)), this, SLOT(PrintMessage(const QString&)));
    deviceWriter = new DeviceWriter(device, comm);
    connect(deviceWriter, SIGNAL(StatusMessage(QString)), this, SLOT(PrintMessage(const QString&)));
    deviceVerifier = new DeviceVerifier(device, comm);
    connect(deviceVerifier, SIGNAL(StatusMessage(QString)), this, SLOT(PrintMessage(const QString&)));

    deviceData->ClearAllData();
    verifyData->ClearAllData();

}

void Bootload::PrintMessage(const QString& msg)
{
    cout << qPrintable(msg) << endl;
}

void Bootload::SetPort(QString portName)
{
    comm->serial->setPortName(portName);
}

void Bootload::SetBaudRate(QString baudRate)
{
    unsigned int rate;
    bool ok;
    rate = baudRate.toUInt(&ok, 10);
    if(ok)
    {
        comm->serial->setBaudRate(rate);
    }
}

int Bootload::Connect(void)
{
    QTime elapsed, totalTime;
    QString msg;
    QString version;

    totalTime.start();
    cout << "Using " << qPrintable(comm->serial->portName()) << " at " << comm->serial->baudRate() << " bps" << endl;

    if(comm->open() != Comm::Success)
    {
        Disconnect();
        msg.append("Could not open ");
        msg.append(comm->serial->portName());
        msg.append(".");
        PrintMessage(msg);
        failed = -1;
        return -1;
    }

    qDebug("time(2): %fs", (double)totalTime.elapsed() / 1000);

    // First, try talking to the bootloader without going through a
    // device reset. Sometimes this will let us achieve high baud rates
    // when config bits have been lost (which would ordinarily cause us
    // to run slowly from the default INTOSC).
    PrintMessage("Connecting...");
    Comm::BootInfo bootInfo = comm->ReadBootloaderInfo(2);
    qDebug("time(3): %fs", (double)totalTime.elapsed() / 1000);

    if(bootInfo.majorVersion == 0 && bootInfo.minorVersion == 0)
    {
        // No such luck. Application firmware might be busy running, so
        // try to force the device into Bootloader mode.
        //comm->assertBreak();          // [UH]
        //comm->assertReset();          // [UH]
        comm->ActivateBootlader();      // [UH]
        qDebug("time(assert reset): %fs", (double)totalTime.elapsed() / 1000);

        PrintMessage("Resetting device...");

        // wait 5ms to allow MCLR and RXD to go to logic 0.
        elapsed.start();
        while(elapsed.elapsed() < 600) // [UH] 600ms
        {
        }

        //qDebug("time(3.2): %fs", (double)totalTime.elapsed() / 1000);
        //comm->releaseIntoBootloader();    // [UH]

        qDebug("time(3.3): %fs", (double)totalTime.elapsed() / 1000);
        bootInfo = comm->ReadBootloaderInfo();
    }

    if(bootInfo.majorVersion == 0 && bootInfo.minorVersion == 0)
    {
        Disconnect();
        PrintMessage("Bootloader not found.");
        failed = -1;
        return -2;
    }

    QTextStream s(&msg);
    QString connectMsg;
    QTextStream ss(&connectMsg);
    ss << "Bootloader Firmware v" << bootInfo.majorVersion << ".";
    ss.setPadChar('0');
    ss.setFieldAlignment(QTextStream::AlignRight);
    ss.setFieldWidth(2);
    ss << bootInfo.minorVersion;
    ss.setPadChar(' ');
    ss.setFieldAlignment(QTextStream::AlignRight);
    ss.setFieldWidth(0);
    PrintMessage(connectMsg);

    Comm::DeviceId deviceId;
    if(bootInfo.deviceId != 0)
    {
        deviceId.id = bootInfo.deviceId;
        deviceId.revision = -1;
    }
    else
    {
        deviceId = comm->ReadDeviceID((Device::Families)bootInfo.familyId);
    }

    msg.clear();

    DeviceSqlLoader::ErrorCode result = DeviceSqlLoader::loadDevice(device, deviceId.id, (Device::Families)bootInfo.familyId);
    switch(result)
    {
        case DeviceSqlLoader::DatabaseMissing:
            cout << "Couldn't find device database file. Please copy DEVICES.DB to the executable's folder." << endl;
            return -1;

        case DeviceSqlLoader::DeviceMissing:
            cout << "Couldn't find Device ID " << deviceId.id << " in the device database." << endl;
            return -1;

        case DeviceSqlLoader::Success:
            break;
    }

    if(device->name.size())
    {
        s << device->name;
    }
    else
    {
        s << "Device " << deviceId.id;
    }

    qDebug("time(6): %fs", (double)totalTime.elapsed() / 1000);

    device->startBootloader = bootInfo.startBootloader;
    device->endBootloader = bootInfo.endBootloader;
    device->commandMask = bootInfo.commandMask;

    int maxRead = (int)comm->serial->baudRate();
    maxRead /= 8;
    maxRead -= maxRead % 0x100;
    if(maxRead < 0x100)
    {
        maxRead = 0x100;
    }
    deviceReader->setMaxRequest(maxRead);

    if(deviceId.revision >= 0)
    {
        s << " Revision " << QString::number(deviceId.revision, 16).toUpper();
    }
    PrintMessage(msg);

    qDebug("total time: %fs", (double)totalTime.elapsed() / 1000);
    return 0;
}

void Bootload::Disconnect(void)
{
    if(comm->IsOpen())
    {
        comm->releaseBreak();
    }
    comm->releaseReset();
    comm->close();
}


int Bootload::LoadFile(QString fileName)
{
    QString msg;
    QTextStream stream(&msg);

    HexImporter import;
    HexImporter::ErrorCode result;
    result = import.ImportHexFile(fileName, deviceData, device);
    switch(result)
    {
        case HexImporter::Success:
            break;

        case HexImporter::CouldNotOpenFile:
            stream << "Could not open file: " << fileName;
            PrintMessage(msg);
            return -2;

        default:
            stream << "Failed to import: " << result;
            PrintMessage(msg);
            return -3;
    }

    if(writeConfig && import.hasConfigBits == false && device->hasConfig())
    {
        cout << "HEX file does not contain config bit settings. Skipping config write..." << endl;
        writeConfig = false;
    }

    if(device->startBootloader != 0)
    {
        if(device->HasValidResetVector(deviceData->ProgramMemory) == false)
        {
            if(device->family == Device::PIC16)
            {
                PrintMessage("The first four instructions do not appear to contain a valid\n"\
                               "instruction sequence for bootloader compatibility.\n\n" \
                               "Please modify your firmware to start by jumping to your main code:\n" \
                               " \tMOVLW\tHIGH(MainApplication)\n" \
                               " \tMOVWF\tPCLATH\n" \
                               " \tGOTO\tMainApplication\n" \
                               " MainApplication:\n" \
                               " \t(...)\n\n" \
                               "Without the jump sequence, the bootloader firmware will not be able\n"\
                               "to execute your application firmware.");
            }
            else
            {
                PrintMessage("The first instruction does not appear to be a GOTO instruction.\n\n"\
                               "Please modify your firmware to have a GOTO as the first instruction\n"\
                               "at address 0. Without the GOTO, bootloader firmware will not be able\n"\
                               "to execute your application firmware.");
            }
        }

        device->RemapResetVector(deviceData->ProgramMemory);
    }
    stream.setIntegerBase(10);

    if(device->family == Device::PIC24)
    {
        RemapInterruptVectors(device, deviceData);
    }

    msg.clear();
    return 0;
}

Comm::ErrorCode Bootload::RemapInterruptVectors(Device* device, DeviceData* deviceData)
{
    Comm::ErrorCode result;

    if(device->IVT == NULL)
    {
        device->IVT = new unsigned int[(device->endIVT - device->startIVT) / 2];
        result = deviceReader->ReadFlash(device->IVT, device->startIVT, device->endIVT);
        if(result)
        {
            return result;
        }
    }

    if(device->AIVT == NULL)
    {
        device->AIVT = new unsigned int[(device->endAIVT - device->startAIVT) / 2];
        result = deviceReader->ReadFlash(device->AIVT, device->startAIVT, device->endAIVT);
        if(result)
        {
            return result;
        }
    }

    unsigned int* hexIvt = &deviceData->ProgramMemory[device->startIVT>>1];
    unsigned int* devIvt = device->IVT;
    unsigned int devAddr, hexAddr;
    unsigned int gotoInstruction1, gotoInstruction2;
    for(unsigned int i = 0; i < (device->endIVT - device->startIVT) >> 1; i++, devIvt++, hexIvt++)
    {
        devAddr = *devIvt;
        hexAddr = *hexIvt;

        if(devAddr == hexAddr)
        {
            // The bootloader's IVT entry already matches the hex file's IVT entry,
            // so no remapping necessary for this one...
            continue;
        }

        gotoInstruction1 = 0x040000 | (hexAddr & 0xFFFF);
        gotoInstruction2 = hexAddr >> 16;

        if((deviceData->ProgramMemory[devAddr>>1] != 0xFFFFFF &&
            deviceData->ProgramMemory[devAddr>>1] != gotoInstruction1) ||
           (deviceData->ProgramMemory[(devAddr>>1) + 1] != 0xFFFFFF &&
            deviceData->ProgramMemory[(devAddr>>1) + 1] != gotoInstruction2))
        {
            qWarning("IVT: %d - Flash memory at %X is not empty for IVT remap goto: %06X %06X", i, devAddr, gotoInstruction1, gotoInstruction2);
            continue;
        }

        *hexIvt = devAddr;
        if(devAddr < device->endFLASH)
        {
            deviceData->ProgramMemory[devAddr>>1] = gotoInstruction1;
            deviceData->ProgramMemory[(devAddr>>1) + 1] = gotoInstruction2;
        }
    }


    hexIvt = &deviceData->ProgramMemory[device->startAIVT>>1];
    devIvt = device->AIVT;
    for(unsigned int i = 0; i < (device->endIVT - device->startIVT) >> 1; i++, devIvt++, hexIvt++)
    {
        devAddr = *devIvt;
        hexAddr = *hexIvt;

        if(devAddr == hexAddr)
        {
            // The bootloader's IVT entry already matches the hex file's IVT entry,
            // so no remapping necessary for this one...
            continue;
        }

        gotoInstruction1 = 0x040000 | (hexAddr & 0xFFFF);
        gotoInstruction2 = hexAddr >> 16;

        if((deviceData->ProgramMemory[devAddr>>1] != 0xFFFFFF &&
            deviceData->ProgramMemory[devAddr>>1] != gotoInstruction1) ||
           (deviceData->ProgramMemory[(devAddr>>1) + 1] != 0xFFFFFF &&
            deviceData->ProgramMemory[(devAddr>>1) + 1] != gotoInstruction2))
        {
            qWarning("AIVT: %d - Flash memory at %X is not empty for IVT remapping.", i, devAddr);
            continue;
        }

        *hexIvt = devAddr;
        if(devAddr < device->endFLASH)
        {
            deviceData->ProgramMemory[devAddr>>1] = gotoInstruction1;
            deviceData->ProgramMemory[(devAddr>>1) + 1] = gotoInstruction2;
        }
    }

    return Comm::Success;
}

int Bootload::EraseDevice(void)
{
    QString msg;
    QTextStream stream(&msg);
    QTime elapsed;
    elapsed.start();

    if(writeFlash)
    {
        QLinkedList<Device::MemoryRange> eraseList;

        writePlan->writeConfig = writeConfig;

        writePlan->planFlashErase(eraseList);

        QLinkedList<Device::MemoryRange>::iterator it;
        for(it = eraseList.begin(); it != eraseList.end(); ++it)
        {
            qWarning("Erasing (%X to %X]", it->end, it->start);
        }

        it = eraseList.begin();
        while(it != eraseList.end())
        {
            if(deviceWriter->EraseFlash(it->start, it->end) != Comm::Success)
            {
                return -4;
            }
            it++;
        }
    }

    stream.setIntegerBase(10);
    stream << "Erase device complete (" << ((double)elapsed.elapsed()) / 1000 << "s)";
    PrintMessage(msg);

    return 0;
}

int Bootload::WriteDevice(void)
{
    return WriteDevice(deviceData);
}

int Bootload::WriteDevice(DeviceData* newData, DeviceData* existingData)
{
    QString msg;
    QTextStream stream(&msg);
    QTime elapsed;
    double flashTime = 0, eepromTime = 0, configTime = 0;
    bool flash = false, eeprom = false, config = false;
    Comm::ErrorCode result;

    failed = 0;
    if(writeConfig && device->hasConfigAsFuses())
    {
        elapsed.start();
        failed = deviceWriter->WriteConfigFuses(newData->ConfigWords);
        config = true;
        configTime = ((double)elapsed.elapsed()) / 1000;
    }

    if(writeConfig && !writeFlash && device->hasConfigAsFlash())
    {
        // user did not check mark write FLASH option, but does
        // want to write FLASH config bits. Need to handle special case here..
        Device::MemoryRange preserve, write;

        write.start = device->endFLASH - device->eraseBlockSizeFLASH;
        write.end   = device->endFLASH;
        preserve.start = write.start;
        preserve.end = device->startConfig;

        elapsed.start();
        deviceWriter->writeConfig = writeConfig;
        deviceVerifier->writeConfig = writeConfig;

        // first, read FLASH memory that needs to be preserved...
        failed = deviceReader->ReadFlash(device->flashPointer(preserve.start, newData->ProgramMemory), preserve.start, preserve.end);

        // next, write the entire page back out
        if(failed == 0)
        {
            if(existingData != NULL)
            {
                failed = deviceWriter->WriteFlash(newData, write.start, write.end, existingData->ProgramMemory);
            }
            else
            {
                failed = deviceWriter->WriteFlash(newData, write.start, write.end);
            }
        }

        if(failed == 0)
        {
            result = deviceVerifier->VerifyFlash(newData->ProgramMemory, write.start, write.end);
            if(result != Comm::Success)
            {
                if(deviceVerifier->failList.count() != 0)
                {
                    if(existingData != NULL)
                    {
                        // incremental update failed, fall back to writing FLASH one more time
                        // without doing incremental bootloading this time...
                        existingData = NULL;
                        failed = deviceWriter->WriteFlash(newData, write.start, write.end);
                        result = deviceVerifier->VerifyFlash(newData->ProgramMemory, write.start, write.end);
                        if(result != Comm::Success)
                        {
                            if(deviceVerifier->failList.count() != 0)
                            {
                                // still couldn't write the device, fail for good
                                failed = 1;
                                deviceVerifier->eraseList.clear();
                            }
                        }
                    }
                    else
                    {
                        failed = 1;
                        deviceVerifier->eraseList.clear();
                    }
                }
            }
        }
        flash = true;
        flashTime = ((double)elapsed.elapsed()) / 1000;
    }

    if(failed == 0 && writeFlash)
    {
        int start = device->startFLASH, end = device->endFLASH;

        elapsed.start();
        deviceWriter->writeConfig = writeConfig;
        deviceVerifier->writeConfig = writeConfig;
        if(!writeConfig && device->hasConfigAsFlash())
        {
            // do not attempt to write the config words page of FLASH memory.
            end -= device->eraseBlockSizeFLASH;
        }

        if(existingData != NULL)
        {
            failed = deviceWriter->WriteFlash(newData, start, end, existingData->ProgramMemory);
        }
        else
        {
            failed = deviceWriter->WriteFlash(newData, start, end);
        }

        if(failed == 0)
        {
            result = deviceVerifier->VerifyFlash(newData->ProgramMemory, start, end);
            if(result != Comm::Success)
            {
                if(deviceVerifier->eraseList.count() == 0 &&
                   deviceVerifier->failList.count() == 0)
                {
                    // we didn't get any erase nor fail list entries, must've been
                    // a communications failure: flag it.
                    failed = 1;
                }

                if(deviceVerifier->failList.count() != 0)
                {
                    if(existingData != NULL)
                    {
                        // incremental update failed, fall back to writing FLASH one more time
                        // without doing incremental bootloading this time...
                        existingData = NULL;
                        failed = deviceWriter->WriteFlash(newData, start, end);
                        result = deviceVerifier->VerifyFlash(newData->ProgramMemory, start, end);
                        if(result != Comm::Success)
                        {
                            if(deviceVerifier->failList.count() != 0)
                            {
                                // still couldn't write the device, fail for good
                                failed = 1;
                                deviceVerifier->eraseList.clear();
                            }
                        }
                    }
                    else
                    {
                        failed = 1;
                        deviceVerifier->eraseList.clear();
                    }
                }

                if(deviceVerifier->eraseList.count() != 0)
                {
                    deviceWriter->EraseFlash(deviceVerifier->eraseList);
                }
            }
        }
        flash = true;
        flashTime = ((double)elapsed.elapsed()) / 1000;
    }

    if(failed == 0 && writeEeprom && device->hasEeprom())
    {
        elapsed.start();
        failed = deviceWriter->WriteEeprom(newData->EEPromMemory, device->startEEPROM, device->endEEPROM);
        eeprom = true;
        eepromTime = ((double)elapsed.elapsed()) / 1000;
    }

    if(failed == 0)
    {
        stream.setIntegerBase(10);
        stream << "Write complete (";
        if(flash)
        {
            stream << "FLASH " << flashTime << "s";
        }
        if(eeprom)
        {
            if(flashTime)
            {
                stream << ", ";
            }
            stream << "EEPROM " << eepromTime << "s";
        }
        if(config)
        {
            if(flash || eeprom)
            {
                stream << ", ";
            }
            stream << "Config " << configTime << "s";
        }
        stream << ")";
        PrintMessage(msg);
        return 0;
    }

    return -5;
}

int Bootload::VerifyDevice(bool verifyFlash)
{
    QString msg;
    QTextStream stream(&msg);
    QTime elapsed;
    double flashTime = 0, eepromTime = 0, configTime = 0;
    bool flash = false, eeprom = false, config = false;
    int i, readFlashFailed = 0, readEepromFailed = 0, readConfigFailed = 0;
    bool refreshViews = false;
    int flashFails = 0, eepromFails = 0, configFails = 0;
    unsigned int failAddress = device->startFLASH;
    Device::MemoryRange range;

    failed = 0;
    abortOperation = false;

    if(!writeFlash && writeConfig && device->hasConfigAsFlash() && verifyFlash)
    {
        // Only verify FLASH config bits.
        elapsed.start();
        range.start = device->startConfig;
        range.end = device->endConfig;
        readFlashFailed = deviceReader->ReadFlash(device->flashPointer(device->endFLASH - device->eraseBlockSizeFLASH, verifyData->ProgramMemory), device->endFLASH - device->eraseBlockSizeFLASH, range.end);
        countFlashVerifyFailures(flashFails, failAddress, range);
        refreshViews = true;

        flashTime = ((double)elapsed.elapsed()) / 1000;
        flash = true;
    }

    if(writeFlash && verifyFlash)
    {
        // Verify FLASH memory
        elapsed.start();
        unsigned int endAddress = device->endFLASH;
        if(!writeConfig && device->hasConfigAsFlash())
        {
            // we aren't trying to program FLASH config words, so don't bother doing a CRC
            // verify of the FLASH config words page.
            endAddress -= device->eraseBlockSizeFLASH;
        }
        deviceVerifier->writeConfig = writeConfig;

        // Use fast CRC verify
        if(deviceVerifier->VerifyFlash(deviceData->ProgramMemory, device->startFLASH, endAddress) != Comm::Success)
        {
            if(deviceVerifier->eraseList.count() == 0 &&
               deviceVerifier->failList.count() == 0)
            {
                // we didn't get any erase nor fail list entries, must've been
                // a communications failure: flag it.
                failed = 1;
            }
            else
            {
                flashFails = deviceVerifier->failList.count() + deviceVerifier->eraseList.count();
            }
        }

        flashTime = ((double)elapsed.elapsed()) / 1000;
        flash = true;
    }

    // Read EEPROM memory from device
    if(writeEeprom && device->hasEeprom())
    {
        elapsed.start();
        readEepromFailed  = deviceReader->ReadEeprom(&verifyData->EEPromMemory[0], device->startEEPROM, device->endEEPROM);
        eepromTime = ((double)elapsed.elapsed()) / 1000;
        eeprom = true;
    }

    // Read Config memory from device
    if(writeConfig && device->hasConfigReadCommand())
    {
        elapsed.start();
        readConfigFailed = deviceReader->ReadConfig(&verifyData->ConfigWords[0], device->startConfig, device->endConfig);
        configTime = ((double)elapsed.elapsed()) / 1000;
        config = true;
    }

    // Count up the number of failures
    if(readFlashFailed || readEepromFailed || readConfigFailed)
    {
        failed = -1;
    }
    else
    {
        unsigned int address;
        unsigned int word, suspectWord;

        stream.setIntegerBase(16);
        stream.setNumberFlags(QTextStream::UppercaseDigits);

        unsigned int* memory;
        unsigned int* suspectMemory;

        memory = &deviceData->EEPromMemory[0];
        suspectMemory = &verifyData->EEPromMemory[0];
        address = device->startEEPROM;
        while(writeEeprom && address < device->endEEPROM && abortOperation == false)
        {
            word = (*memory++) & 0xFF;
            suspectWord = (*suspectMemory++) & 0xFF;
            if(word != suspectWord)
            {
                if(flashFails == 0 && eepromFails == 0)
                {
                    failAddress = address;
                }

                eepromFails++;
            }
            address++;
        }

        if(writeConfig && device->hasConfigReadCommand())
        {
            for(i = 0; i < device->configWords.count(); i++)
            {
                address = device->configWords[i].address - device->startConfig;
                memory = &deviceData->ConfigWords[address >> 1];
                suspectMemory = &verifyData->ConfigWords[address >> 1];
                word = *memory;
                suspectWord = *suspectMemory;

                if(address & 1)
                {
                    if((word & 0xFF00) != (suspectWord & 0xFF00))
                    {
                        if(flashFails == 0 && eepromFails == 0 && configFails == 0)
                        {
                            failAddress = address + device->startConfig;
                        }

                        configFails++;
                    }
                }
                else
                {
                    if((word & 0xFF) != (suspectWord & 0xFF))
                    {
                        if(flashFails == 0 && eepromFails == 0 && configFails == 0)
                        {
                            failAddress = address + device->startConfig;
                        }

                        configFails++;
                    }
                }
            }
        }

        if(flashFails || eepromFails || configFails)
        {
            refreshViews = true;
            failed = -2;
            msg.clear();
            stream.setIntegerBase(16);
            stream << "Verify failed (";
            if(flashFails)
            {
                stream << flashFails << " FLASH ";
            }
            if(eepromFails)
            {
                if(flashFails)
                {
                    stream << ", ";
                }
                stream << eepromFails << " EEPROM ";
            }
            if(configFails)
            {
                if(flashFails || eepromFails)
                {
                    stream << ", ";
                }
                stream << configFails << " Config ";
            }
            stream << "failures)";
            PrintMessage(msg);
        }
        else if(failed == 0)
        {
            stream.setIntegerBase(10);
            if(flash)
            {
                stream << "Verify success (FLASH " << flashTime << "s";
                if(eeprom)
                {
                    stream << ", EEPROM " << eepromTime << "s";
                }
                if(config)
                {
                    stream << ", Config " << configTime << "s";
                }
                stream << ")";
                PrintMessage(msg);
            }
            else if(eeprom)
            {
                stream << "Verify success (EEPROM " << eepromTime << "s";
                if(config)
                {
                    stream << ", Config " << configTime << "s";
                }
                stream << ")";
                PrintMessage(msg);
            }
            else if(config)
            {
                stream << "Verify success (Config " << configTime << "s";
                stream << ")";
                PrintMessage(msg);
            }
        }
    }

    if(flashFails || readFlashFailed)
    {
        return -10;
    }

    if(eepromFails)
    {
        return -11;
    }

    if(configFails)
    {
        return -12;
    }

    return failed;
}

void Bootload::countFlashVerifyFailures(int& flashFails, unsigned int& failAddress, Device::MemoryRange range)
{
    unsigned int address = range.start;
    unsigned int word, suspectWord;
    unsigned int* memory;
    unsigned int* suspectMemory;

    failAddress = address;
    memory = device->flashPointer(address, deviceData->ProgramMemory);
    suspectMemory = device->flashPointer(address, verifyData->ProgramMemory);
    while(address < range.end && abortOperation == false)
    {
        word = (*memory++) & device->flashWordMask;
        suspectWord = (*suspectMemory++) & device->flashWordMask;
        if(!(address >= device->startBootloader && address < device->endBootloader))
        {
            if(word != suspectWord)
            {
                if(flashFails == 0)
                {
                    failAddress = address;
                }

                flashFails++;
            }
        }

        device->IncrementFlashAddressByInstructionWord(address);
    }
}

int Bootload::RunApplication(void)
{
    QTime elapsed;
    QString msg;

    if(!comm->IsOpen())
    {
        cout << "Using " << qPrintable(comm->serial->portName()) << " at " << comm->serial->baudRate() << " bps" << endl;
        if(comm->open() != Comm::Success)
        {
            Disconnect();
            msg.append("Could not open ");
            msg.append(comm->serial->portName());
            msg.append(".");
            PrintMessage(msg);
            return -1;
        }
    }

    comm->releaseBreak();

    elapsed.start();

    // assert RTS/MCLR# reset to get out of bootloader mode.
    comm->assertReset();
    // wait for 5ms to allow part to go into MCLR reset.
    while(elapsed.elapsed() < 5)
    {
    }

    // send a command to enter Application Mode in case
    // RTS/MCLR# reset is not wired up.
    comm->serial->clearReceiveBuffer();
    comm->RunApplication();

    comm->releaseReset();
    return 0;
}

int Bootload::AssertBreak(void)
{
    if(!comm->IsOpen())
    {
        cout << "Using " << qPrintable(comm->serial->portName()) << " at " << comm->serial->baudRate() << " bps" << endl;
        if(comm->open() != Comm::Success)
        {
            Disconnect();
            cout << "Could not open " << qPrintable(comm->serial->portName()) << "." << endl;
            return -1;
        }
    }

    comm->assertBreak();
    return 0;
}
