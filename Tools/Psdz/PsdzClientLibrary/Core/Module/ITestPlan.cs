using PsdzClient.Core;
using System.Collections.Generic;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface ITestPlan : IProtocolTransaction
    {
        bool IsEmpty { get; }

        void AddFilterList(FaultFilter filter);

        ITestPlanNode CreateAndAddTestPlanNode(decimal id, string sysName, string priority, IList<LocalizedText> diagTitle);

        void SetType(bool isIndividual);

        [PreserveSource(Hint = "Fault", Placeholder = true)]
        void SetSelectedSymptom(PlaceholderType symptomFault);
    }
}
