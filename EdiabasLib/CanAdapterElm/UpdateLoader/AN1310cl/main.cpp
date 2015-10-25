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

#include <QtCore/QCoreApplication>
#include <QTimer>
#include <QStringList>

#include "Bootload.h"
#include "../version.h"

void MessageOutput(QtMsgType type, const char *msg);
bool quiet = false;

int main(int argc, char *argv[])
{
    qInstallMsgHandler(MessageOutput);
    QCoreApplication a(argc, argv);
    Bootload w;

    QCoreApplication::setOrganizationName("Microchip");
    QCoreApplication::setOrganizationDomain("microchip.com");
    QCoreApplication::setApplicationName("Serial Bootloader");

    bool erase = false;
    bool program = false;
    bool eeprom = false;
    bool config = false;
    bool verify = false;
    bool run = false;
    bool assertBreak = false;

    QString hexFile;
    QString device = "COM1";
    QString baudRate = "19200";

    QStringList args = a.arguments();
    for(int i = 0; i < args.length(); i++)
    {
        QString arg = args[i].toLower();

        if(arg.length() >= 2 && (arg[0] == '/' || arg[0] == '-'))
        {
            switch(arg[1].toLatin1())
            {
            case 'a':
                assertBreak = true;
                break;

            case 'e':
                erase = true;                
                break;

            case 'p':
                program = true;
                break;

            case 'c':
                config = true;
                break;

            case 'm':
                eeprom = true;
                break;

            case 'v':
                verify = true;
                break;

            case 'r':
                run = true;
                break;

            case 'q':
                quiet = true;
                break;

            case 'd':
                i++;
                if(i < args.length())
                {
                    device = args[i];
                }
                break;

            case 'b':
                i++;
                if(i < args.length())
                {
                    baudRate = args[i];
                }
                break;

            default:
                hexFile = args[i];
            }
        }
        else
        {
            hexFile = args[i];
        }
    }
    w.writeEeprom = eeprom;
    w.writeFlash = program || erase || verify;
    w.writeConfig = config;

    if(!quiet)
    {
        cout << "Serial Bootloader AN1310 v" << VERSION << "\n"
             << "Copyright (c) 2010-2011, Microchip Technology Inc.\n" << endl;
    }

    if(erase || program || eeprom || config || verify || run || assertBreak)
    {
        int result;

        a.processEvents();
        w.SetPort(device);
        if(baudRate.length())
        {
           w.SetBaudRate(baudRate);
        }

        if(assertBreak)
        {
            result = w.AssertBreak();
            if(result)
            {
                return result;
            }

            cout << "BREAK asserted. Manually reset device, then hit CTRL-C to exit when ready." << endl;

            return a.exec();
        }

        result = w.Connect();
        a.processEvents();
        if(result)
        {
            return result;
        }

        if((program || verify || eeprom || config) && !hexFile.isEmpty())
        {
            result = w.LoadFile(hexFile);
            a.processEvents();
            if(result)
            {
                return result;
            }
        }

        if(erase)
        {
            result = w.EraseDevice();
            if(result)
            {
                return result;
            }
        }

        if(program || eeprom || config)
        {
            result = w.WriteDevice();
            if(result)
            {
                return result;
            }
        }

        if(verify)
        {
            result = w.VerifyDevice();
            if(result)
            {
                return result;
            }
        }

        if(run)
        {
            result = w.RunApplication();
            if(result)
            {
                return result;
            }
        }

        return 0;
    }
    else
    {
        cout << "Usage:\n"
             << "   AN1310cl -d DEVICE [-b BAUDRATE] [-e] [-p] [-m] [-c] [-v] [-a] [-r] [filename.hex]\n\n"
             << "Options:\n"
             << "   -d DEVICE - Specifies which COM port to use (COM1 default)\n"
             << "   -b BAUDRATE - Specify what baudrate to attempt to use (115200bps default)\n"
             << "   -e - Erase device to a blank state\n"
             << "   -p - Write program FLASH memory using specified hex file contents\n"
             << "   -m - Write EEPROM memory using specified hex file contents\n"
             << "   -c - Write config bits using specified hex file contents\n"
             << "   -v - Verify device matches the data in the specified hex file\n"
             << "   -a - Assert BREAK state for manual bootloader re-entry\n"
             << "   -r - Run application firmware" << endl;
        return 1;
    }

    QTimer::singleShot(0, &a, SLOT(quit()));
    return a.exec();
}

void MessageOutput(QtMsgType type, const char *msg)
 {
    switch (type)
    {
    case QtDebugMsg:
        // let's suppress debug and warning messages from the console for now
        break;

    case QtWarningMsg:
        // let's suppress debug and warning messages from the console for now
        break;

    case QtCriticalMsg:
        cerr << msg << endl;
        break;

    case QtFatalMsg:
        cerr << msg << endl;
        break;
    }
 }
