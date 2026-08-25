using BMW.ISPI.IstaServices.Contract.PUK.Data;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.FASTA.Model
{
    internal class ActionNop<T> : IAction<T>, IJournalizeManager, IProtocolTransaction
    {
        [PreserveSource(Hint = "BMW.Rheingold.CoreFramework.Contracts.FASTA.ITherapyPlan", Placeholder = true)]
        public PlaceholderType TherapyPlan { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public T SpecialAction { get; set; }

        public ActionResult Result { get; set; }

        public ITestPlan TestPlan => new TestPlanNop();

        public IEnumerable<Warning> Warnings { get; set; }

        public void HandOverData()
        {
            Log.Debug("ActionNop.HandOverResult()", "No operation executed.");
        }

        public void AddFaultPatterns(IEnumerable<PerceivedSymptomWithSource> symptomsWithSource)
        {
            Log.Debug("ActionNop.AddFaultPatterns()", "Not operation executed.");
        }

        public void AddFaultPatternsFromPUK(IEnumerable<FastaPukVfc> pukVfcs)
        {
            Log.Debug("ActionNop.AddFaultPatternsFromPUK()", "Not operation executed.");
        }

        public void AddFaultMemory(Vehicle vehicle)
        {
            Log.Debug("ActionNop.AddFaultMemory()", "Not operation executed.");
        }

        public void AddFaultMemoryAndServiceFault(Vehicle vehicle)
        {
            Log.Debug("ActionNop.AddFaultMemoryAndServiceFault()", "Not operation executed.");
        }

        public void AddCheckControlMessages(Vehicle vehicle)
        {
            Log.Debug("ActionNop.AddCheckControlMessages()", "Not operation executed.");
        }

        public ITestPlanNode CreateAndAddTestPlanNode(decimal id, string sysName, IList<LocalizedText> diagnoseTitle, string priority)
        {
            Log.Debug("ActionNop.CreateAndAddTestPlanNode()", "Not operation executed.");
            return new TestPlanNodeNop();
        }

        [PreserveSource(Hint = "BMW.Rheingold.CoreFramework.Contracts.FASTA.ITherapyPlan", Placeholder = true)]
        public PlaceholderType CreateTherapyPlanCalculation(string id)
        {
            //[-] TherapyPlan = new TherapyPlanNop();
            //[-] TherapyPlan.Init(id, stateType, type, therapyPlan.CurrentContextId, null, therapyPlan.TargetContextId, null);
            //[-] return TherapyPlan;
            //[+] throw new NotImplementedException();
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "BMW.Rheingold.CoreFramework.Contracts.FASTA.ITherapyPlan", Placeholder = true)]
        public void AddTherapyPlanCalculationAction(PlaceholderType entry)
        {
            Log.Debug("ActionNop.CreateAndAddTherapyPlanCalculationAction()", "Not operation executed.");
        }

        public void FillTargetContext(string istufeTarget, IEnumerable<IEcuObj> ecus, BMW.Rheingold.CoreFramework.Contracts.Programming.IFa vehicleOrder, string ereihe, BNType boardnetType = BNType.BN2020)
        {
            Log.Debug("ActionNop.FillTargetContext()", "Not operation executed.");
        }

        public bool HasEcuCommunication(IAction<IEcuCommunication> ecuComToCompare)
        {
            Log.Debug("ActionNop.HasEcuCommunication()", "Not operation executed.");
            return false;
        }

        public void FillCurrentContext(DateTime endTime, Vehicle vehicle, IEnumerable<IEcuJob> ecusJobs, IEnumerable<IEcuObj> psdzEcus, IFFMDynamicResolver ffmDynamicResolver, IDictionary<IEcuIdentifier, IObdData> obdDataMap)
        {
            Log.Debug("ActionNop.FillCurrentContext()", "Not operation executed.");
        }

        public void AddTo(ICollection<object> actionList)
        {
            Log.Debug("ActionNop.AddTo()", "Not operation executed.");
        }

        public void FillCurrentContext(DateTime endTime, Vehicle vehicle, IEnumerable<IEcuObj> ecus)
        {
            Log.Debug("ActionNop.FillCurrentContext()", "Not operation executed.");
        }

        public IBoolResultObject FillRxSWINs(List<IRxSwinObject> rxSWINList, IFasta2Service fasta2Service)
        {
            Log.Debug("ActionNop.FillRxsWin()", "Not operation executed.");
            return null;
        }

        public void FillEnablingCodesCurrentContext(IList<ISwtApplicationReport> fscs, bool resetEnablingCodes)
        {
            Log.Debug("ActionNop.FillEnablingCodes()", "Not operation executed.");
        }

        public void FillEnablingCodesCurrentContext(IList<SecureFeatureData> sfa, bool resetEnablingCodes)
        {
            Log.Debug("ActionNop.FillEnablingCodesCurrentContext()", "Not operation executed.");
        }

        public void FillEnablingCodesTargetContext(IList<SecureFeatureData> sfa)
        {
            Log.Debug("ActionNop.FillEnablingCodesTargetContext()", "Not operation executed.");
        }

        public void RemoveFrom(ICollection<object> actionList)
        {
            Log.Debug("ActionNop.RemoveFrom()", "Not operation executed.");
        }

        public void UpdateEcuUifs(IEnumerable<IPsdzEcuContextInfo> ecuContextInfo)
        {
            Log.Debug("ActionNop.UpdateEcuUifs()", "Not operation executed.");
        }

        public void AddLcSwitchList(IList<ECU> ecus)
        {
            Log.Debug("ActionNop.AddLcSwitchList()", "Not operation executed.");
        }
    }
}
