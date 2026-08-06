using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Measurement.Common;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using BMW.Rheingold.CoreFramework.Contracts.ConnectionManagement;
using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    internal class ServiceDialogCmdBase : IServiceDialog
    {
        private readonly int elementNumber;

        private readonly IModuleExecutionParent globalTabModuleISTA;

        private ISTAServiceDialog dialogImpl;

        protected readonly IFastaGrouping fasta;

        protected IProtocolBasic FastaProtocoler { get; set; }

        protected IStartMeasurementServiceServer MeasurmentService { get; private set; }

        protected IModuleExecutionParent Parent => globalTabModuleISTA;

        protected ISTAModule CallingModule { get; private set; }

        protected IModuleExecutionStep CurrentStep { get; set; }

        private string Path { get; set; }

        protected string DialogName { get; private set; }

        public static bool BatchMode { get; private set; }

        protected bool Display { get; set; }

        public ServiceDialogConfiguration ServiceDialogConfig { get; set; }

        static ServiceDialogCmdBase()
        {
            BatchMode = ConfigSettings.getConfigStringAsBoolean("BatchMode");
        }

        public ServiceDialogCmdBase(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo)
        {
            CallingModule = callingModule;
            fasta = callingModule.FastaGrouping;
            FastaProtocoler = RetrieveFasta(fasta);
            MeasurmentService = callingModule.MeasurementLauncher;
            Path = path;
            this.globalTabModuleISTA = globalTabModuleISTA;
            elementNumber = elementNo;
            Display = true;
            DialogName = ServiceDialogFactory.ResolveDialogRef(path, logMissing: true);
        }

        private IProtocolBasic RetrieveFasta(IFastaGrouping fastaGrouping)
        {
            return fastaGrouping?.ProtocolingInstance;
        }

        public virtual void CreateDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            inParam.setParameter("FASTA", fasta);
            //[-] IXepInfoObject infoObjectByControlId = DatabaseProviderFactory.Instance.GetInfoObjectByControlId(ServiceDialogConfig.ControlId);
            //[+]PsdzDatabase.SwiInfoObj infoObjectByControlId = CallingModule.DBProvider.GetInfoObjectByControlId(ServiceDialogConfig.ControlId.ToString());
            PsdzDatabase.SwiInfoObj infoObjectByControlId = CallingModule.DBProvider.GetInfoObjectByControlId(ServiceDialogConfig.ControlId.ToString());
            inParam.Parameter.Add("ISTAModule.Me", infoObjectByControlId);
            inParam.Parameter.Add("ISTAModule.TextCollection", ServiceDialogConfig.TextCollection);
            inParam.setParameter("__RheinGoldTabModuleISTA__", globalTabModuleISTA);
            inParam.setParameter("__DialogName__", DialogName);
            inParam.setParameter("__RheinGoldCoreModuleParameters__", CallingModule.__RheinGoldCoreModuleParameters__);
            inParam.setParameter("__RheinGoldSOCAccessor__", CallingModule.SOCAccessor);
            inParam.setParameter("__CallingModule__", CallingModule);
            object[] constructorParam = new object[1] { inParam };
            Type[] constructorParamType = new Type[1] { typeof(ParameterContainer) };
            object obj = ServiceDialogConfig.DialogType.CreateInstance(constructorParamType, constructorParam);
            dialogImpl = (ISTAServiceDialog)obj;
            dialogImpl.SetResultSetFromServiceProgram(CallingModule.ResultSet);
            IServiceDlgImplBase<ServiceDialogModelBase> serviceDlgImplBase = dialogImpl as IServiceDlgImplBase<ServiceDialogModelBase>;
            //[-] dialogImpl.ServiceDialogUI = serviceDlgImplBase?.Model;
            //[-] CurrentStep = serviceDlgImplBase?.Model;
        }

        public virtual void InitializeInput(string method, ParameterContainer inParam, ParameterContainer inoutParam)
        {
        }

        protected void InvokeCurrentStepAsIServiceDialog(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CurrentStep != null && typeof(IServiceDialog).IsAssignableFrom(CurrentStep.GetType()))
            {
                try
                {
                    ((IServiceDialog)CurrentStep).Invoke(method, inParam, outParam, inoutParam);
                    return;
                }
                catch (Exception ex)
                {
                    Log.ErrorException("ServiceDialogCmdBase.InvokeCurrentStepAsIServiceDialog()", ex);
                    //[-] if (ex is AppException || ex is UserCanceledException)
                    //[+] if (ex is UserCanceledException)
                    if (ex is UserCanceledException)
                    {
                        throw;
                    }
                    return;
                }
            }
            Log.Error("ServiceDialogCmdBase.InvokeCurrentStepAsIServiceDialog()", "Failed to call invoke() of dialog {0}, because it's not implementing IServiceDialog.", DialogName);
        }

        public virtual void DoInvoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if (CurrentStep != null && !CurrentStep.IsDialogShown && Display)
            {
                NavigateTo(CurrentStep);
            }
            if (CurrentStep != null && Display)
            {
                CurrentStep.IsDialogShown = true;
            }
            dialogImpl.InvokeMain(method, inParam, outParam, inoutParam);
        }

        protected void NavigateTo(IModuleExecutionStep modPage)
        {
            if (!BatchMode)
            {
                if (modPage != null)
                {
                    globalTabModuleISTA.NavigateTo(modPage);
                }
                else
                {
                    Log.Warning("ServiceDialog.NavigateTo()", "modPage was null when trying to NavigateTo(modPage).");
                }
            }
        }

        protected void WriteFasta(ParameterContainer outParam, IModuleStep fasta2ModuleStep)
        {
            if (CallingModule != null)
            {
                _ = CallingModule._VerboseLoopLogs;
            }
            if (CallingModule != null)
            {
                _ = CallingModule._DoLoopHandling;
            }
            if (outParam == null)
            {
                return;
            }
            string text = null;
            if (CallingModule != null)
            {
                text = CallingModule.LastCallingMethod;
            }
            if (fasta2ModuleStep != null)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    fasta2ModuleStep.Title = text;
                    fasta2ModuleStep.EndTime = DateTime.Now;
                }
                else if (string.IsNullOrEmpty(fasta2ModuleStep.Title))
                {
                    fasta2ModuleStep.Title = "n/a";
                }
            }
            else
            {
                Log.Error("ServiceDialogCmdBase.WriteFasta()", "No FASTA available.");
            }
        }

        [PreserveSource(Cleaned = true)]
        protected void CallConnectionManager(ILogic logic, Vehicle vecInfo, ConnectionTargetTypes connectionTargetTypes)
        {
        }

        [PreserveSource(Cleaned = true)]
        protected bool CheckConnectionToImib()
        {
            return false;
        }

        public void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            try
            {
                Display = !method.Equals("ServiceCodeProtokoll") && (inParam == null || (bool)inParam.getParameter("Display", true));
                InitializeInput(method, inParam, inoutParam);
                object obj = null;
                if (inParam != null)
                {
                    inParam.Parameter.Add("ISTAModule.TextCollection", ServiceDialogConfig.TextCollection);
                    if (!inParam.Parameter.ContainsKey("ISTAModule.Me"))
                    {
                        //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                        //[-] inParam.Parameter.Add("ISTAModule.Me", instance.GetInfoObjectByControlId(ServiceDialogConfig.ControlId));
                        //[+]PsdzDatabase instance = CallingModule.DBProvider;
                        PsdzDatabase instance = CallingModule.DBProvider;
                        //[+] inParam.Parameter.Add("ISTAModule.Me", instance.GetInfoObjectByControlId(ServiceDialogConfig.ControlId.ToString()));
                        inParam.Parameter.Add("ISTAModule.Me", instance.GetInfoObjectByControlId(ServiceDialogConfig.ControlId.ToString()));
                    }
                    obj = inParam.getParameter("FASTA");
                }
                if (obj == null && inParam != null)
                {
                    if (CallingModule?.FastaGrouping != null && dialogImpl?.FastaGrouping != null)
                    {
                        dialogImpl.FastaGrouping = CallingModule.FastaGrouping;
                    }
                    inParam.setParameter("FASTA", fasta);
                }
                DoInvoke(method, inParam, outParam, inoutParam);
                IModuleStep moduleStep = fasta as IModuleStep;
                //[-] if (moduleStep == null && fasta is ITestModule)
                //[-] {
                //[-] moduleStep = ((ITestModule)fasta).CurrentUsedStep;
                //[-] }
                //[-] else if (moduleStep == null && fasta is ISubModule)
                //[+] if (moduleStep == null && fasta is ISubModule)
                if (moduleStep == null && fasta is ISubModule)
                {
                    moduleStep = ((ISubModule)fasta).CurrentStep;
                }
                WriteFasta(outParam, moduleStep);
            }
            //[-]catch (ServiceDialogMethodIgnoredException)
            //[-]{
            //[-]Log.Info("ServiceDialogCmdBase.Invoke()", "Method {0} of service dialog {1} will be ignored.", method, DialogName);
            //[-]}
            //[-]catch (ServiceDialogMethodUnsupportedException ex2)
            //[+] catch (Exception ex2)
            catch (Exception ex2)
            {
                Log.Error("ServiceDialogCmdBase.Invoke()", "Method {0} of service dialog {1} is not supported. {2}", method, DialogName, ex2.ToString());
            }
        }
    }
}

