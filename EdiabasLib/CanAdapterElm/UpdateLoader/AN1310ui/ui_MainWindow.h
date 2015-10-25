/********************************************************************************
** Form generated from reading UI file 'MainWindow.ui'
**
** Created: Sun 25. Oct 20:21:37 2015
**      by: Qt User Interface Compiler version 4.6.1
**
** WARNING! All changes made in this file will be lost when recompiling UI file!
********************************************************************************/

#ifndef UI_MAINWINDOW_H
#define UI_MAINWINDOW_H

#include <QtCore/QVariant>
#include <QtGui/QAction>
#include <QtGui/QApplication>
#include <QtGui/QButtonGroup>
#include <QtGui/QHBoxLayout>
#include <QtGui/QHeaderView>
#include <QtGui/QMainWindow>
#include <QtGui/QMenu>
#include <QtGui/QMenuBar>
#include <QtGui/QStatusBar>
#include <QtGui/QTabWidget>
#include <QtGui/QTableView>
#include <QtGui/QToolBar>
#include <QtGui/QVBoxLayout>
#include <QtGui/QWidget>
#include "ConfigBitsView.h"
#include "QSerialTerminal.h"

QT_BEGIN_NAMESPACE

class Ui_MainWindowClass
{
public:
    QAction *actionOpen;
    QAction *action_Record;
    QAction *actionExit;
    QAction *actionClear_Memory;
    QAction *actionAbort_Operation;
    QAction *action_Bootloader_Mode;
    QAction *actionRead_Device;
    QAction *actionWrite_Device;
    QAction *actionErase_Device;
    QAction *action_Run_Mode;
    QAction *action_About;
    QAction *action_Verify_Device;
    QAction *action_BreakReset_Mode;
    QAction *action_Settings;
    QAction *action_Incremental_Bootloading;
    QAction *action_Save;
    QWidget *centralWidget;
    QHBoxLayout *horizontalLayout;
    QTabWidget *tabWidget;
    QWidget *FlashTab;
    QVBoxLayout *verticalLayout;
    QTableView *FlashTableView;
    QWidget *EepromTab;
    QVBoxLayout *verticalLayout_2;
    QTableView *EepromTableView;
    QWidget *ConfigTab;
    QVBoxLayout *verticalLayout_3;
    ConfigBitsView *configBitsView;
    QSerialTerminal *term;
    QMenuBar *menuBar;
    QMenu *menuFile;
    QMenu *menuProgrammer;
    QMenu *menu_Help;
    QToolBar *mainToolBar;
    QStatusBar *statusBar;

    void setupUi(QMainWindow *MainWindowClass)
    {
        if (MainWindowClass->objectName().isEmpty())
            MainWindowClass->setObjectName(QString::fromUtf8("MainWindowClass"));
        MainWindowClass->resize(524, 548);
        QSizePolicy sizePolicy(QSizePolicy::Expanding, QSizePolicy::Expanding);
        sizePolicy.setHorizontalStretch(0);
        sizePolicy.setVerticalStretch(0);
        sizePolicy.setHeightForWidth(MainWindowClass->sizePolicy().hasHeightForWidth());
        MainWindowClass->setSizePolicy(sizePolicy);
        QIcon icon;
        icon.addFile(QString::fromUtf8(":/MainWindow/img/microchip.png"), QSize(), QIcon::Normal, QIcon::Off);
        MainWindowClass->setWindowIcon(icon);
        actionOpen = new QAction(MainWindowClass);
        actionOpen->setObjectName(QString::fromUtf8("actionOpen"));
        actionOpen->setEnabled(false);
        QIcon icon1;
        icon1.addFile(QString::fromUtf8(":/MainWindow/img/open.png"), QSize(), QIcon::Normal, QIcon::Off);
        actionOpen->setIcon(icon1);
        action_Record = new QAction(MainWindowClass);
        action_Record->setObjectName(QString::fromUtf8("action_Record"));
        action_Record->setCheckable(true);
        action_Record->setEnabled(true);
        QIcon icon2;
        icon2.addFile(QString::fromUtf8(":/MainWindow/img/record.png"), QSize(), QIcon::Normal, QIcon::Off);
        action_Record->setIcon(icon2);
        action_Record->setAutoRepeat(false);
        actionExit = new QAction(MainWindowClass);
        actionExit->setObjectName(QString::fromUtf8("actionExit"));
        actionClear_Memory = new QAction(MainWindowClass);
        actionClear_Memory->setObjectName(QString::fromUtf8("actionClear_Memory"));
        actionClear_Memory->setEnabled(false);
        QIcon icon3;
        icon3.addFile(QString::fromUtf8(":/MainWindow/img/clear.png"), QSize(), QIcon::Normal, QIcon::Off);
        actionClear_Memory->setIcon(icon3);
        actionAbort_Operation = new QAction(MainWindowClass);
        actionAbort_Operation->setObjectName(QString::fromUtf8("actionAbort_Operation"));
        actionAbort_Operation->setEnabled(false);
        QIcon icon4;
        icon4.addFile(QString::fromUtf8(":/MainWindow/img/abort.png"), QSize(), QIcon::Normal, QIcon::Off);
        actionAbort_Operation->setIcon(icon4);
        action_Bootloader_Mode = new QAction(MainWindowClass);
        action_Bootloader_Mode->setObjectName(QString::fromUtf8("action_Bootloader_Mode"));
        action_Bootloader_Mode->setCheckable(true);
        action_Bootloader_Mode->setChecked(false);
        QIcon icon5;
        icon5.addFile(QString::fromUtf8(":/MainWindow/img/stop.png"), QSize(), QIcon::Normal, QIcon::Off);
        action_Bootloader_Mode->setIcon(icon5);
        actionRead_Device = new QAction(MainWindowClass);
        actionRead_Device->setObjectName(QString::fromUtf8("actionRead_Device"));
        actionRead_Device->setEnabled(false);
        QIcon icon6;
        icon6.addFile(QString::fromUtf8(":/MainWindow/img/readtqfp.png"), QSize(), QIcon::Normal, QIcon::Off);
        actionRead_Device->setIcon(icon6);
        actionWrite_Device = new QAction(MainWindowClass);
        actionWrite_Device->setObjectName(QString::fromUtf8("actionWrite_Device"));
        actionWrite_Device->setEnabled(false);
        QIcon icon7;
        icon7.addFile(QString::fromUtf8(":/MainWindow/img/writetqfp.png"), QSize(), QIcon::Normal, QIcon::Off);
        actionWrite_Device->setIcon(icon7);
        actionErase_Device = new QAction(MainWindowClass);
        actionErase_Device->setObjectName(QString::fromUtf8("actionErase_Device"));
        actionErase_Device->setEnabled(false);
        QIcon icon8;
        icon8.addFile(QString::fromUtf8(":/MainWindow/img/erasetqfp.png"), QSize(), QIcon::Normal, QIcon::Off);
        actionErase_Device->setIcon(icon8);
        action_Run_Mode = new QAction(MainWindowClass);
        action_Run_Mode->setObjectName(QString::fromUtf8("action_Run_Mode"));
        action_Run_Mode->setCheckable(true);
        action_Run_Mode->setEnabled(true);
        QIcon icon9;
        icon9.addFile(QString::fromUtf8(":/MainWindow/img/execute.png"), QSize(), QIcon::Normal, QIcon::Off);
        action_Run_Mode->setIcon(icon9);
        action_About = new QAction(MainWindowClass);
        action_About->setObjectName(QString::fromUtf8("action_About"));
        action_Verify_Device = new QAction(MainWindowClass);
        action_Verify_Device->setObjectName(QString::fromUtf8("action_Verify_Device"));
        action_Verify_Device->setEnabled(false);
        QIcon icon10;
        icon10.addFile(QString::fromUtf8(":/MainWindow/img/verify.png"), QSize(), QIcon::Normal, QIcon::Off);
        action_Verify_Device->setIcon(icon10);
        action_BreakReset_Mode = new QAction(MainWindowClass);
        action_BreakReset_Mode->setObjectName(QString::fromUtf8("action_BreakReset_Mode"));
        action_BreakReset_Mode->setCheckable(true);
        QIcon icon11;
        icon11.addFile(QString::fromUtf8(":/MainWindow/img/pause.png"), QSize(), QIcon::Normal, QIcon::Off);
        action_BreakReset_Mode->setIcon(icon11);
        action_Settings = new QAction(MainWindowClass);
        action_Settings->setObjectName(QString::fromUtf8("action_Settings"));
        action_Incremental_Bootloading = new QAction(MainWindowClass);
        action_Incremental_Bootloading->setObjectName(QString::fromUtf8("action_Incremental_Bootloading"));
        action_Incremental_Bootloading->setCheckable(true);
        action_Save = new QAction(MainWindowClass);
        action_Save->setObjectName(QString::fromUtf8("action_Save"));
        action_Save->setEnabled(false);
        QIcon icon12;
        icon12.addFile(QString::fromUtf8(":/MainWindow/img/save.png"), QSize(), QIcon::Normal, QIcon::Off);
        action_Save->setIcon(icon12);
        action_Save->setVisible(false);
        centralWidget = new QWidget(MainWindowClass);
        centralWidget->setObjectName(QString::fromUtf8("centralWidget"));
        horizontalLayout = new QHBoxLayout(centralWidget);
        horizontalLayout->setSpacing(6);
        horizontalLayout->setContentsMargins(11, 11, 11, 11);
        horizontalLayout->setObjectName(QString::fromUtf8("horizontalLayout"));
        horizontalLayout->setContentsMargins(0, 0, 0, 1);
        tabWidget = new QTabWidget(centralWidget);
        tabWidget->setObjectName(QString::fromUtf8("tabWidget"));
        tabWidget->setLayoutDirection(Qt::LeftToRight);
        tabWidget->setTabPosition(QTabWidget::South);
        tabWidget->setTabShape(QTabWidget::Rounded);
        tabWidget->setDocumentMode(true);
        tabWidget->setTabsClosable(false);
        FlashTab = new QWidget();
        FlashTab->setObjectName(QString::fromUtf8("FlashTab"));
        verticalLayout = new QVBoxLayout(FlashTab);
        verticalLayout->setSpacing(6);
        verticalLayout->setContentsMargins(0, 0, 0, 0);
        verticalLayout->setObjectName(QString::fromUtf8("verticalLayout"));
        FlashTableView = new QTableView(FlashTab);
        FlashTableView->setObjectName(QString::fromUtf8("FlashTableView"));
        QFont font;
        font.setFamily(QString::fromUtf8("Courier New"));
        font.setPointSize(10);
        FlashTableView->setFont(font);
        FlashTableView->setFrameShape(QFrame::NoFrame);
        FlashTableView->setFrameShadow(QFrame::Sunken);
        FlashTableView->setTabKeyNavigation(false);
        FlashTableView->setAlternatingRowColors(false);
        FlashTableView->setSelectionMode(QAbstractItemView::SingleSelection);
        FlashTableView->setShowGrid(false);
        FlashTableView->horizontalHeader()->setHighlightSections(false);
        FlashTableView->verticalHeader()->setDefaultSectionSize(22);
        FlashTableView->verticalHeader()->setHighlightSections(false);
        FlashTableView->verticalHeader()->setMinimumSectionSize(20);

        verticalLayout->addWidget(FlashTableView);

        tabWidget->addTab(FlashTab, QString());
        EepromTab = new QWidget();
        EepromTab->setObjectName(QString::fromUtf8("EepromTab"));
        verticalLayout_2 = new QVBoxLayout(EepromTab);
        verticalLayout_2->setSpacing(6);
        verticalLayout_2->setContentsMargins(0, 0, 0, 0);
        verticalLayout_2->setObjectName(QString::fromUtf8("verticalLayout_2"));
        EepromTableView = new QTableView(EepromTab);
        EepromTableView->setObjectName(QString::fromUtf8("EepromTableView"));
        EepromTableView->setFont(font);
        EepromTableView->setFrameShape(QFrame::NoFrame);
        EepromTableView->setTabKeyNavigation(false);
        EepromTableView->setAlternatingRowColors(true);
        EepromTableView->setSelectionMode(QAbstractItemView::SingleSelection);
        EepromTableView->setShowGrid(false);
        EepromTableView->horizontalHeader()->setHighlightSections(false);
        EepromTableView->verticalHeader()->setDefaultSectionSize(22);
        EepromTableView->verticalHeader()->setHighlightSections(false);
        EepromTableView->verticalHeader()->setMinimumSectionSize(20);

        verticalLayout_2->addWidget(EepromTableView);

        tabWidget->addTab(EepromTab, QString());
        ConfigTab = new QWidget();
        ConfigTab->setObjectName(QString::fromUtf8("ConfigTab"));
        verticalLayout_3 = new QVBoxLayout(ConfigTab);
        verticalLayout_3->setSpacing(6);
        verticalLayout_3->setContentsMargins(0, 0, 0, 0);
        verticalLayout_3->setObjectName(QString::fromUtf8("verticalLayout_3"));
        configBitsView = new ConfigBitsView(ConfigTab);
        configBitsView->setObjectName(QString::fromUtf8("configBitsView"));
        configBitsView->setEditTriggers(QAbstractItemView::AllEditTriggers);
        configBitsView->setRootIsDecorated(false);
        configBitsView->setItemsExpandable(true);

        verticalLayout_3->addWidget(configBitsView);

        tabWidget->addTab(ConfigTab, QString());

        horizontalLayout->addWidget(tabWidget);

        term = new QSerialTerminal(centralWidget);
        term->setObjectName(QString::fromUtf8("term"));
        QFont font1;
        font1.setFamily(QString::fromUtf8("Courier"));
        font1.setPointSize(9);
        term->setFont(font1);

        horizontalLayout->addWidget(term);

        MainWindowClass->setCentralWidget(centralWidget);
        menuBar = new QMenuBar(MainWindowClass);
        menuBar->setObjectName(QString::fromUtf8("menuBar"));
        menuBar->setGeometry(QRect(0, 0, 524, 18));
        menuFile = new QMenu(menuBar);
        menuFile->setObjectName(QString::fromUtf8("menuFile"));
        menuProgrammer = new QMenu(menuBar);
        menuProgrammer->setObjectName(QString::fromUtf8("menuProgrammer"));
        menu_Help = new QMenu(menuBar);
        menu_Help->setObjectName(QString::fromUtf8("menu_Help"));
        MainWindowClass->setMenuBar(menuBar);
        mainToolBar = new QToolBar(MainWindowClass);
        mainToolBar->setObjectName(QString::fromUtf8("mainToolBar"));
        mainToolBar->setMovable(false);
        mainToolBar->setFloatable(false);
        MainWindowClass->addToolBar(Qt::TopToolBarArea, mainToolBar);
        statusBar = new QStatusBar(MainWindowClass);
        statusBar->setObjectName(QString::fromUtf8("statusBar"));
        MainWindowClass->setStatusBar(statusBar);

        menuBar->addAction(menuFile->menuAction());
        menuBar->addAction(menuProgrammer->menuAction());
        menuBar->addAction(menu_Help->menuAction());
        menuFile->addAction(actionClear_Memory);
        menuFile->addAction(actionOpen);
        menuFile->addAction(action_Save);
        menuFile->addSeparator();
        menuFile->addAction(action_Record);
        menuFile->addSeparator();
        menuFile->addAction(actionExit);
        menuProgrammer->addAction(actionAbort_Operation);
        menuProgrammer->addSeparator();
        menuProgrammer->addAction(action_Run_Mode);
        menuProgrammer->addAction(action_BreakReset_Mode);
        menuProgrammer->addAction(action_Bootloader_Mode);
        menuProgrammer->addSeparator();
        menuProgrammer->addAction(actionRead_Device);
        menuProgrammer->addAction(actionWrite_Device);
        menuProgrammer->addAction(actionErase_Device);
        menuProgrammer->addAction(action_Verify_Device);
        menuProgrammer->addSeparator();
        menuProgrammer->addAction(action_Incremental_Bootloading);
        menuProgrammer->addAction(action_Settings);
        menu_Help->addAction(action_About);
        mainToolBar->addAction(actionClear_Memory);
        mainToolBar->addAction(actionOpen);
        mainToolBar->addAction(action_Save);
        mainToolBar->addSeparator();
        mainToolBar->addAction(actionAbort_Operation);
        mainToolBar->addSeparator();
        mainToolBar->addAction(action_Record);
        mainToolBar->addAction(action_Run_Mode);
        mainToolBar->addAction(action_BreakReset_Mode);
        mainToolBar->addAction(action_Bootloader_Mode);
        mainToolBar->addSeparator();
        mainToolBar->addAction(actionRead_Device);
        mainToolBar->addAction(actionWrite_Device);
        mainToolBar->addAction(actionErase_Device);
        mainToolBar->addAction(action_Verify_Device);
        mainToolBar->addSeparator();

        retranslateUi(MainWindowClass);

        tabWidget->setCurrentIndex(0);


        QMetaObject::connectSlotsByName(MainWindowClass);
    } // setupUi

    void retranslateUi(QMainWindow *MainWindowClass)
    {
        MainWindowClass->setWindowTitle(QApplication::translate("MainWindowClass", "AN1310ui", 0, QApplication::UnicodeUTF8));
        actionOpen->setText(QApplication::translate("MainWindowClass", "&Open", 0, QApplication::UnicodeUTF8));
        actionOpen->setShortcut(QApplication::translate("MainWindowClass", "Ctrl+O", 0, QApplication::UnicodeUTF8));
        action_Record->setText(QApplication::translate("MainWindowClass", "&Record to...", 0, QApplication::UnicodeUTF8));
#ifndef QT_NO_TOOLTIP
        action_Record->setToolTip(QApplication::translate("MainWindowClass", "Record to file", 0, QApplication::UnicodeUTF8));
#endif // QT_NO_TOOLTIP
        action_Record->setShortcut(QApplication::translate("MainWindowClass", "Ctrl+R", 0, QApplication::UnicodeUTF8));
        actionExit->setText(QApplication::translate("MainWindowClass", "E&xit", 0, QApplication::UnicodeUTF8));
        actionExit->setShortcut(QApplication::translate("MainWindowClass", "Ctrl+Q", 0, QApplication::UnicodeUTF8));
        actionClear_Memory->setText(QApplication::translate("MainWindowClass", "&New", 0, QApplication::UnicodeUTF8));
        actionClear_Memory->setShortcut(QApplication::translate("MainWindowClass", "Ctrl+N", 0, QApplication::UnicodeUTF8));
        actionAbort_Operation->setText(QApplication::translate("MainWindowClass", "Abort Operation", 0, QApplication::UnicodeUTF8));
        actionAbort_Operation->setShortcut(QApplication::translate("MainWindowClass", "Esc", 0, QApplication::UnicodeUTF8));
        action_Bootloader_Mode->setText(QApplication::translate("MainWindowClass", "Bootloader Mode", 0, QApplication::UnicodeUTF8));
#ifndef QT_NO_TOOLTIP
        action_Bootloader_Mode->setToolTip(QApplication::translate("MainWindowClass", "Bootloader Mode", 0, QApplication::UnicodeUTF8));
#endif // QT_NO_TOOLTIP
        action_Bootloader_Mode->setShortcut(QApplication::translate("MainWindowClass", "F4", 0, QApplication::UnicodeUTF8));
        actionRead_Device->setText(QApplication::translate("MainWindowClass", "Read Device", 0, QApplication::UnicodeUTF8));
        actionRead_Device->setShortcut(QApplication::translate("MainWindowClass", "F5", 0, QApplication::UnicodeUTF8));
        actionWrite_Device->setText(QApplication::translate("MainWindowClass", "Write Device", 0, QApplication::UnicodeUTF8));
        actionWrite_Device->setShortcut(QApplication::translate("MainWindowClass", "F6", 0, QApplication::UnicodeUTF8));
        actionErase_Device->setText(QApplication::translate("MainWindowClass", "Erase Device", 0, QApplication::UnicodeUTF8));
        actionErase_Device->setShortcut(QApplication::translate("MainWindowClass", "F7", 0, QApplication::UnicodeUTF8));
        action_Run_Mode->setText(QApplication::translate("MainWindowClass", "Run Mode", 0, QApplication::UnicodeUTF8));
        action_Run_Mode->setIconText(QApplication::translate("MainWindowClass", "Run Mode", 0, QApplication::UnicodeUTF8));
#ifndef QT_NO_TOOLTIP
        action_Run_Mode->setToolTip(QApplication::translate("MainWindowClass", "Run Application Firmware", 0, QApplication::UnicodeUTF8));
#endif // QT_NO_TOOLTIP
        action_Run_Mode->setShortcut(QApplication::translate("MainWindowClass", "F2", 0, QApplication::UnicodeUTF8));
        action_About->setText(QApplication::translate("MainWindowClass", "&About", 0, QApplication::UnicodeUTF8));
        action_About->setShortcut(QApplication::translate("MainWindowClass", "F1", 0, QApplication::UnicodeUTF8));
        action_Verify_Device->setText(QApplication::translate("MainWindowClass", "&Verify Device", 0, QApplication::UnicodeUTF8));
        action_Verify_Device->setShortcut(QApplication::translate("MainWindowClass", "F8", 0, QApplication::UnicodeUTF8));
        action_BreakReset_Mode->setText(QApplication::translate("MainWindowClass", "Break/&Reset Mode", 0, QApplication::UnicodeUTF8));
#ifndef QT_NO_TOOLTIP
        action_BreakReset_Mode->setToolTip(QApplication::translate("MainWindowClass", "Break/Reset Application Firmware", 0, QApplication::UnicodeUTF8));
#endif // QT_NO_TOOLTIP
        action_BreakReset_Mode->setShortcut(QApplication::translate("MainWindowClass", "F3", 0, QApplication::UnicodeUTF8));
        action_Settings->setText(QApplication::translate("MainWindowClass", "&Settings...", 0, QApplication::UnicodeUTF8));
        action_Settings->setShortcut(QApplication::translate("MainWindowClass", "F12", 0, QApplication::UnicodeUTF8));
        action_Incremental_Bootloading->setText(QApplication::translate("MainWindowClass", "&Incremental Bootloading", 0, QApplication::UnicodeUTF8));
        action_Incremental_Bootloading->setShortcut(QApplication::translate("MainWindowClass", "F11", 0, QApplication::UnicodeUTF8));
        action_Save->setText(QApplication::translate("MainWindowClass", "&Save", 0, QApplication::UnicodeUTF8));
#ifndef QT_NO_TOOLTIP
        action_Save->setToolTip(QApplication::translate("MainWindowClass", "Save", 0, QApplication::UnicodeUTF8));
#endif // QT_NO_TOOLTIP
        action_Save->setShortcut(QApplication::translate("MainWindowClass", "Ctrl+S", 0, QApplication::UnicodeUTF8));
        tabWidget->setTabText(tabWidget->indexOf(FlashTab), QApplication::translate("MainWindowClass", "FLASH", 0, QApplication::UnicodeUTF8));
        tabWidget->setTabText(tabWidget->indexOf(EepromTab), QApplication::translate("MainWindowClass", "EEPROM", 0, QApplication::UnicodeUTF8));
        tabWidget->setTabText(tabWidget->indexOf(ConfigTab), QApplication::translate("MainWindowClass", "CONFIG", 0, QApplication::UnicodeUTF8));
        menuFile->setTitle(QApplication::translate("MainWindowClass", "&File", 0, QApplication::UnicodeUTF8));
        menuProgrammer->setTitle(QApplication::translate("MainWindowClass", "&Program", 0, QApplication::UnicodeUTF8));
        menu_Help->setTitle(QApplication::translate("MainWindowClass", "&Help", 0, QApplication::UnicodeUTF8));
        mainToolBar->setWindowTitle(QApplication::translate("MainWindowClass", "Display Toolbar", 0, QApplication::UnicodeUTF8));
    } // retranslateUi

};

namespace Ui {
    class MainWindowClass: public Ui_MainWindowClass {};
} // namespace Ui

QT_END_NAMESPACE

#endif // UI_MAINWINDOW_H
