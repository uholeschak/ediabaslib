using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IProgrammingObjectBuilder
    {
        IAsamJobInputDictionary BuildAsamJobParamDictionary();

        IEcuIdentifier BuildEcuIdentifier(string baseVariant, int diagAddrAsInt);

        ISwtApplicationId BuildSwtApplicationId(int appNo, int upgradeIdx);
    }
}
