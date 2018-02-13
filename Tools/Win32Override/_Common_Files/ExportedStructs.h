/*
Copyright (C) 2004 Jacquelin POTIER <jacquelin.potier@free.fr>
Dynamic aspect ratio code Copyright (C) 2004 Jacquelin POTIER <jacquelin.potier@free.fr>
originally based from APISpy32 v2.1 from Yariv Kaplan @ WWW.INTERNALS.COM

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

//-----------------------------------------------------------------------------
// Object: exported structs (needed for overriding api)
//-----------------------------------------------------------------------------
#pragma once

#include <windows.h>
#include "registers.h"

// assume that structs share between WinAPIOverride and the FakeAPI dll
// will have the same alignment
#pragma pack(push)
#pragma pack(4)

typedef struct tagPrePostApiCallHookInfos
{
    PBYTE Rbp;
    PBYTE ReturnAddress;
    HMODULE CallingModuleHandle;
    BOOL  OverridingModulesFiltersSuccessfullyChecked;
}PRE_POST_API_CALL_HOOK_INFOS,*PPRE_POST_API_CALL_HOOK_INFOS;
#pragma pack(pop)


#ifdef _WIN64
// StackParameters : adjusted stack parameters (no shadow for x64, only parameters passed through stack)
typedef BOOL (__stdcall *pfPreApiCallCallBack)(PBYTE StackParameters,REGISTERS* pBeforeCallRegisters,XMM_REGISTERS* pBeforeCallXmmRegisters,PRE_POST_API_CALL_HOOK_INFOS* pHookInfos,PVOID UserParam);// return FALSE to stop pre api call chain functions
// StackParameters : adjusted stack parameters (no shadow for x64, only parameters passed through stack)
typedef BOOL (__stdcall *pfPostApiCallCallBack)(PBYTE StackParameters,REGISTERS* pBeforeCallRegisters,XMM_REGISTERS* pBeforeCallXmmRegisters,REGISTERS* pAfterCallRegisters,XMM_REGISTERS* pAfterCallXmmRegisters,PRE_POST_API_CALL_HOOK_INFOS* pHookInfos,PVOID UserParam);// return FALSE to stop calling post api call chain functions
#else
typedef BOOL (__stdcall *pfPreApiCallCallBack)(PBYTE StackParameters,REGISTERS* pBeforeCallRegisters,PRE_POST_API_CALL_HOOK_INFOS* pHookInfos,PVOID UserParam);// return FALSE to stop pre api call chain functions
typedef BOOL (__stdcall *pfPostApiCallCallBack)(PBYTE StackParameters,REGISTERS* pBeforeCallRegisters,REGISTERS* pAfterCallRegisters,PRE_POST_API_CALL_HOOK_INFOS* pHookInfos,PVOID UserParam);// return FALSE to stop calling post api call chain functions
#endif

typedef BOOL (__stdcall *pfCOMObjectCreationCallBack)(CLSID* pClsid,IID* pIid,PVOID pInterface,PRE_POST_API_CALL_HOOK_INFOS* pHookInfos);// return FALSE to stop calling post api call chain functions
typedef BOOL (__stdcall *pfCOMObjectDeletionCallBack)(CLSID* pClsid,PVOID pIUnknownInterface,PVOID pInterfaceReturnedAtObjectCreation,PRE_POST_API_CALL_HOOK_INFOS* pHookInfos);// return FALSE to stop calling post api call chain functions
