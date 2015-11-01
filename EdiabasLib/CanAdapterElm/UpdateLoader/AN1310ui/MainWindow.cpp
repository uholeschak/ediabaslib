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
* E. Schlunder  2009/05/07  CRC verify added.
* E. Schlunder  2009/04/14  Initial code ported from VB app.
************************************************************************/

#include <QTextStream>
#include <QByteArray>
#include <QList>
#include <QTime>
#include <QFileDialog>
#include <QMessageBox>
#include <QSettings>
#include <QDesktopWidget>
#include <QtConcurrentRun>
#include <QFuture>

#include "Bootload/DeviceSqlLoader.h"

#include "MainWindow.h"
#include "ui_MainWindow.h"

#include "Settings.h"
#include "HexExporter.h"

#include "../version.h"

MainWindow::MainWindow(QWidget *parent)
    : QMainWindow(parent), ui(new Ui::MainWindowClass)
{
    int i;
    fileWatcher = NULL;
    comm = new Comm();

    ui->setupUi(this);
    setWindowTitle(APPLICATION + QString(" v") + VERSION);
    ui->term->setVisible(false);
    txRxLabel.setVisible(false);

    ui->tabWidget->setVisible(false);
    ui->tabWidget->setCurrentIndex(0);
    ui->tabWidget->setTabEnabled(1, false);
    ui->tabWidget->setTabEnabled(2, false);

    QSettings settings;
    settings.beginGroup("MainWindow");
    QSize windowSize = settings.value("size", QSize(604, 548)).toSize();
    QPoint windowPoint = settings.value("pos", QPoint(100, 100)).toPoint();
    QDesktopWidget* desk = QApplication::desktop();
    if(desk->width() < windowPoint.x() + windowSize.width())
    {
        windowPoint.setX(desk->width() - (windowSize.width() + 8));
    }
    if(desk->height() < windowPoint.y() + windowSize.height())
    {
        windowPoint.setY(desk->height() - (windowSize.height() + 55));
    }

    resize(windowSize);
    move(windowPoint);
    fileName = settings.value("fileName").toString();
    txFileName = settings.value("txFileName").toString();
    rxFileName = settings.value("rxFileName").toString();
    allowRxFileOverwrite = settings.value("allowRxFileOverwrite ").toBool();
    ui->configBitsView->columnSplit = settings.value("configColumnSplit", 0).toInt();

    for(i = 0; i < MAX_RECENT_FILES; i++)
    {
        recentFiles[i] = new QAction(this);
        connect(recentFiles[i], SIGNAL(triggered()), this, SLOT(openRecentFile()));
        recentFiles[i]->setVisible(false);
        ui->menuFile->insertAction(ui->actionExit, recentFiles[i]);
    }
    ui->menuFile->insertSeparator(ui->actionExit);

    settings.endGroup();

    settings.beginGroup("Comm");
#ifdef _TTY_POSIX_
    comm->serial->setPortName(settings.value("portName", "/dev/serial").toString());
#else
    comm->serial->setPortName(settings.value("portName", "COM1").toString());
#endif
    bootloadBaud = (BaudRateType)settings.value("bootloadBaudRate", BAUD19200).toInt();
    applicationBaud = (BaudRateType)settings.value("applicationBaudRate", BAUD19200).toInt();
    comm->serial->setBaudRate(bootloadBaud);
    wasBootloaderMode = false;
    baudLabel.setText(comm->baudRate());
    settings.endGroup();

    settings.beginGroup("WriteOptions");
    writeFlash = settings.value("writeFlash", true).toBool();
    writeConfig = settings.value("writeConfig", false).toBool();
    writeEeprom = settings.value("writeEeprom", false).toBool();
    ui->action_Incremental_Bootloading->setChecked(settings.value("incrementalBootloading", true).toBool());
    eraseDuringWrite = true;
    settings.endGroup();

    device = new Device();
    writePlan = new DeviceWritePlanner(device);
    verifyPlan = new DeviceVerifyPlanner(device);
    deviceData = new DeviceData(device);
    verifyData = new DeviceData(device);
    hexData = new DeviceData(device);
    deviceReader = new DeviceReader(device, comm);
    connect(deviceReader, SIGNAL(StatusMessage(QString)), ui->statusBar, SLOT(showMessage(const QString&)));
    connect(ui->actionAbort_Operation, SIGNAL(triggered()), deviceReader, SLOT(AbortOperation()));
    deviceWriter = new DeviceWriter(device, comm);
    connect(deviceWriter, SIGNAL(StatusMessage(QString)), ui->statusBar, SLOT(showMessage(const QString&)));
    connect(ui->actionAbort_Operation, SIGNAL(triggered()), deviceWriter, SLOT(AbortOperation()));
    deviceVerifier = new DeviceVerifier(device, comm);
    connect(deviceVerifier, SIGNAL(StatusMessage(QString)), ui->statusBar, SLOT(showMessage(const QString&)));
    connect(ui->actionAbort_Operation, SIGNAL(triggered()), deviceVerifier, SLOT(AbortOperation()));

    connect(this, SIGNAL(ReadDeviceCompleted(int,bool,bool,bool,double,double,double)), this, SLOT(ReadDeviceComplete(int,bool,bool,bool,double,double,double)));
    connect(this, SIGNAL(WriteDeviceCompleted(int,bool,bool,bool,double,double,double)), this, SLOT(WriteDeviceComplete(int,bool,bool,bool,double,double,double)));
    connect(this, SIGNAL(EraseDeviceCompleted(int, double)), this, SLOT(EraseDeviceComplete(int, double)));
    connect(this, SIGNAL(VerifyDeviceCompleted(QString, bool, bool)), this, SLOT(VerifyDeviceComplete(QString, bool, bool)));

    deviceData->ClearAllData();
    verifyData->ClearAllData();

    flashViewModel = new FlashViewModel(device, deviceData, this);
    ui->FlashTableView->setModel(flashViewModel);

    eepromViewModel = new EepromViewModel(device, deviceData, this);
    ui->EepromTableView->setModel(eepromViewModel);

    RefreshViews();

    portLabel.setText(comm->serial->portName());

    this->statusBar()->addPermanentWidget(&txRxLabel);
    this->statusBar()->addPermanentWidget(&deviceLabel);
    this->statusBar()->addPermanentWidget(&portLabel);
    this->statusBar()->addPermanentWidget(&baudLabel);

    Disconnect();
}

MainWindow::~MainWindow()
{
    ui->term->close();

    QSettings settings;

    settings.beginGroup("MainWindow");
    settings.setValue("size", size());
    settings.setValue("pos", pos());
    settings.setValue("fileName", fileName);
    settings.setValue("txFileName", txFileName);
    settings.setValue("rxFileName", rxFileName);
    settings.setValue("allowRxFileOverwrite", allowRxFileOverwrite);
    settings.setValue("configColumnSplit", ui->configBitsView->columnWidth(0));
    settings.endGroup();

    settings.beginGroup("Comm");
    settings.setValue("portName", comm->serial->portName());
    settings.setValue("bootloadBaudRate", bootloadBaud);
    settings.setValue("applicationBaudRate", applicationBaud);
    settings.endGroup();

    settings.beginGroup("WriteOptions");
    settings.setValue("writeFlash", writeFlash);
    settings.setValue("writeConfig", writeConfig);
    settings.setValue("writeEeprom", writeEeprom);
    settings.setValue("incrementalBootloading", ui->action_Incremental_Bootloading->isChecked());
    settings.endGroup();

    Disconnect();

    delete deviceReader;
    delete deviceWriter;
    delete deviceVerifier;
    delete ui;
    delete comm;
    delete flashViewModel;
    delete eepromViewModel;
    delete deviceData;
    delete verifyData;
    delete hexData;
    delete writePlan;
    delete verifyPlan;
    delete device;
}

void MainWindow::Disconnect(void)
{
    if(!txRxLabel.isVisible())
    {
        disconnect(ui->term, SIGNAL(RefreshStatus()), this, SLOT(RefreshStatus()));
    }

    if(comm->serial->isRecording() == false)
    {
        if(comm->IsOpen())
        {
            comm->releaseBreak();
            if(ui->action_Run_Mode->isCheckable())
            {
                ui->term->close();
            }
        }
        comm->releaseReset();
        comm->close();

        deviceLabel.setText("Disconnected");
        statusBar()->showMessage("Released serial port.");
    }
    else
    {
        deviceLabel.setText("Recording");
    }

    setBootloadEnabled(false);

    ui->action_Run_Mode->setChecked(false);
    ui->action_Bootloader_Mode->setChecked(false);
    ui->action_BreakReset_Mode->setChecked(false);

}

void MainWindow::setBootloadEnabled(bool enable)
{
    ui->actionErase_Device->setEnabled(enable);
    ui->actionRead_Device->setEnabled(enable);
    ui->actionWrite_Device->setEnabled(enable);
    ui->action_Verify_Device->setEnabled(enable);
}

void MainWindow::setBootloadBusy(bool busy)
{
    if(busy)
    {
        abortOperation = false;
        QApplication::setOverrideCursor(Qt::BusyCursor);
    }
    else
    {
        QApplication::restoreOverrideCursor();
    }

    ui->actionAbort_Operation->setEnabled(busy);
    ui->actionErase_Device->setEnabled(!busy);
    ui->actionRead_Device->setEnabled(!busy);
    ui->actionWrite_Device->setEnabled(!busy);
    ui->action_Verify_Device->setEnabled(!busy);
    ui->actionClear_Memory->setEnabled(!busy);
    ui->actionOpen->setEnabled(!busy);
    ui->action_Save->setEnabled(!busy);
    ui->action_Settings->setEnabled(!busy);
}

void MainWindow::RefreshViews(void)
{
    if(ui->tabWidget->currentIndex() == 0)
    {
        ui->FlashTableView->setModel(NULL);
        ui->FlashTableView->setModel(flashViewModel);
        ui->FlashTableView->resizeColumnsToContents();
        ui->FlashTableView->resizeColumnToContents(0);
    }
    else
    {
        ui->FlashTableView->setModel(flashViewModel);
    }

    ui->EepromTableView->setModel(NULL);
    if(device->hasEeprom())
    {
        ui->EepromTableView->setModel(eepromViewModel);
        ui->EepromTableView->resizeColumnsToContents();
        ui->EepromTableView->resizeColumnToContents(0);
    }

    if(device->hasConfig())
    {
        ui->configBitsView->SetDevice(device, deviceData);
    }
}

void MainWindow::RefreshStatus()
{
    double rxRate = ui->term->RxPerSecond;

    if(rxRate > 100)
    {
        txRxLabel.setText(QString::number(rxRate, 'f', 0) + " bps " + QString::number(ui->term->TotalRx) + " Rx " + QString::number(ui->term->TotalTx) + " Tx");
    }
    else
    {
        txRxLabel.setText(QString::number(ui->term->TotalRx) + " Rx " + QString::number(ui->term->TotalTx) + " Tx");
    }
}

void MainWindow::on_actionExit_triggered()
{
    abortOperation = true;
    QApplication::exit();
}

void MainWindow::on_actionAbort_Operation_triggered()
{
    abortOperation = true;
}

void MainWindow::on_actionRead_Device_triggered()
{
    QFuture<int> future = QtConcurrent::run(this, &MainWindow::ReadDevice);

    setBootloadBusy(true);

    flashViewModel->setVerifyData(NULL);
    eepromViewModel->setVerifyData(NULL);
    ui->configBitsView->setVerifyData(NULL);
}

int MainWindow::ReadDevice(void)
{
    QTime elapsed;
    double flashTime = 0, eepromTime = 0, configTime = 0;
    bool flash = false, eeprom = false, config = false;
    int failed = 0;

    elapsed.start();
    failed = deviceReader->ReadFlash(deviceData->ProgramMemory, device->startFLASH, device->endFLASH);
    flashTime = ((double)elapsed.elapsed()) / 1000;
    flash = true;

    if(failed == 0 && device->hasConfigReadCommand())
    {
        elapsed.start();
        failed = deviceReader->ReadConfig(deviceData->ConfigWords, device->startConfig, device->endConfig);
        configTime = ((double)elapsed.elapsed()) / 1000;
        config = true;
    }

    if(failed == 0 && device->hasEeprom())
    {
        elapsed.start();
        failed = deviceReader->ReadEeprom(deviceData->EEPromMemory, device->startEEPROM, device->endEEPROM);
        eepromTime = ((double)elapsed.elapsed()) / 1000;
        eeprom = true;
    }

    emit ReadDeviceCompleted(failed, flash, eeprom, config, flashTime, eepromTime, configTime);

    return failed;
}

void MainWindow::ReadDeviceComplete(int failed, bool flash, bool eeprom, bool config, double flashTime, double eepromTime, double configTime)
{
    QString msg;
    QTextStream stream(&msg);

    QApplication::restoreOverrideCursor();
    QApplication::setOverrideCursor(Qt::WaitCursor);

    if(failed == 0)
    {
        stream.setIntegerBase(10);
        stream << "Read complete (FLASH " << flashTime << "s";
        if(eeprom)
        {
            stream << ", EEPROM " << eepromTime << "s";
        }
        if(config)
        {
            stream << ", Config " << configTime << "s";
        }
        stream << ")";
        statusBar()->showMessage(msg);
    }

    RefreshViews();
    setBootloadBusy(false);
}

void MainWindow::on_action_Verify_Device_triggered()
{
    QFuture<int> future = QtConcurrent::run(this, &MainWindow::VerifyDevice, true);

    setBootloadBusy(true);
}

int MainWindow::VerifyDevice(bool verifyFlash)
{
    QTime elapsed;
    double flashTime = 0, eepromTime = 0, configTime = 0;
    bool flash = false, eeprom = false, config = false;
    int i, readFlashFailed = 0, readEepromFailed = 0, readConfigFailed = 0;
    bool refreshViews = false, clearConfigVerifyData = false;
    int flashFails = 0, eepromFails = 0, configFails = 0;
    unsigned int failAddress = device->startFLASH;
    Device::MemoryRange range;

    failed = 0;

    if(device->hasEncryption())
    {
        Comm::ErrorCode result = comm->SetNonce(deviceData->Nonce);
        if(result != Comm::Success)
        {
            qDebug("Error %d when trying to set Nonce.", result);
        }
    }

    if(flashViewModel->hasVerifyData() || eepromViewModel->hasVerifyData() || ui->configBitsView->hasVerifyData())
    {
        flashViewModel->setVerifyData(NULL);
        eepromViewModel->setVerifyData(NULL);
        ui->configBitsView->setVerifyData(NULL);
        refreshViews = true;
    }

    if(!writeFlash && writeConfig && device->hasConfigAsFlash() && verifyFlash)
    {
        // Only verify FLASH config bits.
        elapsed.start();
        flashViewModel->setVerifyData(verifyData);
        range.start = device->startConfig;
        range.end = device->endConfig;
        readFlashFailed = deviceReader->ReadFlash(device->flashPointer(device->endFLASH - device->eraseBlockSizeFLASH, verifyData->ProgramMemory), device->endFLASH - device->eraseBlockSizeFLASH, range.end);
        countFlashVerifyFailures(flashFails, failAddress, range);
        ui->configBitsView->setVerifyData(verifyData);
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

        if(QApplication::keyboardModifiers() == Qt::ControlModifier)
        {
            // Use slow full read verify
            flashViewModel->setVerifyData(verifyData);
            readFlashFailed = deviceReader->ReadFlash(&verifyData->ProgramMemory[0], device->startFLASH, endAddress);
            if(writeConfig && device->hasConfigAsFlash())
            {
                ui->configBitsView->setVerifyData(verifyData);
                refreshViews = true;
            }
            range.start = device->startFLASH;
            range.end = device->endFLASH;
            if(device->hasConfigAsFlash() && writeConfig == false)
            {
                // if this is a FLASH config device (J devices) and we weren't trying
                // to write the config page, don't try verifying the last page.
                range.end -= device->eraseBlockSizeFLASH;
            }

            countFlashVerifyFailures(flashFails, failAddress, range);
        }
        else
        {
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
                    verifyData->CopyProgramMemory(deviceData->ProgramMemory);
                    flashViewModel->setVerifyData(verifyData);
                    readFlashFailed = deviceReader->ReadFlash(&verifyData->ProgramMemory[0], deviceVerifier->failList);
                    if(readFlashFailed == Comm::Success)
                    {
                        readFlashFailed = deviceReader->ReadFlash(&verifyData->ProgramMemory[0], deviceVerifier->eraseList);
                    }

                    if(writeConfig && device->hasConfigAsFlash())
                    {
                        ui->configBitsView->setVerifyData(verifyData);
                        refreshViews = true;
                    }

                    range.start = device->startFLASH;
                    range.end = device->endFLASH;
                    if(device->hasConfigAsFlash() && writeConfig == false)
                    {
                        // if this is a FLASH config device (J devices) and we weren't trying
                        // to write the config page, don't try verifying the last page.
                        range.end -= device->eraseBlockSizeFLASH;
                    }
                    countFlashVerifyFailures(flashFails, failAddress, range);
                }
            }
        }

        flashTime = ((double)elapsed.elapsed()) / 1000;
        flash = true;
    }

    // Read EEPROM memory from device
    if(writeEeprom && device->hasEeprom())
    {
        elapsed.start();
        eepromViewModel->setVerifyData(verifyData);
        readEepromFailed  = deviceReader->ReadEeprom(&verifyData->EEPromMemory[0], device->startEEPROM, device->endEEPROM);
        eepromTime = ((double)elapsed.elapsed()) / 1000;
        eeprom = true;
    }

    // Read Config memory from device
    if(writeConfig && device->hasConfigReadCommand())
    {
        elapsed.start();
        readConfigFailed = deviceReader->ReadConfig(&verifyData->ConfigWords[0], device->startConfig, device->endConfig);
        ui->configBitsView->setVerifyData(verifyData);
        configTime = ((double)elapsed.elapsed()) / 1000;
        config = true;
    }

    QString msg;
    QTextStream stream(&msg);
    msg.clear();

    stream.setIntegerBase(16);
    stream.setNumberFlags(QTextStream::UppercaseDigits);

    // Count up the number of failures
    if(readFlashFailed || readEepromFailed || readConfigFailed)
    {
        failed = -1;
    }
    else
    {
        unsigned int address;
        unsigned int word, suspectWord;

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
                memory = deviceData->ConfigWordPointer(device->configWords[i].address);
                suspectMemory = verifyData->ConfigWordPointer(device->configWords[i].address);
                word = *memory & device->configWordMask;
                suspectWord = *suspectMemory & device->configWordMask;
                if(word != suspectWord)
                {
                    if(flashFails == 0 && eepromFails == 0 && configFails == 0)
                    {
                        failAddress = address + device->startConfig;
                    }

                    configFails++;
                }
            }
        }

        if(flashFails || eepromFails || configFails)
        {
            refreshViews = true;
            failed = -2;
            stream.setIntegerBase(16);
            stream << "Verify failed at address " << failAddress;
            stream.setIntegerBase(10);
            stream << "h (";
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

                if(writeConfig && device->hasConfigAsFlash())
                {
                    clearConfigVerifyData = true;
                }
            }
            else if(eeprom)
            {
                stream << "Verify success (EEPROM " << eepromTime << "s";
                if(config)
                {
                    stream << ", Config " << configTime << "s";
                }
                stream << ")";
            }
            else if(config)
            {
                stream << "Verify success (Config " << configTime << "s";
                stream << ")";
            }
        }
    }

    emit VerifyDeviceCompleted(msg, refreshViews, clearConfigVerifyData);

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

void MainWindow::VerifyDeviceComplete(QString msg, bool refreshViews, bool clearConfigVerifyData)
{
    QApplication::restoreOverrideCursor();
    QApplication::setOverrideCursor(Qt::WaitCursor);

    if(msg.length())
    {
        statusBar()->showMessage(msg);
    }

    if(clearConfigVerifyData)
    {
        ui->configBitsView->setVerifyData(NULL);
        ui->configBitsView->SetDevice(device, deviceData);
    }

    if(refreshViews)
    {
        RefreshViews();
    }

    setBootloadBusy(false);
}

void MainWindow::on_actionWrite_Device_triggered()
{
    QFuture<int> future = QtConcurrent::run(this, &MainWindow::WriteDevice);
    setBootloadBusy(true);
}

int MainWindow::WriteDevice(void)
{
    return WriteDevice(deviceData);
}

int MainWindow::WriteDevice(DeviceData* newData, DeviceData* existingData)
{
    QTime elapsed;
    double flashTime = 0, eepromTime = 0, configTime = 0;
    bool flash = false, eeprom = false, config = false;
    Comm::ErrorCode result;

    failed = 0;

    if(device->hasEncryption())
    {
        Comm::ErrorCode result = comm->SetNonce(deviceData->Nonce);
        if(result != Comm::Success)
        {
            qDebug("Error %d when trying to set Nonce.", result);
        }
    }

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

        flashViewModel->setVerifyData(NULL);
        ui->configBitsView->setVerifyData(NULL);

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

        flashViewModel->setVerifyData(NULL);
        if(writeConfig && device->hasConfigAsFlash())
        {
            ui->configBitsView->setVerifyData(NULL);
        }
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

    emit WriteDeviceCompleted(failed, flash, eeprom, config, flashTime, eepromTime, configTime);

    if(failed == 0)
    {
        return 0;
    }

    return -5;
}

void MainWindow::WriteDeviceComplete(int failed, bool flash, bool eeprom, bool config, double flashTime, double eepromTime, double configTime)
{
    QString msg;
    QTextStream stream(&msg);

    QApplication::restoreOverrideCursor();
    QApplication::setOverrideCursor(Qt::WaitCursor);

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
        statusBar()->showMessage(msg);
    }

    setBootloadBusy(false);
}


void MainWindow::on_actionErase_Device_triggered()
{
    QFuture<int> future = QtConcurrent::run(this, &MainWindow::EraseDevice);

    setBootloadBusy(true);
}

int MainWindow::EraseDevice(void)
{
    QTime elapsed;
    elapsed.start();

    if(writeFlash)
    {
        QLinkedList<Device::MemoryRange> eraseList;

        flashViewModel->setVerifyData(NULL);
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
                emit EraseDeviceCompleted(-4, ((double)elapsed.elapsed()) / 1000);
                return -4;
            }
            it++;
        }
    }

    emit EraseDeviceCompleted(0, ((double)elapsed.elapsed()) / 1000);
    return 0;
}

void MainWindow::EraseDeviceComplete(int failed, double eraseTime)
{
    QString msg;
    QTextStream stream(&msg);

    if(failed == 0)
    {
        stream.setIntegerBase(10);
        stream << "Erase device complete (" << eraseTime << "s)";
        statusBar()->showMessage(msg);
    }
    setBootloadBusy(false);
}

void MainWindow::countFlashVerifyFailures(int& flashFails, unsigned int& failAddress, Device::MemoryRange range)
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

void MainWindow::on_actionOpen_triggered()
{
    QString msg, newFileName;
    QTextStream stream(&msg);

    if(ui->term->isConnected())
    {
        newFileName =
            QFileDialog::getOpenFileName(this, "Transmit File", txFileName, "All Files (*.*)");

        if(newFileName.isEmpty())
        {
            return;
        }

        TransmitFile(newFileName);
    }
    else if(ui->tabWidget->isVisible())
    {
        newFileName =
            QFileDialog::getOpenFileName(this, "Open Hex File", fileName, "Hex Files (*.hex);;All Files (*.*)");
/*        newFileName =
            QFileDialog::getOpenFileName(this, "Open Hex File", fileName, "Hex Files (*.hex *.ehx);;All Files (*.*)");
*/
        if(newFileName.isEmpty())
        {
            return;
        }

        LoadFile(newFileName);
    }
}

int MainWindow::ConnectBootloader(void)
{
    ui->action_Bootloader_Mode->setChecked(true);
    on_action_Bootloader_Mode_triggered();

    if(ui->action_Bootloader_Mode->isChecked())
    {
        return 0;
    }
    return -1;
}

void MainWindow::TransmitFile(QString newFileName)
{
    txFileName = newFileName;
    ui->term->transmitFile(txFileName);

    QSettings settings;
    settings.beginGroup("MainWindow");

    QStringList files = settings.value("recentTxFileList").toStringList();
    files.removeAll(txFileName);
    files.prepend(txFileName);
    while(files.size() > MAX_RECENT_FILES)
    {
        files.removeLast();
    }
    settings.setValue("recentTxFileList", files);
    UpdateRecentFileList();
}

int MainWindow::LoadFile(QString newFileName)
{
    QString msg;
    QTextStream stream(&msg);

    QApplication::setOverrideCursor(Qt::BusyCursor);

    HexImporter import;
    HexImporter::ErrorCode result;
    result = import.ImportHexFile(newFileName, deviceData, device);
    switch(result)
    {
        case HexImporter::Success:
            break;

        case HexImporter::CouldNotOpenFile:
            QApplication::restoreOverrideCursor();
            stream << "Could not open file: " << newFileName;
            statusBar()->showMessage(msg);
            return -2;

        default:
            QApplication::restoreOverrideCursor();
            stream << "Failed to import: " << result;
            statusBar()->showMessage(msg);
            return -3;
    }

    if(writeConfig && import.hasConfigBits == false && device->hasConfig())
    {
        if(QMessageBox::warning(this, "Warning!",
                             "This HEX file does not contain config bit settings.\n\nThe device and bootloader may stop functioning if config bits are changed. When the bootloader becomes inoperable, restoring the device will not be possible without traditional chip programming tools.\n\nWould you like to disable the \"Write Config Bits\" option now?",
                             QMessageBox::Yes | QMessageBox::No, QMessageBox::Yes) == QMessageBox::Yes)
        {
            writeConfig = false;
        }
    }

    fileName = newFileName;
    watchFileName = newFileName;

    QSettings settings;
    settings.beginGroup("MainWindow");

    QStringList files = settings.value("recentFileList").toStringList();
    files.removeAll(fileName);
    files.prepend(fileName);
    while(files.size() > MAX_RECENT_FILES)
    {
        files.removeLast();
    }
    settings.setValue("recentFileList", files);
    UpdateRecentFileList();

    if(device->startBootloader != 0 && deviceData->Encrypted == false)
    {
        if(device->HasValidResetVector(deviceData->ProgramMemory) == false)
        {
            QMessageBox msgBox(this);
            msgBox.setWindowTitle("Invalid Reset Vector");

            if(device->family == Device::PIC16)
            {
                msgBox.setText("The first four instructions do not appear to contain a valid\n"\
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
                msgBox.setText("The first instruction does not appear to be a GOTO instruction.\n\n"\
                               "Please modify your firmware to have a GOTO as the first instruction\n"\
                               "at address 0. Without the GOTO, bootloader firmware will not be able\n"\
                               "to execute your application firmware.");
            }
            msgBox.setStandardButtons(QMessageBox::Ok);
            msgBox.exec();
        }

        device->RemapResetVector(deviceData->ProgramMemory);
    }
    stream.setIntegerBase(10);

    if(device->family == Device::PIC24 && deviceData->Encrypted == false)
    {
        RemapInterruptVectors(device, deviceData);
    }

    msg.clear();
    QFileInfo fi(fileName);
    QString name = fi.fileName();
    stream << name << " - " << APPLICATION << " v" << VERSION;
    setWindowTitle(msg);

    if(ui->action_Incremental_Bootloading->isChecked())
    {
        setupIncrementalBootloading(true);
    }

    QApplication::restoreOverrideCursor();
    QApplication::setOverrideCursor(Qt::WaitCursor);

    RefreshViews();
    QApplication::restoreOverrideCursor();

    return 0;
}

Comm::ErrorCode MainWindow::RemapInterruptVectors(Device* device, DeviceData* deviceData)
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
        statusBar()->clearMessage();
    }

    if(device->AIVT == NULL)
    {
        device->AIVT = new unsigned int[(device->endAIVT - device->startAIVT) / 2];
        result = deviceReader->ReadFlash(device->AIVT, device->startAIVT, device->endAIVT);
        if(result)
        {
            return result;
        }
        statusBar()->clearMessage();
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

void MainWindow::on_action_Record_triggered()
{
    QString newFileName;

    if(comm->serial->isRecording())
    {
        comm->serial->stopRecording();
        ui->action_Record->setChecked(false);
        if(ui->action_Bootloader_Mode->isChecked() == 0 &&
            ui->action_BreakReset_Mode->isChecked() == 0 &&
            ui->action_Run_Mode->isChecked() == 0)
        {
            Disconnect();
        }
        return;
    }

    newFileName = QFileDialog::getSaveFileName(this, "Record to file...", rxFileName, "All Files (*.*)", 0, QFileDialog::DontConfirmOverwrite);
    if(newFileName.isEmpty())
    {
        ui->action_Record->setChecked(false);
        return;
    }

    QFileInfo file(newFileName);
    if(file.exists())
    {
        QMessageBox msgBox(this);
        msgBox.setWindowTitle("Warning");
        msgBox.setText("The specified file already exists.\n");
        QPushButton *appendButton = msgBox.addButton("&Append", QMessageBox::YesRole);
        QPushButton *overwriteButton = msgBox.addButton("&Overwrite", QMessageBox::NoRole);
        msgBox.addButton(QMessageBox::Cancel);
        if(allowRxFileOverwrite)
        {
            msgBox.setDefaultButton(overwriteButton);
        }
        else
        {
            msgBox.setDefaultButton(appendButton);
        }
        int result = msgBox.exec();
        if(result == QMessageBox::Cancel)
        {
            ui->action_Record->setChecked(false);
            return;
        }

        if(msgBox.clickedButton() == (QAbstractButton *)appendButton)
        {
            allowRxFileOverwrite = false;
        }
        else
        {
            allowRxFileOverwrite = true;
        }
    }

    rxFileName = newFileName;
    ui->action_Record->setChecked(true);
    comm->serial->startRecording(rxFileName, allowRxFileOverwrite);
}

void MainWindow::openRecentFile(void)
{
    QAction *action = qobject_cast<QAction *>(sender());
    if (action)
    {
        if(ui->term->isConnected())
        {
            TransmitFile(action->data().toString());
        }
        else if(ui->tabWidget->isVisible())
        {
            LoadFile(action->data().toString());
        }
    }
}

void MainWindow::UpdateRecentFileList(void)
{
    QSettings settings;
    settings.beginGroup("MainWindow");
    QStringList files;

    if(ui->term->isConnected())
    {
        files = settings.value("recentTxFileList").toStringList();
    }
    else
    {
        files = settings.value("recentFileList").toStringList();
    }

    int recentFileCount = qMin(files.size(), MAX_RECENT_FILES);
    QString text;
    int i;

    for(i = 0; i < recentFileCount; i++)
    {
        text = tr("&%1 %2").arg(i + 1).arg(QFileInfo(files[i]).fileName());

        recentFiles[i]->setText(text);
        recentFiles[i]->setData(files[i]);
        recentFiles[i]->setVisible(true);
    }

    for(; i < MAX_RECENT_FILES; i++)
    {
        recentFiles[i]->setVisible(false);
    }
}

void MainWindow::incrementalBootload(QString& normalTitle, QFileInfo& fileInfo, HexImporter& import, DeviceData* importData)
{
    QString msg;
    QTextStream stream(&msg);

    if(ui->action_Bootloader_Mode->isChecked())
    {
        if(device->startBootloader != 0)
        {
            device->RemapResetVector(importData->ProgramMemory);
        }

        if(device->family == Device::PIC24)
        {
            RemapInterruptVectors(device, importData);
        }

        setBootloadBusy(true);
        if(writeConfig && import.hasConfigBits == false)
        {
            // User has Write Config option turned on, but this HEX file
            // does not have config bit settings. Let's temporarily disable
            // the Write Config option for this incremental update.
            writeConfig = false;
            WriteDevice(importData, deviceData);
            writeConfig = true;
        }
        else
        {
            WriteDevice(importData, deviceData);
        }

        *deviceData = *importData;
        if(failed == 0)
        {
            VerifyDevice(false);
        }
    }
    else
    {
        ui->action_BreakReset_Mode->setChecked(true);
        on_action_BreakReset_Mode_triggered();
    }

    RefreshViews();
}

void MainWindow::modifiedFile(const QString& path)
{
    QString msg;
    QTextStream stream(&msg);
    static int loadQuery = 0;
    double bootTime = 0;

    loadQuery++;
    if(loadQuery > 1)
    {
        loadQuery--;
        return;
    }
    disconnect(fileWatcher, SIGNAL(fileChanged(const QString&)), this, SLOT(modifiedFile(const QString&)));
    QApplication::processEvents();

    QTime elapsed;
    QFileInfo fileInfo(fileName);
    int fileSize = 0;
    elapsed.start();

    bool bootMode = ui->action_Bootloader_Mode->isChecked();
    bool runMode = ui->action_Run_Mode->isChecked();

    QString normalTitle = windowTitle();
    setWindowTitle("Bootloading...");
    QApplication::processEvents();
    failed = 0;

    if(failed == 0)
    {
        DeviceData* importData = new DeviceData(device);
        while(1)
        {
            while(elapsed.elapsed() < 500)
            {
                if(fileInfo.size() != fileSize)
                {
                    fileSize = fileInfo.size();
                    elapsed.start();
                }

#ifdef Q_WS_WIN
                Sleep(100);
#else
                timeval sleepTime;
                sleepTime.tv_sec = 0;
                sleepTime.tv_usec = 100*1000;
                select(0, NULL, NULL, NULL, &sleepTime);
#endif
                QApplication::processEvents();
                fileInfo.refresh();
            }

            if(!fileInfo.exists())
            {
                // ABORT: file has been deleted
                normalTitle = APPLICATION + QString(" v") + VERSION;
                failed = -1;
                break;
            }

            HexImporter import;
            HexImporter::ErrorCode result = import.ImportHexFile(fileName, importData, device);
            if(!import.hasEndOfFileRecord)
            {
                // this file isn't complete yet, continue waiting for file changes
                continue;
            }

            elapsed.start();

            if(!bootMode)
            {
                ui->action_Bootloader_Mode->setChecked(true);
                on_action_Bootloader_Mode_triggered();
            }

            if(result == HexImporter::CouldNotOpenFile)
            {
                QApplication::restoreOverrideCursor();
                stream << "Could not open file: " << fileName;
                statusBar()->showMessage(msg);
                failed = -1;
                break;
            }
            else if(result != HexImporter::Success)
            {
                QApplication::restoreOverrideCursor();
                stream << "Failed to import: " << result;
                statusBar()->showMessage(msg);
                failed = -1;
                break;
            }

            incrementalBootload(normalTitle, fileInfo, import, importData);
            break;
        }
        delete importData;
        bootTime = ((double)elapsed.elapsed()) / 1000;
    }

    QApplication::processEvents();
    loadQuery--;
    connect(fileWatcher, SIGNAL(fileChanged(const QString&)), this, SLOT(modifiedFile(const QString&)));
    setWindowTitle(normalTitle);

    msg = statusBar()->currentMessage();
    if(failed == 0 && runMode)
    {
        ui->action_Run_Mode->setChecked(true);
        on_action_Run_Mode_triggered();

        msg.clear();
        stream << "Update success (" << bootTime << "s)";
    }
    statusBar()->showMessage(msg);
}

void MainWindow::on_actionClear_Memory_triggered()
{
    QTime elapsed;
    QString msg;
    QTextStream stream(&msg);

    if(ui->term->isVisible())
    {
        ui->term->clear();
        return;
    }

    setWindowTitle(APPLICATION + QString(" v") + VERSION);
    watchFileName.clear();
    setupIncrementalBootloading(false);

    flashViewModel->setVerifyData(NULL);
    eepromViewModel->setVerifyData(NULL);
    ui->configBitsView->setVerifyData(NULL);
    deviceData->ClearAllData();
    QApplication::setOverrideCursor(Qt::WaitCursor);

    elapsed.start();
    RefreshViews();

    statusBar()->clearMessage();
    QApplication::restoreOverrideCursor();
}


void MainWindow::on_action_About_triggered()
{
    QString msg;
    QTextStream stream(&msg);

    stream << "Serial Bootloader AN1310 v" << VERSION << "\n";
    stream << "Copyright " << (char)Qt::Key_copyright << " 2009-2011,  Microchip Technology Inc.\n\n";

    stream << "Microchip licenses this software to you solely for use with\n";
    stream << "Microchip products. The software is owned by Microchip and\n";
    stream << "its licensors, and is protected under applicable copyright\n";
    stream << "laws. All rights reserved.\n\n";

    stream << "SOFTWARE IS PROVIDED \"AS IS.\"  MICROCHIP EXPRESSLY\n";
    stream << "DISCLAIMS ANY WARRANTY OF ANY KIND, WHETHER EXPRESS\n";
    stream << "OR IMPLIED, INCLUDING BUT NOT LIMITED TO, THE IMPLIED\n";
    stream << "WARRANTIES OF MERCHANTABILITY, FITNESS FOR A\n";
    stream << "PARTICULAR PURPOSE, OR NON-INFRINGEMENT.  IN NO EVENT\n";
    stream << "SHALL MICROCHIP BE LIABLE FOR ANY INCIDENTAL, SPECIAL,\n";
    stream << "INDIRECT OR CONSEQUENTIAL DAMAGES, LOST PROFITS OR\n";
    stream << "LOST DATA, HARM TO YOUR EQUIPMENT, COST OF\n";
    stream << "PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY OR\n";
    stream << "SERVICES, ANY CLAIMS BY THIRD PARTIES (INCLUDING BUT\n";
    stream << "NOT LIMITED TO ANY DEFENSE THEREOF), ANY CLAIMS FOR\n";
    stream << "INDEMNITY OR CONTRIBUTION, OR OTHER SIMILAR COSTS.\n\n";

    stream << "To the fullest extent allowed by law, Microchip and its\n";
    stream << "licensors liability shall not exceed the amount of fees, if any,\n";
    stream << "that you have paid directly to Microchip to use this software.\n\n";

    stream << "MICROCHIP PROVIDES THIS SOFTWARE CONDITIONALLY UPON\n";
    stream << "YOUR ACCEPTANCE OF THESE TERMS.";

    QMessageBox::about(this, "About", msg);
}

void MainWindow::on_action_Run_Mode_triggered()
{
    QTime elapsed;
    QString msg;

    if(!ui->action_Run_Mode->isChecked())
    {
        ui->term->close();
        Disconnect();
        return;
    }

    ui->action_BreakReset_Mode->setChecked(false);

    if(!comm->IsOpen())
    {
        comm->serial->setBaudRate(applicationBaud);          
        baudLabel.setText(comm->baudRate());

        // don't capture incoming data yet, sometimes just opening the port releases reset for a moment.
        ui->term->close();

        if(comm->open() != Comm::Success)
        {
            Disconnect();
            msg.append("Could not open ");
            msg.append(comm->serial->portName());
            msg.append(".");
            statusBar()->showMessage(msg);
            return;
        }
        baudLabel.setText(comm->baudRate());
    }

    comm->releaseBreak();

    ui->tabWidget->setVisible(false);
    ui->term->setVisible(true);
    txRxLabel.setVisible(true);

    elapsed.start();
    if(wasBootloaderMode || ui->action_Bootloader_Mode->isChecked())
    {
        // we were probably in the bootloader previously.

        // assert RTS/MCLR# reset to get out of bootloader mode.
        comm->assertReset();
        // wait for 5ms to allow part to go into MCLR reset.
        while(elapsed.elapsed() < 5)
        {
            QApplication::processEvents();
        }

        // send a command to enter Application Mode in case
        // RTS/MCLR# reset is not wired up.
        ui->term->clear();
        comm->serial->clearReceiveBuffer();
        comm->RunApplication();

        ui->action_Bootloader_Mode->setChecked(false);
    }
    wasBootloaderMode = false;

    if(comm->serial->baudRate() != applicationBaud)
    {
        comm->serial->close();
        ui->term->clear();
        comm->serial->clearReceiveBuffer();
        comm->serial->setBaudRate(applicationBaud);
        if(comm->open() != Comm::Success)
        {
            Disconnect();
            baudLabel.setText(comm->baudRate());
            msg.append("Could not open ");
            msg.append(comm->serial->portName());
            msg.append(" at ");
            msg.append(comm->baudRate());
            msg.append(".");
            statusBar()->showMessage(msg);
            return;
        }

        baudLabel.setText(comm->baudRate());

        comm->releaseBreak();
    }
    ui->term->open(comm->serial);

    comm->releaseReset();

    statusBar()->clearMessage();
    deviceLabel.setText("Connected");
    ui->term->setFocus();

    setBootloadEnabled(false);
    ui->actionOpen->setEnabled(true);
    ui->actionClear_Memory->setEnabled(true);
    connect(ui->term, SIGNAL(RefreshStatus()), this, SLOT(RefreshStatus()));

    QTextCursor cursor = ui->term->textCursor();
    abortOperation = false;
    UpdateRecentFileList();
}

void MainWindow::on_action_Bootloader_Mode_triggered()
{
    QTime elapsed, totalTime;
    QString msg;
    QString version;

    if(!ui->action_Bootloader_Mode->isChecked())
    {
        Disconnect();
        return;
    }

    totalTime.start();
    qDebug("Connecting...");

    if(ui->action_BreakReset_Mode->isChecked())
    {
        comm->releaseIntoBootloader();
        ui->action_BreakReset_Mode->setChecked(false);
        qWarning("releaseIntoBootloader");
        if(comm->serial->baudRate() != bootloadBaud)
        {
            comm->serial->close();
        }
    }
    qDebug("time(1): %fs", (double)totalTime.elapsed() / 1000);

    if(!comm->IsOpen() || ui->action_Run_Mode->isChecked())
    {
        ui->term->close();
        ui->action_Run_Mode->setChecked(false);

        if(comm->serial->baudRate() != bootloadBaud)
        {
            comm->serial->close();
            comm->serial->setBaudRate(bootloadBaud);
        }

        baudLabel.setText(comm->baudRate());
        if(comm->open() != Comm::Success)
        {
            Disconnect();
            msg.append("Could not open ");
            msg.append(comm->serial->portName());
            msg.append(".");
            statusBar()->showMessage(msg);
            failed = -1;
            return;
        }
        baudLabel.setText(comm->baudRate());
    }

    qDebug("time(2): %fs", (double)totalTime.elapsed() / 1000);

    // First, try talking to the bootloader without going through a
    // device reset. Sometimes this will let us achieve high baud rates
    // when config bits have been lost (which would ordinarily cause us
    // to run slowly from the default INTOSC).
    statusBar()->showMessage("Connecting...");
    Comm::BootInfo bootInfo = comm->ReadBootloaderInfo(2);
    qDebug("time(3): %fs", (double)totalTime.elapsed() / 1000);

    ui->term->setVisible(false);
    txRxLabel.setVisible(false);

    ui->actionOpen->setEnabled(false);
    ui->action_Save->setEnabled(false);
    ui->tabWidget->setVisible(true);
    if(bootInfo.majorVersion == 0 && bootInfo.minorVersion == 0)
    {
        // No such luck. Application firmware might be busy running, so
        // try to force the device into Bootloader mode.
        //comm->assertBreak();          // [UH]
        //comm->assertReset();          // [UH]
        comm->ActivateBootlader();      // [UH]
        qDebug("time(assert reset): %fs", (double)totalTime.elapsed() / 1000);

        statusBar()->showMessage("Resetting device...");

        // wait 5ms to allow MCLR and RXD to go to logic 0.
        elapsed.start();
        while(elapsed.elapsed() < 600) // [UH] 600ms
        {
            QApplication::processEvents();
        }

        //qDebug("time(3.2): %fs", (double)totalTime.elapsed() / 1000);
        //comm->releaseIntoBootloader();    // [UH]

        statusBar()->showMessage("Connecting...");
        qDebug("time(3.3): %fs", (double)totalTime.elapsed() / 1000);
        bootInfo = comm->ReadBootloaderInfo();
    }

    if(bootInfo.majorVersion == 0 && bootInfo.minorVersion == 0)
    {
        Disconnect();
        statusBar()->showMessage("Bootloader not found.");
        failed = -1;
        return;
    }

    wasBootloaderMode = true;

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
    statusBar()->showMessage(connectMsg);
    QApplication::processEvents();

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
    s << SelectDevice(deviceId.id, (Device::Families)bootInfo.familyId);
    qDebug("time(6): %fs", (double)totalTime.elapsed() / 1000);

    device->startBootloader = bootInfo.startBootloader;
    device->endBootloader = bootInfo.endBootloader;
    device->commandMask = bootInfo.commandMask;

#if 0
    int maxRead = (int)bootloadBaud;
    maxRead /= 8;
    maxRead -= maxRead % 0x100;
    if(maxRead < 0x100)
    {
        maxRead = 0x100;
    }
#else
    int maxRead = 0x100;
#endif
    deviceReader->setMaxRequest(maxRead);

    if(deviceId.revision >= 0)
    {
        s << " Revision " << QString::number(deviceId.revision, 16).toUpper();
    }
    deviceLabel.setText(msg);

    QApplication::processEvents();

    setBootloadEnabled(true);

    UpdateRecentFileList();
    ss << " (" << (double)totalTime.elapsed() / 1000 << "s)";
    statusBar()->showMessage(connectMsg);
    qDebug("total time: %fs", (double)totalTime.elapsed() / 1000);
}

QString MainWindow::SelectDevice(unsigned int deviceId, Device::Families familyId)
{
    QString msg;
    QTextStream out(&msg);

    DeviceSqlLoader::ErrorCode result = DeviceSqlLoader::loadDevice(device, deviceId, familyId);
    switch(result)
    {
        case DeviceSqlLoader::DatabaseMissing:
            out << "Could not find a device database file.\n"
                << "Please copy DEVICES.DB to one of these two locations:\n\n"
                << "    The folder where the application starts, OR\n"
                << "    The folder containing the application executable file.\n";
            QMessageBox::warning(0, "Error", msg);
            break;

        case DeviceSqlLoader::DeviceMissing:
            out << "This device does not have an entry in the DEVICES.DB database.\n"
                << "Please add it to the DEVICES.SQL script and regenerate the database.";
            QMessageBox::warning(0, "Unrecognized Device", msg);
            break;

        case DeviceSqlLoader::Success:
            ui->actionClear_Memory->setEnabled(true);
            ui->actionOpen->setEnabled(true);
            ui->action_Save->setEnabled(true);
            ui->tabWidget->setTabEnabled(1, device->hasEeprom());
            ui->tabWidget->setTabEnabled(2, device->hasConfig());

            RefreshViews();
            ui->tabWidget->setFocus();      // otherwise CTRL-TAB/CTRL+SHIFT-TAB may not allow tab switching
            if(device->name.size())
            {
                return device->name;
            }
            break;
    }
    return "Device " + QString::number(deviceId);
}

void MainWindow::on_action_BreakReset_Mode_triggered()
{
    QString msg;
    if(!ui->action_BreakReset_Mode->isChecked())
    {
        comm->releaseBreak();
        wasBootloaderMode = false;
        Disconnect();
        return;
    }

    ui->action_Bootloader_Mode->setChecked(false);
    ui->action_Run_Mode->setChecked(false);
    ui->term->close();

    if(!comm->IsOpen())
    {
        if(comm->serial->baudRate() != bootloadBaud)
        {
            comm->serial->setBaudRate(bootloadBaud);
        }

        baudLabel.setText(comm->baudRate());
        if(comm->open() != Comm::Success)
        {
            Disconnect();
            msg.append("Could not open ");
            msg.append(comm->serial->portName());
            msg.append(".");
            statusBar()->showMessage(msg);
            return;
        }
        baudLabel.setText(comm->baudRate());
    }

    comm->assertReset();
    comm->assertBreak();

    deviceLabel.setText("Reset");
    statusBar()->showMessage("BREAK asserted. Power cycle/reset device to force bootloader entry.");

    setBootloadEnabled(false);
}

void MainWindow::on_action_Settings_triggered()
{
    QString msg;
    Settings* dlg = new Settings(this);
    dlg->setPort(comm->serial->portName());
    dlg->setApplicationBaudRate(applicationBaud);
    dlg->setBootloadBaudRate(bootloadBaud);
    dlg->setWriteFlash(writeFlash);
    dlg->setWriteConfig(writeConfig);
    dlg->setWriteEeprom(writeEeprom);

    if(dlg->exec() == QDialog::Accepted)
    {
        applicationBaud = dlg->applicationBaudRate;
        bootloadBaud = dlg->bootloadBaudRate;

        if(comm->IsOpen() && !ui->action_Run_Mode->isChecked() && comm->serial->baudRate() != bootloadBaud)
        {
            qDebug("Sending ETX to force autobaud re-entry for baud rate change.");
            comm->SendETX();;
        }

        if(comm->serial->portName() != dlg->comPort ||
           (ui->action_Run_Mode->isChecked() && comm->serial->baudRate() != applicationBaud) ||
           (!ui->action_Run_Mode->isChecked() && comm->serial->baudRate() != bootloadBaud))
        {
            Disconnect();
            comm->serial->setPortName(dlg->comPort);
            portLabel.setText(comm->serial->portName());
            baudLabel.setText(comm->baudRate());
        }

        writeFlash = dlg->writeFlash;
        writeEeprom = dlg->writeEeprom;
        writeConfig = dlg->writeConfig;
        writePlan->writeConfig = writeConfig;
        deviceWriter->writeConfig = writeConfig;
        deviceVerifier->writeConfig = writeConfig;
    }

    delete dlg;
}

void MainWindow::setupIncrementalBootloading(bool enabled)
{
    if(enabled)
    {
        if(!watchFileName.isEmpty())
        {
            if(fileWatcher != NULL)
            {
                delete fileWatcher;
            }
            fileWatcher = new QFileSystemWatcher(this);
            fileWatcher->addPath(watchFileName);
            connect(fileWatcher, SIGNAL(fileChanged(const QString&)), this, SLOT(modifiedFile(const QString&)));
        }
    }
    else
    {
        if(fileWatcher != NULL)
        {
            disconnect(fileWatcher, SIGNAL(fileChanged(const QString&)), this, SLOT(modifiedFile(const QString&)));
            delete fileWatcher;
            fileWatcher = NULL;
        }
    }
}

void MainWindow::on_action_Incremental_Bootloading_triggered()
{
    if(ui->action_Incremental_Bootloading->isChecked())
    {
        setupIncrementalBootloading(true);
        statusBar()->showMessage("Incremental bootloading enabled.");
    }
    else
    {
        setupIncrementalBootloading(false);
        statusBar()->showMessage("Incremental bootloading disabled.");
    }
}

void MainWindow::SaveTerminalText(QString fileName)
{
}

void MainWindow::SaveHexFile(QString fileName)
{
    HexExporter hex;

    hex.Open(fileName);
    hex.Export(deviceData, device);
    hex.Close();
}

void MainWindow::on_action_Save_triggered()
{
    QString msg, newFileName;
    QTextStream stream(&msg);

    if(ui->term->isConnected())
    {
        newFileName =
            QFileDialog::getSaveFileName(this, "Save", txFileName, "Text Files (*.txt)");

        if(newFileName.isEmpty())
        {
            return;
        }

        SaveTerminalText(newFileName);
    }
    else if(ui->tabWidget->isVisible())
    {
//        newFileName =
//            QFileDialog::getSaveFileName(this, "Save Hex File", fileName, "Hex Files (*.hex *.ehx);;All Files (*.*)", 0, QFileDialog::DontConfirmOverwrite);
        newFileName = QFileDialog::getSaveFileName(this, "Save...", "", "Hex Files (*.hex)", 0, QFileDialog::DontConfirmOverwrite);

        if(newFileName.isEmpty())
        {
            return;
        }

        SaveHexFile(newFileName);
    }
}
