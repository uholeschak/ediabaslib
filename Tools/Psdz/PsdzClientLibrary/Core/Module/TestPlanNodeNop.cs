using BMW.Authoring.Vehicle;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;
using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.FASTA.Model
{
    public class TestPlanNodeNop : ITestPlanNode, IProtocolTransaction
    {
        public ActionResult Result { get; set; }

        public void AddInfoObject(IList<LocalizedText> infoTitle, string infoType, string identifier, string state)
        {
            Log.Debug("TestPlanNodeNop.AddInfoObject()", "Not operation executed.");
        }

        public void AddSymptom(Fault fault)
        {
            Log.Debug("TestPlanNodeNop.AddSymptom()", "Not operation executed.");
        }

        public void AddFaultPattern(XEP_PERCEIVEDSYMPTOMSEX symptom)
        {
            Log.Debug("TestPlanNodeNop.AddFaultPattern()", "Not operation executed.");
        }
    }
}
