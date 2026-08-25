using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient;
using PsdzClient.Core;
using System.Collections.Generic;
using BMW.Rheingold.FASTA.Model;

namespace BMW.Rheingold.FASTA
{
    public class TestPlanNop : ITestPlan, IProtocolTransaction
    {
        public ActionResult Result { get; set; }

        public bool IsEmpty => true;

        public void SetType(bool isIndividual)
        {
        }

        public void AddFilterList(FaultFilter filter)
        {
            Log.Debug("TestPlanNop.AddFilterList()", "Not operation executed.");
        }

        public ITestPlanNode CreateAndAddTestPlanNode(decimal id, string sysName, string priority, IList<LocalizedText> diagTitle)
        {
            Log.Debug("TestPlanNop.CreateAndAddTestPlanNode()", "Not operation executed.");
            return new TestPlanNodeNop();
        }

        [PreserveSource(Hint = "Fault", Placeholder = true)]
        public void SetSelectedSymptom(PlaceholderType symptomFault)
        {
            Log.Debug(Log.CurrentMethod(), "No operation executed.");
        }
    }
}
