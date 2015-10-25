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

#ifndef SETTINGS_H
#define SETTINGS_H

#include <QtGui/QDialog>
#include <QextSerialPort/qextserialbase.h>
#include <QComboBox>

namespace Ui {
    class Settings;
}

/*!
 * The Settings GUI dialog box for configuring COM Port, Baud Rates, and programming options.
 */
class Settings : public QDialog {
    Q_OBJECT
    Q_DISABLE_COPY(Settings)
public:
    explicit Settings(QWidget *parent = 0);
    virtual ~Settings();

    void setPort(QString port);
    void setBootloadBaudRate(unsigned int baud);
    void setApplicationBaudRate(unsigned int baud);
    void setWriteFlash(bool value);
    void setWriteEeprom(bool value);
    void setWriteConfig(bool value);

    QString comPort;
    BaudRateType applicationBaudRate;
    BaudRateType bootloadBaudRate;
    bool writeFlash;
    bool writeEeprom;
    bool writeConfig;

protected:
    virtual void changeEvent(QEvent *e);
    void populateBaudRates(QComboBox* comboBox);

private:
    Ui::Settings *m_ui;
    bool alreadyWarnedConfigBitWrite;

private slots:
    void on_ConfigBitsCheckBox_toggled(bool checked);
    void on_buttonBox_accepted();
};

#endif // SETTINGS_H
