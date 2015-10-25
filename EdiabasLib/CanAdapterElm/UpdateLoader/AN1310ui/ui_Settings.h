/********************************************************************************
** Form generated from reading UI file 'Settings.ui'
**
** Created: Sun 25. Oct 20:21:37 2015
**      by: Qt User Interface Compiler version 4.6.1
**
** WARNING! All changes made in this file will be lost when recompiling UI file!
********************************************************************************/

#ifndef UI_SETTINGS_H
#define UI_SETTINGS_H

#include <QtCore/QVariant>
#include <QtGui/QAction>
#include <QtGui/QApplication>
#include <QtGui/QButtonGroup>
#include <QtGui/QCheckBox>
#include <QtGui/QComboBox>
#include <QtGui/QDialog>
#include <QtGui/QDialogButtonBox>
#include <QtGui/QGroupBox>
#include <QtGui/QHeaderView>
#include <QtGui/QLabel>

QT_BEGIN_NAMESPACE

class Ui_Settings
{
public:
    QDialogButtonBox *buttonBox;
    QGroupBox *CommunicationGroupBox;
    QComboBox *ComPortComboBox;
    QLabel *label;
    QLabel *label_2;
    QComboBox *BootloadBaudRateComboBox;
    QComboBox *ApplicationBaudRateComboBox;
    QLabel *label_3;
    QGroupBox *WriteOptionsGroupBox;
    QCheckBox *FlashProgramMemorycheckBox;
    QCheckBox *ConfigBitsCheckBox;
    QCheckBox *EepromCheckBox;

    void setupUi(QDialog *Settings)
    {
        if (Settings->objectName().isEmpty())
            Settings->setObjectName(QString::fromUtf8("Settings"));
        Settings->resize(402, 346);
        QIcon icon;
        icon.addFile(QString::fromUtf8(":/MainWindow/img/microchip.png"), QSize(), QIcon::Normal, QIcon::Off);
        Settings->setWindowIcon(icon);
        buttonBox = new QDialogButtonBox(Settings);
        buttonBox->setObjectName(QString::fromUtf8("buttonBox"));
        buttonBox->setGeometry(QRect(30, 300, 341, 32));
        buttonBox->setOrientation(Qt::Horizontal);
        buttonBox->setStandardButtons(QDialogButtonBox::Cancel|QDialogButtonBox::Ok);
        CommunicationGroupBox = new QGroupBox(Settings);
        CommunicationGroupBox->setObjectName(QString::fromUtf8("CommunicationGroupBox"));
        CommunicationGroupBox->setGeometry(QRect(20, 10, 361, 131));
        ComPortComboBox = new QComboBox(CommunicationGroupBox);
        ComPortComboBox->setObjectName(QString::fromUtf8("ComPortComboBox"));
        ComPortComboBox->setGeometry(QRect(10, 40, 341, 22));
        label = new QLabel(CommunicationGroupBox);
        label->setObjectName(QString::fromUtf8("label"));
        label->setGeometry(QRect(10, 20, 341, 16));
        label_2 = new QLabel(CommunicationGroupBox);
        label_2->setObjectName(QString::fromUtf8("label_2"));
        label_2->setGeometry(QRect(10, 70, 141, 16));
        BootloadBaudRateComboBox = new QComboBox(CommunicationGroupBox);
        BootloadBaudRateComboBox->setObjectName(QString::fromUtf8("BootloadBaudRateComboBox"));
        BootloadBaudRateComboBox->setGeometry(QRect(10, 90, 161, 22));
        ApplicationBaudRateComboBox = new QComboBox(CommunicationGroupBox);
        ApplicationBaudRateComboBox->setObjectName(QString::fromUtf8("ApplicationBaudRateComboBox"));
        ApplicationBaudRateComboBox->setGeometry(QRect(190, 90, 161, 22));
        label_3 = new QLabel(CommunicationGroupBox);
        label_3->setObjectName(QString::fromUtf8("label_3"));
        label_3->setGeometry(QRect(190, 70, 141, 16));
        WriteOptionsGroupBox = new QGroupBox(Settings);
        WriteOptionsGroupBox->setObjectName(QString::fromUtf8("WriteOptionsGroupBox"));
        WriteOptionsGroupBox->setGeometry(QRect(20, 150, 361, 131));
        FlashProgramMemorycheckBox = new QCheckBox(WriteOptionsGroupBox);
        FlashProgramMemorycheckBox->setObjectName(QString::fromUtf8("FlashProgramMemorycheckBox"));
        FlashProgramMemorycheckBox->setGeometry(QRect(17, 30, 201, 19));
        ConfigBitsCheckBox = new QCheckBox(WriteOptionsGroupBox);
        ConfigBitsCheckBox->setObjectName(QString::fromUtf8("ConfigBitsCheckBox"));
        ConfigBitsCheckBox->setGeometry(QRect(17, 60, 201, 19));
        EepromCheckBox = new QCheckBox(WriteOptionsGroupBox);
        EepromCheckBox->setObjectName(QString::fromUtf8("EepromCheckBox"));
        EepromCheckBox->setGeometry(QRect(16, 90, 201, 19));

        retranslateUi(Settings);
        QObject::connect(buttonBox, SIGNAL(accepted()), Settings, SLOT(accept()));
        QObject::connect(buttonBox, SIGNAL(rejected()), Settings, SLOT(reject()));

        QMetaObject::connectSlotsByName(Settings);
    } // setupUi

    void retranslateUi(QDialog *Settings)
    {
        Settings->setWindowTitle(QApplication::translate("Settings", "Settings", 0, QApplication::UnicodeUTF8));
        CommunicationGroupBox->setTitle(QApplication::translate("Settings", "Communication", 0, QApplication::UnicodeUTF8));
        label->setText(QApplication::translate("Settings", "COM Port:", 0, QApplication::UnicodeUTF8));
        label_2->setText(QApplication::translate("Settings", "Bootload Baud Rate", 0, QApplication::UnicodeUTF8));
        label_3->setText(QApplication::translate("Settings", "Application Baud Rate", 0, QApplication::UnicodeUTF8));
        WriteOptionsGroupBox->setTitle(QApplication::translate("Settings", "Write Options", 0, QApplication::UnicodeUTF8));
        FlashProgramMemorycheckBox->setText(QApplication::translate("Settings", "FLASH Program Memory", 0, QApplication::UnicodeUTF8));
        ConfigBitsCheckBox->setText(QApplication::translate("Settings", "Config Bits", 0, QApplication::UnicodeUTF8));
        EepromCheckBox->setText(QApplication::translate("Settings", "EEPROM", 0, QApplication::UnicodeUTF8));
    } // retranslateUi

};

namespace Ui {
    class Settings: public Ui_Settings {};
} // namespace Ui

QT_END_NAMESPACE

#endif // UI_SETTINGS_H
