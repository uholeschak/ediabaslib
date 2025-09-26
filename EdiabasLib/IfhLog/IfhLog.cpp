// Usage:
// Rename the original DLL XXX.dll to XXXOrg.dll
// Place this DLL as XXX.dll in the same directory
// Configure logging in XXXLog.ini
// The logfile will be created as XXX.log

#include <windows.h>
#include <Shlwapi.h>
#include <cstdarg>
#include <locale>
#include <codecvt>
#include <string>

#define DLLEXPORT __declspec(dllexport)

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

#define IFH_COMPATIBILITY_NO 6
#define CFGTYPE_PATH 0x13
#define CFGTYPE_STRING 0x23
#define CFGTYPE_INT 0x37
#define CFGTYPE_BOOL 0x47

#define MAXCFGNAME 32
#define APIMAXCONFIG 256

typedef struct
{
    INT16 fktNo;
    TCHAR fktName[100];
} FUNCTION;

typedef short POWERSTATE;
typedef struct
{
    POWERSTATE UbattCurrent;
    POWERSTATE UbattHistory;
    POWERSTATE IgnitionCurrent;
    POWERSTATE IgnitionHistory;
} PSCONTEXT;

typedef short CFGID;
typedef short CFGTYPE;

#pragma pack(2)
typedef struct
{
    CHAR name[MAXCFGNAME]; /* Name des Konfigurationselements */
    CFGTYPE type; /* Konfigurationstyp */
    CFGID id; /* Konfigurations-ID */
    union /* Wert des Konfigurationselements */
    {
        CHAR p[APIMAXCONFIG];
        CHAR s[APIMAXCONFIG];
        INT16 i;
        BOOL b;
    } value;
} CFGCONTEXT;
#pragma pack()

#pragma pack(4)
typedef struct
{
    INT16 fktNo; /* Nummer der IFH-Schnittstellenfunktion */
    INT16 wParam; /* Frei verf�gbar */
    UINT32 reserved; /* reserviert */
    UINT16 channel; /* Kanal-ID */
    UINT16 len; /* Anzahl der Datenbytes */
    UCHAR *data; /* Datenbytes */
} MESSAGE;

typedef struct
{
    INT16 fktNo; /* Nummer der IFH-Schnittstellenfunktion */
    INT16 wParam; /* Frei verf�gbar */
    UINT16 len; /* Anzahl der Datenbytes */
    UCHAR *data; /* Datenbytes */
} MESSAGE_V4;
#pragma pack()

static HANDLE hMutex = NULL;
static HMODULE hIfhDll = NULL;
static FILE *hLogFile = NULL;
static int compatNo = IFH_COMPATIBILITY_NO;
static int iDisableLog = 0;
static int iStatusLog = 0;
static int iAppendLog = 0;
static int iFlushLog = 0;
static int iRawMode = 0;

static const FUNCTION functions[] = 
{
    { 1, TEXT("ifhInit")},
    { 2, TEXT("ifhGetVersion")},
    { 3, TEXT("ifhGetIfhStatus") },
    { 4, TEXT("ifhGetIfhError") },
    { 5, TEXT("ifhGetResult") },
    { 8, TEXT("ifhEnd") },
    { 11, TEXT("ifhPassSetConfig") },
    { 12, TEXT("ifhPassGetConfig") },
    { 13, TEXT("ifhNotifyConfig") },
    { 14, TEXT("ifhGetPowerState") },
    { 20, TEXT("ifhConnect") },
    { 21, TEXT("ifhDisconnect") },
    { 22, TEXT("ifhInterfaceType") },
    { 23, TEXT("ifhPowerSupply") },
    { 24, TEXT("ifhIgnition") },
    { 25, TEXT("ifhWarmStart") },
    { 26, TEXT("ifhReset") },
    { 27, TEXT("ifhSetParameter") },
    { 28, TEXT("ifhSetTelPreface") },
    { 29, TEXT("ifhSendTelegram") },
    { 30, TEXT("ifhSendTelegramFreq") },
    { 31, TEXT("ifhRequTelegramFreq") },
    { 32, TEXT("ifhStopFreqTelegram") },
    { 33, TEXT("ifhRequestKeyBytes") },
    { 34, TEXT("ifhRepeatLastMsg") },
    { 35, TEXT("ifhRequestState") },
    { 36, TEXT("ifhSetPort") },
    { 37, TEXT("ifhGetPort") },
    { 38, TEXT("ifhSetProgVoltage") },
    { 39, TEXT("ifhLoopTest") },
    { 40, TEXT("ifhVersion") },
    { 41, TEXT("ifhDownload") },
    { 42, TEXT("ifhSwitchSiRelais") },
    { 43, TEXT("ifhStopTransmission") },
    { 44, TEXT("ifhRawMode") },
    { 45, TEXT("ifhSend") },
    { 46, TEXT("ifhReceive") },
    { 47, TEXT("ifhSysInfo") },
    { 48, TEXT("ifhOpenChannel") },
    { 49, TEXT("ifhCloseChannel") },
    { 50, TEXT("ifhSendDirect") },
    { 51, TEXT("ifhReceiveDirect") },
    { 54, TEXT("IfhSetParameterRaw") },
};

static const TCHAR *ErrorDescription[] =
{
    TEXT("IFH-0000: INTERNAL ERROR"),
    TEXT("IFH-0001: UART ERROR"),
    TEXT("IFH-0002: NO RESPONSE FROM INTERFACE"),
    TEXT("IFH-0003: DATATRANSMISSION TO INTERFACE DISTURBED"),
    TEXT("IFH-0004: ERROR IN INTERFACE COMMAND"),
    TEXT("IFH-0005: INTERNAL INTERFACE ERROR"),
    TEXT("IFH-0006: COMMAND NOT ACCEPTED"),
    TEXT("IFH-0007: WRONG UBATT"),
    TEXT("IFH-0008: CONTROLUNIT CONNECTION ERROR"),
    TEXT("IFH-0009: NO RESPONSE FROM CONTROLUNIT"),
    TEXT("IFH-0010: DATATRANSMISSION TO CONTROLUNIT DISTURBED"),
    TEXT("IFH-0011: UNKNOWN INTERFACE"),
    TEXT("IFH-0012: BUFFER OVERFLOW"),
    TEXT("IFH-0013: COMMAND NOT IMPLEMENTED"),
    TEXT("IFH-0014: CONCEPT NOT IMPLEMENTED"),
    TEXT("IFH-0015: UBATT ON/OFF ERROR"),
    TEXT("IFH-0016: IGNITION ON/OFF ERROR"),
    TEXT("IFH-0017: INTERFACE DEADLOCK ERROR"),
    TEXT("IFH-0018: INITIALIZATION ERROR"),
    TEXT("IFH-0019: DEVICE ACCESS ERROR"),
    TEXT("IFH-0020: DRIVER ERROR"),
    TEXT("IFH-0021: ILLEGAL PORT"),
    TEXT("IFH-0022: DRIVER STATUS ERROR"),
    TEXT("IFH-0023: INTERFACE STATUS ERROR"),
    TEXT("IFH-0024: CANCEL FAILED"),
    TEXT("IFH-0025: INTERFACE APPLICATION ERROR"),
    TEXT("IFH-0026: SIMULATION ERROR"),
    TEXT("IFH-0027: IFH NOT FOUND"),
    TEXT("IFH-0028: ILLEGAL IFH VERSION"),
    TEXT("IFH-0029: ACCESS DENIED"),
    TEXT("IFH-0030: TASK COMMUNICATION ERROR"),
    TEXT("IFH-0031: DATA OVERFLOW"),
    TEXT("IFH-0032: IGNITION IS OFF"),
    TEXT("IFH-0033"),
    TEXT("IFH-0034: CONFIGURATION FILE NOT FOUND"),
    TEXT("IFH-0035: CONFIGURATION ERROR"),
    TEXT("IFH-0036: LOAD ERROR"),
    TEXT("IFH-0037: LOW UBATT"),
    TEXT("IFH-0038: INTERFACE COMMAND NOT IMPLEMENTED"),
    TEXT("IFH-0039: EDIC USER INTERFACE NOT FOUND"),
    TEXT("IFH-0040: ILLEGAL EDIC USER INTERFACE VERSION"),
    TEXT("IFH-0041: ILLEGAL PARAMETERS"),
    TEXT("IFH-0042: CARD INSTALLATION ERROR"),
    TEXT("IFH-0043: COMMUNICATION TRACE ERROR"),
    TEXT("IFH-0044: FLASH ERROR"),
    TEXT("IFH-0045: RUNBOARD ERROR"),
    TEXT("IFH-0046: EDIC API ACCESS ERROR"),
    TEXT("IFH-0047: PLUGIN ERROR"),
    TEXT("IFH-0048: PLUGIN FUNCTION ERROR"),
    TEXT("IFH-0049: CSS DEVICE DETECTION ERROR"),
};

static void Init();
static void Exit();
static BOOL LogFlush();
static BOOL LogString(const TCHAR *text);
static BOOL LogFormat(const TCHAR *format, ...);
static BOOL LoadIfhDll();
static BOOL OpenLogFile();

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
        case DLL_PROCESS_ATTACH:
            Init();
            break;

        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
            break;

        case DLL_PROCESS_DETACH:
            Exit();
            break;
    }
    return TRUE;
}

static void Init()
{
    if (hMutex == NULL)
    {
        hMutex = CreateMutex(NULL, FALSE, NULL);
    }
    LoadIfhDll();
}

static void Exit()
{
    if (hIfhDll != NULL)
    {
        FreeLibrary(hIfhDll);
        hIfhDll = NULL;
    }
    if (hLogFile != NULL)
    {
        fflush(hLogFile);
        fclose(hLogFile);
        hLogFile = NULL;
    }
    if (hMutex != NULL)
    {
        CloseHandle(hMutex);
        hMutex = NULL;
    }
}

static BOOL AquireMutex()
{
    if (hMutex == NULL)
    {
        return FALSE;
    }
    if (WaitForSingleObject(hMutex, INFINITE) != WAIT_OBJECT_0)
    {
        return FALSE;
    }
    return TRUE;
}

static BOOL ReleaseMutex()
{
    ReleaseMutex(hMutex);
    return TRUE;
}

static BOOL OpenLogFile()
{
    if (hLogFile != NULL)
    {
        return true;
    }
    TCHAR fileName[MAX_PATH];

    if (!GetModuleFileName((HINSTANCE)&__ImageBase, fileName, MAX_PATH))
    {
        return FALSE;
    }
    PathRemoveExtension(fileName);
    std::wstring logFileName = fileName;
    std::wstring iniFileName = fileName;
    logFileName += TEXT(".log");
    iniFileName += TEXT("Log.ini");

    iDisableLog = GetPrivateProfileInt(TEXT("IfhLog"), TEXT("DisableLog"), 0, iniFileName.c_str());
    iStatusLog = GetPrivateProfileInt(TEXT("IfhLog"), TEXT("StatusLog"), 0, iniFileName.c_str());
    iAppendLog = GetPrivateProfileInt(TEXT("IfhLog"), TEXT("AppendLog"), 0, iniFileName.c_str());
    iFlushLog = GetPrivateProfileInt(TEXT("IfhLog"), TEXT("FlushLog"), 0, iniFileName.c_str());
    iRawMode = GetPrivateProfileInt(TEXT("IfhLog"), TEXT("RawMode"), 0, iniFileName.c_str());

    if (!AquireMutex())
    {
        return FALSE;
    }
    hLogFile = _wfopen(logFileName.c_str(), iAppendLog ? TEXT("at") : TEXT("wt"));
    ReleaseMutex();
    if (hLogFile == NULL)
    {
        return FALSE;
    }
    return TRUE;
}

static std::wstring ConvertTextW(char *text)
{
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    std::wstring textConv = converter.from_bytes((text == NULL) ? "(NULL)" : text);
    return textConv;
}

static BOOL LogFlush()
{
    if (hLogFile == NULL)
    {
        return FALSE;
    }
    if (!AquireMutex())
    {
        return FALSE;
    }
    fflush(hLogFile);
    ReleaseMutex();

    return TRUE;
}

static BOOL LogString(const TCHAR *text)
{
    OpenLogFile();
    if (hLogFile == NULL)
    {
        return FALSE;
    }
    if (!AquireMutex())
    {
        return FALSE;
    }
    fwprintf(hLogFile, text);
    fwprintf(hLogFile, TEXT("\n"));
    if (iFlushLog)
    {
        fflush(hLogFile);
    }
    ReleaseMutex();

    return TRUE;
}

static BOOL LogFormat(const TCHAR *format, ...)
{
    va_list args;
    OpenLogFile();
    if (hLogFile == NULL)
    {
        return FALSE;
    }
    va_start(args, format);
    if (!AquireMutex())
    {
        return FALSE;
    }
    vfwprintf(hLogFile, format, args);
    fwprintf(hLogFile, TEXT("\n"));
    if (iFlushLog)
    {
        fflush(hLogFile);
    }
    ReleaseMutex();
    va_end(args);

    return TRUE;
}

static BOOL LogData(std::wstring dataPrefix, UCHAR *data, unsigned int length)
{
    if (data == NULL)
    {
        return TRUE;
    }
    std::wstring dataString;
    if (iRawMode)
    {
        dataString = dataPrefix;
    }
    for (unsigned int i = 0; i < length; i++)
    {
        if (!iRawMode && (i > 0) && (i % 16 == 0))
        {
            dataString += TEXT("\n");
        }
        TCHAR buffer[100];
        swprintf(buffer, 100, TEXT("%02X "), (unsigned int)data[i]);
        dataString += buffer;
    }
    if (dataString.length() > 0)
    {
        return LogString(dataString.c_str());
    }
    return TRUE;
}

static void LogMsg(MESSAGE *msg, BOOL output)
{
    MESSAGE *msgTmp = msg;
    MESSAGE msgLocal;
    if (compatNo < IFH_COMPATIBILITY_NO)
    {
        MESSAGE_V4 *msgV4 = (MESSAGE_V4 *)msg;
        memset(&msgLocal, 0x00, sizeof(msgLocal));
        msgLocal.fktNo = msgV4->fktNo;
        msgLocal.wParam = msgV4->wParam;
        msgLocal.len = msgV4->len;
        msgLocal.data = msgV4->data;
        msgTmp = &msgLocal;
    }

    unsigned int fktNo = static_cast<unsigned int>(msgTmp->fktNo);
    const TCHAR *fktName = TEXT("");
    for (int i = 0; i < sizeof(functions) / sizeof(functions[0]); i++)
    {
        if (functions[i].fktNo == fktNo)
        {
            fktName = functions[i].fktName;
            break;
        }
    }
    LogFormat(TEXT("%s: fktNo = %u (%02Xh) '%s', wParam = %u, channel = %u, len = %u"),
        output ? TEXT("msgOut") : TEXT("msgIn"),
        fktNo,
        fktNo,
        fktName,
        static_cast<unsigned int>(msgTmp->wParam),
        static_cast<unsigned int>(msgTmp->channel),
        static_cast<unsigned int>(msgTmp->len)
    );

    BOOL printData = TRUE;
    std::wstring dataPrefix = TEXT("(") + std::wstring(fktName) + TEXT("): ");
    switch (fktNo)
    {
        case 1:
            if (output)
            {
                break;
            }

            switch (msgTmp->wParam)
            {
                case 1:
                    LogFormat(TEXT("unit = %s"), ConvertTextW((char*)msgTmp->data).c_str());
                    break;

                case 2:
                    LogFormat(TEXT("application = %s"), ConvertTextW((char*)msgTmp->data).c_str());
                    break;
            }
            break;

        case 2:
            if (output)
            {
                break;
            }
            LogFormat(TEXT("version = %s"), ConvertTextW((char*)msgTmp->data).c_str());
            printData = FALSE;
            break;

        case 3:
            if (!output)
            {
                break;
            }
            switch (msgTmp->wParam)
            {
                case 0:
                    LogString(TEXT("IFHREADY"));
                    break;

                case 1:
                    LogString(TEXT("IFHBUSY"));
                    break;

                case 2:
                    LogString(TEXT("IFHERROR"));
                    break;
            }
            break;

        case 4:
            if (!output)
            {
                break;
            }
            if (msgTmp->wParam == 0)
            {
                LogString(TEXT("EDIABAS_ERR_NONE"));
            }
            else
            {
                unsigned int errorCode = msgTmp->wParam + 9;
                unsigned int errorIndex = errorCode - 10;
                const TCHAR *pDescription = TEXT("");
                if (errorIndex < sizeof(ErrorDescription) / sizeof(ErrorDescription[0]))
                {
                    pDescription = ErrorDescription[errorIndex];
                }
                LogFormat(TEXT("Error code: %u (%02Xh) = \"%s\""), errorCode, errorCode, pDescription);
            }
            break;

        case 11:
        case 12:
        case 13:
            if (msgTmp->len == sizeof(CFGCONTEXT) && msgTmp->data != NULL)
            {
                CFGCONTEXT* pCfgContext = (CFGCONTEXT*)msgTmp->data;

                printData = FALSE;
                if (output && msgTmp->wParam == 0)
                {
                    LogFormat(TEXT("name = %s, no data"), ConvertTextW(pCfgContext->name).c_str());
                    break;
                }

                LogFormat(TEXT("name = %s, type = %u (%02Xh), id = %u (%02Xh)"),
                    ConvertTextW(pCfgContext->name).c_str(),
                    static_cast<unsigned int>(pCfgContext->type),
                    static_cast<unsigned int>(pCfgContext->type),
                    static_cast<unsigned int>(pCfgContext->id),
                    static_cast<unsigned int>(pCfgContext->id));

                switch (pCfgContext->type)
                {
                    case CFGTYPE_PATH:
                        LogFormat(TEXT("path = %s"), ConvertTextW(pCfgContext->value.p).c_str());
                        break;

                    case CFGTYPE_STRING:
                        LogFormat(TEXT("string = %s"), ConvertTextW(pCfgContext->value.s).c_str());
                        break;

                    case CFGTYPE_INT:
                        LogFormat(TEXT("int = %u"), static_cast<unsigned int>(pCfgContext->value.i));
                        break;

                    case CFGTYPE_BOOL:
                        LogFormat(TEXT("bool = %s"), pCfgContext->value.b ? TEXT("TRUE") : TEXT("FALSE"));
                        break;
                }
            }
            break;

        case 14:
            if (msgTmp->len == sizeof(PSCONTEXT) && msgTmp->data != NULL)
            {
                PSCONTEXT *pPsContext = (PSCONTEXT *)msgTmp->data;
                LogFormat(TEXT("ubat_curr = %i, ubat_hist = %i, ignit_curr = %i, ignit_hist = %i"),
                    static_cast<int>(pPsContext->UbattCurrent),
                    static_cast<int>(pPsContext->UbattHistory),
                    static_cast<int>(pPsContext->IgnitionCurrent),
                    static_cast<int>(pPsContext->IgnitionHistory));
                printData = FALSE;
            }
            break;

        case 20:
            if (output)
            {
                break;
            }
            LogFormat(TEXT("sgbd = %s"), ConvertTextW((char *)msgTmp->data).c_str());
            break;

    }
    if (printData)
    {
        LogData(dataPrefix, msgTmp->data, msgTmp->len);
    }
}

static BOOL LoadIfhDll()
{
    if (hIfhDll != NULL)
    {
        return TRUE;
    }
    TCHAR fileName[MAX_PATH];

    if (!GetModuleFileName((HINSTANCE)&__ImageBase, fileName, MAX_PATH))
    {
        return FALSE;
    }
    PathRemoveExtension(fileName);
    std::wstring ifhFileName = fileName;
    ifhFileName += TEXT("Org.dll");
    hIfhDll = LoadLibrary(ifhFileName.c_str());
    if (hIfhDll == NULL)
    {
        LogFormat(TEXT("LoadLibrary failed: %s"), ifhFileName.c_str());
        return FALSE;
    }
    return TRUE;
}

#ifdef __cplusplus
extern "C"
#endif
{
    typedef BOOL(FAR PASCAL* PdllLockIFH)(void);

    DLLEXPORT BOOL FAR PASCAL dllLockIFH(void)
    {
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:_dllLockIFH=_dllLockIFH@0")
#endif
        LogFormat(TEXT("dllLockIFH()"));
        if (hIfhDll == NULL)
        {
            return FALSE;
        }
        PdllLockIFH pdllLockIFH = (PdllLockIFH)GetProcAddress(hIfhDll, "dllLockIFH");
        if (pdllLockIFH == NULL)
        {
            pdllLockIFH = (PdllLockIFH)GetProcAddress(hIfhDll, "_dllLockIFH@0");
        }
        if (pdllLockIFH == NULL)
        {
            LogString(TEXT("dllLockIFH not found"));
            return FALSE;
        }
        BOOL result = pdllLockIFH();
        LogFormat(TEXT("dllLockIFH()=%u"), (unsigned int)result);
        return result;
    }

    typedef void(FAR PASCAL* PdllUnlockIFH)(void);

    DLLEXPORT void FAR PASCAL dllUnlockIFH(void)
    {
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:_dllUnlockIFH=_dllUnlockIFH@0")
#endif
        LogFormat(TEXT("dllUnlockIFH()"));
        if (hIfhDll == NULL)
        {
            return;
        }
        PdllUnlockIFH pdllUnlockIFH = (PdllUnlockIFH)GetProcAddress(hIfhDll, "dllUnlockIFH");
        if (pdllUnlockIFH == NULL)
        {
            pdllUnlockIFH = (PdllUnlockIFH)GetProcAddress(hIfhDll, "_dllUnlockIFH@0");
        }
        if (pdllUnlockIFH == NULL)
        {
            LogString(TEXT("dllUnlockIFH not found"));
            return;
        }
        pdllUnlockIFH();
        LogFlush();
    }

    typedef short(FAR PASCAL* PdllStartupIFH)(char* ediabasIniPath, char* ifhName);

    DLLEXPORT short FAR PASCAL dllStartupIFH(char* ediabasIniPath, char* ifhName)
    {
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:_dllStartupIFH=_dllStartupIFH@8")
#endif
        LogFormat(TEXT("dllStartupIFH('%s', '%s')"),
            ConvertTextW(ediabasIniPath).c_str(),
            ConvertTextW(ifhName).c_str());
        if (hIfhDll == NULL)
        {
            return -1;
        }
        PdllStartupIFH pdllStartupIFH = (PdllStartupIFH)GetProcAddress(hIfhDll, "dllStartupIFH");
        if (pdllStartupIFH == NULL)
        {
            pdllStartupIFH = (PdllStartupIFH)GetProcAddress(hIfhDll, "_dllStartupIFH@8");
        }
        if (pdllStartupIFH == NULL)
        {
            LogString(TEXT("dllStartupIFH not found"));
            return -1;
        }
        short result = pdllStartupIFH(ediabasIniPath, ifhName);
        LogFormat(TEXT("dllStartupIFH()=%u"), (unsigned int)result);
        return result;
    }

    typedef void(FAR PASCAL* PdllShutdownIFH)(void);

    DLLEXPORT void FAR PASCAL dllShutdownIFH(void)
    {
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:_dllShutdownIFH=_dllShutdownIFH@0")
#endif
        LogFormat(TEXT("dllShutdownIFH()"));
        if (hIfhDll == NULL)
        {
            return;
        }
        PdllShutdownIFH pdllShutdownIFH = (PdllShutdownIFH)GetProcAddress(hIfhDll, "dllShutdownIFH");
        if (pdllShutdownIFH == NULL)
        {
            pdllShutdownIFH = (PdllShutdownIFH)GetProcAddress(hIfhDll, "_dllShutdownIFH@0");
        }
        if (pdllShutdownIFH == NULL)
        {
            LogString(TEXT("dllShutdownIFH not found"));
            return;
        }
        pdllShutdownIFH();
        LogFlush();
    }

    typedef short(FAR PASCAL* PdllCheckIFH)(short compatibilityNo);

    DLLEXPORT short FAR PASCAL dllCheckIFH(short compatibilityNo)
    {
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:_dllCheckIFH=_dllCheckIFH@4")
#endif
        LogFormat(TEXT("dllCheckIFH(%u)"), (unsigned int)compatibilityNo);
        compatNo = compatibilityNo;
        if (hIfhDll == NULL)
        {
            return -1;
        }
        PdllCheckIFH pdllCheckIFH = (PdllCheckIFH)GetProcAddress(hIfhDll, "dllCheckIFH");
        if (pdllCheckIFH == NULL)
        {
            pdllCheckIFH = (PdllCheckIFH)GetProcAddress(hIfhDll, "_dllCheckIFH@4");
        }
        if (pdllCheckIFH == NULL)
        {
            LogString(TEXT("dllCheckIFH not found"));
            return -1;
        }
        short result = pdllCheckIFH(compatibilityNo);
        LogFormat(TEXT("dllCheckIFH()=%u"), (unsigned int)result);
        return result;
    }

    typedef void(FAR PASCAL* PdllExitIFH)(void);

    DLLEXPORT void FAR PASCAL dllExitIFH(void)
    {
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:_dllExitIFH=_dllExitIFH@0")
#endif
        LogFormat(TEXT("dllExitIFH()"));
        if (hIfhDll == NULL)
        {
            return;
        }
        PdllExitIFH pdllExitIFH = (PdllExitIFH)GetProcAddress(hIfhDll, "dllExitIFH");
        if (pdllExitIFH == NULL)
        {
            pdllExitIFH = (PdllExitIFH)GetProcAddress(hIfhDll, "_dllExitIFH@0");
        }
        if (pdllExitIFH == NULL)
        {
            LogString(TEXT("dllExitIFH not found"));
            return;
        }
        pdllExitIFH();
        LogFlush();
    }

    typedef short(FAR PASCAL* PdllCallIFH)(MESSAGE* msgIn, MESSAGE* msgOut);

    DLLEXPORT short FAR PASCAL dllCallIFH(MESSAGE* msgIn, MESSAGE* msgOut)
    {
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:_dllCallIFH=_dllCallIFH@8")
#endif
        BOOL writeLog = TRUE;

        if (iDisableLog)
        {
            writeLog = FALSE;
        }

        if (!iStatusLog && msgIn->fktNo == 3)
        {   // hide status message
            writeLog = FALSE;
        }

        if (writeLog) LogFormat(TEXT("dllCallIFH()"));

        const TCHAR* fktName = TEXT("");
        for (int i = 0; i < sizeof(functions) / sizeof(functions[0]); i++)
        {
            if (functions[i].fktNo == msgIn->fktNo)
            {
                fktName = functions[i].fktName;
                break;
            }
        }
        if (writeLog) LogMsg(msgIn, FALSE);
        if (hIfhDll == NULL)
        {
            return -1;
        }
        PdllCallIFH pdllCallIFH = (PdllCallIFH)GetProcAddress(hIfhDll, "dllCallIFH");
        if (pdllCallIFH == NULL)
        {
            pdllCallIFH = (PdllCallIFH)GetProcAddress(hIfhDll, "_dllCallIFH@8");
        }
        if (pdllCallIFH == NULL)
        {
            LogString(TEXT("dllCallIFH not found"));
            return -1;
        }
        short result = pdllCallIFH(msgIn, msgOut);
        if (writeLog) LogMsg(msgOut, TRUE);
        if (writeLog) LogFormat(TEXT("dllCallIFH()=%u"), (unsigned int)result);
        return result;
    }

    typedef void (FAR PASCAL* PXControlEnable)(BOOL enable);

    DLLEXPORT void FAR PASCAL XControlEnable(BOOL enable)
    {
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:_XControlEnable=_XControlEnable@4")
#endif
        LogFormat(TEXT("XControlEnable(%u)"), enable);
        if (hIfhDll == NULL)
        {
            return;
        }
        PXControlEnable pXControlEnable = (PXControlEnable)GetProcAddress(hIfhDll, "XControlEnable");
        if (pXControlEnable == NULL)
        {
            pXControlEnable = (PXControlEnable)GetProcAddress(hIfhDll, "_XControlEnable@4");
        }
        if (pXControlEnable == NULL)
        {
            LogString(TEXT("XControlEnable not found"));
            return;
        }
        pXControlEnable(enable);
    }
}
