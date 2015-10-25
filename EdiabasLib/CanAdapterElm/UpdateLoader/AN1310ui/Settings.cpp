/************************************************************************
* Copyright (c) 2009,  Microchip Technology Inc.
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
* E. Schlunder  2009/04/29  Initial code.
************************************************************************/

#include "Settings.h"
#include "ui_Settings.h"

#include <QMessageBox>
#include <QextSerialPort/qextserialenumerator.h>
#include <QextSerialPort/qextserialbase.h>

Settings::Settings(QWidget *parent) :
    QDialog(parent),
    m_ui(new Ui::Settings)
{
    m_ui->setupUi(this);

    alreadyWarnedConfigBitWrite = false;

    // Populate the COM Ports drop down combo list
    QList<QextPortInfo> ports = QextSerialEnumerator::getPorts();
    for(int i = 0; i < ports.size(); i++)
    {
        QextPortInfo* port = &ports[i];
        QString name(port->portName);

        m_ui->ComPortComboBox->addItem(port->friendName, name);
    }

    // Populate the Baud Rates drop down list
    populateBaudRates(m_ui->ApplicationBaudRateComboBox);
    populateBaudRates(m_ui->BootloadBaudRateComboBox);
}

/*
From FTDI 232BM http://www.ftdichip.com/Documents/AppNotes/AN232B-05_BaudRates.pdf:

Divisor = n + 0, 0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875; where n is an integer between 2 and
16384 (214).
Note: Divisor = 1 and Divisor = 0 are special cases. A divisor of 0 will give 3 MBaud, and a divisor
of 1 will give 2 MBaud. Sub-integer divisors between 0 and 2 are not allowed.
Therefore the value of the divisor needed for a given Baud rate is found by dividing 3000000 by the
required Baud rate.
*/
void Settings::populateBaudRates(QComboBox* comboBox)
{
    comboBox->addItem("1200 bps", BAUD1200);        // standard
    comboBox->addItem("2400 bps", BAUD2400);        // standard
    comboBox->addItem("4800 bps", BAUD4800);        // standard
    comboBox->addItem("9600 bps", BAUD9600);        // standard
    comboBox->addItem("19200 bps", BAUD19200);      // standard
    comboBox->addItem("38400 bps", BAUD38400);      // standard
    comboBox->addItem("57600 bps", BAUD57600);      // standard
    comboBox->addItem("115200 bps", BAUD115200);    // standard
    comboBox->addItem("230400 bps", BAUD230400);    // Prolific, Linux
#ifdef _TTY_WIN_
    comboBox->addItem("250000 bps", BAUD250000);    // FTDI
#endif
    comboBox->addItem("460800 bps", BAUD460800);    // Prolific, Linux
    comboBox->addItem("500000 bps", BAUD500000);    // FTDI, Linux
#ifndef _TTY_WIN_
    comboBox->addItem("576000 bps", BAUD576000);    // Linux
#endif
#ifdef _TTY_WIN_
    comboBox->addItem("614400 bps", BAUD614400);    // Prolific
    comboBox->addItem("750000 bps", BAUD750000);    // FTDI
#endif
    comboBox->addItem("921600 bps", BAUD921600);    // Prolific, Linux
    comboBox->addItem("1000000 bps", BAUD1000000);  // FTDI, Linux
#ifdef _TTY_WIN_
    comboBox->addItem("1142857 bps", BAUD1142857);  // FTDI
#endif
#ifndef _TTY_WIN_
    comboBox->addItem("1152000 bps", BAUD1152000);  // Linux
#endif
#ifdef _TTY_WIN_
    comboBox->addItem("1200000 bps", BAUD1200000);  // FTDI
    comboBox->addItem("1228800 bps", BAUD1228800);  // Prolific
    comboBox->addItem("1263158 bps", BAUD1263158);  // FTDI
    comboBox->addItem("1333333 bps", BAUD1333333);  // FTDI
    comboBox->addItem("1411765 bps", BAUD1411765);  // FTDI
#endif
    comboBox->addItem("1500000 bps", BAUD1500000);  // FTDI, Linux
#ifdef _TTY_WIN_
    comboBox->addItem("1714285 bps", BAUD1714285);  // MCP
#endif
    comboBox->addItem("2000000 bps", BAUD2000000);  // FTDI, Linux
#ifdef _TTY_WIN_
    comboBox->addItem("2400000 bps", BAUD2400000);  // MCP
    comboBox->addItem("2457600 bps", BAUD2457600);  // Prolific
#endif
#ifndef _TTY_WIN_
    comboBox->addItem("2500000 bps", BAUD2500000);  // Linux
#endif
    comboBox->addItem("3000000 bps", BAUD3000000);  // FTDI, Prolific, Linux
#ifdef _TTY_WIN_
    comboBox->addItem("4000000 bps", BAUD4000000);  // MCP
    comboBox->addItem("6000000 bps", BAUD6000000);  // Prolific
#endif
}

Settings::~Settings()
{
    delete m_ui;
}

void Settings::setPort(QString port)
{
    int i;
    int items = m_ui->ComPortComboBox->count();

    for(i = 0; i < items; i++)
    {
        if(m_ui->ComPortComboBox->itemData(i) == port)
        {
            m_ui->ComPortComboBox->setCurrentIndex(i);
            return;
        }
    }

    // Couldn't find serial port, select first item instead.
    m_ui->ComPortComboBox->setCurrentIndex(0);
}

void Settings::setBootloadBaudRate(unsigned int baud)
{
    for(int i = 0; i < m_ui->BootloadBaudRateComboBox->count(); i++)
    {
        if(m_ui->BootloadBaudRateComboBox->itemData(i) == baud)
        {
            m_ui->BootloadBaudRateComboBox->setCurrentIndex(i);
            return;
        }
    }

    // Couldn't find baud rate, select first item instead.
    m_ui->BootloadBaudRateComboBox->setCurrentIndex(0);
}

void Settings::setApplicationBaudRate(unsigned int baud)
{
    for(int i = 0; i < m_ui->ApplicationBaudRateComboBox->count(); i++)
    {
        if(m_ui->ApplicationBaudRateComboBox->itemData(i) == baud)
        {
            m_ui->ApplicationBaudRateComboBox->setCurrentIndex(i);
            return;
        }
    }

    // Couldn't find baud rate, select first item instead.
    m_ui->ApplicationBaudRateComboBox->setCurrentIndex(0);
}

void Settings::setWriteFlash(bool value)
{
    writeFlash = value;
    m_ui->FlashProgramMemorycheckBox->setChecked(value);
}

void Settings::setWriteEeprom(bool value)
{
    writeEeprom = value;
    m_ui->EepromCheckBox->setChecked(value);
}

void Settings::setWriteConfig(bool value)
{
    writeConfig = value;
    bool warnedFlag = alreadyWarnedConfigBitWrite;
    alreadyWarnedConfigBitWrite = true;
    m_ui->ConfigBitsCheckBox->setChecked(value);
    alreadyWarnedConfigBitWrite = warnedFlag;
}

void Settings::changeEvent(QEvent *e)
{
    switch (e->type())
    {
        case QEvent::LanguageChange:
            m_ui->retranslateUi(this);
            break;
        default:
            break;
    }
}

void Settings::on_buttonBox_accepted()
{
    QString str;
    int i;

    i = m_ui->ComPortComboBox->currentIndex();
    str = m_ui->ComPortComboBox->itemData(i).toString();
    comPort = str;

    i = m_ui->ApplicationBaudRateComboBox->currentIndex();
    applicationBaudRate = (BaudRateType)m_ui->ApplicationBaudRateComboBox->itemData(i).toInt();

    i = m_ui->BootloadBaudRateComboBox->currentIndex();
    bootloadBaudRate = (BaudRateType)m_ui->BootloadBaudRateComboBox->itemData(i).toInt();

    writeFlash = m_ui->FlashProgramMemorycheckBox->isChecked();
    writeConfig = m_ui->ConfigBitsCheckBox->isChecked();
    writeEeprom = m_ui->EepromCheckBox->isChecked();
}

void Settings::on_ConfigBitsCheckBox_toggled(bool checked)
{
    if(alreadyWarnedConfigBitWrite == false && checked)
    {
        QMessageBox msgBox(this);
        msgBox.setWindowTitle("Warning!");

        msgBox.setText("Write Config Bits is not a safe bootloader operation.\n" \
                       "\n" \
                       "The device and bootloader may stop functioning if config bits are changed.\n" \
                       "When the bootloader becomes inoperable, restoring the device will not be possible\n" \
                       "without traditional chip programming tools.\n" \
                       "\n" \
                       "Are you sure you wish to enable the \"Write Config Bits\" option?");

        msgBox.setStandardButtons(QMessageBox::Ok | QMessageBox::Cancel);
        msgBox.setDefaultButton(QMessageBox::Cancel);
        int result = msgBox.exec();
        if(result != QMessageBox::Ok)
        {
            m_ui->ConfigBitsCheckBox->setChecked(false);
            return;
        }

        alreadyWarnedConfigBitWrite = true;
    }
}
