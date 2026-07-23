using PsdzClient.Core;
using System.Collections.Generic;
using PBMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ITherapyPlanApi
    {
        int EscalationStep { get; }

        IList<ITherapyPlanItem> GetItems();

        int ServiceFunctionsCount();

        void DisableTheFaUpdateAction();
    }
}
