    
    

#pragma once

#include <windows.h>

// HotPatch is a thin wrapper on top of the function hot patching layer.
// It encapsulates a way to replace known functions with own functions by means of patching in jmps in both x86 and x86-64.
// The basic workflow is:
//    1. Declare HotPatch::function<ftype> thePatch; assign target to this object with thePatch = target
//    2. set a replacement callback with thePatch.SetPatch(myTarget)
//    3. call thePatch.Apply() to replace the target with the other callback
//    In the replacement procedure:
//        3.1. do your stuff
//        3.2. call  thePatch(arguments)
//        3.3. do other stuff
//    When done:
//    4. restore the opcodes with thePatch.RemovePatch()
//    5. the destructor or assigning a new function to thePatch will restore the protection status of the memory referred by target
// Limitations:
//    won't work with variadic functions (blame C)
namespace HotPatch {
    class Exception
    {
    };

    class MemoryException : public Exception
    {
    };

    class PatchException : public Exception
    {
    };

    template <typename FType>
    class function_impl
    {
    public:
        function_impl() :
            _pFun(NULL),
            _pPatch(NULL),
            _detoured(false)
        {
        }

        ~function_impl()
        {
            _RestoreProtection();
        }

        // res = function_impl<FType>::IsPatched() tells whether a call to the target will run the target or the hook.
        // Return value:
        //    bool res    - true if ApplyPatch() has been called without an accompanying RemovePatch().
        bool IsPatched()
        {
            return _detoured;
        }

        // function_impl<FType>::operator=(pFun) directs the patch to a target and prepares the target memory for writes.
        // Parameters:
        //    FType *pFun    - the function to patch (see Apply()).
        // Throws:
        //    MemoryException - when changing the protection of the page where pFun points is impossible.
        void operator=(FType *pFun)
        {
            _RestoreProtection();
            _pFun = pFun;
            if (NULL == _pFun) return;
            BOOL res = VirtualProtect(_pFun, _64bit ? _PATCH_LENGTH64 : _PATCH_LENGTH32, PAGE_EXECUTE_READWRITE, &_oldProtection);
            if (!res) throw MemoryException();
        }

        // function_impl<FType>::SetPatch(patch[, alwaysUse32Bit]) prepares internal state for the patch.
        // Parameters:
        //    FType *patch        - the patch to use instead of pFun (see operator=(FType *)).
        //    bool alwaysUse32Bit    - If true, a 32 bit jmp is always inserted.
        //        Else, the best jmp is determined based on necessities (can still be 32 bit if it fits).
        // Throws:
        //    PatchException        - if patch is NULL.
        void SetPatch(FType *patch, bool alwaysUse32Bit = false)
        {
            if (NULL == patch)
                throw PatchException();
            if (NULL != _pPatch)
                RemovePatch();
            _pPatch = patch;

            _64bit = !alwaysUse32Bit;
            // they haven't expressed mandatory 32 bit only path; try to guess the best path
            if (_64bit) {
                LONGLONG jumpDistance =
                    reinterpret_cast<LONGLONG>(_pPatch) -
                    reinterpret_cast<LONGLONG>(_pFun) -
                    _PATCH_LENGTH32;
                if (abs(jumpDistance) > 0x7FFFFFFF) // the jump is too long to fit a regular 32 bit relative jmp
                    _64bit = true;
                else
                    _64bit = false;
            }

            // save the old patch opcodes
            // plain old for is faster; DO NOT call any library functions!
            for (SIZE_T i = 0; i < (_64bit ? _PATCH_LENGTH64 : _PATCH_LENGTH32); ++i)
                *(_backup + i) = *(reinterpret_cast<PBYTE>(_pFun) + i);
        }

        // function_impl<FType>::ApplyPatch() makes pFun (see operator=(FType *)) jmp to patch (see SetPatch(FType *, bool)).
        // This is the actual hot patch mechanism at work.
        void ApplyPatch()
        {
            if (_64bit) {
                // movabs
                *reinterpret_cast<PBYTE>(_pFun) = _OP_MOVABS;
                // r11
                *(reinterpret_cast<PBYTE>(_pFun) + 1) = _R11_WRITE;
                // _detourProc
                *reinterpret_cast<PLONGLONG>(reinterpret_cast<PBYTE>(_pFun) + 2) = reinterpret_cast<LONGLONG>(_pPatch);
                // jmp
                *reinterpret_cast<PWORD>(reinterpret_cast<PBYTE>(_pFun) + 10) = _OP_JMP64;
                // abs r11
                *(reinterpret_cast<PBYTE>(_pFun) + 12) = _R11_JMP;
            }
            else {
                // jmp
                *reinterpret_cast<PBYTE>(_pFun) = _OP_JMP32;
                // distance left to _detourProc
                *reinterpret_cast<PDWORD>(reinterpret_cast<PBYTE>(_pFun) + 1) = static_cast<DWORD>(
                    reinterpret_cast<PBYTE>(_pPatch) -
                    reinterpret_cast<PBYTE>(_pFun) -
                    static_cast<DWORD>(_PATCH_LENGTH32));
            }
            _detoured = true;
        }

        // function_impl<FType>::RemovePatch() undoes what ApplyPatch() did. pFun (see operator=(FType *)) will be its old self again.
        void RemovePatch()
        {
            // plain old for is faster; DO NOT call any library functions!
            for (SIZE_T i = 0; i < (_64bit ? _PATCH_LENGTH64 : _PATCH_LENGTH32); ++i)
                *(reinterpret_cast<PBYTE>(_pFun) + i) = *(_backup + i);
            _detoured = false;
        }

    protected:
        FType *_pFun;
        FType *_pPatch;

        bool _64bit;
        bool _detoured;
        BYTE _backup[13];
        DWORD _oldProtection;

        static const BYTE _OP_JMP32    = 0xE9;
        static const WORD _OP_JMP64    = 0xFF41;
        static const BYTE _OP_MOVABS = 0x49;
        static const BYTE _R11_WRITE = 0xBB;
        static const BYTE _R11_JMP = 0xE3;

        static const SIZE_T _PATCH_LENGTH32 = 5;    // jmp, detourProc - originalProc = 1 + 4
        static const SIZE_T _PATCH_LENGTH64 = 13;    // movabs, R11, detourProc (64 bit), jmp (abs, 64), R11 = 1 + 1 + 8 + 2 + 1

        template <typename T>
        static T abs(T val)
        {
            return val > 0 ? val : -val;
        }

        void _RestoreProtection()
        {
            if (NULL == _pFun) return;
            DWORD unusedOldProtection;
            BOOL res = VirtualProtect(_pFun, _64bit ? _PATCH_LENGTH64 : _PATCH_LENGTH32, _oldProtection, &unusedOldProtection);
            (void) res; // nothing to do
        }

        template <typename FType>
        class _NativeCallGuard {
        public:
            _NativeCallGuard(function_impl<FType> &fun) : _fun(fun) {
                _fun.RemovePatch();
            }

            ~_NativeCallGuard() {
                _fun.ApplyPatch();
            }

        private:
            function_impl<FType> &_fun;
        };
    };

    template <typename>
    class function;

#define HP_TARG0
#define HP_TARG1 , typename Arg1
#define HP_TARG2 HP_TARG1, typename Arg2
#define HP_TARG3 HP_TARG2, typename Arg3
#define HP_TARG4 HP_TARG3, typename Arg4
#define HP_TARG5 HP_TARG4, typename Arg5
#define HP_TARG6 HP_TARG5, typename Arg6
#define HP_TARG7 HP_TARG6, typename Arg7
#define HP_TARG8 HP_TARG7, typename Arg8
#define HP_TARG9 HP_TARG8, typename Arg9
#define HP_TARG10 HP_TARG9, typename Arg10
#define HP_FARG0
#define HP_FARG1 Arg1
#define HP_FARG2 HP_FARG1, Arg2
#define HP_FARG3 HP_FARG2, Arg3
#define HP_FARG4 HP_FARG3, Arg4
#define HP_FARG5 HP_FARG4, Arg5
#define HP_FARG6 HP_FARG5, Arg6
#define HP_FARG7 HP_FARG6, Arg7
#define HP_FARG8 HP_FARG7, Arg8
#define HP_FARG9 HP_FARG8, Arg9
#define HP_FARG10 HP_FARG9, Arg10
#define HP_ARG0
#define HP_ARG1 arg1
#define HP_ARG2 HP_ARG1, arg2
#define HP_ARG3 HP_ARG2, arg3
#define HP_ARG4 HP_ARG3, arg4
#define HP_ARG5 HP_ARG4, arg5
#define HP_ARG6 HP_ARG5, arg6
#define HP_ARG7 HP_ARG6, arg7
#define HP_ARG8 HP_ARG7, arg8
#define HP_ARG9 HP_ARG8, arg9
#define HP_ARG10 HP_ARG9, arg10
#define HP_ARG_DECL0
#define HP_ARG_DECL1 Arg1 arg1
#define HP_ARG_DECL2 HP_ARG_DECL1, Arg2 arg2
#define HP_ARG_DECL3 HP_ARG_DECL2, Arg3 arg3
#define HP_ARG_DECL4 HP_ARG_DECL3, Arg4 arg4
#define HP_ARG_DECL5 HP_ARG_DECL4, Arg5 arg5
#define HP_ARG_DECL6 HP_ARG_DECL5, Arg6 arg6
#define HP_ARG_DECL7 HP_ARG_DECL6, Arg7 arg7
#define HP_ARG_DECL8 HP_ARG_DECL7, Arg8 arg8
#define HP_ARG_DECL9 HP_ARG_DECL8, Arg9 arg9
#define HP_ARG_DECL10 HP_ARG_DECL9, Arg10 arg10

// template partial specialization for function<return_type([arg_types])>
#define HP_RET_FUNCTION(n, callconv)\
    template <typename _Ret HP_TARG##n>\
    class function<_Ret callconv(HP_FARG##n)> : public function_impl<_Ret callconv(HP_FARG##n)>\
    {\
    private:\
        typedef _Ret callconv type(HP_FARG##n);\
        \
    public:\
        ~function<type>()\
        {\
            _RestoreProtection();\
        }\
        \
        using function_impl<type>::operator=;\
        \
        _Ret operator()(HP_ARG_DECL##n)\
        {\
            _NativeCallGuard<type> CallGuard(*this);\
            return (*_pFun)(HP_ARG##n);\
        }\
        \
    protected:\
        using function_impl<type>::_pFun;\
    }

    // declare the 11 templates handling functions in the form:
    // _Ret function()
    // _Ret function(Arg1)
    // _Ret function(Arg1, Arg2)
    // ...
    // _Ret function(Arg1, Arg2, ... Arg10)
    HP_RET_FUNCTION(0, __cdecl);
    HP_RET_FUNCTION(1, __cdecl);
    HP_RET_FUNCTION(2, __cdecl);
    HP_RET_FUNCTION(3, __cdecl);
    HP_RET_FUNCTION(4, __cdecl);
    HP_RET_FUNCTION(5, __cdecl);
    HP_RET_FUNCTION(6, __cdecl);
    HP_RET_FUNCTION(7, __cdecl);
    HP_RET_FUNCTION(8, __cdecl);
    HP_RET_FUNCTION(9, __cdecl);
    HP_RET_FUNCTION(10, __cdecl);

#ifndef _M_X64
    HP_RET_FUNCTION(0, __stdcall);
    HP_RET_FUNCTION(1, __stdcall);
    HP_RET_FUNCTION(2, __stdcall);
    HP_RET_FUNCTION(3, __stdcall);
    HP_RET_FUNCTION(4, __stdcall);
    HP_RET_FUNCTION(5, __stdcall);
    HP_RET_FUNCTION(6, __stdcall);
    HP_RET_FUNCTION(7, __stdcall);
    HP_RET_FUNCTION(8, __stdcall);
    HP_RET_FUNCTION(9, __stdcall);
    HP_RET_FUNCTION(10, __stdcall);
#endif
} // namespace HotPatch