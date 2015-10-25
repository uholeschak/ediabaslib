#ifndef _POSIX_QEXTSERIALPORT_H_
#define _POSIX_QEXTSERIALPORT_H_

#include <QThread>
#include <QWaitCondition>

#include <stdio.h>
#include <termios.h>
#include <errno.h>
#include <unistd.h>
#include <sys/time.h>
#include <sys/ioctl.h>
#include <sys/select.h>

#include "qextserialbase.h"

class Posix_QextSerialReaderThread;

/*!
 * This class encapsulates the Posix (Linux) portion of QextSerialPort.
 */
class Posix_QextSerialPort : public QextSerialBase
{
    friend class Posix_QextSerialReaderThread;

    private:
    /*!
     * This method is a part of constructor.
     */
    void init();

protected:
    QFile* Posix_File;
    int handle;

    struct termios Posix_CommConfig;
    struct timeval Posix_Timeout;
    struct timeval Posix_Copy_Timeout;

    virtual qint64 readData(char * data, qint64 maxSize);
    virtual qint64 writeData(const char * data, qint64 maxSize);

    Posix_QextSerialReaderThread* readerThread;

public:
    Posix_QextSerialPort();
    Posix_QextSerialPort(const Posix_QextSerialPort& s);
    Posix_QextSerialPort(const QString & name);
    Posix_QextSerialPort(const PortSettings& settings);
    Posix_QextSerialPort(const QString & name, const PortSettings& settings);
    Posix_QextSerialPort& operator=(const Posix_QextSerialPort& s);
    virtual ~Posix_QextSerialPort();

    virtual void setBaudRate(unsigned int);
    virtual void setDataBits(DataBitsType);
    virtual void setParity(ParityType);
    virtual void setStopBits(StopBitsType);
    virtual void setFlowControl(FlowType);
    virtual void setTimeout(long);

    virtual bool open(OpenMode mode);
    virtual void close();
    virtual void flush();
    virtual void clearReceiveBuffer();

    virtual qint64 size() const;
    virtual qint64 bytesAvailable() const;

    virtual void ungetChar(char c);

    virtual void translateError(ulong error);

    virtual void setBreak(bool set=true);
    virtual void setDtr(bool set=true);
    virtual void setRts(bool set=true);
    virtual ulong lineStatus();
};

/*!
 * This thread buffers incoming serial data.
 */
class Posix_QextSerialReaderThread : public QThread
{
    Q_OBJECT

public:
    int handle;

    QByteArray data;
    bool shutdown;

    int waitCount;
    int count;

    QWaitCondition waitingForData;
    QMutex mutex;

    Posix_QextSerialReaderThread(Posix_QextSerialPort* parent);
    ~Posix_QextSerialReaderThread();
    void run();

signals:
    void readyRead();

protected:
    Posix_QextSerialPort* parent;
    char* rxbuffer[2];
    int buffer;
};

#endif
