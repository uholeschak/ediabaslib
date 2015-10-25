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

#include <QtGui/QApplication>
#include "MainWindow.h"

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    QCoreApplication::setOrganizationName("Microchip");
    QCoreApplication::setOrganizationDomain("microchip.com");
    QCoreApplication::setApplicationName("Serial Bootloader");

    bool erase = false;
    bool program = false;
    bool verify = false;
    bool quiet = false;
    QString hexFile;

    QStringList args = a.arguments();
    for(int i = 0; i < args.length(); i++)
    {
        QString arg = args[i].toLower();

        if(arg == "/e")
        {
            erase = true;
        }
        else if(arg == "/p")
        {
            program = true;
        }
        else if(arg == "/v")
        {
            verify = true;
        }
        else if(arg == "/q")
        {
            quiet = true;
        }
        else
        {
            hexFile = arg;
        }
    }

    if(erase || program || verify)
    {
        int result;

        MainWindow w;
        w.show();
        QApplication::processEvents();

        result = w.ConnectBootloader();
        QApplication::processEvents();
        if(result)
        {
            if(!quiet)
            {
                a.exec();
            }
            return result;
        }

        if((program || verify) && !hexFile.isEmpty())
        {
            result = w.LoadFile(hexFile);
            QApplication::processEvents();
            if(result)
            {
                if(!quiet)
                {
                    a.exec();
                }
                return result;
            }
        }

        if(erase)
        {
            w.setBootloadBusy(true);
            result = w.EraseDevice();
            QCoreApplication::processEvents();
            if(result)
            {
                if(!quiet)
                {
                    a.exec();
                }
                return result;
            }
        }

        if(program)
        {
            w.setBootloadBusy(true);
            result = w.WriteDevice();
            QCoreApplication::processEvents();
            if(result)
            {
                if(!quiet)
                {
                    a.exec();
                }
                return result;
            }
        }

        if(verify)
        {
            w.setBootloadBusy(true);
            result = w.VerifyDevice();
            QCoreApplication::processEvents();
            if(result)
            {
                if(!quiet)
                {
                    a.exec();
                }
                return result;
            }
        }

        return 0;
    }

    MainWindow w;
    w.show();
    return a.exec();
}
