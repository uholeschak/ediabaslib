#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <stdio.h>
#include <Shlobj.h>
#include <Shlwapi.h>
#include <tlhelp32.h>
#include <list>
#include <map>
#include <string>
#include "../_Common_Files/GenericFakeAPI.h"
#include "HotPatch.h"
// You just need to edit this file to add new fake api 
// WARNING YOUR FAKE API MUST HAVE THE SAME PARAMETERS AND CALLING CONVENTION AS THE REAL ONE,
//                  ELSE YOU WILL GET STACK ERRORS

///////////////////////////////////////////////////////////////////////////////
// fake API prototype MUST HAVE THE SAME PARAMETERS 
// for the same calling convention see MSDN : 
// "Using a Microsoft modifier such as __cdecl on a data declaration is an outdated practice"
///////////////////////////////////////////////////////////////////////////////

#define LOGFILE _T("DebugHelper.txt")
#define CRYPTFILE1 _T("CryptTable1.bin")
#define CRYPTFILE2 _T("CryptTable2.bin")
#define VALID_TIME_DATE_STAMP 0x597C995A

#define STATUS_SUCCESS                   ((NTSTATUS)0x00000000L)    // ntsubauth

#define DECRYPT_TEXT_LINE_ADDR_REL      0x09D091
#define READ_DECRYPTED_LINE_ADDR_REL    0x0416CC

typedef std::basic_string<TCHAR> tstring;

typedef struct
{
    WNDENUMPROC lpEnumFunc;
    LPARAM      lParam;
} ENUMWINDOWS_INFO;

typedef enum _THREADINFOCLASS
{
    ThreadHideFromDebugger=17
} THREADINFOCLASS;

typedef NTSTATUS (WINAPI *ptrNtSetInformationThread)
(
    __in HANDLE ThreadHandle,
    __in THREADINFOCLASS ThreadInformationClass,
    __in_bcount(ThreadInformationLength) PVOID ThreadInformation,
    __in ULONG ThreadInformationLength
);

typedef NTSTATUS (WINAPI *ptrNtQueryInformationThread)(
    _In_      HANDLE          ThreadHandle,
    _In_      THREADINFOCLASS ThreadInformationClass,
    _Inout_   PVOID           ThreadInformation,
    _In_      ULONG           ThreadInformationLength,
    _Out_opt_ PULONG          ReturnLength
);

typedef NTSTATUS(NTAPI *ptrNtSuspendProcess)(IN HANDLE ProcessHandle);

static std::string string_format(const char *fmt, ...);
static BOOL SuspendProcess();
static BOOL IsOwnWindowHandle(HWND hWnd);
static FILE* OpenLogFile();
static void LogPrintf(TCHAR *format, ...);
static void LogFlush();
static void LogData(BYTE *data, unsigned int length, unsigned int max_length = 0x100);
static void LogAsc(BYTE *data, unsigned int length, unsigned int max_length = 0x100);
static BOOL PatchDbgUiRemoteBreakin();
static BOOL GetModuleBaseAddress();
static BOOL GetCryptTables();
static void *DisMember(size_t size, ...);
static BOOL RedirectDecryptTextLine();
static BOOL RedirectReadDecryptedLine();

static HWND WINAPI mFindWindowA(
    _In_opt_ LPCSTR lpClassName,
    _In_opt_ LPCSTR lpWindowName
);

static BOOL CALLBACK EnumWindowsProc(
    __in  HWND hWnd,
    __in  LPARAM lParam
);

static BOOL WINAPI mEnumWindows(
    _In_ WNDENUMPROC lpEnumFunc,
    _In_ LPARAM      lParam
);

static LRESULT WINAPI mSendMessageA(
    _In_ HWND   hWnd,
    _In_ UINT   Msg,
    _In_ WPARAM wParam,
    _In_ LPARAM lParam
);

static BOOL WINAPI mIsDebuggerPresent(void);

static HRSRC WINAPI mFindResourceA(
    _In_opt_ HMODULE hModule,
    _In_     LPCSTR lpName,
    _In_     LPCSTR lpType
);

static HGLOBAL WINAPI mLoadResource(
    _In_opt_ HMODULE hModule,
    _In_     HRSRC   hResInfo
);

static DWORD WINAPI mSizeofResource(
    _In_opt_ HMODULE hModule,
    _In_     HRSRC   hResInfo
);

static HANDLE WINAPI mCreateFileA(
    _In_     LPCSTR                lpFileName,
    _In_     DWORD                 dwDesiredAccess,
    _In_     DWORD                 dwShareMode,
    _In_opt_ LPSECURITY_ATTRIBUTES lpSecurityAttributes,
    _In_     DWORD                 dwCreationDisposition,
    _In_     DWORD                 dwFlagsAndAttributes,
    _In_opt_ HANDLE                hTemplateFile
);

static BOOL WINAPI mReadFile(
    _In_        HANDLE       hFile,
    _Out_       LPVOID       lpBuffer,
    _In_        DWORD        nNumberOfBytesToRead,
    _Out_opt_   LPDWORD      lpNumberOfBytesRead,
    _Inout_opt_ LPOVERLAPPED lpOverlapped
);

static BOOL WINAPI mWriteFile(
    _In_        HANDLE       hFile,
    _In_        LPCVOID      lpBuffer,
    _In_        DWORD        nNumberOfBytesToWrite,
    _Out_opt_   LPDWORD      lpNumberOfBytesWritten,
    _Inout_opt_ LPOVERLAPPED lpOverlapped
);

static BOOL WINAPI mCloseHandle(
    _In_ HANDLE hObject
);

static LPVOID WINAPI mHeapAlloc(
    _In_ HANDLE hHeap,
    _In_ DWORD  dwFlags,
    _In_ SIZE_T dwBytes
);

static LPVOID WINAPI mHeapReAlloc(
    _In_ HANDLE hHeap,
    _In_ DWORD  dwFlags,
    _In_ LPVOID lpMem,
    _In_ SIZE_T dwBytes
);

static BOOL WINAPI mHeapFree(
    _In_ HANDLE hHeap,
    _In_ DWORD  dwFlags,
    _In_ LPVOID lpMem
);

static NTSTATUS WINAPI mNtSetInformationThread(
    __in HANDLE ThreadHandle,
    __in THREADINFOCLASS ThreadInformationClass,
    __in_bcount(ThreadInformationLength) PVOID ThreadInformation,
    __in ULONG ThreadInformationLength
);

static ptrNtSetInformationThread pNtSetInformationThread = NULL;
static ptrNtQueryInformationThread pNtQueryInformationThread = NULL;
static ptrNtSuspendProcess pNtSuspendProcess = NULL;
static FILE *fLog = NULL;
static BYTE* pModuleBaseAddr = 0;
static DWORD dwModuleBaseSize = 0;
static DWORD dwTimeDateStamp = 0;
static std::list<HANDLE> FileWatchList;
static std::list<HANDLE> FileMemWatchList;
static std::list<LPVOID> MemWatchList;
static std::list<HRSRC> ResWatchList;
static std::map<HRSRC, DWORD>ResSizeMap;
static HotPatch PatchDecryptTextLine;
static HotPatch PatchReadDecryptedLine;
static HANDLE hLastLogRFile = INVALID_HANDLE_VALUE;
static HANDLE hLastLogWFile = INVALID_HANDLE_VALUE;
static BOOL bHalted = FALSE;
static BOOL bTablesStored = FALSE;
static BOOL bDecryptPatched = FALSE;

///////////////////////////////////////////////////////////////////////////////
// fake API array. Redirection are defined here
///////////////////////////////////////////////////////////////////////////////
STRUCT_FAKE_API pArrayFakeAPI[]=
{
    // library name ,function name, function handler, stack size (required to allocate enough stack space), FirstBytesCanExecuteAnywhereSize (optional put to 0 if you don't know it's meaning)
    //                                                stack size= sum(StackSizeOf(ParameterType))           Same as monitoring file keyword (see monitoring file advanced syntax)
    {_T("User32.dll"),_T("FindWindowA"),(FARPROC)mFindWindowA,StackSizeOf(LPCSTR)+StackSizeOf(LPCSTR),0 },
    {_T("User32.dll"),_T("EnumWindows"),(FARPROC)mEnumWindows,StackSizeOf(WNDENUMPROC)+StackSizeOf(LPARAM),0 },
    {_T("User32.dll"),_T("SendMessageA"),(FARPROC)mSendMessageA,StackSizeOf(HWND)+StackSizeOf(UINT)+StackSizeOf(WPARAM)+StackSizeOf(LPARAM),0 },
    {_T("Kernel32.dll"),_T("IsDebuggerPresent"),(FARPROC)mIsDebuggerPresent,0,0},
    {_T("Kernel32.dll"),_T("FindResourceA"),(FARPROC)mFindResourceA,StackSizeOf(HMODULE)+StackSizeOf(LPCSTR)+StackSizeOf(LPCSTR),0 },
    {_T("Kernel32.dll"),_T("LoadResource"),(FARPROC)mLoadResource,StackSizeOf(HMODULE)+StackSizeOf(HRSRC),0 },
    {_T("Kernel32.dll"),_T("SizeofResource"),(FARPROC)mSizeofResource,StackSizeOf(HMODULE)+StackSizeOf(HRSRC),0 },
    {_T("Kernel32.dll"),_T("CreateFileA"),(FARPROC)mCreateFileA,StackSizeOf(LPCSTR)+StackSizeOf(DWORD)+StackSizeOf(DWORD)+StackSizeOf(LPSECURITY_ATTRIBUTES)+StackSizeOf(DWORD)+StackSizeOf(DWORD)+StackSizeOf(HANDLE),0 },
    {_T("Kernel32.dll"),_T("ReadFile"),(FARPROC)mReadFile,StackSizeOf(HANDLE)+StackSizeOf(LPVOID)+StackSizeOf(DWORD)+StackSizeOf(LPDWORD)+StackSizeOf(LPOVERLAPPED),0 },
    {_T("Kernel32.dll"),_T("WriteFile"),(FARPROC)mWriteFile,StackSizeOf(HANDLE)+StackSizeOf(LPCVOID)+StackSizeOf(DWORD)+StackSizeOf(LPDWORD)+StackSizeOf(LPOVERLAPPED),0 },
    {_T("Kernel32.dll"),_T("CloseHandle"),(FARPROC)mCloseHandle,StackSizeOf(HANDLE),0 },
    {_T("Kernel32.dll"),_T("HeapAlloc"),(FARPROC)mHeapAlloc,StackSizeOf(HANDLE)+StackSizeOf(DWORD)+StackSizeOf(SIZE_T),0 },
    {_T("Kernel32.dll"),_T("HeapReAlloc"),(FARPROC)mHeapReAlloc,StackSizeOf(HANDLE) + StackSizeOf(DWORD)+StackSizeOf(LPVOID)+StackSizeOf(SIZE_T),0 },
    {_T("Kernel32.dll"),_T("HeapFree"),(FARPROC)mHeapFree,StackSizeOf(HANDLE)+StackSizeOf(DWORD)+StackSizeOf(LPVOID),0 },
    {_T("Ntdll.dll"),_T("NtSetInformationThread"),(FARPROC)mNtSetInformationThread,StackSizeOf(HANDLE)+StackSizeOf(THREADINFOCLASS)+StackSizeOf(PVOID)+StackSizeOf(ULONG),0 },
    {_T(""),_T(""),NULL,0,0}// last element for ending loops
};

///////////////////////////////////////////////////////////////////////////////
// Before API call array. Redirection are defined here
///////////////////////////////////////////////////////////////////////////////
STRUCT_FAKE_API_WITH_USERPARAM pArrayBeforeAPICall[]=
{
    // library name ,function name, function handler, stack size (required to allocate enough stack space), FirstBytesCanExecuteAnywhereSize (optional put to 0 if you don't know it's meaning),userParam : a value that will be post back to you when your hook will be called
    //                                                stack size= sum(StackSizeOf(ParameterType))           Same as monitoring file keyword (see monitoring file advanced syntax)
    {_T(""),_T(""),NULL,0,0,0}// last element for ending loops
};

///////////////////////////////////////////////////////////////////////////////
// After API call array. Redirection are defined here
///////////////////////////////////////////////////////////////////////////////
STRUCT_FAKE_API_WITH_USERPARAM pArrayAfterAPICall[]=
{
    // library name ,function name, function handler, stack size (required to allocate enough stack space), FirstBytesCanExecuteAnywhereSize (optional put to 0 if you don't know it's meaning),userParam : a value that will be post back to you when your hook will be called
    //                                                stack size= sum(StackSizeOf(ParameterType))           Same as monitoring file keyword (see monitoring file advanced syntax)
    {_T(""),_T(""),NULL,0,0,0}// last element for ending loops
};

BOOL WINAPI DllMain(HINSTANCE hInstDLL, DWORD dwReason, PVOID pvReserved)
{
	UNREFERENCED_PARAMETER(hInstDLL);
    UNREFERENCED_PARAMETER(pvReserved);
    switch (dwReason)
    {
        case DLL_PROCESS_ATTACH:
            {
                // get func address
                HMODULE hNtdll = GetModuleHandle(_T("Ntdll.dll"));
                if (hNtdll != NULL)
                {
                    pNtSetInformationThread=(ptrNtSetInformationThread)GetProcAddress(hNtdll,"NtSetInformationThread");
                    pNtQueryInformationThread=(ptrNtQueryInformationThread)GetProcAddress(hNtdll, "NtQueryInformationThread");
                    pNtSuspendProcess = (ptrNtSuspendProcess)GetProcAddress(hNtdll, "NtSuspendProcess");
                }
                if (pNtSetInformationThread == NULL || pNtQueryInformationThread == NULL || pNtSuspendProcess == NULL)
                {
                    return FALSE;
                }
                fLog = OpenLogFile();
                GetModuleBaseAddress();
                PatchDbgUiRemoteBreakin();
            }
            break;

        case DLL_PROCESS_DETACH:
            if (fLog != NULL)
            {
                fclose(fLog);
                fLog = NULL;
            }
            break;
    }

    return TRUE;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////// NEW API DEFINITION //////////////////////////////
/////////////////////// You don't need to export these functions //////////////
///////////////////////////////////////////////////////////////////////////////

std::string string_format(const char *fmt, ...)
{
    char buffer[1000];
    va_list ap;

    va_start(ap, fmt);
    vsprintf_s(buffer, fmt, ap);
    va_end(ap);

    std::string str(buffer);

    return str;
}

BOOL SuspendProcess()
{
    if (bHalted)
    {
        return FALSE;
    }
    bHalted = true;

    LogPrintf(_T("Suspending process\n"));
    LogPrintf(_T("Resume it with PSSuspend -r %u\n"), GetCurrentProcessId());
    LogFlush();
    pNtSuspendProcess(GetCurrentProcess());
    LogPrintf(_T("Resumed ...\n"));
    return TRUE;
}

BOOL IsOwnWindowHandle(HWND hWnd)
{
    DWORD dwProcessId = 0;
    DWORD dwThreadId = GetWindowThreadProcessId(hWnd, &dwProcessId);
    if (dwThreadId == 0)
    {
        return FALSE;
    }
    return dwProcessId == GetCurrentProcessId();
}

FILE* OpenLogFile()
{
    TCHAR szPath[MAX_PATH];

    if (SUCCEEDED(SHGetFolderPath(NULL,
        CSIDL_PERSONAL | CSIDL_FLAG_CREATE,
        NULL,
        0,
        szPath)))
    {
        if (PathAppend(szPath, LOGFILE))
        {
            return _tfopen(szPath, _T("wt"));
        }
    }
    return NULL;
}

void LogPrintf(TCHAR *format, ...)
{
    va_list args;

    if (fLog != NULL)
    {
        if (hLastLogRFile != INVALID_HANDLE_VALUE || hLastLogWFile != INVALID_HANDLE_VALUE)
        {
            hLastLogRFile = INVALID_HANDLE_VALUE;
            hLastLogWFile = INVALID_HANDLE_VALUE;
            _fputts(_T("\n"), fLog);
        }
        va_start(args, format);
        _vftprintf(fLog, format, args);
        va_end(args);
    }
}

void LogFlush()
{
    if (fLog != NULL)
    {
        fflush(fLog);
    }
}

void LogData(BYTE *data, unsigned int length, unsigned int max_length)
{
    if (fLog == NULL || data == NULL || length == 0 || length > max_length)
    {
        return;
    }

    for (unsigned int i = 0; i < length; i++)
    {
        if ((i > 0) && (i % 64 == 0))
        {
            _fputts(_T("\n"), fLog);
        }
        _ftprintf(fLog, _T("%02X "), (unsigned int)data[i]);
    }
}

void LogAsc(BYTE *data, unsigned int length, unsigned int max_length)
{
    if (fLog == NULL || data == NULL || length == 0 || length > max_length)
    {
        return;
    }

    if (length >= 4)
    {
        DWORD dwType = *(DWORD *)(data + 0);
        if (length >= 0x10 && dwType == 0x0053DF24)
        {   // CString type
            DWORD dwStrLen = *(DWORD *)(data + 4);
            if ((dwStrLen + 0x10) <= length)
            {
                _fputts(_T("CString: "), fLog);
                for (unsigned int i = 0; i < dwStrLen; i++)
                {
                    BYTE value = data[i + 0x10];
                    if (isprint(value))
                    {
                        _fputtc(value, fLog);
                    }
                    else
                    {
                        _ftprintf(fLog, _T("<%02X>"), (unsigned int)value);
                    }
                }
                _fputts(_T("\n"), fLog);
                return;
            }
        }
    }

    for (unsigned int i = 0; i < length; i++)
    {
        BYTE value = data[i];
        if (isprint(value))
        {
            _fputtc(value, fLog);
        }
        else
        {
            _ftprintf(fLog, _T("<%02X>"), (unsigned int)value);
        }
    }
    _fputts(_T("\n"), fLog);
}

BOOL PatchDbgUiRemoteBreakin()
{
    HMODULE hNtdll = GetModuleHandle(_T("Ntdll.dll"));
    if (hNtdll == NULL)
    {
        LogPrintf(_T("PatchDbgUiRemoteBreakin: GetModuleHandle failed\n"));
        return FALSE;
    }
    FARPROC ntdll = GetProcAddress(hNtdll, "DbgUiRemoteBreakin");
    if (ntdll == NULL)
    {
        LogPrintf(_T("PatchDbgUiRemoteBreakin: GetProcAddress failed\n"));
        return FALSE;
    }
    BYTE buffer[] = { 0x00, 0x00, 0x00, 0x00 };
    BYTE code[] = { 0x6A, 0x08, 0x68, 0x30 };   // int 3, ret
    SIZE_T count = 0;

    HANDLE hProcess = GetCurrentProcess();
    if (!ReadProcessMemory(hProcess, ntdll, &buffer, sizeof(buffer), &count))
    {
        LogPrintf(_T("PatchDbgUiRemoteBreakin: ReadProcessMemory failed\n"));
        return FALSE;
    }

    if (memcmp(code, buffer, sizeof(code)) == 0)
    {   // already patched
        return TRUE;
    }

    if (!WriteProcessMemory(hProcess, ntdll, &code, sizeof(code), &count))
    {
        LogPrintf(_T("PatchDbgUiRemoteBreakin: WriteProcessMemory failed\n"));
        return FALSE;
    }
    LogPrintf(_T("PatchDbgUiRemoteBreakin: Patched\n"));
    return TRUE;
}

BOOL GetModuleBaseAddress()
{
    LogPrintf(_T("GetModuleBaseAddress\n"));
    HANDLE hModuleSnap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, NULL);
    if (hModuleSnap == INVALID_HANDLE_VALUE)
    {
        LogPrintf(_T("CreateToolhelp32Snapshot failed\n"));
        return FALSE;
    }
    __try
    {
        MODULEENTRY32 me32;
        me32.dwSize = sizeof(MODULEENTRY32);

        if (!Module32First(hModuleSnap, &me32))
        {
            LogPrintf(_T("Module32First failed\n"));
            return FALSE;
        }
        BOOL bFound = FALSE;
        do
        {
            LPTSTR ext = PathFindExtension(me32.szExePath);
            if (ext != NULL)
            {
                if (_tcsicmp(ext, _T(".exe")) == 0)
                {
                    bFound = TRUE;
                    break;
                }
            }
        } while (Module32Next(hModuleSnap, &me32));

        if (!bFound)
        {
            LogPrintf(_T("Executable not found.\n"));
            return FALSE;
        }

        LogPrintf(_T("Exe name: %s\n"), me32.szModule);
        LogPrintf(_T("Exe path: %s\n"), me32.szExePath);
        LogPrintf(_T("Exe base: %08p\n"), me32.modBaseAddr);
        LogPrintf(_T("Exe size: %08X\n"), me32.modBaseSize);

        pModuleBaseAddr = me32.modBaseAddr;
        dwModuleBaseSize = me32.modBaseSize;

        PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)pModuleBaseAddr;
        if (pDosHeader->e_magic != IMAGE_DOS_SIGNATURE)
        {
            LogPrintf(_T("Invalid DOS signature.\n"));
            return FALSE;
        }
        PIMAGE_NT_HEADERS pImageHeader = (PIMAGE_NT_HEADERS)((char*)pDosHeader + pDosHeader->e_lfanew);
        if (pImageHeader->Signature != IMAGE_NT_SIGNATURE)
        {
            LogPrintf(_T("Invalid NT signature.\n"));
            return FALSE;
        }
        dwTimeDateStamp = pImageHeader->FileHeader.TimeDateStamp;
        LogPrintf(_T("Time stamp: %08X\n"), dwTimeDateStamp);
    }
    __finally
    {
        if (hModuleSnap != INVALID_HANDLE_VALUE)
        {
            CloseHandle(hModuleSnap);
        }
    }

    return TRUE;
}

BOOL GetCryptTables()
{
    if (pModuleBaseAddr == NULL)
    {
        LogPrintf(_T("GetCryptTables: No module base address\n"));
        return FALSE;
    }

    const char *pSignature1 = "Copyright(c) 2004, Ross-Tech LLC";
    BYTE *pSig1Addr = NULL;
    for (DWORD dwOffset = 0; dwOffset < dwModuleBaseSize; dwOffset += 16)
    {
        BYTE *pAddr = pModuleBaseAddr + dwOffset;
        if (memcmp(pAddr, pSignature1, strlen(pSignature1) - 1) == 0)
        {
            pSig1Addr = pAddr;
            break;
        }
    }
    if (pSig1Addr == NULL)
    {
        LogPrintf(_T("Table1 signature not found\n"));
    }
    else
    {
        BYTE *pTab1Addr = pSig1Addr - 0x00E0;
        LogPrintf(_T("Table1 signature at: %08p, table at: %08p\n"), pSig1Addr, pTab1Addr);

        TCHAR szPath[MAX_PATH];

        if (SUCCEEDED(SHGetFolderPath(NULL,
            CSIDL_PERSONAL | CSIDL_FLAG_CREATE,
            NULL,
            0,
            szPath)))
        {
            if (PathAppend(szPath, CRYPTFILE1))
            {
                FILE *fw = _tfopen(szPath, _T("wb"));
                if (fw != NULL)
                {
                    fwrite(pTab1Addr, 1, 0x0100, fw);
                    fclose(fw);
                    LogPrintf(_T("Table1 stored\n"));
                }
            }
        }
    }

    const DWORD pSignature2[] =
    {
         2,  3,  5,  7, 11, 13, 17, 19,
        23, 29, 31, 37, 41, 43, 47, 53
    };
    BYTE *pSig2Addr = NULL;
    for (DWORD dwOffset = 0; dwOffset < dwModuleBaseSize; dwOffset += 16)
    {
        BYTE *pAddr = pModuleBaseAddr + dwOffset;
        if (memcmp(pAddr, pSignature2, sizeof(pSignature2)) == 0)
        {
            pSig2Addr = pAddr;
            break;
        }
    }
    if (pSig2Addr == NULL)
    {
        LogPrintf(_T("Table2 signature not found\n"));
    }
    else
    {
        BYTE *pTab2Addr = pSig2Addr - 0x0300;
        LogPrintf(_T("Table2 signature at: %08p, table at: %08p\n"), pSig2Addr, pTab2Addr);

        TCHAR szPath[MAX_PATH];

        if (SUCCEEDED(SHGetFolderPath(NULL,
            CSIDL_PERSONAL | CSIDL_FLAG_CREATE,
            NULL,
            0,
            szPath)))
        {
            if (PathAppend(szPath, CRYPTFILE2))
            {
                FILE *fw = _tfopen(szPath, _T("wb"));
                if (fw != NULL)
                {
                    fwrite(pTab2Addr, 1, 0x0300, fw);
                    fclose(fw);
                    LogPrintf(_T("Table2 stored\n"));
                }
            }
        }
    }
    TCHAR szBuffer[100];
    if (LoadString(GetModuleHandle(NULL), 383, szBuffer, sizeof(szBuffer) / sizeof(szBuffer[0])) > 0)
    {
        LogPrintf(_T("Version code: %s\n"), szBuffer);
    }

    return TRUE;
}

void *DisMember(size_t size, ...)
{
    // the pointer can be more complicated than a plain data pointer
    // think of virtual member functions, multiple inheritance, etc
    if (size < sizeof(void *)) return NULL;
    va_list args;
    va_start(args, size);
    void *res = va_arg(args, void *);
    va_end(args);
    return res;
}

class CDecryptTextLine
{
    public:
        typedef void(__thiscall *ptrDecryptTextLine)(void *pthis, void *p1, char *pline, int source);

        void mDecryptTextLine(void *p1, char *pline, int source)
        {
            if (source == 5)
            {
                //LogPrintf(_T("DecryptTextLine in: \"%S\"\n"), pline);
            }
            PatchDecryptTextLine.RestoreOpcodes();

            ptrDecryptTextLine pDecryptTextLine = (ptrDecryptTextLine) (pModuleBaseAddr + DECRYPT_TEXT_LINE_ADDR_REL);
            pDecryptTextLine(this, p1, pline, source);

            PatchDecryptTextLine.WriteOpcodes();
#if false
            if (source == 1)
            {
                if (_stricmp(pline, "101027,00,,,,,00") == 0)
                {
                    LogPrintf(_T("DecryptTextLine patching (%u): \"%S\"\n"), source, pline);
                    strcpy(pline, "101027,01,,,,,00");
                }
            }
#endif
            LogPrintf(_T("DecryptTextLine out (%u): \"%S\"\n"), source, pline);
        }
};

BOOL RedirectDecryptTextLine()
{
    if (pModuleBaseAddr == NULL)
    {
        LogPrintf(_T("RedirectDecryptTextLine: No module base address\n"));
        return FALSE;
    }
    if (dwTimeDateStamp != VALID_TIME_DATE_STAMP)
    {
        LogPrintf(_T("RedirectDecryptTextLine: Time date stamp invalid\n"));
        return FALSE;
    }
    PVOID pDecrypt = DisMember(sizeof(&CDecryptTextLine::mDecryptTextLine), &CDecryptTextLine::mDecryptTextLine);
    PatchDecryptTextLine.SetDetour((PVOID)(pModuleBaseAddr + DECRYPT_TEXT_LINE_ADDR_REL), pDecrypt);
    PatchDecryptTextLine.SetWriteProtection();
    PatchDecryptTextLine.SaveOpcodes();
    PatchDecryptTextLine.WriteOpcodes();
    LogPrintf(_T("DecryptTextLine patched\n"));

    return TRUE;
}

class CReadDecryptedLine
{
public:
    typedef int(__thiscall *ptrReadDecryptedLine)(void *pthis, FILE *pfile, char *ptext, int max_len);

    int mReadDecryptedLine(FILE *pfile, char *ptext, int max_len)
    {
        PatchReadDecryptedLine.RestoreOpcodes();

        ptrReadDecryptedLine pReadDecryptedLine = (ptrReadDecryptedLine)(pModuleBaseAddr + READ_DECRYPTED_LINE_ADDR_REL);
        int result = pReadDecryptedLine(this, pfile, ptext, max_len);

        PatchReadDecryptedLine.WriteOpcodes();
        if (ptext[0] != 0)
        {
            LogPrintf(_T("ReadDecryptedLine out: \"%S\"\n"), ptext);
        }
        return result;
    }
};

BOOL RedirectReadDecryptedLine()
{
    if (pModuleBaseAddr == NULL)
    {
        LogPrintf(_T("RedirectReadDecryptedLine: No module base address\n"));
        return FALSE;
    }
    if (dwTimeDateStamp != VALID_TIME_DATE_STAMP)
    {
        LogPrintf(_T("RedirectReadDecryptedLine: Time date stamp invalid\n"));
        return FALSE;
    }
    PVOID pDecrypt = DisMember(sizeof(&CReadDecryptedLine::mReadDecryptedLine), &CReadDecryptedLine::mReadDecryptedLine);
    PatchReadDecryptedLine.SetDetour((PVOID)(pModuleBaseAddr + READ_DECRYPTED_LINE_ADDR_REL), pDecrypt);
    PatchReadDecryptedLine.SetWriteProtection();
    PatchReadDecryptedLine.SaveOpcodes();
    PatchReadDecryptedLine.WriteOpcodes();
    LogPrintf(_T("ReadDecryptedLine patched\n"));

    return TRUE;
}

HWND WINAPI mFindWindowA(
    _In_opt_ LPCSTR lpClassName,
    _In_opt_ LPCSTR lpWindowName
)
{
    HWND hWnd = FindWindowA(lpClassName, lpWindowName);
    LPCSTR pClassName = lpClassName;
    if (HIWORD(pClassName) == 0)
    {
        pClassName = "-";
    }
    LPCSTR pWindowName = lpWindowName;
    if (pWindowName == NULL)
    {
        pWindowName = "-";
    }
    LogPrintf(_T("FindWindowA %S %S (%08p)\n"), pClassName, pWindowName, hWnd);
    if (hWnd != NULL)
    {
        if (!IsOwnWindowHandle(hWnd))
        {
            LogPrintf(_T("Window suppressed\n"));
            return NULL;
        }
    }
    return hWnd;
}

BOOL CALLBACK EnumWindowsProc(
    __in  HWND hWnd,
    __in  LPARAM lParam
)
{
    ENUMWINDOWS_INFO *pEnumWindowsInfo = (ENUMWINDOWS_INFO *)lParam;
    if (IsOwnWindowHandle(hWnd))
    {
        LogPrintf(_T("HWND accepted: %08p\n"), hWnd);
        return pEnumWindowsInfo->lpEnumFunc(hWnd, pEnumWindowsInfo->lParam);
    }
    return TRUE;
}

BOOL WINAPI mEnumWindows(
    _In_ WNDENUMPROC lpEnumFunc,
    _In_ LPARAM      lParam
)
{
    ENUMWINDOWS_INFO EnumWindowsInfo;
    EnumWindowsInfo.lpEnumFunc = lpEnumFunc;
    EnumWindowsInfo.lParam = lParam;

    LogPrintf(_T("EnumWindows Start %08X\n"), lParam);
    BOOL bResult = EnumWindows(EnumWindowsProc, (LPARAM)&EnumWindowsInfo);
    LogPrintf(_T("EnumWindows End %08X (%u)\n"), lParam, bResult);

    return bResult;
}

LRESULT WINAPI mSendMessageA(
    _In_ HWND   hWnd,
    _In_ UINT   Msg,
    _In_ WPARAM wParam,
    _In_ LPARAM lParam
)
{
    //LogPrintf(_T("SendMessageA %08X, %08X, %08X, %08X\n"), hWnd, Msg, wParam, lParam);
    switch (Msg)
    {
        case LVM_SETITEMTEXTA:
        {
            LPLVITEM plvItem = (LPLVITEM)lParam;
            if (plvItem->pszText != NULL)
            {
                LogPrintf(_T("SetItemTextA (%u, %u): \"%S\"\n"), wParam, plvItem->iSubItem, plvItem->pszText);
            }
            break;
        }
    }
    BOOL bResult = SendMessageA(hWnd, Msg, wParam, lParam);
    return bResult;
}

BOOL WINAPI mIsDebuggerPresent(void)
{
    LogPrintf(_T("IsDebuggerPresent\n"));
    //LogFlush();
    return FALSE;
}

HRSRC WINAPI mFindResourceA(
    _In_opt_ HMODULE hModule,
    _In_     LPCSTR lpName,
    _In_     LPCSTR lpType
)
{
    HRSRC hRes = FindResourceA(hModule, lpName, lpType);

    BOOL bEnableLog = FALSE;
    BOOL bResWatch = FALSE;
    std::string name;
    if (IS_INTRESOURCE(lpName))
    {
        name = string_format("NameID=%u", (DWORD)lpName);
#if false
        if ((DWORD)lpType == (DWORD)RT_STRING)
        {
            switch ((DWORD)lpName)
            {
                case 43:    // Contains resource 680
                    LogPrintf(_T("Access to resource block %u\n"), (DWORD)lpName);
                    bResWatch = TRUE;
                    break;
            }
        }
#endif
    }
    else
    {
        bEnableLog = TRUE;
        bResWatch = TRUE;
        name = lpName;
    }

    std::string type;
    if (IS_INTRESOURCE(lpType))
    {
        type = string_format("TypeID=%u", (DWORD)lpType);
    }
    else
    {
        bEnableLog = TRUE;
        type = lpName;
    }
    if (hRes == NULL)
    {
        //SuspendProcess();
    }

    if (bEnableLog)
    {
        if (hRes != NULL)
        {
            LogPrintf(_T("FindResourceA OK: %S %S (%08p)\n"), name.c_str(), type.c_str(), hRes);
        }
        else
        {
            LogPrintf(_T("FindResourceA Fail: %S %S\n"), name.c_str(), type.c_str());
        }
    }
    if (bResWatch)
    {
        LogPrintf(_T("FindResourceA: Start Reswatch\n"));
        ResWatchList.push_back(hRes);
    }
    return hRes;
}

HGLOBAL WINAPI mLoadResource(
    _In_opt_ HMODULE hModule,
    _In_     HRSRC   hResInfo
)
{
    HGLOBAL hMem = LoadResource(hModule, hResInfo);
    if (hMem != NULL)
    {
        bool found = (std::find(ResWatchList.begin(), ResWatchList.end(), hResInfo) != ResWatchList.end());
        if (found)
        {
            DWORD dwSize = 0;
            std::map<HRSRC, DWORD>::iterator it = ResSizeMap.find(hResInfo);
            if (it != ResSizeMap.end())
            {	// found
                dwSize = it->second;
            }
            LogPrintf(_T("LoadResource OK: %08p (%08p, %u)=\n"), hResInfo, hMem, dwSize);
            if (dwSize > 0)
            {
                LPVOID pMem = LockResource(hMem);
                if (pMem != NULL && dwSize <= 0x200)
                {
                    LogData((BYTE *)pMem, dwSize, 0x200);
                    LogPrintf(_T("\n"));
                    DWORD dwSum = 0;
                    for (DWORD i = 0; i < dwSize; i++)
                    {
                        dwSum += ((BYTE*)pMem)[i];
                    }
                    LogPrintf(_T("Sum: %04X\n"), dwSum);
                }
            }
        }
    }

    return hMem;
}

DWORD WINAPI mSizeofResource(
    _In_opt_ HMODULE hModule,
    _In_     HRSRC   hResInfo
)
{
    DWORD dwSize = SizeofResource(hModule, hResInfo);
    if (dwSize != 0)
    {
        bool found = (std::find(ResWatchList.begin(), ResWatchList.end(), hResInfo) != ResWatchList.end());
        if (found)
        {
            LogPrintf(_T("SizeofResource OK: %08p (%u)\n"), hResInfo, dwSize);
            ResSizeMap[hResInfo] = dwSize;
        }
    }

    return dwSize;
}

HANDLE WINAPI mCreateFileA(
    _In_     LPCSTR                lpFileName,
    _In_     DWORD                 dwDesiredAccess,
    _In_     DWORD                 dwShareMode,
    _In_opt_ LPSECURITY_ATTRIBUTES lpSecurityAttributes,
    _In_     DWORD                 dwCreationDisposition,
    _In_     DWORD                 dwFlagsAndAttributes,
    _In_opt_ HANDLE                hTemplateFile
)
{
    HANDLE hFile = CreateFileA(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
    BOOL bEnableLog = FALSE;
    if ((dwDesiredAccess & GENERIC_READ) != 0)
    {
        if (hFile != INVALID_HANDLE_VALUE)
        {
            bEnableLog = TRUE;
        }
        else
        {
            LogPrintf(_T("CreateFileA ERROR: %S\n"), lpFileName);
        }
    }
    if (bEnableLog)
    {
        LogPrintf(_T("CreateFileA OK: %S %08p\n"), lpFileName, hFile);
        bool bWatchMem = false;
        LPSTR ext = PathFindExtensionA(lpFileName);
        LPSTR name = PathFindFileNameA(lpFileName);
        if (ext != NULL && name != NULL)
        {
            if (!bDecryptPatched)
            {
                bDecryptPatched = true;
                RedirectDecryptTextLine();
                RedirectReadDecryptedLine();
            }
            if (_stricmp(ext, ".rod") == 0)
            {
                if (!bTablesStored)
                {
                    bTablesStored = TRUE;
                    GetCryptTables();
                }
            }
#if true
#if false
            if (_stricmp(ext, ".clb") == 0)
            {
                bWatchMem = true;
            }
#endif
            if (_stricmp(ext, ".rod") == 0)
            {
                if (strlen(name) >= 19)
                {
                    bWatchMem = true;
                }
            }
//            if (_stricmp(name, "IV_EV_ECM20TDI01103L906018DQ_M12.rod") == 0)
//            {
//                bWatchMem = true;
//            }
#else
            if (_stricmp(ext, ".clb") == 0)
            {
                if (strlen(name) >= 19)
                {
                    bWatchMem = true;
                }
            }
#endif
        }
        FileWatchList.push_back(hFile);
        if (bWatchMem)
        {
            LogPrintf(_T("CreateFileA: Start Memwatch\n"));
            FileMemWatchList.push_back(hFile);
        }
        //LogFlush();
    }
    return hFile;
}

BOOL WINAPI mReadFile(
    _In_        HANDLE       hFile,
    _Out_       LPVOID       lpBuffer,
    _In_        DWORD        nNumberOfBytesToRead,
    _Out_opt_   LPDWORD      lpNumberOfBytesRead,
    _Inout_opt_ LPOVERLAPPED lpOverlapped
)
{
    BOOL bResult = ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, lpNumberOfBytesRead, lpOverlapped);
    bool found = std::find(FileWatchList.begin(), FileWatchList.end(), hFile) != FileWatchList.end();
    if (bResult && found)
    {
        if (hLastLogRFile != hFile)
        {
            DWORD readCount = 0;
            if (lpNumberOfBytesRead != NULL)
            {
                readCount = *lpNumberOfBytesRead;
            }
            LogPrintf(_T("ReadFile: %08p (%08p, %u, %u)="), hFile, lpBuffer, nNumberOfBytesToRead, readCount);
            hLastLogRFile = hFile;
        }
        LogData((BYTE *)lpBuffer, nNumberOfBytesToRead);
    }

    return bResult;
}

BOOL WINAPI mWriteFile(
    _In_        HANDLE       hFile,
    _In_        LPCVOID      lpBuffer,
    _In_        DWORD        nNumberOfBytesToWrite,
    _Out_opt_   LPDWORD      lpNumberOfBytesWritten,
    _Inout_opt_ LPOVERLAPPED lpOverlapped
)
{
    BOOL bResult = WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);
    bool found = std::find(FileWatchList.begin(), FileWatchList.end(), hFile) != FileWatchList.end();
    if (bResult && found)
    {
        if (hLastLogWFile != hFile)
        {
            DWORD writeCount = 0;
            if (lpNumberOfBytesWritten != NULL)
            {
                writeCount = *lpNumberOfBytesWritten;
            }
            LogPrintf(_T("WriteFile: %08p (%08p, %u, %u)="), hFile, lpBuffer, nNumberOfBytesToWrite, writeCount);
            hLastLogWFile = hFile;
        }
        LogData((BYTE *) lpBuffer, nNumberOfBytesToWrite);
    }

    return bResult;
}

BOOL WINAPI mCloseHandle(
    _In_ HANDLE hObject
)
{
    bool found = std::find(FileWatchList.begin(), FileWatchList.end(), hObject) != FileWatchList.end();
    if (found)
    {
        LogPrintf(_T("CloseHandle: %08p\n"), hObject);
        FileWatchList.remove(hObject);
        bool foundMem = std::find(FileMemWatchList.begin(), FileMemWatchList.end(), hObject) != FileMemWatchList.end();
        if (foundMem)
        {
            LogPrintf(_T("CloseHandle: Stop Memwatch\n"));
            FileMemWatchList.remove(hObject);
        }
        //LogFlush();
    }
    return CloseHandle(hObject);
}

LPVOID WINAPI mHeapAlloc(
    _In_ HANDLE hHeap,
    _In_ DWORD  dwFlags,
    _In_ SIZE_T dwBytes
)
{
    LPVOID pMem = HeapAlloc(hHeap, dwFlags, dwBytes);
    if (pMem != NULL && FileMemWatchList.size() > 0)
    {
        BOOL bLog = FALSE;
        if (dwBytes >= 1000)
        {
            bLog = TRUE;
        }
        if (bLog)
        {
            LogPrintf(_T("HeapAlloc: %u=%08p\n"), dwBytes, pMem);
        }
//        if (FileMemWatchList.size() > 0 && dwBytes == 7120)
//        {
//            LogPrintf(_T("Start trace here\n"));
//        }
        bool found = (std::find(MemWatchList.begin(), MemWatchList.end(), pMem) != MemWatchList.end());
        if (!found)
        {
            MemWatchList.push_back(pMem);
        }
        else
        {
            if (bLog)
            {
                LogPrintf(_T("HeapAlloc: Existing!\n"));
            }
        }
    }
    return pMem;
}

LPVOID WINAPI mHeapReAlloc(
    _In_ HANDLE hHeap,
    _In_ DWORD  dwFlags,
    _In_ LPVOID lpMem,
    _In_ SIZE_T dwBytes
)
{
    LPVOID pMem = HeapReAlloc(hHeap, dwFlags, lpMem, dwBytes);
    if (pMem != NULL)
    {
        bool found = (std::find(MemWatchList.begin(), MemWatchList.end(), lpMem) != MemWatchList.end());
        if (found)
        {
            LogPrintf(_T("HeapReAlloc: %08p %u=%08p\n"), lpMem, dwBytes, pMem);
            MemWatchList.remove(lpMem);
            MemWatchList.push_back(pMem);
        }
    }
    return pMem;
}

BOOL WINAPI mHeapFree(
    _In_ HANDLE hHeap,
    _In_ DWORD  dwFlags,
    _In_ LPVOID lpMem
)
{
    bool found = (std::find(MemWatchList.begin(), MemWatchList.end(), lpMem) != MemWatchList.end());
    if (found)
    {
        SIZE_T size = HeapSize(hHeap, dwFlags, lpMem);
        LogPrintf(_T("HeapFree: %08p=%u\n"), lpMem, size);
        if (size <= 0x10000)
        {
            LogAsc((BYTE *)lpMem, size, 0x10000);
        }
        MemWatchList.remove(lpMem);
    }
    return HeapFree(hHeap, dwFlags, lpMem);
}

NTSTATUS WINAPI mNtSetInformationThread(
    __in HANDLE ThreadHandle,
    __in THREADINFOCLASS ThreadInformationClass,
    __in_bcount(ThreadInformationLength) PVOID ThreadInformation,
    __in ULONG ThreadInformationLength
)
{
    LogPrintf(_T("NtSetInformationThread: %08p %08X %08p %u\n"), ThreadHandle, ThreadInformationClass, ThreadInformation, ThreadInformationLength);

    if (ThreadInformationClass == ThreadHideFromDebugger && ThreadInformation == NULL && ThreadInformationLength == 0)
    {
        BOOLEAN value = FALSE;
        if (pNtQueryInformationThread(ThreadHandle, ThreadInformationClass, &value, sizeof(value), 0) == STATUS_SUCCESS)
        {
            LogPrintf(_T("NtQueryInformationThread: %u\n"), (unsigned int)value);
        }
        else
        {
            LogPrintf(_T("NtQueryInformationThread failed!\n"));
        }
        PatchDbgUiRemoteBreakin();
        LogPrintf(_T("Override NtSetInformationThread: STATUS_SUCCESS\n"));
        //LogFlush();
        return STATUS_SUCCESS;
    }

    LogPrintf(_T("Redirect to NtSetInformationThread\n"));
    return pNtSetInformationThread(ThreadHandle, ThreadInformationClass, ThreadInformation, ThreadInformationLength);
}
