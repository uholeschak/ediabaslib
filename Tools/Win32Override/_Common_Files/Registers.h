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
// Object: registers
//-----------------------------------------------------------------------------
#pragma once

#include <windows.h>

#pragma pack(push)
#pragma pack(4)


typedef struct tagRegisters32
{
    DWORD eax;
    DWORD ebx;
    DWORD ecx;
    DWORD edx;
    DWORD esi;
    DWORD edi;
    DWORD efl;
    DWORD es;
    DWORD fs;
    DWORD gs;
    DWORD esp;
    DWORD ebp;
}REGISTERS32,*PREGISTERS32;


// warning must be defined in the same way as in ApiOverrideKernel64.asm
typedef struct tagRegisters64
{
    ULONG64 rax;
    ULONG64 rbx;
    ULONG64 rcx;
    ULONG64 rdx;
    ULONG64 rsi;
    ULONG64 rdi;
    ULONG64 rfl;
    ULONG64 fs;
    ULONG64 gs;
    ULONG64 rsp;
    ULONG64 rbp;
    ULONG64 r8; 
    ULONG64 r9; 
    ULONG64 r10; 
    ULONG64 r11;
    ULONG64 r12; 
    ULONG64 r13; 
    ULONG64 r14; 
    ULONG64 r15;
}REGISTERS64,*PREGISTERS64;

typedef union tagRegistersUnion
{
    REGISTERS32 x86; // must be in first position for cast of PREGISTERS_UNION to PREGISTERS on x86
    REGISTERS64 Amd64;
}REGISTERS_UNION,*PREGISTERS_UNION;

typedef struct tagXmmRegisters
{
    BYTE Xmm0[16];
    BYTE Xmm1[16];
    BYTE Xmm2[16];
    BYTE Xmm3[16];
    BYTE Xmm4[16];
    BYTE Xmm5[16];
    BYTE Xmm6[16];
    BYTE Xmm7[16];
    BYTE Xmm8[16];
    BYTE Xmm9[16];
    BYTE Xmm10[16];
    BYTE Xmm11[16];
    BYTE Xmm12[16];
    BYTE Xmm13[16];
    BYTE Xmm14[16];
    BYTE Xmm15[16];
}XMM_REGISTERS,*PXMM_REGISTERS;

#ifdef _WIN64
typedef REGISTERS_UNION REGISTERS;
typedef PREGISTERS_UNION PREGISTERS;
#else
typedef union tagRegisters // allow to save memory in win32
{
    REGISTERS32 x86;
}REGISTERS,*PREGISTERS;  
#endif

#pragma pack(pop)