/************************************************************************
* Copyright (c) 2010-2011,  Microchip Technology Inc.
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
* E. Schlunder  2010/02/13  Moved SQL code out of Device class so that
*                           XML converter tool can avoid linking to
*                           Qt SQL libraries.
************************************************************************/

#include <QtSql>

#include "DeviceSqlLoader.h"

#define DBFILE "devices.db"

DeviceSqlLoader::DeviceSqlLoader()
{
}

DeviceSqlLoader::ErrorCode DeviceSqlLoader::loadDevice(Device* device, QString partName)
{
    QString msg;
    bool deviceFound = false;

    device->setUnknown();

    // Try to find the "devices.db" database in the current working directory.
    QString deviceDatabaseFile = DBFILE;
    QFileInfo fi(deviceDatabaseFile);
    if(fi.exists() == false)
    {
        // Didn't find it, try looking in the application's EXE directory.
        deviceDatabaseFile = QCoreApplication::applicationDirPath() + QDir::separator() + DBFILE;
        if(QFileInfo(deviceDatabaseFile).exists() == false)
        {
            return DatabaseMissing;
        }
    }

    // the following opening code block provides a limited stack context for the
    // database connection, allowing us to disconnect from the database when
    // we are done.
    {
        QSqlDatabase db = QSqlDatabase::addDatabase("QSQLITE", "devices");
        db.setDatabaseName(deviceDatabaseFile);
        db.open();
        db.transaction();
        QSqlQuery qry(db);
        qry.setForwardOnly(true);
        msg = "select DEVICEID, FAMILYID\n" \
                    "from DEVICES\n" \
                    "where PARTNAME = :partName";
        qry.prepare(msg);
        msg.clear();

        qry.bindValue(0, partName);
        qry.exec();
        deviceFound = qry.next();
        if(deviceFound)
        {
            device->id = Device::toInt(qry.value(0));
            device->family = (Device::Families)Device::toInt(qry.value(1));
        }
        db.close();
    }
    QSqlDatabase::removeDatabase("devices");

    if(deviceFound)
    {
        return loadDevice(device, device->id, device->family);
    }

    return DeviceMissing;
}

QStringList DeviceSqlLoader::findDevices(QString query)
{
    QStringList results;
    QString msg;

    // Try to find the "devices.db" database in the current working directory.
    QString deviceDatabaseFile = DBFILE;
    QFileInfo fi(deviceDatabaseFile);
    if(fi.exists() == false)
    {
        // Didn't find it, try looking in the application's EXE directory.
        deviceDatabaseFile = QCoreApplication::applicationDirPath() + QDir::separator() + DBFILE;
        if(QFileInfo(deviceDatabaseFile).exists() == false)
        {
            return results;
        }
    }

    // the following opening code block provides a limited stack context for the
    // database connection, allowing us to disconnect from the database when
    // we are done.
    {
        QSqlDatabase db = QSqlDatabase::addDatabase("QSQLITE", "devices");
        db.setDatabaseName(deviceDatabaseFile);
        db.open();
        db.transaction();
        QSqlQuery qry(db);
        qry.setForwardOnly(true);
        msg = "select PARTNAME from DEVICES where PARTNAME like :query order by PARTNAME";
        qry.prepare(msg);
        msg.clear();

        qry.bindValue(0, "%" + query + "%");
        qry.exec();
        while(qry.next())
        {
            results.append(qry.value(0).toString());
        }
        qry.clear();

        db.commit();
        db.close();
    }
    QSqlDatabase::removeDatabase("devices");

    return results;
}

DeviceSqlLoader::ErrorCode DeviceSqlLoader::loadDevice(Device* device, int deviceId, Device::Families familyId)
{
    unsigned int deviceRowId;
    int i, j, t;
    QString msg;
    bool deviceFound = false;

    device->setUnknown();
    device->id = deviceId;

    // Try to find the "devices.db" database in the current working directory.
    QString deviceDatabaseFile = DBFILE;
    QFileInfo fi(deviceDatabaseFile);
    if(fi.exists() == false)
    {
        // Didn't find it, try looking in the application's EXE directory.
        deviceDatabaseFile = QCoreApplication::applicationDirPath() + QDir::separator() + DBFILE;
        if(QFileInfo(deviceDatabaseFile).exists() == false)
        {
            return DatabaseMissing;
        }
    }

    // the following opening code block provides a limited stack context for the
    // database connection, allowing us to disconnect from the database when
    // we are done.
    {
        QSqlDatabase db = QSqlDatabase::addDatabase("QSQLITE", "devices");
        db.setDatabaseName(deviceDatabaseFile);
        db.open();
        db.transaction();
        QSqlQuery qry(db);
        qry.setForwardOnly(true);
        msg = "select PARTNAME, WRITEFLASHBLOCKSIZE, ERASEFLASHBLOCKSIZE, STARTFLASH, ENDFLASH," \
                    "STARTEE, ENDEE, STARTUSER, ENDUSER, STARTCONFIG, ENDCONFIG, STARTGPR, ENDGPR," \
                    "BYTESPERWORDFLASH, FAMILYID, DEVICEROWID\n" \
                    "from DEVICES\n" \
                    "where ";
        if(familyId != Device::Unknown)
        {
            msg.append(" FAMILYID = ");
            msg.append(QString::number((int)familyId));
            msg.append(" and ");
        }
        msg.append("DEVICEID = :deviceId");
        qry.prepare(msg);
        msg.clear();

        qry.bindValue(0, deviceId);
        qry.exec();
        deviceFound = qry.next();
        if(deviceFound)
        {
            device->name = qry.value(0).toString();
            device->writeBlockSizeFLASH = Device::toInt(qry.value(1));
            device->eraseBlockSizeFLASH = Device::toInt(qry.value(2));
            device->startFLASH  = Device::toInt(qry.value(3));
            device->endFLASH    = Device::toInt(qry.value(4));
            device->startEEPROM = Device::toInt(qry.value(5));
            device->endEEPROM   = Device::toInt(qry.value(6));
            device->startUser   = Device::toInt(qry.value(7));
            device->endUser     = Device::toInt(qry.value(8));
            device->startConfig = Device::toInt(qry.value(9));
            device->endConfig   = Device::toInt(qry.value(10));
            device->startGPR    = Device::toInt(qry.value(11));
            device->endGPR      = Device::toInt(qry.value(12));
            device->bytesPerWordFLASH = Device::toInt(qry.value(13));
            device->family = (Device::Families)Device::toInt(qry.value(14));
            switch(device->family)
            {
                case Device::PIC16:
                    device->bytesPerAddressFLASH = 2;
                    device->bytesPerWordEEPROM = 1;
                    device->flashWordMask = 0x3FFF;
                    device->configWordMask = 0xFF;

                    break;

                case Device::PIC24:
                    device->bytesPerAddressFLASH = 2;
                    device->bytesPerWordEEPROM = 2;
                    device->flashWordMask = 0xFFFFFF;
                    device->configWordMask = 0xFFFF;
                    device->writeBlockSizeFLASH *= 2;       // temporary
                    device->eraseBlockSizeFLASH *= 2;
                    break;

                case Device::PIC32:
                    device->flashWordMask = 0xFFFFFFFF;
                    device->configWordMask = 0xFFFFFFFF;
                    device->bytesPerAddressFLASH = 1;
                    break;

                case Device::PIC18:
                default:
                    device->flashWordMask = 0xFFFF;
                    device->configWordMask = 0xFF;
                    device->bytesPerAddressFLASH = 1;
                    device->bytesPerWordEEPROM = 1;
            }
            deviceRowId = Device::toUInt(qry.value(15));
            qry.clear();

            qry.prepare("select ROWID, CONFIGNAME, ADDRESS, DEFAULTVALUE, IMPLEMENTEDBITS\n" \
                        "from CONFIGWORDS\n" \
                        "where DEVICEROWID = :deviceRowId\n" \
                        "order by ADDRESS");
            qry.bindValue(0, deviceRowId);
            qry.exec();
            Device::ConfigWord config;
            while(qry.next())
            {
                config.rowId = Device::toUInt(qry.value(0));
                config.name = qry.value(1).toString();
                config.address = Device::toUInt(qry.value(2));
                config.defaultValue = Device::toUInt(qry.value(3));
                config.implementedBits = Device::toUInt(qry.value(4));

                device->configWords.append(config);
            }
            qry.clear();

            qry.prepare("select ROWID, FIELDCNAME, DESCRIPTION, CONFIGWORDID\n" \
                        "from CONFIGFIELDS\n" \
                        "where DEVICEROWID = :deviceRowId\n" \
                        "order by CONFIGWORDID");
            qry.bindValue(0, deviceRowId);
            qry.exec();
            Device::ConfigField field;
            Device::ConfigWord* configWord = NULL;
            unsigned int id;
            while(qry.next())
            {
                field.rowId = Device::toUInt(qry.value(0));
                field.cname = qry.value(1).toString();
                field.description = qry.value(2).toString();
                id = Device::toUInt(qry.value(3).toString());

                if(configWord == NULL || configWord->rowId != id)
                {
                    for(i = 0; i < device->configWords.count(); i++)
                    {
                        if(device->configWords[i].rowId == id)
                        {
                            configWord = &device->configWords[i];
                            break;
                        }
                    }
                }
                configWord->fields.append(field);
            }
            qry.clear();

            qry.prepare("select SETTINGCNAME, DESCRIPTION, BITMASK, BITVALUE, CONFIGFIELDID\n" \
                        "from CONFIGSETTINGS\n" \
                        "where DEVICEROWID = :deviceRowId\n" \
                        "order by CONFIGFIELDID");
            qry.bindValue(0, deviceRowId);
            qry.exec();
            Device::ConfigFieldSetting setting;
            Device::ConfigField* parent = NULL;
            i = 0;
            bool searching;
            while(qry.next())
            {
                setting.cname = qry.value(0).toString();
                setting.description = qry.value(1).toString();
                setting.bitMask = Device::toUInt(qry.value(2));
                setting.bitValue = Device::toUInt(qry.value(3));
                id = Device::toUInt(qry.value(4));

                if(parent == NULL || parent->rowId != id)
                {
                    searching = true;
                    for(t = 0; searching && t < device->configWords.count(); t++, i++)
                    {
                        if(i >= device->configWords.count())
                        {
                            i = 0;
                        }
                        configWord = &device->configWords[i];

                        for(j = 0; searching && j < configWord->fields.count(); j++)
                        {
                            if(configWord->fields[j].rowId == id)
                            {
                                parent = &configWord->fields[j];
                                searching = false;
                            }
                        }
                    }
                }
                parent->settings.append(setting);
            }
        }
        qry.clear();

        db.commit();
        db.close();
    }
    QSqlDatabase::removeDatabase("devices");

    if(!deviceFound)
    {
        return DeviceMissing;
    }

    return Success;
}
