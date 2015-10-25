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

#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QtGui/QMainWindow>
#include <QLabel>
#include <QFileInfo>
#include <QFileSystemWatcher>
#include <QtCore/QProcess>
#include <QMenu>

#include "Bootload/Comm.h"
#include "Bootload/DeviceData.h"
#include "Bootload/Device.h"
#include "Bootload/DeviceWritePlanner.h"
#include "Bootload/DeviceVerifyPlanner.h"
#include "Bootload/DeviceReader.h"
#include "Bootload/DeviceWriter.h"
#include "Bootload/DeviceVerifier.h"
#include "Bootload/ImportExportHex.h"
#include "FlashViewModel.h"
#include "EepromViewModel.h"
#include "ConfigBitsView.h"
#include "QSerialTerminal.h"

namespace Ui
{
    class MainWindowClass;
}

#define MAX_RECENT_FILES 6

/*!
 * The main Serial Bootloader GUI window.
 */
class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    MainWindow(QWidget *parent = 0);
    ~MainWindow();

    int ConnectBootloader(void);
    int LoadFile(QString fileName);
    void TransmitFile(QString newFileName);

    QString SelectDevice(unsigned int deviceId, Device::Families familyId);

    int ReadDevice(void);
    int EraseDevice(void);
    int WriteDevice(void);
    int WriteDevice(DeviceData* newData, DeviceData* existingData = NULL);
    int VerifyDevice(bool verifyFlash = true);
    void SaveTerminalText(QString fileName);
    void SaveHexFile(QString fileName);

    void setBootloadBusy(bool busy);

signals:
    void ReadDeviceCompleted(int failed, bool flash, bool eeprom, bool config, double flashTime, double eepromTime, double configTime);
    void WriteDeviceCompleted(int failed, bool flash, bool eeprom, bool config, double flashTime, double eepromTime, double configTime);
    void EraseDeviceCompleted(int failed, double eraseTime);
    void VerifyDeviceCompleted(QString msg, bool refreshViews, bool clearConfigVerifyData);

public slots:
    void modifiedFile(const QString& path);
    void openRecentFile(void);
    void ReadDeviceComplete(int failed, bool flash, bool eeprom, bool config, double flashTime, double eepromTime, double configTime);
    void WriteDeviceComplete(int failed, bool flash, bool eeprom, bool config, double flashTime, double eepromTime, double configTime);
    void EraseDeviceComplete(int failed, double eraseTime);
    void VerifyDeviceComplete(QString msg, bool refreshViews, bool clearConfigVerifyData);

protected:
    Comm* comm;
    unsigned int applicationBaud;
    unsigned int bootloadBaud;
    DeviceData* deviceData;
    DeviceData* hexData;
    DeviceData* verifyData;
    Device* device;
    DeviceWritePlanner* writePlan;
    DeviceVerifyPlanner* verifyPlan;
    DeviceReader* deviceReader;
    DeviceWriter* deviceWriter;
    DeviceVerifier* deviceVerifier;

    FlashViewModel* flashViewModel;
    EepromViewModel* eepromViewModel;

    QString fileName, watchFileName, txFileName, rxFileName;
    QFileSystemWatcher* fileWatcher;

    bool writeFlash;
    bool writeEeprom;
    bool writeConfig;
    bool eraseDuringWrite;

    void Disconnect(void);
    void setBootloadEnabled(bool enable);

    void RefreshViews(void);
    void incrementalBootload(QString& normalTitle, QFileInfo& fi, HexImporter& import, DeviceData* importData);
    void UpdateRecentFileList(void);
    void setupIncrementalBootloading(bool enabled);
    void countFlashVerifyFailures(int& flashFails, unsigned int& failAddress, Device::MemoryRange range);

    Comm::ErrorCode RemapInterruptVectors(Device* device, DeviceData* deviceData);

    bool abortOperation;

private:
    Ui::MainWindowClass *ui;
    QLabel deviceLabel;
    QLabel portLabel;
    QLabel baudLabel;
    QLabel txRxLabel;

    int failed;
    QAction *recentFiles[MAX_RECENT_FILES];

    bool allowRxFileOverwrite;
    bool wasBootloaderMode;

private slots:
    void on_action_Save_triggered();
    void on_action_Incremental_Bootloading_triggered();
    void on_action_Record_triggered();
    void on_action_Settings_triggered();
    void on_action_Bootloader_Mode_triggered();
    void on_action_Run_Mode_triggered();
    void on_action_BreakReset_Mode_triggered();
    void on_action_Verify_Device_triggered();
    void on_action_About_triggered();
    void on_actionWrite_Device_triggered();
    void on_actionClear_Memory_triggered();
    void on_actionOpen_triggered();
    void on_actionRead_Device_triggered();
    void on_actionAbort_Operation_triggered();
    void on_actionErase_Device_triggered();
    void on_actionExit_triggered();

    void RefreshStatus();
};

#endif // MAINWINDOW_H
