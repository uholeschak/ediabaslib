using System;

namespace BMW.Rheingold.CoreFramework.Contracts.KMM
{
    [Flags]
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum KmmFlashFlags
    {
        FlashNormal = 1,
        FlashCD = 2,
        FlashDVD = 4,
        FlashMOSTSync = 8,
        FlashMOSTAsync = 0x10,
        FlashMOSTControl = 0x20,
        FlashCAN = 0x40,
        FlashByteFlight = 0x80
    }
}