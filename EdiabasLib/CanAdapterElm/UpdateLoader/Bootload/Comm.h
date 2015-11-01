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
* E. Schlunder  2009/04/14  Initial code ported from VB app.
************************************************************************/

#ifndef COMM_H
#define COMM_H

#include "QextSerialPort/qextserialport.h"
#include "Device.h"

/*!
 * Provides low level serial bootloader communication.
 */
class Comm
{
protected:

public:
    QextSerialPort *serial;

    Comm();
    ~Comm();

    static const int SyncWaitTime;

    enum ErrorCode
    {
        Success = 0, Aborted, PortDoesNotExist, InvalidSettings, CouldNotTransmit,
        RetryLimitReached, NoAcknowledgement,
        ERROR_GEN_READWRITE, ERROR_READ_TIMEOUT, ERROR_BAD_CHKSUM, ERROR_INVALID_COMMAND,
        ERROR_BLOCK_TOO_SMALL, ERROR_PACKET_TOO_BIG, ERROR_BPA_TOO_SMALL, ERROR_BPA_TOO_BIG,
        JunkInsteadOfSTX
    };
    QString ErrorString(ErrorCode errorCode) const;

    struct DeviceId
    {
        unsigned int id;
        int revision;
    };

    struct BootInfo
    {
        unsigned char majorVersion;
        unsigned char minorVersion;
        unsigned char familyId;
        unsigned int commandMask;
        unsigned int startBootloader;
        unsigned int endBootloader;
        unsigned int deviceId;
    };

    ErrorCode open(void);
    void close(void);
    bool IsOpen(void);

    void assertReset(void);
    void releaseReset(void);
    void assertBreak(void);
    void releaseBreak(void);
    void releaseIntoBootloader(void);
    void ActivateBootloader(); // [UH]

    ErrorCode GetPacket(QByteArray& packetData, int timeout = 1000);
    ErrorCode GetCrcData(QByteArray& packetData);
    ErrorCode SendPacket(const QByteArray& sendPacket);
    ErrorCode SendGetPacket(const QByteArray& sendPacketData, QByteArray& getPacketData, int retryLimit, int timeout = 1000);
    void SendETX(void);
    int XferMilliseconds(int bytes);

    BootInfo ReadBootloaderInfo(int timeout = 10);
    DeviceId ReadDeviceID(Device::Families deviceFamily);
    Comm::ErrorCode RunApplication(void);
    Comm::ErrorCode SetNonce(unsigned int nonce);

    QString baudRate(void);
};

#endif // COMM_H
