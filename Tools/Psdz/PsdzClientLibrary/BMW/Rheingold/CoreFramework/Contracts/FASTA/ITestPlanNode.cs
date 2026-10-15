using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface ITestPlanNode : IProtocolTransaction
    {
        void AddInfoObject(IList<LocalizedText> infoTitle, string infoType, string identifier, string state);

        void AddSymptom(Fault fault);

        void AddFaultPattern(XEP_PERCEIVEDSYMPTOMSEX symptom);
    }
}
