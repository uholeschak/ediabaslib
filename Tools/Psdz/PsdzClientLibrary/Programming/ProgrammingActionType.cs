using PsdzClient.Core;
using System;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [Flags]
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum ProgrammingActionType
    {
        Programming = 2,
        BootloaderProgramming = 4,
        DataProgramming = 8,
        Coding = 0x10,
        Unmounting = 0x20,
        Mounting = 0x40,
        Replacement = 0x80,
        FscBakup = 0x100,
        FscStore = 0x400,
        FscActivate = 0x800,
        FscDeactivate = 0x1000,
        HddUpdate = 0x2000,
        IdSave = 0x4000,
        IdRestore = 0x8000,
        IbaDeploy = 0x10000,
        SFAWrite = 0x20000,
        SFADelete = 0x40000,
        SFAVerfy = 0x80000,
        HDDUpdateAndroid = 0x100000
    }
}
