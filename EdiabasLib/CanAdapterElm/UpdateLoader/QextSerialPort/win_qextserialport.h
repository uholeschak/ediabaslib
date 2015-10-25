#ifndef _WIN_QEXTSERIALPORT_H_
#define _WIN_QEXTSERIALPORT_H_

#include "qextserialbase.h"
#include <windows.h>
#include <QThread>
#include <QWaitCondition>

/*if all warning messages are turned off, flag portability warnings to be turned off as well*/
#ifdef _TTY_NOWARN_
#define _TTY_NOWARN_PORT_
#endif

class Win_QextSerialReaderThread;

/*!
This class encapsulates the Windows portion of QextSerialPort.

\author Stefan Sander
\author Michal Policht

A cross-platform serial port class.
The user will be notified of errors and possible portability conflicts at run-time by default - this
behavior can be turned off by defining _TTY_NOWARN_ (to turn off all warnings) or _TTY_NOWARN_PORT_
(to turn off portability warnings) in the project.  Note that defining _TTY_NOWARN_ also defines
_TTY_NOWARN_PORT_.

\note
On Windows NT/2000/XP this class uses Win32 serial port functions by default.  The user may
select POSIX behavior under NT, 2000, or XP ONLY by defining _TTY_POSIX_ in the project. I can
make no guarantees as to the quality of POSIX support under NT/2000 however.

*/
class Win_QextSerialPort: public QextSerialBase
{
    friend class Win_QextSerialReaderThread;

	private:
		/*!
		 * This method is a part of constructor.
		 */
		void init();

	protected:
        HANDLE Win_Handle;
        COMMTIMEOUTS Win_CommTimeouts;
        Win_QextSerialReaderThread* readerThread;
		bool UpdateComConfig(void);

	public:
        virtual qint64 writeData(const char *data, qint64 maxSize);
        virtual qint64 readData(char *data, qint64 maxSize);
        Win_QextSerialPort();
	    Win_QextSerialPort(Win_QextSerialPort const& s);
        Win_QextSerialPort(const QString & name);
        Win_QextSerialPort(const PortSettings& settings);
        Win_QextSerialPort(const QString & name, const PortSettings& settings);
	    Win_QextSerialPort& operator=(const Win_QextSerialPort& s);
	    virtual ~Win_QextSerialPort();
	    virtual bool open(OpenMode mode);
	    virtual void close();
	    virtual void flush();
	    virtual qint64 size() const;
	    virtual void ungetChar(char c);
	    virtual void setFlowControl(FlowType);
	    virtual void setParity(ParityType);
	    virtual void setDataBits(DataBitsType);
	    virtual void setStopBits(StopBitsType);
        virtual void setBaudRate(unsigned int);
        virtual void setBreak(bool set=true);
        virtual void setDtr(bool set=true);
	    virtual void setRts(bool set=true);
	    virtual ulong lineStatus(void);
	    virtual qint64 bytesAvailable() const;
	    virtual void setTimeout(long);
        virtual void clearReceiveBuffer();
        virtual bool waitForReadyRead(int msecs);
};

/*!
 * This thread buffers incoming serial data.
 */
class Win_QextSerialReaderThread : public QThread
{
    Q_OBJECT

public:
    HANDLE handle;
    OVERLAPPED overlapRead;

    QByteArray data;
    bool shutdown;

    int waitCount;
    int count;

    QWaitCondition waitingForData;
    QMutex mutex;

    Win_QextSerialReaderThread(Win_QextSerialPort* parent);
    ~Win_QextSerialReaderThread();
    void run();

signals:
    void readyRead();

protected:
    Win_QextSerialPort* parent;
    char* rxbuffer[2];
    int buffer;
};

#endif
