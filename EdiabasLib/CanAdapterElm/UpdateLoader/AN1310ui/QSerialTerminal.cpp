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
* E. Schlunder  2009/05/03  Simple serial terminal widget extended from
*                           QTextEdit widget.
************************************************************************/

#include <QtGui/QApplication>
#include <QKeyEvent>
#include <QTime>
#include <QFile>

#include "QextSerialPort/qextserialport.h"
#include "QextSerialPort/qextserialbase.h"

#include "QSerialTerminal.h"

QSerialTerminal::QSerialTerminal(QWidget *parent) : QPlainTextEdit(parent)
{
    serial = NULL;

    setUndoRedoEnabled(false);
    setMouseTracking(true);
    cursor = textCursor();

    TotalRx = 0;
    TotalTx = 0;
    runningAverageDivisor = 1;
    editorCharacterCount = 0;
    rxTime.start();

    transmitThread = new TransmitThread();
}

QSerialTerminal::~QSerialTerminal()
{
    close();
    delete transmitThread;
}

void QSerialTerminal::open(QextSerialPort* newSerialPort)
{
    if(serial != NULL)
    {
        close();
    }

    serial = newSerialPort;
    serial->setTimeout(500);

    transmitThread->serial = serial;
    connect(serial, SIGNAL(readyRead()), SLOT(serialReceive()));
    serial->QueueReceiveSignals = 10;
    timer.start(200, this);
    dirty = true;
}

void QSerialTerminal::close(void)
{
    if(serial != NULL)
    {
        timer.stop();
        disconnect(serial, SIGNAL(readyRead()), this, SLOT(serialReceive()));

        transmitThread->shutdown = true;
        transmitThread->wait();
        serial = NULL;
    }
}

void QSerialTerminal::clear()
{
    TotalRx = 0;
    TotalTx = 0;
    runningAverageDivisor = 1;
    RxPerSecond = 0;

    emit RefreshStatus();
    dirty = false;

    editorCharacterCount = 0;
    QPlainTextEdit::clear();
}

void QSerialTerminal::transmitFile(QString fileName)
{
    QFile file(fileName);

    if (!file.open(QIODevice::ReadOnly))
    {
        return;
    }

    int bytesRead;
    char buffer[4096];
    while(!file.atEnd())
    {
        bytesRead = file.read(buffer, 4096);
        transmitThread->enqueue(buffer, bytesRead);
        TotalTx += bytesRead;
    }
    file.close();
    dirty = true;
}

QextSerialPort* QSerialTerminal::serialPort(void)
{
    return serial;
}

bool QSerialTerminal::isConnected(void)
{
    if(serial == NULL)
    {
        return false;
    }

    return serial->isOpen();
}

void QSerialTerminal::timerEvent(QTimerEvent* e)
{
    if(lastReceive.elapsed() > 200 &&
        serial != NULL &&
        serial->QueueReceiveSignals > 0)
    {
        if(serial->bytesAvailable() > 0)
        {
            serialReceive();
        }

        if(dirty)
        {
            emit RefreshStatus();
            dirty = false;
        }
    }

    QPlainTextEdit::timerEvent(e);
}

void QSerialTerminal::mouseMoveEvent(QMouseEvent* e)
{
    if(lastReceive.elapsed() > 200 &&
        serial != NULL &&
        serial->QueueReceiveSignals > 0)
    {
        if(serial->bytesAvailable() > 0)
        {
            serialReceive();
        }

        if(dirty)
        {
            emit RefreshStatus();
            dirty = false;
        }
    }


    QPlainTextEdit::mouseMoveEvent(e);
}

void QSerialTerminal::keyPressEvent(QKeyEvent* event)
{
    if(!this->isVisible())
    {
        return;
    }

    if(serial != NULL)
    {
        char data = event->text().toAscii()[0];
        int key = event->key();

        switch(key)
        {
            case Qt::Key_Return:
            case Qt::Key_Enter:
                data = '\n';
                key = '\n';
                break;
        }

        if(key < 0x100)
        {
            transmitThread->enqueue(data);
            TotalTx++;
            if(serial->QueueReceiveSignals)
            {
                emit RefreshStatus();
                dirty = false;
            }
            else
            {
                dirty = true;
            }
        }

        event->accept();

        if(lastReceive.elapsed() > 200 &&
            serial->QueueReceiveSignals > 0 &&
            serial->bytesAvailable() > 0)
        {
            serialReceive();
        }
    }
    else
    {
        // Not connected to serial port, revert to being a plain text editor widget
        // so that Cut and Paste key shortcuts work.
        QPlainTextEdit::keyPressEvent(event);
    }
}

void QSerialTerminal::flushText(QString& text, QTextCursor& cursor)
{
    if(text.size() == 0)
    {
        return;
    }

    if(!cursor.atEnd())
    {
        cursor.clearSelection();
        cursor.movePosition(QTextCursor::NextCharacter, QTextCursor::KeepAnchor, text.size());
    }
    cursor.insertText(text);
    text.clear();
}

void QSerialTerminal::serialReceive()
{
    QString text;
    unsigned char byte;
    int i;
    QByteArray data = serial->read(serial->bytesAvailable());    
    int dataCount = data.count();

    double delta, bitsPerSecond;
    static int rxBytes = 0;
    static double elapsed = 0;

    elapsed = rxTime.elapsed() - elapsed;
    rxBytes += dataCount;
    TotalRx += dataCount;

    lastReceive.start();
    if(editorCharacterCount > 5 * 1024)
    {
        QPlainTextEdit::clear();
        editorCharacterCount = 0;
    }

    if(editorCharacterCount + dataCount > 5 * 1024)
    {
        // We are getting too much data to safely display in the GUI.
        QPlainTextEdit::clear();
        editorCharacterCount = 0;

        // Drop some data to prevent GUI lockup.
        data = data.right(5 * 1024);
        dataCount = data.count();
    }

    editorCharacterCount += dataCount;

    for(i = 0; i < dataCount; i++)
    {
        byte = data[i];
        if(byte == '\r')
        {
            flushText(text, cursor);
            int j = cursor.position() + 1;
            cursor.movePosition(QTextCursor::StartOfLine);
            j -= cursor.position();
            editorCharacterCount -= j;
        }
        else if(byte == '\n')
        {
            flushText(text, cursor);
            cursor.movePosition(QTextCursor::EndOfLine);
            text.append(byte);
        }
/*        else if(byte == 0x07)   // audible bell
        {

        }*/
        else if(byte == 0x08)   // backspace
        {
            flushText(text, cursor);
            if(cursor.columnNumber() != 0)
            {
                cursor.movePosition(QTextCursor::PreviousCharacter);
            }
            editorCharacterCount -= 2;
        }
        else
        {
            if(byte >= ' ' && byte < 127)
            {
                text.append(byte);
            }
            else
            {
                text.append('<');
                text.append(QString::number(byte, 16));
                text.append('>');
            }
        }
    }
    flushText(text, cursor);
    setTextCursor(cursor);
    ensureCursorVisible();

    // calculate incoming data rate
    if(rxTime.elapsed() > 1000)
    {
        bitsPerSecond = ((double)(rxBytes * 10 * 1000)) / rxTime.restart();;
        rxBytes = 0;

        // now average the data rate over time so that the display is easier to read
        delta = bitsPerSecond - RxPerSecond;
        if(elapsed < 500)
        {
            delta /= runningAverageDivisor;
            if(runningAverageDivisor < 100)
            {
                runningAverageDivisor++;
            }
        }
        else
        {
            runningAverageDivisor = 1;
        }
        RxPerSecond += delta;
    }
    elapsed = rxTime.elapsed();

    emit RefreshStatus();
    dirty = false;
    serial->QueueReceiveSignals = 1; // allow serial engine to queue another receive event
}

TransmitThread::TransmitThread()
{
    threadBuffer = 0;
    serial = NULL;
    shutdown = true;
}

void TransmitThread::enqueue(char data)
{
    int queueBuffer;

    mutex.lock();
    // pick up the buffer that the thread is NOT using
    queueBuffer = threadBuffer ^ 1;
    transmit[queueBuffer].append(data);
    if(shutdown)
    {
        threadBuffer = queueBuffer;
        shutdown = false;
        start();
    }
    mutex.unlock();
}

void TransmitThread::enqueue(char* data, int length)
{
    int queueBuffer;

    mutex.lock();
    // pick up the buffer that the thread is NOT using
    queueBuffer = threadBuffer ^ 1;
    transmit[queueBuffer].append(data, length);
    if(shutdown)
    {
        threadBuffer = queueBuffer;
        shutdown = false;
        mutex.unlock();
        start();
    }
    else
    {
        mutex.unlock();
    }
}

void TransmitThread::run()
{
    QByteArray* data;
    const char* constData;
    int size;
    while(shutdown == false)
    {
        data = &transmit[threadBuffer];
        size = data->size();
        constData = data->constData();

        serial->write(constData, size);
        transmit[threadBuffer].clear();

        mutex.lock();
        threadBuffer ^= 1;
        if(transmit[threadBuffer].length() == 0)
        {
            shutdown = true;
        }
        mutex.unlock();
    }
}
