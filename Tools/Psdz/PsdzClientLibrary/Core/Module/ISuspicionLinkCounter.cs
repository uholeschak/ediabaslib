using PsdzClient.Core;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface ISuspicionLinkCounter
    {
        void GetSuspicionLinkCounts(string[] diagnosticObjectNames, out int[] selectedSuspicionLinksCount, out int[] validSuspicionLinksCount, out int[] allSuspicionLinksCount);
    }
}
