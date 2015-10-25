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
* E. Schlunder  2009/04/14  Initial code ported from AN851 VB app.
************************************************************************/

#include "Comm.h"
#include "BootPackets.h"
#include "Crc.h"

#include "QextSerialPort/qextserialport.h"
#include "QextSerialPort/qextserialbase.h"

#include <QByteArray>
#include <QCoreApplication>
#include <QTime>
#include <QThread>

const int Comm::SyncWaitTime = 50;


Comm::Comm()
{
    serial = new QextSerialPort();
}

Comm::~Comm()
{
    if(serial != NULL)
    {
        delete serial;
    }
}

QString Comm::baudRate(void)
{
    switch(serial->baudRate())
    {
        case BAUD9600:
            return "9600";

        case BAUD19200:
            return "19200";

        case BAUD38400:
            return "38400";

        case  BAUD57600:
            return "57600";

        case BAUD115200:
            return "115200";

        case BAUD128000:
            return "128000";

        case BAUD230400:
            return "230400";

        case BAUD250000:
            return "250000";

        case BAUD460800:
            return "460800";

        case BAUD500000:
            return "500000";

        case BAUD614400:
            return "614400";

        case BAUD750000:
            return "750000";

        case BAUD921600:
            return "921600";

        case BAUD1000000:
            return "1000000";

        case BAUD1228800:
            return "1228800";

        case BAUD2000000:
            return "2000000";

        case BAUD2457600:
            return "2457600";

        case BAUD3000000:
            return "3000000";

        case BAUD6000000:
            return "6000000";

        default:
            return QString::number(serial->baudRate());
    }
}

/*!
 * Calculates the minimum number of milliseconds required to transmit/receive the
 * specified number of bytes at the current baud rate.
 */
int Comm::XferMilliseconds(int bytes)
{
    unsigned int bps = serial->baudRate();
    unsigned int bits = bytes * 10; // each byte is 8 bits long, plus start and stop bits
    return ((bits * 1000) / bps);
}

bool Comm::IsOpen(void)
{
    return serial->isOpen();
}

Comm::ErrorCode Comm::open(void)
{
    serial->setFlowControl(FLOW_OFF);
    serial->setParity(PAR_NONE);
    serial->setDataBits(DATA_8);
    serial->setStopBits(STOP_1);
    serial->open(QIODevice::ReadWrite | QIODevice::Unbuffered);
    if(serial->isOpen())
    {
        return Success;
    }

    return InvalidSettings;
}

void Comm::close(void)
{
    serial->close();
}

void Comm::assertReset(void)
{
    serial->setRts(true);
    serial->setDtr(false);
}

void Comm::releaseReset(void)
{
    serial->setRts(false);
    serial->setDtr(true);
}

void Comm::assertBreak(void)
{
    serial->flush();
    serial->setBreak(true);
}

void Comm::releaseBreak(void)
{
    serial->setBreak(false);
}

/*!
 * Releases the microcontroller out of reset and into bootloader mode.
 */
void Comm::releaseIntoBootloader(void)
{
    QTime elapsed;

    releaseReset();
    // wait 25ms to allow MCLR to release
    elapsed.start();
    while(elapsed.elapsed() < 35) // ms
    {
        QCoreApplication::processEvents();
    }
    releaseBreak();

    // wait 35ms to allow RXD to go into IDLE state, crystal to stabilize, PLL to lock, etc..
    elapsed.start();
    while(elapsed.elapsed() < 35) // ms
    {
        QCoreApplication::processEvents();
    }
}

// [UH] enable bootloader mode
void Comm::ActivateBootlader()
{
    QByteArray resetCmd;

    serial->flush();
    resetCmd.resize(6);
    resetCmd[0] = 0x82;
    resetCmd[1] = 0xF1;
    resetCmd[2] = 0xF1;
    resetCmd[3] = 0xFF;
    resetCmd[4] = 0xFF;
    resetCmd[5] = 0x62;

    serial->write(resetCmd);
}

/**
 * This function captures data from the bootloader, strips out special control characters, and
 * verifies that the packet CRC is correct.
 */
Comm::ErrorCode Comm::GetPacket(QByteArray& receivePacket, int timeout)
{
    QByteArray rawData;
    int errorCount = 0;
    QTime elapsed;
    char byte;
    Crc crc;
    int size, i;
    unsigned int packetCrc;

    // Scan for a start condition
    int junkReceived = 0;
    elapsed.start();
    for(;;)
    {
        if (serial->read(&byte, 1) != 1)
        {
            if(elapsed.elapsed() > timeout)
            {
                return ERROR_READ_TIMEOUT;
            }
            else
            {
                continue;
            }
        }

        if (byte == STX)
        {
            break;
        }
        else
        {
            junkReceived++;
            if(junkReceived < 10)
            {
                qWarning("Expected STX, received junk: %X", (unsigned char) byte);
            }
            else if(junkReceived == 10)
            {
                qWarning("Expected STX, received junk: %X... (further messages suppressed)", (unsigned char) byte);
            }
            else if(junkReceived > 100)
            {
                return JunkInsteadOfSTX;
            }
        }
    }

    // Get the data and unstuff when necessary
    receivePacket.clear();
    elapsed.start();
    for(;;)
    {
        rawData = serial->read(serial->bytesAvailable());
        if(rawData.size() == 0)
        {
            if(elapsed.elapsed() > timeout)
            {
                return ERROR_READ_TIMEOUT;
            }
            continue;
        }

        size = rawData.size();
        for(i = 0; i < size; i++)
        {
            byte = rawData[i];
            if(byte == DLE)
            {
                i++;
                if(i < size)
                {
                    byte = rawData[i];
                }
                else
                {
                    if (serial->read(&byte, 1) != 1)
                    {
                        return ERROR_READ_TIMEOUT;
                    }
                }
            }
            else
            {
                if(byte == STX)
                {
                    errorCount++;
                    if(errorCount > 5)
                    {
                        qWarning("Too many errors, aborting.");
                        return ERROR_READ_TIMEOUT;
                    }
                    else
                    {
                        qWarning("Unexpected STX received, restarting receive packet.");
                        receivePacket.clear();
                        elapsed.start();
                        continue;
                    }
                }

                if(byte == ETX)
                {
                    if(receivePacket.count() < 2)
                    {
                        return ERROR_BAD_CHKSUM;
                    }

                    size = receivePacket.count() - 2;
                    packetCrc = (unsigned char)receivePacket[size+1];
                    packetCrc <<= 8;
                    packetCrc |= (unsigned char)receivePacket[size];
                    receivePacket.chop(2);      // chop off the CRC, nobody above us needs that

                    if(crc.Value() != packetCrc)
                    {
                        qWarning("Packet CRC: %X Computed CRC: %X Length: %d", packetCrc, crc.Value(), size);
                        return ERROR_BAD_CHKSUM;
                    }

                    return Success;
                }
            }
            receivePacket.append(byte);
            if(receivePacket.size() > 2)
            {
                crc.Add(receivePacket[receivePacket.size() - 3]);
            }
        }
    }
}

/**
 * This function captures data from the bootloader and strips out special control characters. It
 * does not attempt to verify CRC data.
 */
Comm::ErrorCode Comm::GetCrcData(QByteArray& receivePacket)
{
    int timeout = 4000; // maximum ms to wait between characters received before aborting as timed out
    QByteArray rawData;
    int errorCount = 0;
    QTime elapsed;
    char byte;
    int size, i;

    elapsed.start();
    while(serial->bytesAvailable() == 0)
    {
        if(elapsed.elapsed() > timeout)
        {
            return ERROR_READ_TIMEOUT;
        }

        QCoreApplication::processEvents();
    }

    // Scan for a start condition
    int junkReceived = 0;
    for(;;)
    {
        if (serial->read(&byte, 1) != 1)
        {
            return ERROR_READ_TIMEOUT;
        }

        if (byte == STX)
        {
            break;
        }
        else
        {
            junkReceived++;
            if(junkReceived < 10)
            {
                qWarning("Expected STX, received junk: %X", (unsigned char) byte);
            }
            else if(junkReceived == 10)
            {
                qWarning("Expected STX, received junk: %X... (further messages suppressed)", (unsigned char) byte);
            }
            else if(junkReceived > 100)
            {
                return JunkInsteadOfSTX;
            }
        }
    }

    // Get the data and unstuff when necessary
    receivePacket.clear();
    elapsed.start();
    for(;;)
    {
        rawData = serial->read(serial->bytesAvailable());
        if(rawData.size() == 0)
        {
            if(elapsed.elapsed() > timeout)
            {
                return ERROR_READ_TIMEOUT;
            }
            continue;
        }

        elapsed.restart(); // we got some data, don't timeout

        size = rawData.size();
        for(i = 0; i < size; i++)
        {
            byte = rawData[i];
            if(byte == DLE)
            {
                if(++i < size)
                {
                    byte = rawData[i];
                }
                else
                {
                    if (serial->read(&byte, 1) != 1)
                    {
                        return ERROR_READ_TIMEOUT;
                    }
                }
            }
            else
            {
                if(byte == STX)
                {
                    errorCount++;
                    if(errorCount > 5)
                    {
                        qWarning("Too many errors, aborting.");
                        return ERROR_READ_TIMEOUT;
                    }
                    else
                    {
                        qWarning("Unexpected STX received, restarting receive packet.");
                        receivePacket.clear();
                        elapsed.start();
                        continue;
                    }
                }

                if(byte == ETX)
                {
                    return Success;
                }
            }
            receivePacket.append(byte);
        }
    }
}

/**
 * This function sends a packet. It supports bit-banged and half-duplex serial 
 * bootloaders by waiting for the STX response before sending the rest of the 
 * packet data back-to-back.
 *
 * @author edwards
 * @date 07/07/2010
 */
Comm::ErrorCode Comm::SendPacket(const QByteArray& sendPacket)
{
    QTime elapsed;

    // send the STX
    if(serial->write(sendPacket.left(1)) != 1)
    {
        return CouldNotTransmit;
    }

    // wait for the responding STX echoed back
    elapsed.start();
    while(serial->bytesAvailable() == 0)
    {
        if(elapsed.elapsed() > SyncWaitTime * 100)
        {
            return ERROR_READ_TIMEOUT;
        }

        QCoreApplication::processEvents();
#ifdef Q_WS_WIN
        Sleep(100);
#else
        timeval sleepTime;
        sleepTime.tv_sec = 0;
        sleepTime.tv_usec = 100*1000;
        select(0, NULL, NULL, NULL, &sleepTime);
#endif

    }

    //qWarning("Received STX, sending packet. %fs", (double)elapsed.elapsed() / 1000);

    // now we are free to send the rest of the packet
    if(serial->write(sendPacket.mid(1)) != (sendPacket.size() - 1))
    {
        return CouldNotTransmit;
    }

    return Success;
}

/**
 * This function is a combined function of the SendPacket and
 * GetPacket functions. A retry option has been added to allow
 * retransmission and reception in the event of normal
 * communications failure.
 *
 * @param timeOut Number of milliseconds to wait for a response before returning with a timeout error.
 */
Comm::ErrorCode Comm::SendGetPacket(const QByteArray& sendPacket, QByteArray& receivePacket, int retryLimit, int timeOut)
{
    Comm::ErrorCode RetStatus;
    QTime elapsed;
    int txTime = XferMilliseconds(sendPacket.length());

    // send the STX
    if(serial->write(sendPacket.left(1)) != 1)
    {
        return CouldNotTransmit;
    }

    // wait for the responding STX echoed back
    elapsed.start();
    while(serial->bytesAvailable() == 0)
    {
        if(elapsed.elapsed() > SyncWaitTime)
        {
            return ERROR_READ_TIMEOUT;
        }

        QCoreApplication::processEvents();
    }

    //qWarning("Received STX, sending packet. %fs", (double)elapsed.elapsed() / 1000);

    // now we are free to send the rest of the packet
    while(retryLimit-- >= 0)
    {
        if (serial->write(sendPacket.mid(1)) != (sendPacket.size() - 1))
        {
            continue;   // could not send packet, try again or abort...
        }

        RetStatus = GetPacket(receivePacket, txTime + timeOut);
        switch (RetStatus)
        {
            case ERROR_READ_TIMEOUT:
            case ERROR_BAD_CHKSUM:
                continue;

            default:
                return RetStatus;
        }
    }

    return RetryLimitReached;
}

void Comm::SendETX(void)
{
    serial->putChar(ETX);
}

QString Comm::ErrorString(ErrorCode errorCode) const
{
    switch(errorCode)
    {
        case ERROR_BAD_CHKSUM:
            return "Bad checksum";

        case ERROR_READ_TIMEOUT:
            return "Timed out";

        case RetryLimitReached:
            return "Retry limit reached";

        case JunkInsteadOfSTX:
            return "Received junk instead of STX";

        default:
            return QString::number(errorCode);
    }
}

Comm::BootInfo Comm::ReadBootloaderInfo(int timeout)
{
    QByteArray sendPacket;
    QByteArray response, sync;
    BootInfo bootInfo;
    int junkReport = 25;
    ErrorCode result;

    QTime elapsed;

    bootInfo.majorVersion = 0;
    bootInfo.minorVersion = 0;
    bootInfo.familyId = 0;
    bootInfo.commandMask = 0;
    bootInfo.startBootloader = 0;
    bootInfo.endBootloader = 0;
    bootInfo.deviceId = 0;

    elapsed.start();
    // clean out all pending incoming data from the serial port
    if(serial->bytesAvailable())
    {
        serial->clearReceiveBuffer();
    }

    // Now send STX start of transmission characters until the PIC responds back with
    // an STX character, then we know it has sync'ed up with our baud rate.
    sync[0] = STX;
    serial->write(sync);
    qWarning("write 1. %fs", (double)elapsed.elapsed() / 1000);

    BootloaderInfoPacket cmd;
    cmd.FramePacket(sendPacket);
    sendPacket.remove(0, 1);

    for(;;)
    {
        while(serial->bytesAvailable() < 1)
        {
            if(timeout <= 0)
            {
                return bootInfo;
            }

            if(elapsed.elapsed() > SyncWaitTime)
            {
                qWarning("Timed out waiting for sync, sending another STX. timeout: %d", timeout);
                serial->write(sync);
                elapsed.start();
                timeout--;
            }
        }

        response = serial->peek(1);
        if(response == sync)
        {
            qWarning("Received STX, sending packet. %fs", (double)elapsed.elapsed() / 1000);
            if(serial->write(sendPacket) != sendPacket.size())
            {
                qWarning("could not transmit bootinfo request");
                return bootInfo;
            }
            qWarning("send packet. %fs", (double)elapsed.elapsed() / 1000);
            break;
        }
        else
        {
            serial->read(1);
            if(junkReport)
            {
                qWarning("Received junk %x %fs", (unsigned char) response[0], (double)elapsed.elapsed() / 1000);
                junkReport--;
            }
        }
    }

    elapsed.start();
    response.clear();
    result = GetPacket(response);
    qWarning("get packet: %fs", (double)elapsed.elapsed() / 1000);
    if(result != Success)
    {
        qWarning(ErrorString(result).toAscii());
        return bootInfo;
    }

    bootInfo.minorVersion = (unsigned char)response[3];
    bootInfo.majorVersion = (unsigned char)response[2];
    bootInfo.familyId = response[5] & 0x0F;
    bootInfo.commandMask = (unsigned char)response[4] |
                           (((unsigned char)response[5] & 0xF0) << 4);
    bootInfo.startBootloader = ((unsigned char)response[6] & 0xFF) |
                               (((unsigned char)response[7] & 0xFF) << 8) |
                               (((unsigned char)response[8] & 0xFF) << 16) |
                               (((unsigned char)response[9] & 0xFF) << 24);
    bootInfo.endBootloader  = bootInfo.startBootloader;
    bootInfo.endBootloader += (unsigned char)response[0];
    bootInfo.endBootloader += (unsigned char)response[1] << 8;
    if(response.size() > 10)
    {
        bootInfo.deviceId  = (unsigned char)response[10];
        bootInfo.deviceId += (unsigned char)response[11] << 8;
    }
    qWarning("Firmware v%d.%02d at address %x to %x.", bootInfo.majorVersion, bootInfo.minorVersion,
             bootInfo.startBootloader, bootInfo.endBootloader);

    return bootInfo;
}

Comm::DeviceId Comm::ReadDeviceID(Device::Families deviceFamily)
{
    QByteArray sendPacket, response;
    QString ReadVersion;
    Comm::DeviceId result;

    ReadFlashPacket cmd;

    switch(deviceFamily)
    {
        case Device::PIC32:
            cmd.setAddress(0xBF80F220);
            cmd.setBytes(1);
            cmd.FramePacket(sendPacket);

            if(SendGetPacket(sendPacket, response, 4) != Success)
            {
                result.id = 0;
                result.revision = 0;
                return result;
            }

            result.id = ((response[2] & 0x0F) << 4) | ((response[1] & 0xF0) >> 4);
            result.revision = (response[3] << 4) | ((response[2] & 0xF0) >> 4);
            break;

        case Device::PIC24:
            cmd.setAddress(0xFF0000);
            cmd.setBytes(2);
            cmd.FramePacket(sendPacket);

            if(SendGetPacket(sendPacket, response, 4) != Success)
            {
                result.id = 0;
                result.revision = 0;
                return result;
            }

            result.id = response[0] | (response[1] << 8);
            result.revision = response[3] | ((response[4] & 0x01) << 8);
            break;

        case Device::PIC18:
        default:
            cmd.setAddress(0x3FFFFE);
            cmd.setBytes(2);
            cmd.FramePacket(sendPacket);

            if(SendGetPacket(sendPacket, response, 4) != Success)
            {
                result.id = 0;
                result.revision = 0;
                return result;
            }

            result.id = ((response[1] & 0xFF) << 8 | (response[0] & 0xFF)) >> 5;
            result.revision = response[0] & 0x1F;
            break;
    }

    return result;
}

Comm::ErrorCode Comm::SetNonce(unsigned int nonce)
{
    QByteArray sendPacket, receivePacket;
    SetNoncePacket cmd;

    cmd.setNonce(nonce);
    cmd.FramePacket(sendPacket);

    return SendGetPacket(sendPacket, receivePacket, 2);
}


Comm::ErrorCode Comm::RunApplication(void)
{
    QByteArray sendPacket;
    ErrorCode result;
    char response;

    RunApplicationPacket cmd;
    cmd.FramePacket(sendPacket);
    result = SendPacket(sendPacket);
    if(result != Success)
    {
        return result;
    }

    serial->read(&response, 1);
    if(response != STX)
    {
        return NoAcknowledgement;
    }
    return Success;
}
