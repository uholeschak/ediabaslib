using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IPsdzInfo
    {
        bool IsPsdzInitialized { get; }

        bool IsValidPsdzVersion { get; }

        string PsdzDataPath { get; }

        string PsdzVersion { get; }

        [PreserveSource(Hint = "For backward compatibility")]
        string ExpectedPsdzVersion { get; }
    }
}