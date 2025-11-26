using PsdzClient.Core;

namespace BMW.Rheingold.Psdz
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum KmmPlanElementAction : uint
    {
        MountEcu = 8u,
        FlashEcu = 16u,
        UnmountEcu = 32u,
        ReplaceEcu = 64u,
        CodeEcu = 128u,
        ImportFsc = 129u,
        ActivateSwt = 130u,
        DeactivateSwt = 131u
    }
}