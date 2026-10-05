using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwIdType
    {
        string ApplicationNo { get; set; }

        string UpgradeIndex { get; set; }
    }
}
