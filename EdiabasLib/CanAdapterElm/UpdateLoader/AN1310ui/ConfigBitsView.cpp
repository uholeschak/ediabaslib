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
* E. Schlunder  2010/02/13  Implemented new config bits view with full
*                           bit field editing/browsing.
************************************************************************/

#include <QStandardItemModel>

#include "ConfigBitsView.h"
#include "ConfigBitsDelegate.h"
#include "ConfigBitsItem.h"

ConfigBitsView::ConfigBitsView(QWidget *parent) : QTreeView(parent)
{
    device = NULL;
    deviceData = NULL;
    verifyData = NULL;
    columnSplit = 0;
}

void ConfigBitsView::setVerifyData(DeviceData* newVerifyData)
{
    verifyData = newVerifyData;
}

bool ConfigBitsView::hasVerifyData(void)
{
    return(verifyData != NULL);
}

void ConfigBitsView::SetDevice(Device* newDevice, DeviceData* newDeviceData)
{
    int i, j;

    device = newDevice;
    deviceData = newDeviceData;

    model.clear();
    model.setHorizontalHeaderItem(0, new QStandardItem("Description"));
    model.setHorizontalHeaderItem(1, new QStandardItem("Setting"));
    QList<QStandardItem*> row;
    Device::ConfigWord* word;
    unsigned int* wordValue;
    unsigned int* wordVerify;
    Device::ConfigField* field;
    ConfigBitsItem* item;

    for(i = 0; i < device->configWords.count(); i++)
    {
        word = &device->configWords[i];
        wordValue = deviceData->ConfigWordPointer(word->address);

        if(verifyData != NULL)
        {
            wordVerify = verifyData->ConfigWordPointer(word->address);
        }
        else
        {
            wordVerify = NULL;
        }

        item = new ConfigBitsItem(device, word, wordValue);
        for(j = 0; j < word->fields.count(); j++)
        {
            field = &word->fields[j];

            row.append(new ConfigBitsItem(device, field->description));
            row.append(new ConfigBitsItem(device, field, word, wordValue, wordVerify));
            item->appendRow(row);
            row.clear();
        }

        row.append(item);
        if(wordVerify != NULL)
        {
            row.append(new ConfigBitsItem(device, word, wordValue, wordVerify));
        }
        else
        {
            row.append(new ConfigBitsItem(device, wordValue));
        }
        model.appendRow(row);
        row.clear();
    }

    setModel(&model);
    setItemDelegate(new ConfigBitsDelegate(&model));
    setEditTriggers(QAbstractItemView::CurrentChanged);
    setWordWrap(true);
    setRootIsDecorated(false);
    setUniformRowHeights(true);
    setDragDropMode(QAbstractItemView::NoDragDrop);

    expandAll();
    setItemsExpandable(false);
    if(columnSplit)
    {
        setColumnWidth(0, columnSplit);
    }
    else
    {
        resizeColumnToContents(0);
    }
}

