// Ifh.cpp : Definiert die exportierten Funktionen für die DLL-Anwendung.
//

#include <windows.h>
#include <Shlwapi.h>
#include <stdarg.h>
#include <locale>
#include <codecvt>
#include <string>

#define DLLEXPORT __declspec(dllexport)

static HMODULE hIfhDll = NULL;
static FILE *hLogFile = NULL;

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

typedef struct
{
    INT16 fktNo; /* Nummer der IFH-Schnittstellenfunktion */
    INT16 wParam; /* Frei verfügbar */
    UINT32 reserved; /* reserviert */
    UINT16 channel; /* Kanal-ID */
    UINT16 len; /* Anzahl der Datenbytes */
    UCHAR *data; /* Datenbytes */
} MESSAGE;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
        case DLL_PROCESS_ATTACH:
        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
            break;

        case DLL_PROCESS_DETACH:
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
            break;
    }
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

    hLogFile = _wfopen(logFileName.c_str(), TEXT("at"));
    if (hLogFile == NULL)
    {
        return FALSE;
    }
    return TRUE;
}

static BOOL LogString(const TCHAR *text)
{
    OpenLogFile();
    if (hLogFile == NULL)
    {
        return FALSE;
    }
    fwprintf(hLogFile, text);
    fwprintf(hLogFile, TEXT("\n"));

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
    vfwprintf(hLogFile, format, args);
    fwprintf(hLogFile, TEXT("\n"));
    va_end(args);

    return TRUE;
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
    if (!LoadIfhDll())
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
    if (!LoadIfhDll())
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
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    std::wstring wediabasIniPath = converter.from_bytes((ediabasIniPath == NULL) ? "(NULL)" : ediabasIniPath);
    std::wstring wifhName = converter.from_bytes((ifhName == NULL) ? "(NULL)" : ifhName);

    LogFormat(TEXT("dllStartupIFH('%s', '%s')"), wediabasIniPath.c_str(), wifhName.c_str());
    if (!LoadIfhDll())
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
    if (!LoadIfhDll())
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
    if (!LoadIfhDll())
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
    if (!LoadIfhDll())
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
    LogFormat(TEXT("msgIn: fktNo = %u, wParam = %u, channel = %u, len = %u"),
        (unsigned int)msgIn->fktNo,
        (unsigned int)msgIn->wParam,
        (unsigned int)msgIn->channel,
        (unsigned int)msgIn->len
    );
    if (msgIn->len > 0)
    {
        std::wstring inData;
        for (int i = 0; i < msgIn->len; i++)
        {
            if ((i > 0) && (i % 16 == 0))
            {
                inData += TEXT("\n");
            }
            TCHAR buffer[100];
            swprintf(buffer, 100, TEXT("%02X "), (unsigned int)msgIn->data[i]);
            inData += buffer;
        }
        LogString(inData.c_str());
    }
    if (!LoadIfhDll())
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
    LogFormat(TEXT("msgOut: fktNo = %u, wParam = %u, channel = %u, len = %u"),
        (unsigned int)msgOut->fktNo,
        (unsigned int)msgOut->wParam,
        (unsigned int)msgOut->channel,
        (unsigned int)msgOut->len
    );
    if (msgOut->len > 0)
    {
        std::wstring outData;
        for (int i = 0; i < msgOut->len; i++)
        {
            if ((i > 0) && (i % 16 == 0))
            {
                outData += TEXT("\n");
            }
            TCHAR buffer[100];
            swprintf(buffer, 100, TEXT("%02X "), (unsigned int)msgOut->data[i]);
            outData += buffer;
        }
        LogString(outData.c_str());
    }
    LogFormat(TEXT("dllCallIFH()=%u"), (unsigned int)result);
    return result;
}

typedef void (WINAPI *PXControlEnable)(BOOL enable);

extern "C" DLLEXPORT void WINAPI XControlEnable(BOOL enable)
{
    LogFormat(TEXT("XControlEnable(%u)"), enable);
    if (!LoadIfhDll())
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
