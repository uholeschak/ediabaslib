// Ifh.cpp : Definiert die exportierten Funktionen für die DLL-Anwendung.
//

#include <windows.h>
#include <Shlwapi.h>
#include <stdarg.h>
#include <locale>
#include <codecvt>
#include <string>

#define DLLEXPORT __declspec(dllexport)

static HANDLE hMutex = NULL;
static HMODULE hIfhDll = NULL;
static FILE *hLogFile = NULL;

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

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
#pragma pack()

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
    hLogFile = _wfopen(logFileName.c_str(), TEXT("at"));
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
    if (!AquireMutex)
    {
        return FALSE;
    }
    vfwprintf(hLogFile, format, args);
    fwprintf(hLogFile, TEXT("\n"));
    ReleaseMutex();
    va_end(args);

    return TRUE;
}

static BOOL LogData(UCHAR *data, unsigned int length)
{
    if (length > 0)
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
    TCHAR *fktName = TEXT("");
    for (int i = 0; i < sizeof(functions) / sizeof(functions[0]); i++)
    {
        if (functions[i].fktNo == msg->fktNo)
        {
            fktName = functions[i].fktName;
            break;
        }
    }
    LogFormat(TEXT("%s: fktNo = %u '%s', wParam = %u, channel = %u, len = %u"),
        output ? TEXT("msgOut") : TEXT("msgIn"),
        (unsigned int)msg->fktNo,
        fktName,
        (unsigned int)msg->wParam,
        (unsigned int)msg->channel,
        (unsigned int)msg->len
    );

    BOOL printData = TRUE;
    switch (msg->fktNo)
    {
        case 2:
            if (!output)
            {
                break;
            }
            switch (msg->wParam)
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

        case 3:
            if (!output)
            {
                break;
            }
            LogFormat(TEXT("version = %s"), ConvertTextW((char *)msg->data).c_str());
            break;

        case 11:
        case 12:
        case 13:
            if (msg->len == sizeof(CFGCONTEXT))
            {
                CFGCONTEXT *pCfgContext = (CFGCONTEXT *)msg->data;

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
            if (msg->len == sizeof(PSCONTEXT))
            {
                PSCONTEXT *pPsContext = (PSCONTEXT *)msg->data;
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
            LogFormat(TEXT("sgbd = %s"), ConvertTextW((char *)msg->data).c_str());
            break;

    }
    if (printData)
    {
        LogData(msg->data, msg->len);
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
}

typedef short(WINAPI *PdllCheckIFH)(short compatibilityNo);

extern "C" DLLEXPORT short WINAPI dllCheckIFH(short compatibilityNo)
{
    LogFormat(TEXT("dllCheckIFH(%u)"), (unsigned int) compatibilityNo);
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
