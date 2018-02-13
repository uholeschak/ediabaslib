/*
Copyright (C) 2004 Jacquelin POTIER <jacquelin.potier@free.fr>
Dynamic aspect ratio code Copyright (C) 2004 Jacquelin POTIER <jacquelin.potier@free.fr>

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
// Object: Generic data and functions used by fake api
//          You don't need to edit this file.
//         It just allows to export function and does all the common work for all faking dll
//-----------------------------------------------------------------------------

#include "GenericFakeAPI.h"

extern STRUCT_FAKE_API pArrayFakeAPI[];
extern STRUCT_FAKE_API_WITH_USERPARAM pArrayBeforeAPICall[];
extern STRUCT_FAKE_API_WITH_USERPARAM pArrayAfterAPICall[];

APIOVERRIDE_EXPORTED_FUNCTIONS* pApiOverrideExportedFunctions = 0;

//-----------------------------------------------------------------------------
// Name: GetFakeAPIArray
// Object: allow calling module to get fake api array pointer
// Parameters :
//     in  : 
//     out : 
//     return : pArrayFakeAPI array
//-----------------------------------------------------------------------------
extern "C" __declspec(dllexport) void* __stdcall GetFakeAPIArray()
{
    return (void*)pArrayFakeAPI;
}

//-----------------------------------------------------------------------------
// Name: GetPreAPICallArray
// Object: allow calling module to get before api call array pointer
// Parameters :
//     in  : 
//     out : 
//     return : pArrayBeforeAPICall array
//-----------------------------------------------------------------------------
extern "C" __declspec(dllexport) void* __stdcall GetPreAPICallArray()
{
    return (void*)pArrayBeforeAPICall;
}

//-----------------------------------------------------------------------------
// Name: GetPostAPICallArray
// Object: allow calling module to get after api call array pointer
// Parameters :
//     in  : 
//     out : 
//     return : pArrayAfterAPICall array
//-----------------------------------------------------------------------------
extern "C" __declspec(dllexport) void* __stdcall GetPostAPICallArray()
{
    return (void*)pArrayAfterAPICall;
}

//-----------------------------------------------------------------------------
// Name: GetFakeAPIEncoding
// Object: allow calling module to know character encoding of pArrayFakeAPI
// Parameters :
//     in  : 
//     out : 
//     return : encoding type
//-----------------------------------------------------------------------------
extern "C" __declspec(dllexport) int __stdcall GetFakeAPIEncoding()
{
#if (defined(UNICODE)||defined(_UNICODE))
    return FakeAPIEncodingUNICODE;
#else
    return FakeAPIEncodingANSI;
#endif
}

//-----------------------------------------------------------------------------
// Name: GetAPIOverrideBuildVersion
// Object: allow calling module to know character with which version of ApiOverrideFramework
//          dll was built
// Parameters :
//     in  : 
//     out : 
//     return : encoding type
//-----------------------------------------------------------------------------
extern "C" __declspec(dllexport) int __stdcall GetAPIOverrideBuildVersion()
{
    return OVERRIDING_DLL_API_OVERRIDE_BUILD_VERSION;
}

//-----------------------------------------------------------------------------
// Name: InitializeFakeDll
// Object: Called at the end of dll loading, before faking and pre/post hook are installed
//         provides apioverride.dll functions to overriding dll writers
//         Provide a way of communication between overriding dlls and plugins
// Parameters :
//     in  : APIOVERRIDE_EXPORTED_FUNCTIONS* pApiOverrideExportedFunc : pointer to struct of functions
//     out : 
//     return : 
//-----------------------------------------------------------------------------
extern "C" __declspec(dllexport) void __stdcall InitializeFakeDll(APIOVERRIDE_EXPORTED_FUNCTIONS* pApiOverrideExportedFunc)
{
    // only store pointer to struct containing functions pointers
    pApiOverrideExportedFunctions = pApiOverrideExportedFunc;
}