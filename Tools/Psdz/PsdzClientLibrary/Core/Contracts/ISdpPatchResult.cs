using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISdpPatchResult
    {
        bool IsSdpPatchAvailable { get; }

        string SwiDataTarget { get; }

        string ILevel { get; }

        int ILevelVersion { get; }

        int ILeveTimeSlot { get; }
    }
}
