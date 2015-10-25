#ifndef _QEXTSERIALBASE_H_
#define _QEXTSERIALBASE_H_

#include <QIODevice>
#include <QFile>
#include <QThread>
#include <QMutex>

/*if all warning messages are turned off, flag portability warnings to be turned off as well*/
#ifdef _TTY_NOWARN_
#define _TTY_NOWARN_PORT_
#endif

/*macros for thread support*/
#define LOCK_MUTEX() mutex->lock()
#define UNLOCK_MUTEX() mutex->unlock()

/*macros for warning and debug messages*/
#ifdef _TTY_NOWARN_PORT_
#define TTY_PORTABILITY_WARNING(s)
#else
#define TTY_PORTABILITY_WARNING(s) qWarning(s)
#endif /*_TTY_NOWARN_PORT_*/
#ifdef _TTY_NOWARN_
#define TTY_WARNING(s)
#else
#define TTY_WARNING(s) qWarning(s)
#endif /*_TTY_NOWARN_*/


/*line status constants*/
#define LS_CTS  0x01
#define LS_DSR  0x02
#define LS_DCD  0x04
#define LS_RI   0x08
#define LS_RTS  0x10
#define LS_DTR  0x20
#define LS_ST   0x40
#define LS_SR   0x80

/*error constants*/
#define E_NO_ERROR                   0
#define E_INVALID_FD                 1
#define E_NO_MEMORY                  2
#define E_CAUGHT_NON_BLOCKED_SIGNAL  3
#define E_PORT_TIMEOUT               4
#define E_INVALID_DEVICE             5
#define E_BREAK_CONDITION            6
#define E_FRAMING_ERROR              7
#define E_IO_ERROR                   8
#define E_BUFFER_OVERRUN             9 // BUG (weder) seems to be unused
#define E_RECEIVE_OVERFLOW          10 // BUG (weder) seems to be unused
#define E_RECEIVE_PARITY_ERROR      11
#define E_TRANSMIT_OVERFLOW         12 // BUG (weder) seems to be unused
#define E_READ_FAILED               13
#define E_WRITE_FAILED              14

/*!
 * Enums for port settings.
 */
enum NamingConvention
{
    WIN_NAMES,
    IRIX_NAMES,
    HPUX_NAMES,
    SUN_NAMES,
    DIGITAL_NAMES,
    FREEBSD_NAMES,
    OPENBSD_NAMES,
    LINUX_NAMES
};

enum BaudRateType
{
    BAUD50 = 50,                //POSIX ONLY
    BAUD75 = 75,                //POSIX ONLY
    BAUD110 = 110,
    BAUD134 = 134,               //POSIX ONLY
    BAUD150 = 150,               //POSIX ONLY
    BAUD200 = 200,               //POSIX ONLY
    BAUD300 = 300,
    BAUD600 = 600,
    BAUD1200 = 1200,
    BAUD1800 = 1800,              //POSIX ONLY
    BAUD2400 = 2400,
    BAUD4800 = 4800,
    BAUD9600 = 9600,
    BAUD14400 = 14400,             //WINDOWS ONLY
    BAUD19200 = 19200,
    BAUD38400 = 38400,
    BAUD56000 = 56000,             //WINDOWS ONLY
    BAUD57600 = 57600,
    BAUD76800 = 76800,             //POSIX ONLY
    BAUD115200 = 115200,
    BAUD128000 = 128000,            //WINDOWS ONLY
    BAUD230400 = 230400,
    BAUD250000 = 250000,            //WINDOWS ONLY
    BAUD460800 = 460800,
    BAUD500000 = 500000,
    BAUD576000 = 576000,
    BAUD614400 = 614400,
    BAUD750000 = 750000,
    BAUD921600 = 921600,
    BAUD1000000 = 1000000,
    BAUD1142857 = 1142857,
    BAUD1152000 = 1152000,
    BAUD1200000 = 1200000,
    BAUD1228800 = 1228800,
    BAUD1263158 = 1263158,
    BAUD1333333 = 1333333,
    BAUD1411765 = 1411765,
    BAUD1500000 = 1500000,
    BAUD1714285 = 1714285,
    BAUD2000000 = 2000000,
    BAUD2400000 = 2400000,
    BAUD2457600 = 2457600,
    BAUD2500000 = 2500000,
    BAUD3000000 = 3000000,
    BAUD3500000 = 3500000,
    BAUD4000000 = 4000000,
    BAUD6000000 = 6000000
};

enum DataBitsType
{
    DATA_5,
    DATA_6,
    DATA_7,
    DATA_8
};

enum ParityType
{
    PAR_NONE,
    PAR_ODD,
    PAR_EVEN,
    PAR_MARK,               //WINDOWS ONLY
    PAR_SPACE
};

enum StopBitsType
{
    STOP_1,
    STOP_1_5,               //WINDOWS ONLY
    STOP_2
};

enum FlowType
{
    FLOW_OFF,
    FLOW_HARDWARE,
    FLOW_XONXOFF
};

/*!
 * Structure to contain port settings.
 */
struct PortSettings
{
    unsigned int BaudRate;
    DataBitsType DataBits;
    ParityType Parity;
    StopBitsType StopBits;
    FlowType FlowControl;
    long Timeout_Millisec;
};

/*!
 * A common base class for Win_QextSerialBase, Posix_QextSerialBase and QextSerialPort.
 *
 * \author Stefan Sander
 * \author Michal Policht
 */
class QextSerialBase : public QIODevice
{
	Q_OBJECT

	public:
        int QueueReceiveSignals;

	protected:
	    QMutex* mutex;
	    QString port;
	    ulong lastErr;
        QFile recordingFile;

	    virtual qint64 readData(char * data, qint64 maxSize)=0;
	    virtual qint64 writeData(const char * data, qint64 maxSize)=0;

	public:
        PortSettings Settings;

        QextSerialBase();
	    QextSerialBase(const QString & name);
	    virtual ~QextSerialBase();
	    virtual void construct();
	    virtual void setPortName(const QString & name);
	    virtual QString portName() const;
        virtual void clearReceiveBuffer() = 0;

        bool isRecording(void)
        {
            return recordingFile.isOpen();
        }

        virtual void startRecording(QString fileName, bool allowOverwrite = false);
        virtual void stopRecording(void);

        virtual void setBaudRate(unsigned int)=0;
        virtual unsigned int baudRate() const;
	    virtual void setDataBits(DataBitsType)=0;
	    virtual DataBitsType dataBits() const;
	    virtual void setParity(ParityType)=0;
	    virtual ParityType parity() const;
	    virtual void setStopBits(StopBitsType)=0;
	    virtual StopBitsType stopBits() const;
	    virtual void setFlowControl(FlowType)=0;
	    virtual FlowType flowControl() const;
	    virtual void setTimeout(long)=0;

	    virtual bool open(OpenMode mode)=0;
	    virtual bool isSequential() const;
	    virtual void close()=0;
	    virtual void flush()=0;

	    virtual qint64 size() const = 0;
	    virtual qint64 bytesAvailable() const = 0;
	    virtual bool atEnd() const;

	    virtual void ungetChar(char c)=0;
	    virtual qint64 readLine(char * data, qint64 maxSize);

	    virtual ulong lastError() const;

        virtual void setBreak(bool set=true)=0;
        virtual void setDtr(bool set=true)=0;
	    virtual void setRts(bool set=true)=0;
	    virtual ulong lineStatus()=0;

	signals:
		/*!
		 * This signal is emitted whenever dsr line has changed its state. You may
		 * use this signal to check if device is connected.
		 * 	\param status \p true when DSR signal is on, \p false otherwise.
		 *
		 * 	\see lineStatus().
		 */
		void dsrChanged(bool status);
};

#endif
