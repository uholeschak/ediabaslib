// CarControlSetupDll.cpp : Definiert den Einstiegspunkt für die DLL-Anwendung.
//

#include "stdafx.h"
#include "ce_setup.h"

#include <string>

const std::wstring UNLOAD_FILE_NAME_BASE = L"Ulrich Holeschak CarControl";
const std::wstring DESTINATION_PATH = L"\\UM\\Program Files\\CarControl\\";

const std::wstring APP_MGR_PATH = L"\\Windows\\AppMgr\\";
const std::wstring UNLOAD_FILE_NAME = UNLOAD_FILE_NAME_BASE + L".unload";
const std::wstring ORIGINAL_UNLOAD_FILE_PATH = L"\\Windows\\" + UNLOAD_FILE_NAME;
const std::wstring ORIGINAL_APP_MGR_DAT_FILE_PATH = APP_MGR_PATH + UNLOAD_FILE_NAME_BASE + L".dat";
const std::wstring ORIGINAL_APP_MGR_DLL_FILE_PATH = APP_MGR_PATH + UNLOAD_FILE_NAME_BASE + L".dll";
const std::wstring DESTINATION_UNLOAD_FILE_PATH = DESTINATION_PATH + UNLOAD_FILE_NAME;
const std::wstring DESTINATION_APP_MGR_DAT_FILE_PATH = DESTINATION_PATH + UNLOAD_FILE_NAME_BASE + L".dat";
const std::wstring DESTINATION_APP_MGR_DLL_FILE_PATH = DESTINATION_PATH + UNLOAD_FILE_NAME_BASE + L".dll";

const std::wstring INSTALL_COMPLETE_CAPTION = L"CarControl";
const std::wstring INSTALL_COMPLETE_TEXT = L"CarControl application was successfully installed!";

extern "C" BOOL APIENTRY DllMain( HANDLE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    return TRUE;
}

extern "C" codeINSTALL_INIT Install_Init(HWND hwndParent, BOOL fFirstCall, BOOL fPreviouslyInstalled, LPCTSTR pszInstallDir)
{
    return codeINSTALL_INIT_CONTINUE;
}

extern "C" codeINSTALL_EXIT Install_Exit(HWND hwndParent, LPCTSTR pszInstallDir, WORD cFailedDirs, WORD cFailedFiles,
                                         WORD cFailedRegKeys, WORD cFailedRegVals, WORD cFailedShortcuts)
{
    ::CopyFile(ORIGINAL_UNLOAD_FILE_PATH.c_str(), DESTINATION_UNLOAD_FILE_PATH.c_str(), false);
    ::CopyFile(ORIGINAL_APP_MGR_DAT_FILE_PATH.c_str(), DESTINATION_APP_MGR_DAT_FILE_PATH.c_str(), false);
    ::CopyFile(ORIGINAL_APP_MGR_DLL_FILE_PATH.c_str(), DESTINATION_APP_MGR_DLL_FILE_PATH.c_str(), false);
    //MessageBox(hwndParent, INSTALL_COMPLETE_TEXT.c_str(), INSTALL_COMPLETE_CAPTION.c_str(), MB_OK);
    return codeINSTALL_EXIT_DONE;
}

extern "C" codeUNINSTALL_INIT Uninstall_Init(HWND hwndParent, LPCTSTR pszInstallDir)
{
    //MessageBox(hwndParent, DESTINATION_UNLOAD_FILE_PATH.c_str(), INSTALL_COMPLETE_CAPTION.c_str(), MB_OK);
    ::DeleteFile(DESTINATION_UNLOAD_FILE_PATH.c_str());
    ::DeleteFile(DESTINATION_APP_MGR_DAT_FILE_PATH.c_str());
    ::DeleteFile(DESTINATION_APP_MGR_DLL_FILE_PATH.c_str());
    return codeUNINSTALL_INIT_CONTINUE;
}

extern "C" codeUNINSTALL_EXIT Uninstall_Exit(HWND hwndParent)
{
    return codeUNINSTALL_EXIT_DONE;
}
