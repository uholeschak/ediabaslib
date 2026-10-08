using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using System.Collections.Generic;
using BMW.ISPI.IstaServices.Contract.PUK.Data;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.Programming.Data.Obd;
using PsdzClient;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IAction<out T> : IJournalizeManager, IProtocolTransaction
    {
        DateTime StartTime { get; set; }

        DateTime EndTime { get; set; }

        T SpecialAction { get; }
        new ActionResult Result { get; set; }

        ITestPlan TestPlan { get; }

        void AddFaultPatterns(IEnumerable<PerceivedSymptomWithSource> symptomsWithSource);
        void AddFaultPatternsFromPUK(IEnumerable<FastaPukVfc> pukVfcs);
        void AddFaultMemoryAndServiceFault(BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle);
        void AddCheckControlMessages(BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle);
        ITestPlanNode CreateAndAddTestPlanNode(decimal id, string sysName, IList<LocalizedText> diagnoseTitle, string priority);
        [PreserveSource(Hint = "BMW.Rheingold.CoreFramework.Contracts.FASTA.ITherapyPlan", Placeholder = true)]
        PlaceholderType CreateTherapyPlanCalculation(string id);
        void FillTargetContext(string istufeTarget, IEnumerable<IEcuObj> ecus, BMW.Rheingold.CoreFramework.Contracts.Programming.IFa vehicleOrder, string ereihe, BNType boardnetType = BNType.BN2020);
        void FillCurrentContext(DateTime endTime, BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle, IEnumerable<IEcuObj> ecus);
        IBoolResultObject FillRxSWINs(List<IRxSwinObject> rxSWINList, IFasta2Service fasta2Service);
        void FillEnablingCodesCurrentContext(IList<ISwtApplicationReport> fscs, bool resetEnablingCodes);
        void FillEnablingCodesCurrentContext(IList<SecureFeatureData> sfa, bool resetEnablingCodes);
        void FillEnablingCodesTargetContext(IList<SecureFeatureData> sfa);
        void FillCurrentContext(DateTime endTime, BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle, IEnumerable<IEcuJob> ecusJobs, IEnumerable<IEcuObj> psdzEcus, IFFMDynamicResolver ffmDynamicResolver, IDictionary<IEcuIdentifier, IObdData> obdDataMap);
        bool HasEcuCommunication(IAction<IEcuCommunication> ecuComToCompare);
        void AddTo(ICollection<object> actionList);
        void RemoveFrom(ICollection<object> actionList);
        void UpdateEcuUifs(IEnumerable<IPsdzEcuContextInfo> ecuContextInfo);
        void AddLcSwitchList(IList<ECU> ecus);
    }
}
