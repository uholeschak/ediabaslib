using PsdzClient.Contracts;
using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface ISWTProcessor
    {
        bool CheckSWTCode(string applicationNo, string upgradeIndex);

        void GetSWTCodes(out IList<IFSCProvided> swtCodes);

        IList<IFSCProvided> GetSWTCodes(string type);
    }
}
