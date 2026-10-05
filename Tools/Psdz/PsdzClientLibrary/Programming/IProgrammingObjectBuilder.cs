using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient.Core;
using PsdzClient.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IProgrammingObjectBuilder
    {
        IAsamJobInputDictionary BuildAsamJobParamDictionary();

        IEcuIdentifier BuildEcuIdentifier(string baseVariant, int diagAddrAsInt);

        ISwtApplicationId BuildSwtApplicationId(int appNo, int upgradeIdx);
    }
}
