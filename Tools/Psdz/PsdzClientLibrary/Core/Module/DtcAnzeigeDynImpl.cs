using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows.Threading;
using BMW.Rheingold.CoreFramework.ServiceProgram;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    internal class DtcAnzeigeDynImpl : ServiceDlgImplBase<DtcAnzeigeDynModel>
    {
        private IProtocolBasic fasta;

        private IList<string> lang;

        [PreserveSource(Hint = "IDatabaseProvider", Placeholder = true)]
        private PsdzDatabase database;

        private IFFMDynamicResolver ffmResolver;

        private ParameterContainer outParameter;

        private ParameterContainer inAndOutParameters;

        private HashSet<decimal> markedFaultCodes;

        private List<FaultModelDtcDyn> faults;

        private FaultModelDtcDyn SelectedFault { get; set; }

        public IList<string> Lang
        {
            get
            {
                return lang;
            }
            set
            {
                lang = value;
            }
        }

        protected ICollection<decimal> MarkedFaultCodes => markedFaultCodes;

        public string CallingName { get; private set; }

        private ICollection<FaultModelDtcDyn> FaultList => faults;

        private InfoObject InfoObjectStarted { get; set; }

        private string SelectedFehlerOrt { get; set; }

        private string PriorText
        {
            get
            {
                return base.Model.PriorText;
            }
            set
            {
                base.Model.PriorText = value;
            }
        }

        private string PastText
        {
            get
            {
                return base.Model.PastText;
            }
            set
            {
                base.Model.PastText = value;
            }
        }

        private string Title
        {
            get
            {
                return base.Model.Title;
            }
            set
            {
                base.Model.Title = value;
            }
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public DtcAnzeigeDynImpl(ParameterContainer inParameters)
            : base(inParameters)
        {
            //[-] Initialize(inParameters, RetrieveFasta(inParameters), DatabaseProviderFactory.Instance);
            //[+] Initialize(inParameters, RetrieveFasta(inParameters), null);
            Initialize(inParameters, RetrieveFasta(inParameters), null);
        }

        [PreserveSource(Hint = "IDatabaseProvider", SignatureModified = true)]
        public DtcAnzeigeDynImpl(ParameterContainer InParameters, IProtocolBasic fasta, IList<string> lang, PsdzDatabase database, Vehicle vehicle, ILogic logic, Dispatcher dispatcher)
            : base(InParameters)
        {
            Initialize(InParameters, fasta, database);
        }

        [PreserveSource(Hint = "IDatabaseProvider", SignatureModified = true)]
        private void Initialize(ParameterContainer InParameters, IProtocolBasic fasta, PsdzDatabase database)
        {
            outParameter = new ParameterContainer();
            inAndOutParameters = new ParameterContainer();
            markedFaultCodes = new HashSet<decimal>();
            faults = new List<FaultModelDtcDyn>();
            this.fasta = fasta;
            this.database = database;
            lang = logic.Lang;
            Lang = logic.Lang;
            ffmResolver = logic.FFMResolver;
            if (InParameters.getParameter("__CallingModule__") is ISTAModule iSTAModule)
            {
                CallingName = iSTAModule.GetType().Name;
            }
            Log.Info("DtcAnzeigeDynImpl.Initialize()", "calling module for fault filtering: {0}", CallingName);
        }

        private void SetOutParameters(ParameterContainer outParameter, ParameterContainer inAndOutParameters)
        {
            inAndOutParameters.clearParameters();
            outParameter.clearParameters();
            if (SelectedFault != null && inAndOutParameters != null && SelectedFault.DTC != null && SelectedFault.DTC.F_ORT.HasValue && SelectedFault.ECU != null)
            {
                inAndOutParameters.setParameter("Status_Fehlerkode_selektiert_dez", (int)SelectedFault.DTC.F_ORT.Value);
                inAndOutParameters.setParameter("Status_Fehlerkode_selektiert_hex", $"{(int)SelectedFault.DTC.F_ORT.Value:X}");
                inAndOutParameters.setParameter("SGBD_Fehlerkode_selektiert", SelectedFault.ECU.VARIANTE);
            }
            else
            {
                inAndOutParameters.setParameter("Status_Fehlerkode_selektiert_dez", -1);
                inAndOutParameters.setParameter("Status_Fehlerkode_selektiert_hex", $"{0:X}");
                inAndOutParameters.setParameter("SGBD_Fehlerkode_selektiert", string.Empty);
            }
            outParameter.setParameter("Result", -1);
        }

        public bool TrySelectFirstFault()
        {
            ResetPageTitle();
            bool result = false;
            if (FaultList != null && FaultList.Any() && FaultList.OfType<FaultModelDtcDyn>().All((FaultModelDtcDyn fault) => !fault.IsSelected))
            {
                result = true;
                SelectFault(FaultList.ElementAt(0));
            }
            return result;
        }

        private InfoObject GetInfoObjStarted(ModuleParameter moduleParameter, InfoObject infoObjectStarted)
        {
            InfoObject infoObject = moduleParameter.getParameter(ModuleParameter.ParameterName.InfoObjStarted) as InfoObject;
            try
            {
                if (infoObject == null)
                {
                    Log.Info("DtcAnzeigeDynImpl.GetInfoObjStarted()", "InfoObjStarted is null.");
                    //[-] if (!(moduleParameter.getParameter(ModuleParameter.ParameterName.XepInfoObjectStarted) is XepInfoObject xep))
                    {
                        Log.Info("DtcAnzeigeDynImpl.GetInfoObjStarted()", "XepInfoObjectStarted is null.");
                    }
                    //[-] else
                    //[-] {
                    //[-] Log.Info("DtcAnzeigeDynImpl.GetInfoObjStarted()", "Create info object from XepInfoObjectStarted.");
                    //[-] infoObject = logic.Factory.CreateInfoObject(xep);
                    //[-] if (infoObject != null)
                    //[-] {
                    //[-] infoObject.ParentDiagnosisObject = null;
                    //[-] moduleParameter.setParameter(ModuleParameter.ParameterName.InfoObjStarted, infoObjectStarted);
                    //[-] }
                    //[-] }
                }
                if (infoObject == null)
                {
                    Log.Error("DtcAnzeigeDynImpl.GetInfoObjStarted()", "Failed to get info object. Returning null.");
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("DtcAnzeigeDynImpl.GetInfoObjStarted()", exception);
            }
            return infoObject;
        }

        protected virtual void SetMarkedInformation(FaultModelDtcDyn selectedFault)
        {
            decimal? num = null;
            if (selectedFault.DTC.Id.HasValue)
            {
                num = selectedFault.DTC.Id.Value;
            }
            if (num.HasValue)
            {
                MarkedFaultCodes.AddIfNotContains(num.Value);
            }
        }

        private void SetTitle()
        {
            if (PriorText != null)
            {
                if (!string.IsNullOrEmpty(PriorText))
                {
                    Title = PriorText;
                }
                else
                {
                    Title = ToString();
                }
            }
        }

        private void UpdateFaultList(IEnumerable<FaultModelDtcDyn> list)
        {
            base.Model.Buttons.Clear();
            faults.Clear();
            foreach (FaultModelDtcDyn item in list)
            {
                base.Model.Buttons.Add(item.ButtonModel);
                faults.Add(item);
            }
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            Log.Info("DtcAnzeigeDynImpl.Invoke()", "called with method: {0}", method);
            DateTime now = DateTime.Now;
            if ("InitializeDialog".Equals(method))
            {
                StoreKeyboardEnabled();
                SetKeyboardEnabled(enable: false);
                FaultList.Clear();
                UpdateFaultList(new List<FaultModelDtcDyn>());
                SetNextButtonEnabled(value: false);
                InfoObjectStarted = GetInfoObjStarted(__RheinGoldCoreModuleParameters__, InfoObjectStarted);
                if (InfoObjectStarted?.ParentDiagnosisObject is ManualDiagObj)
                {
                    InfoObjectStarted.ParentDiagnosisObject = SelectDiagParent("Invoke()");
                }
                ShowProgressDialog(new FormatedData("#SearchingRelevantDTCs", false));
                List<FaultModelDtcDyn> list = CalculateFaults(InfoObjectStarted);
                CloseProgressDialog();
                if (list != null && list.Any())
                {
                    TextContent textContent = GetText(inParam, "Anzeige_Text_Anfang") as TextContent;
                    PriorText = GetContent(textContent);
                    TextContent textContent2 = GetText(inParam, "Anzeige_Text_Ende") as TextContent;
                    PastText = GetContent(textContent2);
                    SetNextButtonEnabled(value: false);
                    SetTitle();
                    UpdateFaultList(list);
                    TrySelectFirstFault();
                    SetNextButtonEnabled(value: true);
                    WaitOnUserInteraction();
                    ProtocolInFasta(inParam, textContent, textContent2, now);
                }
                else
                {
                    ResetScreenMode();
                }
                SetOutParameters(outParameter, inAndOutParameters);
                if (inoutParam != null && inAndOutParameters != null)
                {
                    inoutParam.cloneParameters(inAndOutParameters);
                }
                if (outParam != null && outParameter != null)
                {
                    outParam.cloneParameters(outParameter);
                }
            }
            else
            {
                Log.Error("DtcAnzeigeDynImpl.Invoke()", "Unsupported method {0} will be ignored.", method);
            }
            base.ServiceDialogUI.IsDialogShown = false;
        }

        protected void WaitOnUserInteraction()
        {
            ServiceProgramNavigationAction obj;
            do
            {
                ServiceProgramAction serviceProgramAction = base.ServiceProgramController.AwaitUserAction(-1);
                if (serviceProgramAction is ServiceProgramButtonSelectionAction serviceProgramButtonSelectionAction)
                {
                    int selectedIndex = serviceProgramButtonSelectionAction.SelectedIndex;
                    Log.Info("DtcAnzeigeDynImpl.WaitOnUserInteraction()", "Selected button index: {0}", selectedIndex);
                    SelectFault(selectedIndex);
                }
                obj = serviceProgramAction as ServiceProgramNavigationAction;
            }
            while (obj == null || obj.NavigationAction != NavigationAction.Next);
            Log.Info("DtcAnzeigeDynImpl.WaitOnUserInteraction()", "Next button was clicked.");
            if (SelectedFault != null)
            {
                SelectedFault.IsMarked = true;
                SetMarkedInformation(SelectedFault);
                SelectedFehlerOrt = $"{SelectedFault.DTC.F_ORT:X}";
                base.Model.SelectedIndex = SelectedFault.Index;
                base.Model.Buttons.ForEach(delegate (DtcAnzeigeButtonModel btn)
                {
                    btn.IsEnabled = false;
                });
            }
        }

        private void SelectFault(FaultModelDtcDyn item)
        {
            item.IsSelected = true;
            base.Model.SelectedButton(item.Index);
            SelectFault(item.Index);
        }

        public void SelectFault(int index)
        {
            try
            {
                SelectedFault = FaultList.ElementAt(index);
                base.Model.SelectedFault(SelectedFault.Fault);
                SetNextButtonEnabled(value: true);
            }
            catch (Exception exception)
            {
                Log.WarningException("DtcAnzeigeDynImpl.SelectFault()", exception);
            }
        }

        public ParameterContainer FinishDialog(ParameterContainer inoutParam)
        {
            inoutParam.cloneParameters(inAndOutParameters);
            return outParameter;
        }

        private void JournalizeFaultList(IAction<IUiDialog> fastaUiDialog, IMessageText fastaMsgTxt, DateTime startTime)
        {
            if (FaultList != null)
            {
                ISelectable selectable = null;
                List<LocalizedText> list = new List<LocalizedText>();
                {
                    foreach (FaultModelDtcDyn item2 in FaultList.OfType<FaultModelDtcDyn>())
                    {
                        string arg = $"{item2.DTC.F_ORT:X}";
                        foreach (string item3 in lang)
                        {
                            string arg2 = item2.FaultLabel;
                            //[-] if (item2.Fault.XepFaultLabel != null)
                            //[-] {
                            //[-] arg2 = item2.Fault.XepFaultLabel.GetLocalizedTitle(item3);
                            //[-] }
                            //[-] else
                            {
                                Log.Error("DtcAnzeigeDynImpl.JournalizeFaultList()", "Fault contains no localized label.");
                            }
                            LocalizedText item = new LocalizedText($"<BR/>{arg}: {arg2}", item3);
                            list.Add(item);
                        }
                        fastaMsgTxt.AddText(list);
                        if (!list.Any())
                        {
                            fastaUiDialog.StartTime = startTime;
                            selectable = fastaMsgTxt.CreateAndAddSelectable("n/a");
                        }
                        else if (selectable == null)
                        {
                            selectable = fastaMsgTxt.CreateAndAddSelectable("DTC_ANZEIGE_DYN");
                        }
                        bool selectionState = object.Equals(base.Model.SelectedIndex, item2.Index);
                        selectable.AddEntry(selectionState, null, null);
                        list.Clear();
                    }
                    return;
                }
            }
            Log.Info("DtcAnzeigeDynImpl.JournalizeFaultList()", "No faultlist to journalize.");
        }

        private void ProtocolInFasta(ParameterContainer inParam, ITextContent priorText, ITextContent pastText, DateTime startTime)
        {
            fasta = RetrieveFasta(inParam);
            if (fasta != null)
            {
                IAction<IUiDialog> action = fasta.CreateAndAddUiDialogFromServiceProgram("DTC_ANZEIGE_DYN", base.LastCallingMethod);
                IMessageText messageText = null;
                string text = string.Empty;
                if (priorText != null && !string.IsNullOrEmpty(priorText.PlainText))
                {
                    text += priorText.Text;
                    IList<LocalizedText> textForUI = priorText.GetTextForUI(lang);
                    messageText = action.SpecialAction.CreateAndAddMessageText(textForUI);
                    action.StartTime = startTime;
                    messageText.AddText(textForUI);
                }
                if (messageText == null)
                {
                    IList<LocalizedText> textForUI2 = new TextContent(text).GetTextForUI(lang);
                    messageText = action.SpecialAction.CreateAndAddMessageText(textForUI2);
                    action.StartTime = startTime;
                }
                JournalizeFaultList(action, messageText, startTime);
                if (pastText != null && !string.IsNullOrEmpty(pastText.PlainText))
                {
                    if (messageText == null)
                    {
                        IList<LocalizedText> textForUI3 = new TextContent("empty").GetTextForUI(lang);
                        messageText = action.SpecialAction.CreateAndAddMessageText(textForUI3);
                    }
                    IList<LocalizedText> textForUI4 = pastText.GetTextForUI(lang);
                    messageText.AddText(textForUI4);
                }
                if (!string.IsNullOrEmpty(SelectedFehlerOrt))
                {
                    List<LocalizedText> list = new List<LocalizedText>();
                    list.AddRange(lang.Select((string x) => new LocalizedText(SelectedFehlerOrt, x)));
                    action.SpecialAction.AddAnswer(list, null);
                }
            }
            else
            {
                Log.Warning("DtcAnzeigeDynImpl.FinishDialog()", "No FASTA available.");
            }
        }

        private void MarkAndAddToFaultCodeList(IList<FaultModelDtcDyn> listToAdd, FaultModelDtcDyn fault, bool mark)
        {
            listToAdd.Add(fault);
            fault.Initialize(mark, listToAdd.IndexOf(fault));
        }

        private ICollection<FaultModelDtcDyn> CreateFaultList(IEnumerable<string> faultIds)
        {
            List<FaultModelDtcDyn> list = new List<FaultModelDtcDyn>();
            if (faultIds == null)
            {
                return list;
            }
            foreach (string id in faultIds)
            {
                try
                {
                    //[-] Fault fault = Vehicle.FaultList.FirstOrDefault(delegate (Fault x)
                    //[-] {
                    //[-] DTC dTC = x.DTC;
                    //[-] return dTC != null && dTC.Id.HasValue && x.DTC.Id.ToString() == id;
                    //[-] });
                    //[-] if (fault != null)
                    //[-] {
                    //[-] string faultLabel = ((!string.IsNullOrEmpty(fault.XepFaultLabel?.Title)) ? fault.XepFaultLabel.Title : FaultCodeConverters.LocalizedFaultLabel(fault.ECU, fault.DTC, Vehicle, FFMResolver));
                    //[-] FaultModelDtcDyn fault2 = new FaultModelDtcDyn(fault, faultLabel);
                    //[-] MarkAndAddToFaultCodeList(list, fault2, MarkedFaultCodes.Contains(fault.DTC.Id ?? ((decimal)fault.DTC.F_ORT.Value)));
                    //[-] }
                }
                catch (Exception exception)
                {
                    Log.ErrorException("DtcAnzeigeDynImpl.CreateFaultList()", exception);
                }
            }
            return list;
        }

        protected void UpdateFaultList(IEnumerable<string> faultIds)
        {
            base.Model.Buttons.Clear();
            faults.Clear();
            faults.AddRange(CreateFaultList(faultIds));
            base.Model.Buttons.AddRange(faults.Select((FaultModelDtcDyn x) => x.ButtonModel));
        }

        [PreserveSource(Cleaned = true)]
        private List<FaultModelDtcDyn> CalculateFaults(object status)
        {
            List<FaultModelDtcDyn> list = new List<FaultModelDtcDyn>();
            return list;
        }
    }
}
