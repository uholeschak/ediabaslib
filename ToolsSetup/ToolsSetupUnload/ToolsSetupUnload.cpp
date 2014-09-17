// CarControlUnload.cpp : Definiert den Einstiegspunkt für die Anwendung.
//

#include "stdafx.h"
#include <string>

int WINAPI WinMain(HINSTANCE hInstance,
                   HINSTANCE hPrevInstance,
                   LPTSTR    lpCmdLine,
                   int       nCmdShow)
{
    TCHAR fileName[MAX_PATH];
    if (GetModuleFileName( NULL, fileName, MAX_PATH ))
    {
        std::wstring splitPath = fileName;
        unsigned int found = splitPath.find_last_of(L"/\\");
        std::wstring searchPath = splitPath.substr(0, found);
        std::wstring searchMask = searchPath + L"\\*.unload";

        WIN32_FIND_DATA ffd;
        HANDLE hFind = FindFirstFile(searchMask.c_str(), &ffd);
        if (hFind != INVALID_HANDLE_VALUE)
        {
            std::wstring appMgrPath = L"\\Windows\\AppMgr";
            ::CreateDirectory(appMgrPath.c_str(), NULL);
            do
            {
                if (!(ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
                {
                    std::wstring srcFile = searchPath + L"\\" + ffd.cFileName;
                    std::wstring dstFile = L"\\Windows\\";
                    dstFile += ffd.cFileName;
                    ::CopyFile(srcFile.c_str(), dstFile.c_str(), false);

                    std::wstring fileName = ffd.cFileName;
                    unsigned int dotPos = fileName.find_last_of(L".");
                    std::wstring baseFile = fileName.substr(0, dotPos);

                    srcFile = searchPath + L"\\" + baseFile + L".dat";
                    dstFile = appMgrPath + L"\\" + baseFile + L".dat";
                    ::CopyFile(srcFile.c_str(), dstFile.c_str(), false);

                    srcFile = searchPath + L"\\" + baseFile + L".dll";
                    dstFile = appMgrPath + L"\\" + baseFile + L".dll";
                    ::CopyFile(srcFile.c_str(), dstFile.c_str(), false);
                }
            }
            while (FindNextFile(hFind, &ffd) != 0);
            FindClose(hFind);
        }
    }

    return TRUE;
}
