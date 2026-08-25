using BMW.Authoring.Vehicle;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
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

        [PreserveSource(Hint = "Fault", Placeholder = true)]
        public void AddSymptom(PlaceholderType fault)
        {
            Log.Debug("TestPlanNodeNop.AddSymptom()", "Not operation executed.");
        }

        [PreserveSource(Hint = "XEP_PERCEIVEDSYMPTOMSEX", Placeholder = true)]
        public void AddFaultPattern(PlaceholderType symptom)
        {
            Log.Debug("TestPlanNodeNop.AddFaultPattern()", "Not operation executed.");
        }
    }
}
