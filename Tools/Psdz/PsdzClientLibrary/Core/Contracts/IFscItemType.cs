using System;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFscItemType
    {
        string BuildPurpose { get; set; }

        string DiagnoseAddr { get; set; }

        string EcuData { get; set; }

        IFscType Fsc { get; set; }

        DateTime GenTime { get; set; }

        string Individualization { get; set; }

        ISwIdType SwID { get; set; }
    }
}
