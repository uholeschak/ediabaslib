#include <QtGlobal> //Q_ASSERT()
#include <QReadWriteLock>
#include "win_qextserialport.h"


/*!
\fn Win_QextSerialPort::Win_QextSerialPort()
Default constructor.  Note that the name of the device used by a Win_QextSerialPort constructed
with this constructor will be determined by #defined constants, or lack thereof - the default
behavior is the same as _TTY_LINUX_.  Possible naming conventions and their associated constants
are:

\verbatim

Constant         Used By         Naming Convention
----------       -------------   ------------------------
_TTY_WIN_        Windows         COM1, COM2
_TTY_IRIX_       SGI/IRIX        /dev/ttyf1, /dev/ttyf2
_TTY_HPUX_       HP-UX           /dev/tty1p0, /dev/tty2p0
_TTY_SUN_        SunOS/Solaris   /dev/ttya, /dev/ttyb
_TTY_DIGITAL_    Digital UNIX    /dev/tty01, /dev/tty02
_TTY_FREEBSD_    FreeBSD         /dev/ttyd0, /dev/ttyd1
_TTY_LINUX_      Linux           /dev/ttyS0, /dev/ttyS1
<none>           Linux           /dev/ttyS0, /dev/ttyS1
\endverbatim

This constructor associates the object with the first port on the system, e.g. COM1 for Windows
platforms.  See the other constructor if you need a port other than the first.
*/
Win_QextSerialPort::Win_QextSerialPort():
	QextSerialBase()
{
    Win_Handle=INVALID_HANDLE_VALUE;
    init();
}

/*!
\fn Win_QextSerialPort::Win_QextSerialPort(const Win_QextSerialPort&)
Copy constructor.
*/
Win_QextSerialPort::Win_QextSerialPort(const Win_QextSerialPort& s):
	QextSerialBase(s.port)
{
    Win_Handle=INVALID_HANDLE_VALUE;
    setOpenMode(s.openMode());
    lastErr=s.lastErr;
    port = s.port;
    Settings.FlowControl=s.Settings.FlowControl;
    Settings.Parity=s.Settings.Parity;
    Settings.DataBits=s.Settings.DataBits;
    Settings.StopBits=s.Settings.StopBits;
    Settings.BaudRate=s.Settings.BaudRate;
    Win_Handle=s.Win_Handle;
    memcpy(&Win_CommTimeouts, &s.Win_CommTimeouts, sizeof(COMMTIMEOUTS));
}

/*!
\fn Win_QextSerialPort::Win_QextSerialPort(const QString & name)
Constructs a serial port attached to the port specified by devName.
devName is the name of the device, which is system-specific,
e.g. "COM2" or "/dev/ttyS0".
*/
Win_QextSerialPort::Win_QextSerialPort(const QString & name):
	QextSerialBase(name)
{
    Win_Handle=INVALID_HANDLE_VALUE;
    init();
}

/*!
\fn Win_QextSerialPort::Win_QextSerialPort(const PortSettings& settings)
Constructs a port with default name and specified settings.
*/
Win_QextSerialPort::Win_QextSerialPort(const PortSettings& settings):
	QextSerialBase()
{
    Win_Handle=INVALID_HANDLE_VALUE;
  	Settings.BaudRate=settings.BaudRate;
  	Settings.DataBits=settings.DataBits;
  	Settings.StopBits=settings.StopBits;
  	Settings.Parity=settings.Parity;
  	Settings.FlowControl=settings.FlowControl;
  	Settings.Timeout_Millisec=settings.Timeout_Millisec;
    init();
}

/*!
 \fn Win_QextSerialPort::Win_QextSerialPort(const QString & name, const PortSettings& settings)
 Constructs a port with specified name and settings.
 */
Win_QextSerialPort::Win_QextSerialPort(const QString & name, const PortSettings& settings) : QextSerialBase(name)
{
	Win_Handle=INVALID_HANDLE_VALUE;
	setPortName(name);
	Settings.BaudRate=settings.BaudRate;
	Settings.DataBits=settings.DataBits;
	Settings.StopBits=settings.StopBits;
	Settings.Parity=settings.Parity;
	Settings.FlowControl=settings.FlowControl;
	Settings.Timeout_Millisec=settings.Timeout_Millisec;
	init();
}

void Win_QextSerialPort::init()
{
    readerThread = new Win_QextSerialReaderThread(this);
    connect(readerThread, SIGNAL(readyRead()), SIGNAL(readyRead()));
}

/*!
\fn Win_QextSerialPort::~Win_QextSerialPort()
Standard destructor.
*/
Win_QextSerialPort::~Win_QextSerialPort()
{
    if (isOpen())
    {
        close();
    }

    delete readerThread;
}

/*!
\fn Win_QextSerialPort& Win_QextSerialPort::operator=(const Win_QextSerialPort& s)
overrides the = operator
*/
Win_QextSerialPort& Win_QextSerialPort::operator=(const Win_QextSerialPort& s)
{
    setOpenMode(s.openMode());
    lastErr=s.lastErr;
    port = s.port;
    Settings.FlowControl=s.Settings.FlowControl;
    Settings.Parity=s.Settings.Parity;
    Settings.DataBits=s.Settings.DataBits;
    Settings.StopBits=s.Settings.StopBits;
    Settings.BaudRate=s.Settings.BaudRate;
    Win_Handle=s.Win_Handle;
    memcpy(&Win_CommTimeouts, &s.Win_CommTimeouts, sizeof(COMMTIMEOUTS));
    return *this;
}


/*!
\fn bool Win_QextSerialPort::open(OpenMode mode)
Opens a serial port.  Note that this function does not specify which device to open.  If you need
to open a device by name, see Win_QextSerialPort::open(const char*).  This function has no effect
if the port associated with the class is already open.  The port is also configured to the current
settings, as stored in the Settings structure.
*/
bool Win_QextSerialPort::open(OpenMode mode)
{
	LOCK_MUTEX();
    if (mode == QIODevice::NotOpen)
    {
        UNLOCK_MUTEX();
        return isOpen();
	}

    if (isOpen())
    {
        UNLOCK_MUTEX();
        return false;
    }

    // open the port
    QString portName;
    QueueReceiveSignals = 10;
    portName = "\\\\.\\";       // required for COM ports beyond COM9
    portName.append(port);
    Win_Handle = CreateFileA(portName.toAscii(),
                             GENERIC_READ|GENERIC_WRITE,
                             0,
                             0,
                             OPEN_EXISTING,
                             FILE_FLAG_OVERLAPPED,
                             0);

    if(Win_Handle == INVALID_HANDLE_VALUE)
    {
        UNLOCK_MUTEX();
        return false;
    }

    // configure port settings
    UpdateComConfig(); // update all win_CommConfig settings

    if (Settings.Timeout_Millisec == -1)
    {
        Win_CommTimeouts.ReadIntervalTimeout = MAXDWORD;
        Win_CommTimeouts.ReadTotalTimeoutConstant = 0;
    }
    else
    {
        Win_CommTimeouts.ReadIntervalTimeout = Settings.Timeout_Millisec;
        Win_CommTimeouts.ReadTotalTimeoutConstant = Settings.Timeout_Millisec;
    }
    Win_CommTimeouts.ReadTotalTimeoutMultiplier = 0;
    Win_CommTimeouts.WriteTotalTimeoutMultiplier = Settings.Timeout_Millisec;
    Win_CommTimeouts.WriteTotalTimeoutConstant = 0;
    Win_CommTimeouts.WriteTotalTimeoutMultiplier = 0;
    Win_CommTimeouts.WriteTotalTimeoutConstant = 0;
    SetCommTimeouts(Win_Handle, &Win_CommTimeouts);

    QIODevice::open(mode);
    readerThread->handle = Win_Handle;
    readerThread->shutdown = false;
    readerThread->start();

    UNLOCK_MUTEX();
	return isOpen();
}

/*!
\fn void Win_QextSerialPort::close()
Closes a serial port.  This function has no effect if the serial port associated with the class
is not currently open.
*/
void Win_QextSerialPort::close()
{
    LOCK_MUTEX();

    if (isOpen())
    {
		flush();

        readerThread->shutdown = true;
        if(readerThread->isRunning())
        {
            if (QThread::currentThread() != readerThread)
            {
                readerThread->wait();
            }
        }

        if (CloseHandle(Win_Handle))
        {
            Win_Handle = INVALID_HANDLE_VALUE;
        }

        QIODevice::close();
    }

    UNLOCK_MUTEX();
}

/*!
\fn void Win_QextSerialPort::flush()
Flushes all pending I/O to the serial port.  This function has no effect if the serial port
associated with the class is not currently open.
*/
void Win_QextSerialPort::flush()
{
    LOCK_MUTEX();
    if (isOpen())
    {
        FlushFileBuffers(Win_Handle);
    }
    UNLOCK_MUTEX();
}

/*!
\fn qint64 Win_QextSerialPort::size() const
This function will return the number of bytes waiting in the receive queue of the serial port.
It is included primarily to provide a complete QIODevice interface, and will not record errors
in the lastErr member (because it is const).  This function is also not thread-safe - in
multithreading situations, use Win_QextSerialPort::bytesAvailable() instead.
*/
qint64 Win_QextSerialPort::size() const
{
    int availBytes;
    COMSTAT Win_ComStat;
    DWORD Win_ErrorMask=0;
  	Q_ASSERT(Win_Handle!=INVALID_HANDLE_VALUE);
    ClearCommError(Win_Handle, &Win_ErrorMask, &Win_ComStat);
    availBytes = Win_ComStat.cbInQue;
    return (qint64)availBytes;
}

/*!
\fn qint64 Win_QextSerialPort::bytesAvailable()
Returns the number of bytes waiting in the port's receive queue.  This function will return 0 if
the port is not currently open, or -1 on error.
*/
qint64 Win_QextSerialPort::bytesAvailable() const
{
    qint64 result = 0;

    LOCK_MUTEX();
    if (isOpen())
    {
        result = readerThread->count;
    }
    UNLOCK_MUTEX();
    return result;
}

/**
 * Purges all buffered receive data from memory without requiring explicit read of said data.
 */
void Win_QextSerialPort::clearReceiveBuffer()
{
    readerThread->mutex.lock();
    readerThread->data.clear();
    readerThread->count = 0;
    readerThread->mutex.unlock();
}

/*!
\fn qint64 Win_QextSerialPort::readData(char *data, qint64 maxSize)
Reads a block of data from the serial port.  This function will read at most maxSize bytes from
the serial port and place them in the buffer pointed to by data.  Return value is the number of
bytes actually read, or -1 on error.

\warning before calling this function ensure that serial port associated with this class
is currently open (use isOpen() function to check if port is open).
*/
qint64 Win_QextSerialPort::readData(char *data, qint64 maxSize)
{
    readerThread->mutex.lock();
    if(readerThread->count >= maxSize)
    {
        // all the data we need already exists in readerThread->data.
        memcpy(data, readerThread->data.constData(), maxSize);
        readerThread->data.remove(0, maxSize);
        readerThread->count = readerThread->data.count();
        readerThread->mutex.unlock();
        return maxSize;
    }

    readerThread->waitCount = maxSize;
    readerThread->waitingForData.wait(&readerThread->mutex);
    if(readerThread->count < maxSize)
    {
        maxSize = readerThread->count;
    }

    if(readerThread->count > 0)
    {
        memcpy(data, readerThread->data.constData(), maxSize);
        readerThread->data.remove(0, maxSize);
        readerThread->count = readerThread->data.count();
    }
    readerThread->mutex.unlock();
    return maxSize;
}

Win_QextSerialReaderThread::Win_QextSerialReaderThread(Win_QextSerialPort* newParent)
{
    parent = newParent;
    shutdown = false;
    handle = NULL;
    overlapRead.Internal = 0;
    overlapRead.InternalHigh = 0;
    overlapRead.Offset = 0;
    overlapRead.OffsetHigh = 0;
    overlapRead.hEvent = CreateEvent(NULL, true, false, NULL);
    count = 0;
    waitCount = INT_MAX;

    data.reserve(128*1024);

    buffer = 0;
    rxbuffer[0] = new char[4096];
    rxbuffer[1] = new char[4096];
}

Win_QextSerialReaderThread::~Win_QextSerialReaderThread()
{
    CloseHandle(overlapRead.hEvent);
    delete rxbuffer[0];
    delete rxbuffer[1];
}

void Win_QextSerialReaderThread::run()
{
    COMSTAT status;
    DWORD readSize = 0, lastReadSize = 0;
    DWORD lastError = 0;
    BOOL readComplete;
    DWORD requestSize, lastRequestSize = 0;

    while(shutdown == false)
    {
        if(waitCount != INT_MAX)
        {
            requestSize = waitCount - count;
        }
        else
        {
            requestSize = 1;
            if(ClearCommError(handle, &lastError, &status))
            {
                if(status.cbInQue)
                {
                    requestSize = status.cbInQue;
                }
            }
        }

        if(requestSize > 4096)
        {
            requestSize = 4096;
        }

        readComplete = ReadFile(handle, (void*)rxbuffer[buffer], requestSize, &readSize, &overlapRead);
        if(lastReadSize)
        {
            if(parent->recordingFile.isOpen());
            {
                parent->recordingFile.write(rxbuffer[buffer ^ 1], lastReadSize);
            }

            if(count < 1024 * 1024) // stop collecting data if we've already got 1MB of data waiting.
            {
                mutex.lock();
                data.append(rxbuffer[buffer ^ 1], lastReadSize);
                count = data.count();
                mutex.unlock();
                if(parent->QueueReceiveSignals > 0)
                {
                    parent->QueueReceiveSignals--;
                    emit readyRead();
                }
            }
        }
        else
        {
            if(parent->recordingFile.isOpen());
            {
                parent->recordingFile.flush();
            }
        }

        if(count >= waitCount || lastReadSize != lastRequestSize)
        {
            waitCount = INT_MAX;
            waitingForData.wakeAll();
        }

        if (!readComplete)
        {
            while(shutdown == false)
            {
                lastError = WaitForSingleObject(overlapRead.hEvent, parent->Settings.Timeout_Millisec);
                if(lastError == WAIT_OBJECT_0)
                {
                    if (!GetOverlappedResult(handle, &overlapRead, &readSize, true))
                    {
                        // Error in communications
                        readSize = 0;
#ifdef DEBUG_RX
                        lastError = GetLastError();
#endif
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                else if(lastError == WAIT_TIMEOUT)
                {
                    // Operation isn't complete yet.
                }
                else
                {
                    // Error in the WaitForSingleObject; abort.
                    readSize = 0;
#ifdef DEBUG_RX
                    lastError = GetLastError();
#endif
                    break;
                }
            }

        }

        lastReadSize = readSize;
        lastRequestSize = requestSize;
        buffer ^= 1;
    }
}


/*!
\fn qint64 Win_QextSerialPort::writeData(const char *data, qint64 maxSize)
Writes a block of data to the serial port.  This function will write len bytes
from the buffer pointed to by data to the serial port.  Return value is the number
of bytes actually written, or -1 on error.

\warning before calling this function ensure that serial port associated with this class
is currently open (use isOpen() function to check if port is open).
*/
qint64 Win_QextSerialPort::writeData(const char *data, qint64 maxSize)
{
    DWORD bytesWritten = 0;
    LOCK_MUTEX();

    OVERLAPPED overlapWrite = {0};
    overlapWrite.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

    // Issue write.
    if(!WriteFile(Win_Handle, (void*)data, (DWORD)maxSize, &bytesWritten, &overlapWrite))
    {
        if (GetLastError() == ERROR_IO_PENDING)
        {
            // Write is pending, wait for it to complete.
            if(WaitForSingleObject(overlapWrite.hEvent, INFINITE) == WAIT_OBJECT_0)
            {
                GetOverlappedResult(Win_Handle, &overlapWrite, &bytesWritten, FALSE);
            }
        }
    }

    CloseHandle(overlapWrite.hEvent);

    UNLOCK_MUTEX();

    return (qint64)bytesWritten;
}

/*!
\fn void Win_QextSerialPort::ungetChar(char c)
This function is included to implement the full QIODevice interface, and currently has no
purpose within this class.  This function is meaningless on an unbuffered device and currently
only prints a warning message to that effect.
*/
void Win_QextSerialPort::ungetChar(char c) {

    /*meaningless on unbuffered sequential device - return error and print a warning*/
    TTY_WARNING("Win_QextSerialPort: ungetChar() called on an unbuffered sequential device - operation is meaningless");
}

/*!
\fn void Win_QextSerialPort::setFlowControl(FlowType flow)
Sets the flow control used by the port.  Possible values of flow are:
\verbatim
    FLOW_OFF            No flow control
    FLOW_HARDWARE       Hardware (RTS/CTS) flow control
    FLOW_XONXOFF        Software (XON/XOFF) flow control
\endverbatim
*/
void Win_QextSerialPort::setFlowControl(FlowType flow)
{
    LOCK_MUTEX();
    Settings.FlowControl=flow;
    UNLOCK_MUTEX();
}

/*!
\fn void Win_QextSerialPort::setParity(ParityType parity)
Sets the parity associated with the serial port.  The possible values of parity are:
\verbatim
    PAR_SPACE       Space Parity
    PAR_MARK        Mark Parity
    PAR_NONE        No Parity
    PAR_EVEN        Even Parity
    PAR_ODD         Odd Parity
\endverbatim
*/
void Win_QextSerialPort::setParity(ParityType parity)
{
	LOCK_MUTEX();
	Settings.Parity=parity;
	UNLOCK_MUTEX();
}

/*!
\fn void Win_QextSerialPort::setDataBits(DataBitsType dataBits)
Sets the number of data bits used by the serial port.  Possible values of dataBits are:
\verbatim
    DATA_5      5 data bits
    DATA_6      6 data bits
    DATA_7      7 data bits
    DATA_8      8 data bits
\endverbatim

\note
This function is subject to the following restrictions:
\par
    5 data bits cannot be used with 2 stop bits.
\par
    1.5 stop bits can only be used with 5 data bits.
\par
    8 data bits cannot be used with space parity on POSIX systems.

*/
void Win_QextSerialPort::setDataBits(DataBitsType dataBits)
{
	LOCK_MUTEX();
    if (Settings.DataBits!=dataBits)
    {
        if ((Settings.StopBits == STOP_2    && dataBits == DATA_5) ||
            (Settings.StopBits == STOP_1_5  && dataBits != DATA_5))
        {
        }
        else
        {
			Settings.DataBits=dataBits;
		}
	}
	UNLOCK_MUTEX();
}

/*!
\fn void Win_QextSerialPort::setStopBits(StopBitsType stopBits)
Sets the number of stop bits used by the serial port.  Possible values of stopBits are:
\verbatim
    STOP_1      1 stop bit
    STOP_1_5    1.5 stop bits
    STOP_2      2 stop bits
\endverbatim

\note
This function is subject to the following restrictions:
\par
    2 stop bits cannot be used with 5 data bits.
\par
    1.5 stop bits cannot be used with 6 or more data bits.
\par
    POSIX does not support 1.5 stop bits.
*/
void Win_QextSerialPort::setStopBits(StopBitsType stopBits) {
	LOCK_MUTEX();
	if (Settings.StopBits!=stopBits) {
		if ((Settings.DataBits==DATA_5 && stopBits==STOP_2) || (stopBits==STOP_1_5
				&& Settings.DataBits!=DATA_5)) { //BUG this will not work in all cases since it assumes that DataBits was configured before in the code
		} else {
			Settings.StopBits=stopBits;
		}
	}
	UNLOCK_MUTEX();
}

/*!
\fn void Win_QextSerialPort::setBaudRate(unsigned int baudRate)
Sets the baud rate of the serial port.  Note that not all rates are applicable on
all platforms.  The following table shows translations of the various baud rate
constants on Windows(including NT/2000) and POSIX platforms.  Speeds marked with an *
are speeds that are usable on both Windows and POSIX.
\verbatim

  RATE          Windows Speed   POSIX Speed
  -----------   -------------   -----------
   BAUD50                 110          50
   BAUD75                 110          75
  *BAUD110                110         110
   BAUD134                110         134.5
   BAUD150                110         150
   BAUD200                110         200
  *BAUD300                300         300
  *BAUD600                600         600
  *BAUD1200              1200        1200
   BAUD1800              1200        1800
  *BAUD2400              2400        2400
  *BAUD4800              4800        4800
  *BAUD9600              9600        9600
   BAUD14400            14400        9600
  *BAUD19200            19200       19200
  *BAUD38400            38400       38400
   BAUD56000            56000       38400
  *BAUD57600            57600       57600
   BAUD76800            57600       76800
  *BAUD115200          115200      115200
   BAUD128000          128000      115200
   BAUD256000          256000      115200
\endverbatim
*/
void Win_QextSerialPort::setBaudRate(unsigned int baudRate)
{
	LOCK_MUTEX();
    if (Settings.BaudRate != baudRate)
    {
        Settings.BaudRate = baudRate;
	}
	UNLOCK_MUTEX();
}

/*!
\fn void Win_QextSerialPort::setDtr(bool set)
Sets DTR line to the requested state (high by default).  This function will have no effect if
the port associated with the class is not currently open.
*/
void Win_QextSerialPort::setDtr(bool set) {
    LOCK_MUTEX();
    if (isOpen()) {
        if (set) {
            EscapeCommFunction(Win_Handle, SETDTR);
        }
        else {
            EscapeCommFunction(Win_Handle, CLRDTR);
        }
    }
    UNLOCK_MUTEX();
}

/*!
\fn void Win_QextSerialPort::setBreak(bool set)
Sets TXD line to the requested state (break/constant logic low if parameter set is true).
This function will have no effect if the port associated with the class is not currently open.
*/
void Win_QextSerialPort::setBreak(bool set)
{
    LOCK_MUTEX();
    if (isOpen())
    {
        if (set)
        {
            EscapeCommFunction(Win_Handle, SETBREAK);
        }
        else
        {
            EscapeCommFunction(Win_Handle, CLRBREAK);
        }
    }
    UNLOCK_MUTEX();
}

/*!
\fn void Win_QextSerialPort::setRts(bool set)
Sets RTS line to the requested state (high by default).  This function will have no effect if
the port associated with the class is not currently open.
*/
void Win_QextSerialPort::setRts(bool set) {
    LOCK_MUTEX();
    if (isOpen()) {
        if (set) {
            EscapeCommFunction(Win_Handle, SETRTS);
        }
        else {
            EscapeCommFunction(Win_Handle, CLRRTS);
        }
    }
    UNLOCK_MUTEX();
}

/*!
\fn ulong Win_QextSerialPort::lineStatus(void)
returns the line status as stored by the port function.  This function will retrieve the states
of the following lines: DCD, CTS, DSR, and RI.  On POSIX systems, the following additional lines
can be monitored: DTR, RTS, Secondary TXD, and Secondary RXD.  The value returned is an unsigned
long with specific bits indicating which lines are high.  The following constants should be used
to examine the states of individual lines:

\verbatim
Mask        Line
------      ----
LS_CTS      CTS
LS_DSR      DSR
LS_DCD      DCD
LS_RI       RI
\endverbatim

This function will return 0 if the port associated with the class is not currently open.
*/
ulong Win_QextSerialPort::lineStatus(void) {
    unsigned long Status=0, Temp=0;
    LOCK_MUTEX();
    if (isOpen()) {
        GetCommModemStatus(Win_Handle, &Temp);
        if (Temp&MS_CTS_ON) {
            Status|=LS_CTS;
        }
        if (Temp&MS_DSR_ON) {
            Status|=LS_DSR;
        }
        if (Temp&MS_RING_ON) {
            Status|=LS_RI;
        }
        if (Temp&MS_RLSD_ON) {
            Status|=LS_DCD;
        }
    }
    UNLOCK_MUTEX();
    return Status;
}

/**
 * Not implemented.
 */
bool Win_QextSerialPort::waitForReadyRead(int msecs)
{
	return false;
}

/*!
\fn void Win_QextSerialPort::setTimeout(ulong millisec);
Sets the read and write timeouts for the port to millisec milliseconds.
Setting 0 indicates that timeouts are not used for read nor write operations;
however read() and write() functions will still block. Set -1 to provide
non-blocking behaviour (read() and write() will return immediately).
*/
void Win_QextSerialPort::setTimeout(long millisec) {
    LOCK_MUTEX();
    Settings.Timeout_Millisec = millisec;
    // BUG it might be necessary to change the timeout while the port is open
    UNLOCK_MUTEX();
}

// This function reads the current Win_CommConfig settings and updates this
// structure with saved settings
bool Win_QextSerialPort::UpdateComConfig(void)
{
	// Question: Is it possible to change the win_commConfig settings while the port is open? - yes, but not all settings!!! Baud rate can only be changed on closed ports.
	// BUG replace global win_commConfig
	COMMCONFIG Win_CommConfig;
	COMMCONFIG readCommConfig;
	Q_ASSERT(Win_Handle!=INVALID_HANDLE_VALUE);
	LOCK_MUTEX();

	unsigned long confSize = sizeof(COMMCONFIG);
	Win_CommConfig.dwSize = confSize; // TODO: what is this?


	// read current settings
    GetCommConfig(Win_Handle, &Win_CommConfig, &confSize);
    GetCommState(Win_Handle, &(Win_CommConfig.dcb));

	/*set up default parameters*/
    Win_CommConfig.dcb.fBinary = TRUE;
    Win_CommConfig.dcb.fAbortOnError = FALSE;
    Win_CommConfig.dcb.fNull = FALSE;

    Win_CommConfig.dcb.Parity = NOPARITY;
    Win_CommConfig.dcb.StopBits = ONESTOPBIT;
    Win_CommConfig.dcb.fParity = TRUE;

    // data bit settings
    switch (Settings.DataBits)
    {
	case DATA_5:/*5 data bits*/
		if (Settings.StopBits==STOP_2) {  //BUG think about warnings
			TTY_WARNING("Win_QextSerialPort: 5 Data bits cannot be used with 2 stop bits.");
		} else {
			Win_CommConfig.dcb.ByteSize=5;
		}
		break;
	case DATA_6:/*6 data bits*/
		if (Settings.StopBits==STOP_1_5) {
			TTY_WARNING("Win_QextSerialPort: 6 Data bits cannot be used with 1.5 stop bits.");
		} else {
			Win_CommConfig.dcb.ByteSize=6;
		}
		break;
	case DATA_7:/*7 data bits*/
		if (Settings.StopBits==STOP_1_5) {
			TTY_WARNING("Win_QextSerialPort: 7 Data bits cannot be used with 1.5 stop bits.");
		} else {
			Win_CommConfig.dcb.ByteSize=7;
		}
		break;
	case DATA_8:/*8 data bits*/
		if (Settings.StopBits==STOP_1_5) {
			TTY_WARNING("Win_QextSerialPort: 8 Data bits cannot be used with 1.5 stop bits.");
		} else {
			Win_CommConfig.dcb.ByteSize=8;
		}
		break;
	default:
		Q_ASSERT(0); // This should never happen BUG replace by a error message
	}


  // parity settings
  switch (Settings.Parity) {
	case PAR_SPACE: /*space parity*/
		if (Settings.DataBits==DATA_8) { // BUG this assumes that data was set first
			TTY_PORTABILITY_WARNING("Win_QextSerialPort Portability Warning: Space parity with 8 data bits is not supported by POSIX systems.");
		}
		Win_CommConfig.dcb.fParity=TRUE; // enable parity checking
		Win_CommConfig.dcb.Parity=SPACEPARITY;
		break;
	case PAR_MARK: /* mark parity - WINDOWS ONLY */
		TTY_PORTABILITY_WARNING("Win_QextSerialPort Portability Warning:  Mark parity is not supported by POSIX systems");
		Win_CommConfig.dcb.fParity=TRUE; // enable parity checking
		Win_CommConfig.dcb.Parity=MARKPARITY;
		break;
	case PAR_NONE: /* no parity */
		Win_CommConfig.dcb.fParity=FALSE; // disable parity checking
		Win_CommConfig.dcb.Parity=NOPARITY;
		break;
	case PAR_EVEN:/* even parity */
		Win_CommConfig.dcb.fParity=TRUE; // enable parity checking
		Win_CommConfig.dcb.Parity=EVENPARITY;
		break;
	case PAR_ODD:/* odd parity */
		Win_CommConfig.dcb.fParity=TRUE; // enable parity checking
		Win_CommConfig.dcb.Parity=ODDPARITY;
		break;
	default:
		Q_ASSERT(0); // This should never happen BUG replace by a error message
	}

  // baud settings
  switch (Settings.BaudRate) {
	case BAUD50:/*50 baud*/
		TTY_WARNING("Win_QextSerialPort: Windows does not support 50 baud operation.  Switching to 110 baud.");
		Win_CommConfig.dcb.BaudRate=CBR_110;
		break;
	case BAUD75:/*75 baud*/
		TTY_WARNING("Win_QextSerialPort: Windows does not support 75 baud operation.  Switching to 110 baud.");
		Win_CommConfig.dcb.BaudRate=CBR_110;
		break;
	case BAUD110:/*110 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_110;
		break;
	case BAUD134:		/*134.5 baud*/
		TTY_WARNING("Win_QextSerialPort: Windows does not support 134.5 baud operation.  Switching to 110 baud.");
		Win_CommConfig.dcb.BaudRate=CBR_110;
		break;
	case BAUD150:/*150 baud*/
		TTY_WARNING("Win_QextSerialPort: Windows does not support 150 baud operation.  Switching to 110 baud.");
		Win_CommConfig.dcb.BaudRate=CBR_110;
		break;
	case BAUD200:/*200 baud*/
		TTY_WARNING("Win_QextSerialPort: Windows does not support 200 baud operation.  Switching to 110 baud.");
		Win_CommConfig.dcb.BaudRate=CBR_110;
		break;
	case BAUD300:/*300 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_300;
		break;
	case BAUD600:/*600 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_600;
		break;
	case BAUD1200:/*1200 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_1200;
		break;
	case BAUD1800:/*1800 baud*/
		TTY_WARNING("Win_QextSerialPort: Windows does not support 1800 baud operation.  Switching to 1200 baud.");
		Win_CommConfig.dcb.BaudRate=CBR_1200;
		break;
	case BAUD2400:/*2400 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_2400;
		break;
	case BAUD4800:/*4800 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_4800;
		break;
	case BAUD9600:/*9600 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_9600;
		break;
	case BAUD14400:/*14400 baud*/
		TTY_PORTABILITY_WARNING("Win_QextSerialPort Portability Warning: POSIX does not support 14400 baud operation.");
		Win_CommConfig.dcb.BaudRate=CBR_14400;
		break;
	case BAUD19200:/*19200 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_19200;
		break;
	case BAUD38400:/*38400 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_38400;
		break;
	case BAUD56000:/*56000 baud*/
		TTY_PORTABILITY_WARNING("Win_QextSerialPort Portability Warning: POSIX does not support 56000 baud operation.");
		Win_CommConfig.dcb.BaudRate=CBR_56000;
		break;
	case BAUD57600:/*57600 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_57600;
		break;
	case BAUD76800:/*76800 baud*/
		TTY_WARNING("Win_QextSerialPort: Windows does not support 76800 baud operation.  Switching to 57600 baud.");
		Win_CommConfig.dcb.BaudRate=CBR_57600;
		break;
	case BAUD115200:/*115200 baud*/
		Win_CommConfig.dcb.BaudRate=CBR_115200;
		break;
        case BAUD128000:
		Win_CommConfig.dcb.BaudRate=CBR_128000;
		break;

        case BAUD230400:
            Win_CommConfig.dcb.BaudRate=230400;
            break;

        case BAUD250000:
            Win_CommConfig.dcb.BaudRate=250000;
            break;

        case BAUD460800:
            Win_CommConfig.dcb.BaudRate = 460800;
            break;

        case BAUD500000:
            Win_CommConfig.dcb.BaudRate = 500000;
            break;

        case BAUD614400:
            Win_CommConfig.dcb.BaudRate = 614400;
            break;

        case BAUD750000:
            Win_CommConfig.dcb.BaudRate = 750000;
            break;

        case BAUD921600:
            Win_CommConfig.dcb.BaudRate = 921600;
            break;

        case BAUD1000000:
            Win_CommConfig.dcb.BaudRate = 1000000;
            break;

        case BAUD1228800:
            Win_CommConfig.dcb.BaudRate = 1228800;
            break;

        case BAUD2457600:
            Win_CommConfig.dcb.BaudRate = 2457600;
            break;

        case BAUD3000000:
            Win_CommConfig.dcb.BaudRate = 3000000;
            break;

        case BAUD6000000:
            Win_CommConfig.dcb.BaudRate = 6000000;
            break;

        default:
            Win_CommConfig.dcb.BaudRate = (unsigned int)Settings.BaudRate;
            break;
	}

  // STOP bits
  switch (Settings.StopBits) {
	case STOP_1:/*one stop bit*/
		Win_CommConfig.dcb.StopBits=ONESTOPBIT;
		break;
	case STOP_1_5:/*1.5 stop bits*/
		TTY_PORTABILITY_WARNING("Win_QextSerialPort Portability Warning: 1.5 stop bit operation is not supported by POSIX.");
		if (Settings.DataBits!=DATA_5) {
			TTY_WARNING("Win_QextSerialPort: 1.5 stop bits can only be used with 5 data bits");
		} else {
			Win_CommConfig.dcb.StopBits=ONE5STOPBITS;
		}
		break;
	case STOP_2:/*two stop bits*/
		if (Settings.DataBits==DATA_5) {// BUG this assumes, that DATA was set first
			TTY_WARNING("Win_QextSerialPort: 2 stop bits cannot be used with 5 data bits");
		} else {
			Win_CommConfig.dcb.StopBits=TWOSTOPBITS;
		}
		break;
	default:
		Q_ASSERT(0); // This should never happen BUG replace by a error message
	}


  switch (Settings.FlowControl) {
	case FLOW_OFF:/*no flow control*/
        Win_CommConfig.dcb.fOutxCtsFlow = FALSE;
        Win_CommConfig.dcb.fOutxDsrFlow = FALSE;
        Win_CommConfig.dcb.fRtsControl=RTS_CONTROL_DISABLE;
		Win_CommConfig.dcb.fInX=FALSE;
		Win_CommConfig.dcb.fOutX=FALSE;
		break;
	case FLOW_XONXOFF:/*software (XON/XOFF) flow control*/
        Win_CommConfig.dcb.fOutxCtsFlow = FALSE;
        Win_CommConfig.dcb.fOutxDsrFlow = FALSE;
        Win_CommConfig.dcb.fRtsControl=RTS_CONTROL_DISABLE;
		Win_CommConfig.dcb.fInX=TRUE;
		Win_CommConfig.dcb.fOutX=TRUE;
		break;
	case FLOW_HARDWARE:
		Win_CommConfig.dcb.fOutxCtsFlow=TRUE;
        Win_CommConfig.dcb.fOutxDsrFlow = FALSE; // guess?
        Win_CommConfig.dcb.fRtsControl=RTS_CONTROL_HANDSHAKE;
		Win_CommConfig.dcb.fInX=FALSE;
		Win_CommConfig.dcb.fOutX=FALSE;
		break;
	default:
		Q_ASSERT(0); // This should never happen BUG replace by a error message
	}

    // write configuration back
    SetCommConfig(Win_Handle, &Win_CommConfig, sizeof(COMMCONFIG));

    // read current settings
    GetCommConfig(Win_Handle, &readCommConfig, &confSize);
	UNLOCK_MUTEX();

    if(Win_CommConfig.dcb.BaudRate != readCommConfig.dcb.BaudRate)
    {
        Settings.BaudRate = readCommConfig.dcb.BaudRate;
	}
	return true;
}
