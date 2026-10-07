using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.CoreFramework.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISfaPerEcuOptions
    {
        TalFilterOptions CategoryAction { get; set; }

        TalFilterOptions SfaWriteAction { get; set; }

        TalFilterOptions SfaDeleteAction { get; set; }
    }
}
