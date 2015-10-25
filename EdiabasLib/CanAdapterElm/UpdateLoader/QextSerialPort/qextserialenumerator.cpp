/**
 * @file qextserialenumerator.cpp
 * @author Michał Policht
 * @see QextSerialEnumerator
 */
 
#include "qextserialenumerator.h"
#include <QDir>
#include <QStringList>

#ifdef _TTY_WIN_
#include <objbase.h>
#include <initguid.h>

    //this is serial port GUID
    #ifndef GUID_CLASS_COMPORT
            DEFINE_GUID(GUID_DEVINTERFACE_COMPORT, 0x86E0D1E0, 0x8089, 0x11D0, 0x9C, 0xE4, 0x08, 0x00, 0x3E, 0x30, 0x1F, 0x73);
    #endif

	/* Gordon Schumacher's macros for TCHAR -> QString conversions and vice versa */	
	#ifdef UNICODE
		#define QStringToTCHAR(x)     (wchar_t*) x.utf16()
		#define PQStringToTCHAR(x)    (wchar_t*) x->utf16()
		#define TCHARToQString(x)     QString::fromUtf16((ushort*)(x))
		#define TCHARToQStringN(x,y)  QString::fromUtf16((ushort*)(x),(y))
	#else
		#define QStringToTCHAR(x)     x.local8Bit().constData()
		#define PQStringToTCHAR(x)    x->local8Bit().constData()
		#define TCHARToQString(x)     QString::fromLocal8Bit((x))
		#define TCHARToQStringN(x,y)  QString::fromLocal8Bit((x),(y))
	#endif /*UNICODE*/


	//static
	QString QextSerialEnumerator::getRegKeyValue(HKEY key, LPCTSTR property)
	{
		DWORD size = 0;
		RegQueryValueEx(key, property, NULL, NULL, NULL, & size);
		BYTE * buff = new BYTE[size];
		if (RegQueryValueEx(key, property, NULL, NULL, buff, & size) == ERROR_SUCCESS) {
                        return TCHARToQString(buff);
			delete [] buff;
		} else {
			qWarning("QextSerialEnumerator::getRegKeyValue: can not obtain value from registry");
			delete [] buff;
			return QString();
		}
	}
	
	//static
	QString QextSerialEnumerator::getDeviceProperty(HDEVINFO devInfo, PSP_DEVINFO_DATA devData, DWORD property)
	{
		DWORD buffSize = 0;
		SetupDiGetDeviceRegistryProperty(devInfo, devData, property, NULL, NULL, 0, & buffSize);
		BYTE * buff = new BYTE[buffSize];
		if (!SetupDiGetDeviceRegistryProperty(devInfo, devData, property, NULL, buff, buffSize, NULL))
			qCritical("Can not obtain property: %ld from registry", property); 
		QString result = TCHARToQString(buff);
		delete [] buff;
		return result;
	}

	//static
	void QextSerialEnumerator::setupAPIScan(QList<QextPortInfo> & infoList)
	{
		HDEVINFO devInfo = INVALID_HANDLE_VALUE;

        DWORD dwGuids = 0;
        SetupDiClassGuidsFromName(TEXT("Ports"), NULL, 0, &dwGuids);
        if (dwGuids == 0)
        {
            qCritical("SetupDiClassGuidsFromName failed. Error code: %ld", GetLastError());
            return;
        }
        GUID *pGuids = new GUID[dwGuids];
        if (!SetupDiClassGuidsFromName(TEXT("Ports"), pGuids, dwGuids, &dwGuids))
        {
            qCritical("SetupDiClassGuidsFromName second call failed. Error code: %ld", GetLastError());
            return;
        }

        devInfo = SetupDiGetClassDevs(pGuids, NULL, NULL, DIGCF_PRESENT);
        if(devInfo == INVALID_HANDLE_VALUE)
        {
            qCritical("SetupDiGetClassDevs failed. Error code: %ld", GetLastError());
            return;
		}

		//enumerate the devices
		bool ok = true;
		SP_DEVICE_INTERFACE_DATA ifcData;
		ifcData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
		SP_DEVICE_INTERFACE_DETAIL_DATA * detData = NULL;
        SP_DEVINFO_DATA devData = {sizeof(SP_DEVINFO_DATA)};

        for (DWORD i = 0; ok; i++)
        {
            ok = SetupDiEnumDeviceInfo(devInfo, i, &devData);
            if (ok)
            {
                // Got a device. Get the details.
                QextPortInfo info;
                info.friendName = getDeviceProperty(devInfo, & devData, SPDRP_FRIENDLYNAME);
                info.physName = getDeviceProperty(devInfo, & devData, SPDRP_PHYSICAL_DEVICE_OBJECT_NAME);
                info.enumName = getDeviceProperty(devInfo, & devData, SPDRP_ENUMERATOR_NAME);
                //anyway, to get the port name we must still open registry directly :( ???
                //Eh...
                HKEY devKey = SetupDiOpenDevRegKey(devInfo, & devData, DICS_FLAG_GLOBAL, 0,
                                                                                        DIREG_DEV, KEY_READ);
                info.portName = getRegKeyValue(devKey, TEXT("PortName"));
                RegCloseKey(devKey);
                if(info.portName.startsWith("COM"))
                {
                    infoList.append(info);
                }
            }
            else
            {
                if (GetLastError() != ERROR_NO_MORE_ITEMS)
                {
                    delete [] detData;
                    qCritical("SetupDiEnumDeviceInfo failed. Error code: %ld", GetLastError());
                    return;
                }
            }
        }
        delete [] detData;
        delete[] pGuids;
    }

#endif /*_TTY_WIN_*/


//static
QList<QextPortInfo> QextSerialEnumerator::getPorts()
{
    QList<QextPortInfo> ports;

    #ifdef _TTY_WIN_
        OSVERSIONINFO vi;
        vi.dwOSVersionInfoSize = sizeof(vi);
        if (!::GetVersionEx(&vi)) {
                qCritical("Could not get OS version.");
                return ports;
        }
        // Handle windows 9x and NT4 specially
        if (vi.dwMajorVersion < 5) {
                qCritical("Enumeration for this version of Windows is not implemented yet");
/*			if (vi.dwPlatformId == VER_PLATFORM_WIN32_NT)
                        EnumPortsWNt4(ports);
                else
                        EnumPortsW9x(ports);*/
        } else	//w2k or later
                setupAPIScan(ports);
    #endif /*_TTY_WIN_*/

    #ifdef _TTY_POSIX_
        QDir devices("/dev");
        devices.setFilter(QDir::System | QDir::Readable | QDir::Writable);

        QStringList filters;
        filters << "ttyS*" << "ttyUSB*" << "ttyACM*";
        devices.setNameFilters(filters);

        QStringList deviceList;
        deviceList = devices.entryList();
        QextPortInfo info;
        if(deviceList.isEmpty())
        {
            info.friendName = "Serial Port (/dev/serial)";
            info.physName = "/dev/serial";
            info.enumName = "/dev/serial";
            info.portName = "/dev/serial";
            ports.append(info);
        }
        else
        {
            for(int i = 0; i < deviceList.count(); i++)
            {
                info.friendName = "/dev/" + deviceList[i];
                info.physName = "/dev/" + deviceList[i];
                info.enumName = "/dev/" + deviceList[i];
                info.portName = "/dev/" + deviceList[i];
                ports.append(info);
            }
        }
    #endif /*_TTY_POSIX_*/
	
    return ports;
}
