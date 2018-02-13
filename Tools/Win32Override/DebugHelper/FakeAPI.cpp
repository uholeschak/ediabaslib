#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <stdio.h>
#include <Shlobj.h>
#include <Shlwapi.h>
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

static FILE* OpenLogFile();

static BOOL WINAPI mIsDebuggerPresent(void);

static NTSTATUS WINAPI mNtSetInformationThread(
    __in HANDLE ThreadHandle,
    __in THREADINFOCLASS ThreadInformationClass,
    __in_bcount(ThreadInformationLength) PVOID ThreadInformation,
    __in ULONG ThreadInformationLength
);

static ptrNtSetInformationThread pNtSetInformationThread = NULL;
static ptrNtQueryInformationThread pNtQueryInformationThread = NULL;
static FILE *fLog = NULL;

///////////////////////////////////////////////////////////////////////////////
// fake API array. Redirection are defined here
///////////////////////////////////////////////////////////////////////////////
STRUCT_FAKE_API pArrayFakeAPI[]=
{
    // library name ,function name, function handler, stack size (required to allocate enough stack space), FirstBytesCanExecuteAnywhereSize (optional put to 0 if you don't know it's meaning)
    //                                                stack size= sum(StackSizeOf(ParameterType))           Same as monitoring file keyword (see monitoring file advanced syntax)
    {_T("Kernel32.dll"),_T("IsDebuggerPresent"),(FARPROC)mIsDebuggerPresent,0,0},
    {_T("Ntdll.dll"),_T("NtSetInformationThread"),(FARPROC)mNtSetInformationThread,StackSizeOf(HANDLE)+StackSizeOf(THREADINFOCLASS)+StackSizeOf(PVOID)+StackSizeOf(ULONG),0},
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
                HMODULE hNtdll=GetModuleHandle(_T("ntdll.dll"));
                if (hNtdll != NULL)
                {
                    pNtSetInformationThread=(ptrNtSetInformationThread)GetProcAddress(hNtdll,"NtSetInformationThread");
                    pNtQueryInformationThread=(ptrNtQueryInformationThread)GetProcAddress(hNtdll, "NtQueryInformationThread");
                }
                if (pNtSetInformationThread == NULL || pNtQueryInformationThread == NULL)
                {
                    return FALSE;
                }
                fLog = OpenLogFile();
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
        va_start(args, format);
        _vftprintf(fLog, format, args);
        va_end(args);
    }
}

BOOL WINAPI mIsDebuggerPresent(void)
{
    LogPrintf(_T("IsDebuggerPresent\n"));
    return FALSE;
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
        BYTE value = 0;
        if (pNtQueryInformationThread(ThreadHandle, ThreadInformationClass, &value, sizeof(value), 0) == STATUS_SUCCESS)
        {
            LogPrintf(_T("NtQueryInformationThread: %u\n"), (unsigned int) value);
        }
        else
        {
            LogPrintf(_T("NtQueryInformationThread failed!\n"));
        }
        LogPrintf(_T("Override NtSetInformationThread: STATUS_SUCCESS\n"));
        return STATUS_SUCCESS;
    }

    LogPrintf(_T("Redirect to NtSetInformationThread\n"));
    return pNtSetInformationThread(ThreadHandle, ThreadInformationClass, ThreadInformation, ThreadInformationLength);
}
