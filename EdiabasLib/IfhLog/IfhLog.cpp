// Ifh.cpp : Definiert die exportierten Funktionen für die DLL-Anwendung.
//

#include <windows.h>
#include <Shlwapi.h>
#include <stdarg.h>
#include <locale>
#include <codecvt>
#include <string>

#define DLLEXPORT __declspec(dllexport)

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

#define FLUSH_LOG 0

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
    INT16 wParam; /* Frei verfügbar */
    UINT32 reserved; /* reserviert */
    UINT16 channel; /* Kanal-ID */
    UINT16 len; /* Anzahl der Datenbytes */
    UCHAR *data; /* Datenbytes */
} MESSAGE;

typedef struct
{
    INT16 fktNo; /* Nummer der IFH-Schnittstellenfunktion */
    INT16 wParam; /* Frei verfügbar */
    UINT16 len; /* Anzahl der Datenbytes */
    UCHAR *data; /* Datenbytes */
} MESSAGE_V4;
#pragma pack()

static HANDLE hMutex = NULL;
static HMODULE hIfhDll = NULL;
static FILE *hLogFile = NULL;
static int compatNo = IFH_COMPATIBILITY_NO;

static FUNCTION functions[] = 
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
    logFileName += TEXT(".log");

    if (!AquireMutex())
    {
        return FALSE;
    }
    hLogFile = _wfopen(logFileName.c_str(), TEXT("wt"));
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
#if FLUSH_LOG
    fflush(hLogFile);
#endif
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
#if FLUSH_LOG
    fflush(hLogFile);
#endif
    ReleaseMutex();
    va_end(args);

    return TRUE;
}

static BOOL LogData(UCHAR *data, unsigned int length)
{
    if (data != NULL && length > 0)
    {
        std::wstring dataString;
        for (unsigned int i = 0; i < length; i++)
        {
            if ((i > 0) && (i % 16 == 0))
            {
                dataString += TEXT("\n");
            }
            TCHAR buffer[100];
            swprintf(buffer, 100, TEXT("%02X "), (unsigned int)data[i]);
            dataString += buffer;
        }
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
    TCHAR *fktName = TEXT("");
    for (int i = 0; i < sizeof(functions) / sizeof(functions[0]); i++)
    {
        if (functions[i].fktNo == msgTmp->fktNo)
        {
            fktName = functions[i].fktName;
            break;
        }
    }
    LogFormat(TEXT("%s: fktNo = %u '%s', wParam = %u, channel = %u, len = %u"),
        output ? TEXT("msgOut") : TEXT("msgIn"),
        (unsigned int)msgTmp->fktNo,
        fktName,
        (unsigned int)msgTmp->wParam,
        (unsigned int)msgTmp->channel,
        (unsigned int)msgTmp->len
    );

    BOOL printData = TRUE;
    switch (msgTmp->fktNo)
    {
        case 2:
            if (!output)
            {
                break;
            }
            LogFormat(TEXT("version = %s"), ConvertTextW((char *)msgTmp->data).c_str());
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
            LogFormat(TEXT("error = %u"), (unsigned int)msgTmp->wParam + 9);
            break;

        case 11:
        case 12:
        case 13:
            if (msgTmp->len == sizeof(CFGCONTEXT) && msgTmp->data != NULL)
            {
                CFGCONTEXT *pCfgContext = (CFGCONTEXT *)msgTmp->data;

                LogFormat(TEXT("name = %s, type = %u, id = %u"),
                    ConvertTextW(pCfgContext->name).c_str(),
                    (unsigned int)pCfgContext->type,
                    (unsigned int)pCfgContext->id);
                switch (pCfgContext->type)
                {
                case CFGTYPE_PATH:
                    LogFormat(TEXT("path = %s"),
                        ConvertTextW(pCfgContext->value.p).c_str());
                    break;

                case CFGTYPE_STRING:
                    LogFormat(TEXT("string = %s"),
                        ConvertTextW(pCfgContext->value.s).c_str());
                    break;

                case CFGTYPE_INT:
                    LogFormat(TEXT("int = %u"),
                        (unsigned int)pCfgContext->value.i);
                    break;

                case CFGTYPE_BOOL:
                    LogFormat(TEXT("bool = %s"),
                        pCfgContext->value.b ? TEXT("TRUE") : TEXT("FALSE"));
                    break;
                }
                printData = FALSE;
            }
            break;

        case 14:
            if (msgTmp->len == sizeof(PSCONTEXT) && msgTmp->data != NULL)
            {
                PSCONTEXT *pPsContext = (PSCONTEXT *)msgTmp->data;
                LogFormat(TEXT("ubat_curr = %i, ubat_hist = %i, ignit_curr = %i, ignit_hist = %i"),
                    (int)pPsContext->UbattCurrent,
                    (int)pPsContext->UbattHistory,
                    (int)pPsContext->IgnitionCurrent,
                    (int)pPsContext->IgnitionHistory);
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
        LogData(msgTmp->data, msgTmp->len);
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

typedef BOOL(WINAPI *PdllLockIFH)(void);

extern "C" DLLEXPORT BOOL WINAPI dllLockIFH(void)
{
    LogFormat(TEXT("dllLockIFH()"));
    if (hIfhDll == NULL)
    {
        return FALSE;
    }
    PdllLockIFH pdllLockIFH = (PdllLockIFH) GetProcAddress(hIfhDll, "_dllLockIFH@0");
    if (pdllLockIFH == NULL)
    {
        LogString(TEXT("dllLockIFH not found"));
        return FALSE;
    }
    BOOL result = pdllLockIFH();
    LogFormat(TEXT("dllLockIFH()=%u"), (unsigned int)result);
    return result;
}

typedef void(WINAPI *PdllUnlockIFH)(void);

extern "C" DLLEXPORT void WINAPI dllUnlockIFH(void)
{
    LogFormat(TEXT("dllUnlockIFH()"));
    if (hIfhDll == NULL)
    {
        return;
    }
    PdllUnlockIFH pdllLockIFH = (PdllUnlockIFH) GetProcAddress(hIfhDll, "_dllUnlockIFH@0");
    if (pdllLockIFH == NULL)
    {
        LogString(TEXT("dllUnlockIFH not found"));
        return;
    }
    pdllLockIFH();
    LogFlush();
}

typedef short(WINAPI *PdllStartupIFH)(char *ediabasIniPath, char *ifhName);

extern "C" DLLEXPORT short WINAPI dllStartupIFH(char *ediabasIniPath, char *ifhName)
{
    LogFormat(TEXT("dllStartupIFH('%s', '%s')"),
        ConvertTextW(ediabasIniPath).c_str(),
        ConvertTextW(ifhName).c_str());
    if (hIfhDll == NULL)
    {
        return -1;
    }
    PdllStartupIFH pdllStartupIFH = (PdllStartupIFH) GetProcAddress(hIfhDll, "_dllStartupIFH@8");
    if (pdllStartupIFH == NULL)
    {
        LogString(TEXT("dllStartupIFH not found"));
        return -1;
    }
    short result = pdllStartupIFH(ediabasIniPath, ifhName);
    LogFormat(TEXT("dllStartupIFH()=%u"), (unsigned int)result);
    return result;
}

typedef void(WINAPI *PdllShutdownIFH)(void);

extern "C" DLLEXPORT void WINAPI dllShutdownIFH(void)
{
    LogFormat(TEXT("dllShutdownIFH()"));
    if (hIfhDll == NULL)
    {
        return;
    }
    PdllShutdownIFH pdllShutdownIFH = (PdllShutdownIFH)GetProcAddress(hIfhDll, "_dllShutdownIFH@0");
    if (dllShutdownIFH == NULL)
    {
        LogString(TEXT("dllShutdownIFH not found"));
        return;
    }
    dllShutdownIFH();
    LogFlush();
}

typedef short(WINAPI *PdllCheckIFH)(short compatibilityNo);

extern "C" DLLEXPORT short WINAPI dllCheckIFH(short compatibilityNo)
{
    LogFormat(TEXT("dllCheckIFH(%u)"), (unsigned int) compatibilityNo);
    compatNo = compatibilityNo;
    if (hIfhDll == NULL)
    {
        return -1;
    }
    PdllCheckIFH pdllCheckIFH = (PdllCheckIFH)GetProcAddress(hIfhDll, "_dllCheckIFH@4");
    if (pdllCheckIFH == NULL)
    {
        LogString(TEXT("dllCheckIFH not found"));
        return -1;
    }
    short result = pdllCheckIFH(compatibilityNo);
    LogFormat(TEXT("dllCheckIFH()=%u"), (unsigned int)result);
    return result;
}

typedef void(WINAPI *PdllExitIFH)(void);

extern "C" DLLEXPORT void WINAPI dllExitIFH(void)
{
    LogFormat(TEXT("dllExitIFH()"));
    if (hIfhDll == NULL)
    {
        return;
    }
    PdllExitIFH pdllExitIFH = (PdllExitIFH)GetProcAddress(hIfhDll, "_dllExitIFH@0");
    if (pdllExitIFH == NULL)
    {
        LogString(TEXT("dllExitIFH not found"));
        return;
    }
    pdllExitIFH();
    LogFlush();
}

typedef short(WINAPI *PdllCallIFH)(MESSAGE *msgIn, MESSAGE *msgOut);

extern "C" DLLEXPORT short WINAPI dllCallIFH(MESSAGE *msgIn, MESSAGE *msgOut)
{
    LogFormat(TEXT("dllCallIFH()"));

    TCHAR *fktName = TEXT("");
    for (int i = 0; i < sizeof(functions) / sizeof(functions[0]); i++)
    {
        if (functions[i].fktNo == msgIn->fktNo)
        {
            fktName = functions[i].fktName;
            break;
        }
    }
    LogMsg(msgIn, FALSE);
    if (hIfhDll == NULL)
    {
        return -1;
    }
    PdllCallIFH pdllCallIFH = (PdllCallIFH)GetProcAddress(hIfhDll, "_dllCallIFH@8");
    if (pdllCallIFH == NULL)
    {
        LogString(TEXT("dllCallIFH not found"));
        return -1;
    }
    short result = pdllCallIFH(msgIn, msgOut);
    LogMsg(msgOut, TRUE);
    LogFormat(TEXT("dllCallIFH()=%u"), (unsigned int)result);
    return result;
}

typedef void (WINAPI *PXControlEnable)(BOOL enable);

extern "C" DLLEXPORT void WINAPI XControlEnable(BOOL enable)
{
    LogFormat(TEXT("XControlEnable(%u)"), enable);
    if (hIfhDll == NULL)
    {
        return;
    }
    PXControlEnable pXControlEnable = (PXControlEnable)GetProcAddress(hIfhDll, "_XControlEnable@4");
    if (pXControlEnable == NULL)
    {
        LogString(TEXT("XControlEnable not found"));
        return;
    }
    pXControlEnable(enable);
}
