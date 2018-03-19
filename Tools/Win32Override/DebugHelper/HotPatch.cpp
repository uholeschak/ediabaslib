#include "HotPatch.h"

template <typename T>
T abs(T val)
{
	return val < 0 ? -val : val;
}

void HotPatch::_init()
{
	if (!_64bit) return; // we already know not to use 64 bit, so why care anymore?
	LONGLONG jumpDistance =
		reinterpret_cast<LONGLONG>(_detourProc)
		- reinterpret_cast<LONGLONG>(_originalProc)
		- _PATCH_LENGTH32;
	if (abs(jumpDistance) > 0x7FFFFFFF) // the jump is too long to fit a regular 32 bit relative jmp
		_64bit = true;
	else
		_64bit = false;
}

HotPatch::HotPatch()
{
}

HotPatch::HotPatch(PVOID originalProc, PVOID detourProc, bool alwaysUse32Bit/* = false*/) :
	_originalProc(reinterpret_cast<PBYTE>(originalProc)),
	_detourProc(reinterpret_cast<PBYTE>(detourProc)),
	_64bit(!alwaysUse32Bit)
{
	_init();
}

void HotPatch::SetDetour(PVOID originalProc, PVOID detourProc, bool alwaysUse32Bit/* = false*/)
{
	_originalProc = reinterpret_cast<PBYTE>(originalProc);
	_detourProc = reinterpret_cast<PBYTE>(detourProc);
	_64bit = !alwaysUse32Bit;
	_init();
}

HotPatch::~HotPatch()
{
}

bool HotPatch::SetWriteProtection()
{
	return 0 != VirtualProtect(_originalProc, _64bit ? _PATCH_LENGTH64 : _PATCH_LENGTH32, PAGE_EXECUTE_READWRITE, &_oldProtection);
}

bool HotPatch::RestoreProtection()
{
	DWORD unusedOldProtection;
	return 0 != VirtualProtect(_originalProc, _64bit ? _PATCH_LENGTH64 : _PATCH_LENGTH32, _oldProtection, &unusedOldProtection);
}

void HotPatch::SaveOpcodes()
{
	// plain old for is faster; DO NOT call any library functions!
	for (SIZE_T i = 0; i < (_64bit ? _PATCH_LENGTH64 : _PATCH_LENGTH32); ++i)
		*(_backup + i) = *(reinterpret_cast<PBYTE>(_originalProc) + i);
}

void HotPatch::RestoreOpcodes()
{
	// plain old for is faster; DO NOT call any library functions!
	for (SIZE_T i = 0; i < (_64bit ? _PATCH_LENGTH64 : _PATCH_LENGTH32); ++i)
		*(reinterpret_cast<PBYTE>(_originalProc) + i) = *(_backup + i);
}

void HotPatch::WriteOpcodes()
{
	if (_64bit) {
		// movabs
		*_originalProc = _OP_MOVABS;
		// r11
		*(_originalProc + 1) = _R11_WRITE;
		// _detourProc
		*reinterpret_cast<PLONGLONG>(_originalProc + 2) = reinterpret_cast<LONGLONG>(_detourProc);
		// jmp
		*reinterpret_cast<PWORD>(_originalProc + 10) = _OP_JMP64;
		// abs r11
		*(_originalProc + 12) = _R11_JMP;
	}
	else {
		// jmp
		*_originalProc = _OP_JMP32;
		// distance left to _detourProc
		*reinterpret_cast<PDWORD>(_originalProc + 1) = static_cast<DWORD>(_detourProc - _originalProc - static_cast<DWORD>(_PATCH_LENGTH32));
	}
}
