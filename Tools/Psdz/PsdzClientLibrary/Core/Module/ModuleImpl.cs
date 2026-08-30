using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BMW.Rheingold.CoreFramework.Module;
using PsdzClient;

#pragma warning disable CS0169
namespace BMW.Rheingold.RheingoldSessionController.Module
{
    internal class ModuleImpl : IModuleImpl
    {
        private string runParameter = string.Empty;

        private IFastaGrouping currentUsedFasta;

        [PreserveSource(Hint = "ITestModule", Placeholder = true)]
        private PlaceholderType currentTestModule;

        private IServiceProgramProgramming infoObjPrg;

        private IList<string> lang;

        private ModuleData data;

        [PreserveSource(Hint = "IStartMeasurementService", Placeholder = true)]
        private PlaceholderType measurmentLauncher;

        public ModuleParameter Parameters { get; private set; }

        public bool GroupModule { get; set; }

        public ModuleData Data => data;

        public ModuleImpl(IList<string> lang)
        {
            GroupModule = false;
            this.lang = lang;
        }

        public ModuleImpl(IList<string> lang, string moduleName)
            : this(lang)
        {
            data = ModuleData.CreateModuleDataFromModuleName(moduleName);
        }

        public void SetModuleState(ModuleExecutionStateType moduleState)
        {
            if (Data.ModuleState.Equals(moduleState))
            {
                return;
            }
            Log.Info("Module.ModuleState", "Change to module state-{0} from module state-{1}.", moduleState, Data.ModuleState);
            Data.ModuleState = moduleState;
            switch (moduleState)
            {
                case ModuleExecutionStateType.aborted:
                    Data.Status = typeDiagObjectState.Canceled;
                    break;
                case ModuleExecutionStateType.finished:
                    Data.Status = typeDiagObjectState.Performed;
                    break;
                case ModuleExecutionStateType.error:
                    Data.Status = typeDiagObjectState.Canceled;
                    break;
            }
            if (!Data.IsExecutionCompleted)
            {
                return;
            }
            //[-] if (currentTestModule != null)
            //[-] {
            //[-] switch (Data.Status)
            //[-] {
            //[-] case typeDiagObjectState.Performed:
            //[-] currentTestModule.TechnicalResult = TestModulResult.NormalTermination;
            //[-] break;
            //[-] case typeDiagObjectState.Canceled:
            //[-] currentTestModule.TechnicalResult = TestModulResult.UserTermination;
            //[-] break;
            //[-] default:
            //[-] Log.Error("Module.ModuleState_set", "Unsupported state \"{0}\" for setting FASTA2.", Data.Status);
            //[-] break;
            //[-] }
            //[-] }
            //[-] if (measurmentLauncher != null)
            //[-] {
            //[-] measurmentLauncher.FinishMeasurement(CallingSource.TestModul);
            //[-] }
            //[-] else
            //[-] {
            //[-] Log.Error("ModuleImpl.SetModuleState()", "Measurement is not freed because the instance is null.");
            //[-] }
            Clear();
        }

        internal void init(ModuleParameter _parameters)
        {
            try
            {
                Parameters = _parameters;
                if (!string.IsNullOrEmpty(runParameter))
                {
                    Parameters.setParameter(ModuleParameter.ParameterName.runParameter, runParameter);
                }
                if (Data != null)
                {
                    Data.IsActive = true;
                    Log.Info("Module.init()", "Module: {0} - Active: {1}", Data.Name, Data.IsActive);
                }
                else
                {
                    Log.Warning("Module.init()", "Module Data is null");
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("Module.init()", exception);
            }
        }

        public IModuleExecutionHandle Execute(bool foreground, bool overall = false, bool exception = false)
        {
            return Run(foreground, exception, overall);
        }

        internal void Clear()
        {
            if ("ISTA".Equals(Data.Name))
            {
                ClearModuleIsta();
            }
        }

        private void ClearModuleIsta()
        {
            runParameter = null;
        }

        [PreserveSource(Cleaned = true)]
        public void setFASTAAblaufName(string fastaTitle)
        {
            Log.Warning("Module.setFASTAAblaufName()", "Is no FASTA2 test module.");
        }

        public void Initialize(ModuleExecutionOrigin origin, IXepInfoObject infoObjToStart, bool gui, string subModulePath, string testmoduleType, IServiceProgramProgramming infoObjPrg)
        {
            Parameters.removeParameter(ModuleParameter.ParameterName.InfoObjStarted);
            Parameters.setParameter(ModuleParameter.ParameterName.XepInfoObjectStarted, infoObjToStart);
            data = new ModuleData(infoObjToStart);
            this.infoObjPrg = infoObjPrg;
            DoInitialize(origin, gui, subModulePath, testmoduleType);
        }

        public void Initialize(ModuleExecutionOrigin origin, InfoObject infoObjToStart, bool gui, string subModulePath, string testmoduleType)
        {
            Parameters.setParameter(ModuleParameter.ParameterName.InfoObjStarted, infoObjToStart);
            data = new ModuleData(infoObjToStart);
            DoInitialize(origin, gui, subModulePath, testmoduleType);
        }

        private void DoInitialize(ModuleExecutionOrigin origin, bool gui, string subModulePath, string testmoduleType)
        {
            Data.ExecutedFrom = origin;
            Parameters.setParameter(ModuleParameter.ParameterName.ForegroundThread, !gui);
            //[-] measurmentLauncher = Parameters.getParameter(ModuleParameter.ParameterName.MeasurementLauncher, null) as IStartMeasurementService;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string key = "BMW.Rheingold.Diagnostics.Module.ISTA.ISTATabModuleCore.TestmoduleType";
            if (testmoduleType == null)
            {
                testmoduleType = ConfigSettings.getConfigString(key, "SingleAssemblyContainer");
            }
            dictionary.Add(key, testmoduleType);
            key = "BMW.Rheingold.Diagnostics.Module.ISTA.ISTATabModuleCore.SubModulePath";
            if (subModulePath == null)
            {
                subModulePath = ConfigSettings.getConfigString(key, "..\\..\\..\\Testmodule");
                subModulePath = Path.Combine(ConfigSettings.AppBaseDirectory, subModulePath);
            }
            dictionary.Add(key, subModulePath);
            Parameters.setParameter(ModuleParameter.ParameterName.Configuration, dictionary);
        }

        [PreserveSource(Cleaned = true)]
        private IModuleExecutionHandle Run(bool foreground, bool exception, bool overall)
        {
            try
            {
                if (!foreground)
                {
                    SetModuleState(ModuleExecutionStateType.running);
                    Data.Status = typeDiagObjectState.NotCalled;
                    UpdateStatus(StateType.running);
                }
                return null;
            }
            catch (Exception exception2)
            {
                Log.ErrorException("Module.run()", exception2);
                if (!foreground)
                {
                    SetModuleState(ModuleExecutionStateType.error);
                    UpdateStatus(StateType.error);
                }
                if (exception)
                {
                    throw;
                }
            }
            return null;
        }

        private string FindLayoutGroup(bool foreground, bool overall)
        {
            if (!foreground)
            {
                if (Data.ExecutedFrom == ModuleExecutionOrigin.TherapyPlan)
                {
                    return LayoutGroup.PS.ToString();
                }
                return LayoutGroup.D.ToString();
            }
            if (infoObjPrg == null)
            {
                if (overall)
                {
                    return ((Parameters.getParameter(ModuleParameter.ParameterName.Logic) is ILogic logic) ? logic.FindLayoutGroupVehicleTest().ToString() : null) ?? LayoutGroup.X.ToString();
                }
                return LayoutGroup.X.ToString();
            }
            switch (infoObjPrg.LinkType)
            {
                case SwiActionLinkType.MPB:
                case SwiActionLinkType.SMP:
                    return LayoutGroup.PBV.ToString();
                case SwiActionLinkType.AUS:
                case SwiActionLinkType.HDD:
                    {
                        ILogic logic3 = Parameters.getParameter(ModuleParameter.ParameterName.Logic) as ILogic;
                        if (infoObjPrg.SwiActionReport.Any((ISwiActionReport x) => x.Name?.StartsWith("FZA_AL_EXECUTION_ORDER", StringComparison.Ordinal) ?? false))
                        {
                            return (logic3?.ProgrammingSession?.FindLayoutGroupVehicleTest() ?? LayoutGroup.P).ToString();
                        }
                        IProgrammingSessionData programmingSessionData = logic3?.ProgrammingSessionDataContext;
                        //[-] if (programmingSessionData != null && programmingSessionData.IsValid && programmingSessionData.TherapyPlan != null && (programmingSessionData.TherapyPlan.ProgrammingState == ProgrammingExecutionState.Calculated || programmingSessionData.TherapyPlan.ProgrammingState == ProgrammingExecutionState.NotCalculated))
                        //[-] {
                        //[-] return LayoutGroup.P.ToString();
                        //[-] }
                        return LayoutGroup.PBV.ToString();
                    }
                case SwiActionLinkType.PRF:
                    return LayoutGroup.PAP.ToString();
                case SwiActionLinkType.MHV:
                case SwiActionLinkType.MVF:
                case SwiActionLinkType.MVS:
                    return LayoutGroup.PAV.ToString();
                case SwiActionLinkType.ESK_VA:
                case SwiActionLinkType.ESK_VF:
                case SwiActionLinkType.ESK_VS:
                case SwiActionLinkType.ESK_MPB:
                case SwiActionLinkType.ESK_PRF:
                    {
                        int num = ((!(Parameters.getParameter(ModuleParameter.ParameterName.Logic) is ILogic logic2)) ? ((int?)null) : logic2.ProgrammingSession?.TherapyPlanApi?.EscalationStep) ?? 1;
                        return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", LayoutGroup.PAE.ToString(), num);
                    }
                case SwiActionLinkType.MNS:
                case SwiActionLinkType.MNF:
                case SwiActionLinkType.MHN:
                case SwiActionLinkType.TN:
                    return LayoutGroup.PAN.ToString();
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Link type \"{0}\" is not supported.", infoObjPrg.LinkType));
            }
        }

        private List<LocalizedText> GetLocalizedInfoObjectTitle(IXepInfoObject xepInfoObject, string fastaTitle)
        {
            List<LocalizedText> list = new List<LocalizedText>();
            list.AddRange(lang.Select((string x) => new LocalizedText(string.IsNullOrEmpty(xepInfoObject?.GetLocalizedInfoObjectTitle(x)) ? fastaTitle : xepInfoObject.GetLocalizedInfoObjectTitle(x), x)));
            return list;
        }

        private void UpdateStatus(StateType newState)
        {
            try
            {
                if (Parameters != null)
                {
                    ((Vehicle)Parameters.getParameter(ModuleParameter.ParameterName.Vehicle))?.UpdateStatus(null, newState, 0.0);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("Module.SetState()", exception);
            }
        }

        public ModuleImpl Clone(string moduleName)
        {
            ModuleImpl moduleImpl = new ModuleImpl(lang, moduleName);
            moduleImpl.Data.IsActive = Data.IsActive;
            moduleImpl.Parameters = Parameters.Clone();
            return moduleImpl;
        }
    }
}
