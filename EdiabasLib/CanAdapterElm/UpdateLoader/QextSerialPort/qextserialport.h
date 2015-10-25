
#ifndef _QEXTSERIALPORT_H_
#define _QEXTSERIALPORT_H_

/*POSIX CODE*/
#ifdef _TTY_POSIX_
#include "posix_qextserialport.h"
#define QextBaseType Posix_QextSerialPort

/*MS WINDOWS CODE*/
#else
#include "win_qextserialport.h"
#define QextBaseType Win_QextSerialPort
#endif

/*!
 * Provides low level COM port serial communications.
 */
class QextSerialPort: public QextBaseType 
{
	Q_OBJECT
	
	public:
        QextSerialPort();
        QextSerialPort(const QString & name);
        QextSerialPort(PortSettings const& s);
        QextSerialPort(const QString & name, PortSettings const& s);
	    QextSerialPort(const QextSerialPort& s);
	    QextSerialPort& operator=(const QextSerialPort&);
	    virtual ~QextSerialPort();

};

#endif
