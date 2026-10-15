using System.Collections.Generic;

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
