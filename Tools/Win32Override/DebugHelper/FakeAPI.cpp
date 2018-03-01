#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <stdio.h>
#include <Shlobj.h>
#include <Shlwapi.h>
#include <tlhelp32.h>
#include <list>
#include "../_Common_Files/GenericFakeAPI.h"
// You just need to edit this file to add new fake api 
// WARNING YOUR FAKE API MUST HAVE THE SAME PARAMETERS AND CALLING CONVENTION AS THE REAL ONE,
//                  ELSE YOU WILL GET STACK ERRORS

///////////////////////////////////////////////////////////////////////////////
// fake API prototype MUST HAVE THE SAME PARAMETERS 
// for the same calling convention see MSDN : 
// "Using a Microsoft modifier such as __cdecl on a data declaration is an outdated practice"
///////////////////////////////////////////////////////////////////////////////

#define LOGFILE _T("DebugHelper.txt")

#define STATUS_SUCCESS                   ((NTSTATUS)0x00000000L)    // ntsubauth

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

static FILE* OpenLogFile();
static void LogPrintf(TCHAR *format, ...);
static void LogFlush();
static void LogData(BYTE *data, unsigned int length, unsigned int max_length = 0x100);
static void LogAsc(BYTE *data, unsigned int length, unsigned int max_length = 0x100);
static BOOL PatchDbgUiRemoteBreakin();
static BOOL GetCryptTables();

static BOOL WINAPI mIsDebuggerPresent(void);

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
static std::list<HANDLE> FileWatchList;
static std::list<HANDLE> FileMemWatchList;
static std::list<LPVOID> MemWatchList;
static HANDLE hLastLogRFile = INVALID_HANDLE_VALUE;
static HANDLE hLastLogWFile = INVALID_HANDLE_VALUE;
static BOOL bHalted = FALSE;
static BOOL bTablesStored = FALSE;

///////////////////////////////////////////////////////////////////////////////
// fake API array. Redirection are defined here
///////////////////////////////////////////////////////////////////////////////
STRUCT_FAKE_API pArrayFakeAPI[]=
{
    // library name ,function name, function handler, stack size (required to allocate enough stack space), FirstBytesCanExecuteAnywhereSize (optional put to 0 if you don't know it's meaning)
    //                                                stack size= sum(StackSizeOf(ParameterType))           Same as monitoring file keyword (see monitoring file advanced syntax)
    {_T("Kernel32.dll"),_T("IsDebuggerPresent"),(FARPROC)mIsDebuggerPresent,0,0},
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

BOOL GetCryptTables()
{
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
            LPWSTR ext = PathFindExtensionW(me32.szExePath);
            if (ext != NULL)
            {
                if (_wcsicmp(ext, L".exe") == 0)
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

        BYTE *pSig1Addr = NULL;
        const char *pSignature1 = "Copyright(c) 2004, Ross - Tech LLC";
        for (DWORD dwOffset = 0; dwOffset < me32.modBaseSize; dwOffset += 16)
        {
            BYTE *pAddr = me32.modBaseAddr + dwOffset;
            if (memcmp(pAddr, pSignature1, 10/*strlen(pSignature1) - 1*/) == 0)
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
        }
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

BOOL WINAPI mIsDebuggerPresent(void)
{
    LogPrintf(_T("IsDebuggerPresent\n"));
    //LogFlush();
    return FALSE;
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
    if ((dwDesiredAccess & GENERIC_READ) != 0 && hFile != INVALID_HANDLE_VALUE)
    {
        bEnableLog = TRUE;
    }
    if (bEnableLog)
    {
        LogPrintf(_T("CreateFileA OK: %S %08p\n"), lpFileName, hFile);
        bool bWatchMem = false;
        LPSTR ext = PathFindExtensionA(lpFileName);
        LPSTR name = PathFindFileNameA(lpFileName);
        if (ext != NULL && name != NULL)
        {
            if (_stricmp(ext, ".rod") == 0)
            {
                if (!bTablesStored)
                {
                    bTablesStored = TRUE;
                    GetCryptTables();
                }
            }
#if false
#if false
            if (_stricmp(ext, ".clb") == 0)
            {
                bWatchMem = true;
            }
#endif
            if (_stricmp(ext, ".rod") == 0)
            {
#if false
                if (!bHalted)
                {
                    bHalted = true;
                    LogPrintf(_T("Suspending process\n"));
                    LogPrintf(_T("Resume it with PSSuspend -r <Process ID>\n"));
                    LogFlush();
                    pNtSuspendProcess(GetCurrentProcess());
                }
#endif
                bWatchMem = true;
            }
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
