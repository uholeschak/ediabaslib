using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwIdType
    {
        string ApplicationNo { get; set; }

        string UpgradeIndex { get; set; }
    }
}
