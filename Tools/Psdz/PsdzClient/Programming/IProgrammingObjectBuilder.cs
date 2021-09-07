using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IProgrammingObjectBuilder
    {
        IAsamJobInputDictionary BuildAsamJobParamDictionary();

        IEcuIdentifier BuildEcuIdentifier(string baseVariant, int diagAddrAsInt);

        ISwtApplicationId BuildSwtApplicationId(int appNo, int upgradeIdx);
    }
}
