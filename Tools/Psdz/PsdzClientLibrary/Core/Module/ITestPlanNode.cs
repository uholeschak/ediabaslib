using BMW.Authoring.Vehicle;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient.Core;
using System.Collections.Generic;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface ITestPlanNode : IProtocolTransaction
    {
        void AddInfoObject(IList<LocalizedText> infoTitle, string infoType, string identifier, string state);

        [PreserveSource(Hint = "Fault", Placeholder = true)]
        void AddSymptom(PlaceholderType fault);

        [PreserveSource(Hint = "XEP_PERCEIVEDSYMPTOMSEX", Placeholder = true)]
        void AddFaultPattern(PlaceholderType symptom);
    }
}
