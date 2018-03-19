#pragma once

#include <windows.h>

// HotPatch is a thin wrapper on top of the function hot patching layer.
// It encapsulates a way to replace known functions with own functions by means of patching in jmps in both x86 and x86-64.
// The basic workflow is:
//	1. set an original and a replacement procedure
//	2. make the original code writable
//	3. make a backup of the original
//	4. write the opcodes redirecting to the replacement procedure on top of the original
//	In the replacement procedure:
//		4.1. restore the old opcodes
//		4.2. call the original procedure
//		4.3. write the opcodes again (step 4)
//	When done:
//	5. restore the opcodes (step 4.1)
//	6. restore the protection
// Limitations:
//	not RAII right now
//	won't work with non-virtual class members
class HotPatch
{
public:
	// HotPatch::HotPatch() is the default constructor.
	// It does nothing.
	HotPatch();

	// HotPatch::HotPatch(originalProc, detourProc[, alwaysUse32Bit]) constructs the patch and
	// assigns the appropriate original and replacement procedures.
	// Parameters:
	//	PVOID originalProc	- the function to overwrite.
	//		Remember to call SetWriteProtection() on it because the patch will apply opcodes!
	//	PVOID detourProc	- the replacement function.
	HotPatch(PVOID originalProc, PVOID detourProc, bool alwaysUse32Bit = false);

	// HotPatch::~HotPatch() does nothing.
	virtual ~HotPatch();

	// HotPatch::SetDetour(originalProc, detourProc[, alwaysUse32Bit]) prepares internal state for the patch.
	// Parameters:
	//	PVOID originalProc	- the patched function.
	//	PVOID detourProc	- the patch to use instead.
	//	bool alwaysUse32Bit	- If true, a 32 bit jmp is always inserted.
	//		Else, the best jmp is determined based on necessities (can still be 32 bit if it fits).
	void SetDetour(PVOID originalProc, PVOID detourProc, bool alwaysUse32Bit = false);

	// bool res = HotPatch::SetWriteProtection() makes the code of the patched function writable.
	// Return value:
	// 	bool res	- true on success.
	bool SetWriteProtection();

	// bool res = HotPatch::RestoreProtection() undoes what SetWriteProtection() did.
	// Return value:
	// 	bool res	- true on success.
	bool RestoreProtection();

	// HotPatch::SaveOpcodes() backs up the code within the patched function.
	void SaveOpcodes();

	// HotPatch::RestoreOpcodes() undoes the effects of WriteOpcodes(). Call SaveOpcodes() before!
	void RestoreOpcodes();

	// HotPatch::WriteOpcodes() applies the patch to the patched function. Call SetWriteProtection() and SaveOpcodes() before!
	void WriteOpcodes();

protected:
	PBYTE _originalProc;
	PBYTE _detourProc;
	bool _64bit;
	bool _detoured;
	BYTE _backup[13];
	DWORD _oldProtection;

	static const BYTE _OP_JMP32	= 0xE9;
	static const WORD _OP_JMP64	= 0xFF41;
	static const BYTE _OP_MOVABS = 0x49;
	static const BYTE _R11_WRITE = 0xBB;
	static const BYTE _R11_JMP = 0xE3;

	static const SIZE_T _PATCH_LENGTH32 = 5;		// jmp, detourProc - originalProc = 1 + 4
	static const SIZE_T _PATCH_LENGTH64 = 13;	// movabs, R11, detourProc (64 bit), jmp (abs, 64), R11 = 1 + 1 + 8 + 2 + 1

private:
	void _init();
};
