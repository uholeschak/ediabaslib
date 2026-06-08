using PsdzClient.Core;

namespace BMW.Rheingold.Psdz
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISfaPerEcuOptions
    {
        TalFilterOptions CategoryAction { get; set; }

        TalFilterOptions SfaWriteAction { get; set; }

        TalFilterOptions SfaDeleteAction { get; set; }
    }
}
