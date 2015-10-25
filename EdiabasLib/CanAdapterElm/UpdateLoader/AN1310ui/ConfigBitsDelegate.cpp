/************************************************************************
* Copyright (c) 2010,  Microchip Technology Inc.
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

#include <QComboBox>

#include "ConfigBitsDelegate.h"
#include "ConfigBitsItem.h"
#include "ConfigBitsView.h"

ConfigBitsDelegate::ConfigBitsDelegate(QStandardItemModel* parent) :
    QStyledItemDelegate(parent)
{
    model = parent;
}

/**
 * Forces the combobox item changes to commit back to the model immediately rather than waiting for
 * the user to select a different row or press Enter.
 */
void ConfigBitsDelegate::activated(int index)
{
    QWidget* widget = (QWidget*)sender();
    commitData(widget);
}

void ConfigBitsDelegate::setModelData(QWidget *editor, QAbstractItemModel *junkModel, const QModelIndex &index) const
{
    QComboBox* comboBox = static_cast<QComboBox*>(editor);

    ConfigBitsItem* item = static_cast<ConfigBitsItem*>(model->itemFromIndex(index));
    if(item->field != NULL)
    {
        int selectedIndex = comboBox->currentIndex();
        if(selectedIndex != -1)
        {
            Device::ConfigFieldSetting* setting = &item->field->settings[selectedIndex];
            unsigned int wordVal = *item->wordValue;
            unsigned int bitMask = ~setting->bitMask;
            unsigned int bitVal = setting->bitValue;
            wordVal &= bitMask;
            wordVal |= bitVal;
            *item->wordValue = wordVal;
            item->setText(setting->description);

            ConfigBitsItem* parent = static_cast<ConfigBitsItem*>(item->parent());
            if(parent)
            {
                ConfigBitsItem* parentValue = static_cast<ConfigBitsItem*>(model->item(parent->row(), 1));
                parentValue->setText(parentValue->ToText(wordVal));
            }
        }
    }
    model->submit();
}

QWidget* ConfigBitsDelegate::createEditor(QWidget* parent, const QStyleOptionViewItem& option,
                                          const QModelIndex& index) const
{
    if(index.parent().isValid() && index.column() == 1)
    {
        QComboBox* editor = new QComboBox(parent);        
        ConfigBitsItem* item = static_cast<ConfigBitsItem*>(model->itemFromIndex(index));
        int selectedIndex = 0;
        if(item->field != NULL)
        {
            for(int i = 0; i < item->field->settings.count(); i++)
            {
                Device::ConfigFieldSetting* setting = &item->field->settings[i];
                editor->addItem(setting->description);
                if((*item->wordValue & setting->bitMask) == setting->bitValue)
                {
                    selectedIndex = i;
                }
            }
            editor->setCurrentIndex(selectedIndex);
        }

        connect(editor, SIGNAL(activated(int)), this, SLOT(activated(int)));
        return editor;
    }

    return NULL;
}

QSize ConfigBitsDelegate::sizeHint(const QStyleOptionViewItem& option, const QModelIndex& index) const
{
    QSize size = QStyledItemDelegate::sizeHint(option, index);
    size.setHeight(size.height() + 6);
    return size;
}
