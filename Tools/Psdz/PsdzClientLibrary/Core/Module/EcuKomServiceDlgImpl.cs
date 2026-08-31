using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.ConnectionManagement;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using BMW.Rheingold.VehicleCommunication;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace BMW.Rheingold.Module.ISTA
{
    internal class EcuKomServiceDlgImpl : ServiceDlgImplBase<EcuKomServiceDlgModel>
    {
        private bool p_WeiterButtonEnabledStack;
        private IProtocolBasic fasta;
        private ParameterContainer inParameters;
        private bool display = true;
        private bool p_Fehlermeldung;
        private ITextLocator m_IOFrageText;
        private ITextLocator concatTxt;
        private bool m_IOFrage;
        private ConfigurationContainer configContainer;
        private IAction<IUiDialog> fastaDlg;
        private ParameterContainer fastaParameter = new ParameterContainer();
        private ParameterContainer outParameter = new ParameterContainer();
        private IDiagnosticDeviceResult dscJob = new EDIABASAdapterDeviceResult(new ECUJob());
        private bool m_bStartPressed;
        private bool p_DSCError;
        private Timer executionTimer;
        private bool showErrorPopupForNotOkay;
        private bool executionInProgress;
        private ITextLocator wertFeld;
        private ITextLocator wertFeld1;
        private ISTAModule callingModule;
        private static DateTime lastErrorMessage = DateTime.MinValue;
        private int selectionIndex;
        private Thread parentThread;
        public EcuKomServiceDlgImpl(ParameterContainer inParam) : base(inParam)
        {
            fasta = RetrieveFasta(inParam);
            callingModule = inParam.getParameter("__CallingModule__") as ISTAModule;
            showErrorPopupForNotOkay = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Module.ISTA.ECUKOMServiceDlg.ShowErrorPopupForNotOkay", defaultValue: false);
            executionTimer = new Timer(ExecuteAdapter, null, -1, 500);
            parentThread = Thread.CurrentThread;
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            DateTime now = DateTime.Now;
            fasta = RetrieveFasta(inParam);
            if (fasta == null)
            {
                Log.Error("EcuKomServiceDlgImpl.Invoke()", "FASTA protocoling not possible.");
            }

            bool flag = (bool)__RheinGoldCoreModuleParameters__.getParameter(ModuleParameter.ParameterName.ForegroundThread, false);
            p_WeiterButtonEnabledStack = IsNextButtonEnabled();
            if (!"InitializeDialog".Equals(method))
            {
                return;
            }

            InitializeInParameters(inParam);
            if ((logic as Logic).IsModuleExecutionMinimized() && !flag)
            {
                Log.Info("EcuKomServiceDlgImpl.Invoke()", "found minimized testmodule! Execution will be suspended here until maximized again");
                while ((logic as Logic).IsModuleExecutionMinimized())
                {
                    Thread.Sleep(500);
                }

                Log.Info("EcuKomServiceDlgImpl.Invoke()", "found shortly maximmized testmodule! Execution will be resumed");
            }

            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Module.ISTA.ECUKOMServiceDlg.ShowVMDialog", defaultValue: true))
            {
            //[-] if (!flag)
            //[-] {
            //[-] if (logic.VecInfo.VCI == null || logic.VecInfo.VCI.VCIType == VCIDeviceType.INFOSESSION || logic.EcuKom == null)
            //[-] {
            //[-] Log.Warning("EcuKomServiceDlgImpl.Invoke()", "InitializeDialog: no ecuKom available due to infosession. Show up connection manager");
            //[-] InteractionConnectionManagerResponse interactionConnectionManagerResponse = ConnectionManagerHandler.ShowConnectionManager(null, logic, logic.Services.InteractionService, logic.VecInfo?.VCI, null, ConnectionTargetTypes.VCI);
            //[-] ConnectionManagerResponseAction? connectionManagerResponseAction = interactionConnectionManagerResponse?.Action;
            //[-] if (connectionManagerResponseAction.HasValue && connectionManagerResponseAction.GetValueOrDefault() == ConnectionManagerResponseAction.Connect)
            //[-] {
            //[-] logic.CheckVinAndConnectOverConnectionManager(new ProgressMonitor(), interactionConnectionManagerResponse.VciDevice);
            //[-] }
            //[-] else
            //[-] {
            //[-] Log.Warning("EcuKomServiceDlgImpl.Invoke()", "Response action '{0}' is not allowed.", interactionConnectionManagerResponse?.Action);
            //[-] }
            //[-] }
            //[-] }
            //[-] else
            //[-] {
            //[-] Log.Info("EcuKomServiceDlgImpl.Invoke()", "no ecukom handle available; no connection manager popup due to FFM resolving");
            //[-] }
            }

            if (display)
            {
                base.SPEUserInterface.DisplayWaitCursor(bWaitCursor: false);
                base.Model.IsCustomButton0Enabled = true;
                base.Model.IsCustomButton0Visible = true;
                base.Model.TxtParamFlow = ((TextContent)concatTxt.TextContent).GetTextForUI(logic.Lang)[0].TextItem;
                DoStartStopAsynch();
                if (p_DSCError && p_Fehlermeldung)
                {
                    dscJob = DscSynchron(standardErrorHandling: true, configContainer);
                }

                if (m_IOFrage)
                {
                    base.Model.IOFrageTextFlow = ((TextContent)m_IOFrageText.TextContent).GetTextForUI(logic.Lang)[0].TextItem;
                    base.Model.TextInfo[0] = __Text("51945227").TextContent.PlainText;
                    base.Model.TextInfo[1] = __Text("51943563").TextContent.PlainText;
                    base.Model.IsButtonBarVisible = true;
                    SetNextButtonEnabled(value: true);
                    base.SPEUserInterface.DisplayWaitCursor(bWaitCursor: false);
                    NavigateTo(base.Model);
                    WaitOnUserInteraction();
                    base.SPEUserInterface.DisplayWaitCursor(bWaitCursor: true);
                    if (selectionIndex == 0)
                    {
                        callingModule.ResultSet.CollectiveResult = CollectiveResultSet.Ok;
                    }
                    else
                    {
                        callingModule.ResultSet.CollectiveResult = CollectiveResultSet.NotOk;
                    }
                }
                else
                {
                    base.Model.IsButtonBarVisible = false;
                    NavigateTo(base.Model);
                    SetNextButtonEnabled(value: true);
                    WaitForContinueButton(-1);
                    base.Model.IsCustomButton0Enabled = false;
                    base.Model.IsCustomButton0Visible = false;
                }

                if (executionTimer != null)
                {
                    Log.Info("EcuKomServiceDlgImpl.Invoke()", "stop execution timer");
                    executionTimer.Change(-1, -1);
                }

                ProtocolFasta();
            }
            else
            {
                dscJob = DscSynchron(p_Fehlermeldung, configContainer);
            }

            if (dscJob != null && dscJob.ECUJob != null)
            {
                List<IEcuJob> list = new List<IEcuJob>();
                list.Add(dscJob.ECUJob);
                fastaParameter.setParameter("ECUKom", list);
                outParam.setParameter("FASTAJobs", list);
                outParam.setParameter("/WurzelOut/DSCResult", dscJob);
                IEcu ecu = logic.VecInfo.getECUbyECU_GRUPPE(dscJob.ECUJob.EcuName);
                if (ecu == null)
                {
                    ecu = logic.VecInfo.getECUbyECU_SGBD(dscJob.ECUJob.EcuName);
                }

                if (fasta != null)
                {
                    if (ecu != null)
                    {
                    //[-] IAction<IEcuCommunication> action = fasta.CreateEcuCommunication(ecu, new List<IEcuJob> { dscJob.ECUJob }, doFastaRelevantFiltering: true, LayoutGroup.X);
                    //[-] action.StartTime = now;
                    //[-] fasta.AddIfIsNotInLoopOrDoLoopHandling(action, callingModule._VerboseLoopLogs, callingModule._DoLoopHandling);
                    }
                    else if (!string.IsNullOrWhiteSpace(dscJob.ECUJob.JobErrorText))
                    {
                        IAction<IUiDialog> action2 = fasta.CreateAndAddUiDialogFromServiceProgram("MessageServiceDlg", method);
                        action2.SpecialAction.Display = false;
                        List<LocalizedText> list2 = new List<LocalizedText>();
                        string text = dscJob.ECUJob.EcuName ?? "";
                        string text2 = dscJob.ECUJob.JobName ?? "";
                        foreach (LocalizedText item in logic.Lang.Select((string x) => new LocalizedText(dscJob.ECUJob.JobErrorText, x)))
                        {
                            LocalizedText localizedText = item;
                            localizedText.TextItem = localizedText.TextItem + " (ECUName = \"" + text + "\", JobName = \"" + text2 + "\")";
                            list2.Add(item);
                        }

                        action2.SpecialAction.CreateAndAddMessageText(list2);
                    }
                    else if (!string.IsNullOrEmpty(dscJob.ECUJob.EcuName))
                    {
                    //[-] IAction<IEcuCommunication> action3 = fasta.CreateEcuCommunication(dscJob.ECUJob.EcuName, new List<IEcuJob> { dscJob.ECUJob }, doFastaRelevantFiltering: true, LayoutGroup.X);
                    //[-] action3.StartTime = now;
                    //[-] fasta.AddIfIsNotInLoopOrDoLoopHandling(action3, callingModule._VerboseLoopLogs, callingModule._DoLoopHandling);
                    }
                }
            }

            outParam.setParameter("_FASTA", fastaParameter);
        }

        private void InitializeInParameters(ParameterContainer inParameters)
        {
            this.inParameters = inParameters;
            display = (bool)inParameters.getParameter("Display", true);
            p_Fehlermeldung = (bool)inParameters.getParameter("/WurzelIn/FehlerMeldung", false);
            m_IOFrageText = inParameters.getParameter("/WurzelIn/IO_FrageText", null) as ITextLocator;
            concatTxt = inParameters.getParameter("/WurzelIn/StateLists/Result[0]/Text", null) as ITextLocator;
            string methodName = inParameters.getParameter("methodname", null) as string;
            m_IOFrage = m_IOFrageText != null;
            concatTxt = ((concatTxt == null) ? __Text() : concatTxt);
            configContainer = inParameters.getParameter("/WurzelIn/DSCConfig") as ConfigurationContainer;
            if (display && fasta != null)
            {
                fastaDlg = fasta.CreateAndAddUiDialogFromServiceProgram("EcuKomServiceDlg", methodName);
                fastaDlg.SpecialAction.Display = display;
            }

            outParameter.setParameter("_FASTA", fastaParameter);
        }

        private void ExecuteAdapter(object state)
        {
            if (!parentThread.IsAlive)
            {
                Log.Info("EcuKomServiceDlgImpl.ExecuteAdapter()", "Parent thread is not alive => Timer disabled.");
                executionTimer.Change(-1, -1);
            }

            if (!executionInProgress)
            {
                executionInProgress = true;
                try
                {
                    Log.Info("EcuKomServiceDlgImpl.ExecuteAdapter()", "called");
                    EDIABASAdapter eDIABASAdapter = new EDIABASAdapter(StandardErrorHandling: true, base.EcuKom, configContainer);
                    eDIABASAdapter.DoParameterization();
                    dscJob = eDIABASAdapter.Execute(inParameters);
                    SetupGUI();
                }
                catch (Exception exception)
                {
                    Log.WarningException("EcuKomServiceDlgImpl.ExecuteAdapter()", exception);
                }

                executionInProgress = false;
            }
        }

        private void SetupGUI()
        {
            try
            {
                if (dscJob != null && dscJob.ECUJob.JobErrorCode == 0)
                {
                    string text = inParameters.getParameter("/WurzelIn/StateLists/Result[0]/Path", null) as string;
                    string text2 = inParameters.getParameter("/WurzelIn/StateLists/Result[1]/Path", null) as string;
                    string text3 = inParameters.getParameter("/WurzelIn/StateLists/Result[0]/Unit", null) as string;
                    string text4 = inParameters.getParameter("/WurzelIn/StateLists/Result[1]/Unit", null) as string;
                    object parameter = inParameters.getParameter("/WurzelIn/StateLists/Result[0]/ReplaceResultWithState", false);
                    if (parameter is bool)
                    {
                        _ = (bool)parameter;
                    }

                    parameter = inParameters.getParameter("/WurzelIn/StateLists/Result[1]/ReplaceResultWithState", false);
                    if (parameter is bool)
                    {
                        _ = (bool)parameter;
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        object iSTAResultAsType = dscJob.getISTAResultAsType(text, typeof(object));
                        if (iSTAResultAsType != null)
                        {
                            try
                            {
                                bool flag = false;
                                foreach (KeyValuePair<string, object> item in inParameters.Parameter)
                                {
                                    Match match = Regex.Match(item.Key, "/WurzelIn/StateLists/Result\\[0\\]/States/State\\[\\d+\\]/Value");
                                    if (!match.Success)
                                    {
                                        continue;
                                    }

                                    string text5 = null;
                                    text5 = ((!(iSTAResultAsType is int num)) ? iSTAResultAsType.ToString() : num.ToString(CultureInfo.InvariantCulture));
                                    if (string.Compare(text5, item.Value.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        string name = match.Value.Replace("/Value", "/Text");
                                        if (inParameters.getParameter(name)is ITextLocator textLocator)
                                        {
                                            wertFeld = textLocator;
                                            flag = true;
                                        }
                                    }
                                }

                                if (!flag)
                                {
                                    foreach (KeyValuePair<string, object> item2 in inParameters.Parameter)
                                    {
                                        if (Regex.Match(item2.Key, "/WurzelIn/StateLists/Result\\[0\\]/Path").Success)
                                        {
                                            string text6 = null;
                                            text6 = ((!(iSTAResultAsType is int num2)) ? iSTAResultAsType.ToString() : num2.ToString(CultureInfo.InvariantCulture));
                                            wertFeld = new TextLocator(text6 + " " + text3);
                                        }
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                Log.WarningException("EcuKomServiceDlgImpl.SetupGUI()", exception);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(text2))
                    {
                        object iSTAResultAsType2 = dscJob.getISTAResultAsType(text2, typeof(object));
                        if (iSTAResultAsType2 != null)
                        {
                            try
                            {
                                foreach (KeyValuePair<string, object> item3 in inParameters.Parameter)
                                {
                                    Match match2 = Regex.Match(item3.Key, "/WurzelIn/StateLists/Result\\[1\\]/States/State\\[\\d+\\]/Value");
                                    if (match2.Success)
                                    {
                                        string text7 = null;
                                        text7 = ((!(iSTAResultAsType2 is int num3)) ? iSTAResultAsType2.ToString() : num3.ToString(CultureInfo.InvariantCulture));
                                        if (string.Compare(text7, item3.Value.ToString(), ignoreCase: true) == 0)
                                        {
                                            string name2 = match2.Value.Replace("/Value", "/Text");
                                            if (inParameters.getParameter(name2)is ITextLocator textLocator2)
                                            {
                                                wertFeld1 = textLocator2;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        match2 = Regex.Match(item3.Key, "/WurzelIn/StateLists/Result\\[0\\]/Path");
                                        if (match2.Success)
                                        {
                                            string text8 = null;
                                            text8 = ((!(iSTAResultAsType2 is int num4)) ? iSTAResultAsType2.ToString() : num4.ToString(CultureInfo.InvariantCulture));
                                            wertFeld1 = new TextLocator(text8 + text4);
                                        }
                                    }
                                }
                            }
                            catch (Exception exception2)
                            {
                                Log.WarningException("EcuKomServiceDlgImpl.SetupGUI()", exception2);
                            }
                        }

                        base.Model.WertFeldFlow1 = ((TextContent)wertFeld1.TextContent).GetTextForUI(logic.Lang)[0].TextItem;
                    }
                }
                else if (p_Fehlermeldung)
                {
                    if (dscJob == null)
                    {
                        wertFeld = new TextLocator("null");
                    }
                    else
                    {
                        wertFeld = __Text("51946123", new __TextParameter[2] { new __TextParameter("p1", dscJob.ECUJob.JobErrorText), new __TextParameter("p2", " ") });
                    }
                }
                else
                {
                    wertFeld = null;
                }

                if (wertFeld != null)
                {
                    base.Model.WertFeldFlow = ((TextContent)wertFeld.TextContent).GetTextForUI(logic.Lang)[0].TextItem;
                }
                else
                {
                    wertFeld = new TextLocator("null");
                }

                IList<LocalizedText> list = new List<LocalizedText>();
                if (!string.IsNullOrEmpty(concatTxt.TextContent.PlainText))
                {
                    if (fastaDlg != null)
                    {
                        _ = string.Empty;
                        list.AddRange(concatTxt.TextContent.GetTextForUI(logic.Lang));
                        fastaDlg.SpecialAction.CreateAndAddMessageText(list);
                    }
                }
                else if (fastaDlg != null)
                {
                    list.AddRange(logic.Lang.Select((string x) => new LocalizedText("(empty)", x)));
                    fastaDlg.SpecialAction.CreateAndAddMessageText(list).AddText(concatTxt.TextContent.GetTextForUI(logic.Lang));
                }
            }
            catch (Exception exception3)
            {
                Log.WarningException("EcuKomServiceDlgImpl.SetupGUI()", exception3);
            }
        }

        private new IDiagnosticDeviceResult DscSynchron(bool standardErrorHandling, ConfigurationContainer cfgContainer)
        {
            bool flag = false;
            bool flag2 = true;
            if (cfgContainer != null && parentTab != null && base.EcuKom != null)
            {
                EDIABASAdapter eDIABASAdapter = new EDIABASAdapter(StandardErrorHandling: true, base.EcuKom, cfgContainer);
                eDIABASAdapter.DoParameterization();
                IDiagnosticDeviceResult diagnosticDeviceResult = eDIABASAdapter.Execute(inParameters);
                if (diagnosticDeviceResult.ECUJob != null && diagnosticDeviceResult.ECUJob.JobResultSets > 0)
                {
                    flag = diagnosticDeviceResult.ECUJob.IsOkay((ushort)diagnosticDeviceResult.ECUJob.JobResultSets);
                }

                if (standardErrorHandling)
                {
                    if (diagnosticDeviceResult.Error != null && diagnosticDeviceResult.ECUJob != null && diagnosticDeviceResult.ECUJob.JobErrorCode != 0)
                    {
                        string text = FindEcuName(diagnosticDeviceResult.ECUJob.EcuName);
                        string text2 = FormatedData.Localize(diagnosticDeviceResult.ECUJob.JobErrorText, "EDIABAS", false, text);
                        ITextLocator textLocator = __Text("51946123", new __TextParameter[2] { new __TextParameter("p1", text2), new __TextParameter("p2", " ") });
                        if (text2.Equals("NET-0014: CONNECTION ABORTED") || text2.Equals("NET-0009: TIMEOUT"))
                        {
                            Log.Info("EcuKomServiceDlgImpl.DscSynchron()", text2);
                            ISessionLogic sessionLogic = (logic as Logic).SessionLogic;
                            sessionLogic.StopWatchDogTimer();
                            try
                            {
                                sessionLogic.ShowVciLossConnectionInEcuKomServiceDlg();
                                IProtocolBasic protocolBasic = RetrieveFasta(inParameters);
                                DateTime now = DateTime.Now;
                                Log.Info("EcuKomServiceDlgImpl.DscSynchron()", "Show question dialog");
                                IList<LocalizedText> list = new FormatedData("#NotificationMessageTitle.Error").Localize(logic.Lang);
                                IList<LocalizedText> list2 = new FormatedData("#VCILoss.ResendJob").Localize(logic.Lang);
                                bool flag3 = ShowQuestionDialog(list, list2);
                                protocolBasic?.ProtocolDialog(now, "QuestionDialog", list, list2, new string[2] { "Yes", "No" }, flag3 ? "Yes" : "No", LayoutGroup.X);
                                if (flag3)
                                {
                                    diagnosticDeviceResult = eDIABASAdapter.Execute(inParameters);
                                    flag2 = false;
                                    Log.Info("EcuKomServiceDlgImpl.DscSynchron()", "Resend job");
                                }
                                else
                                {
                                    AbortTestModule();
                                    Log.Info("EcuKomServiceDlgImpl.DscSynchron()", "Abort service dialog");
                                }
                            }
                            catch (UserCanceledException ex)
                            {
                                AbortTestModule();
                                Log.Info("EcuKomServiceDlgImpl.DscSynchron()", ex.Message);
                            }
                            finally
                            {
                                sessionLogic.StartWatchDogTimer();
                            }
                        }

                        if ((textLocator != null && (!callingModule._DoLoopHandling || lastErrorMessage == DateTime.MinValue || lastErrorMessage.AddSeconds(10.0) < DateTime.Now)) & flag2)
                        {
                            LocalizedText localizedText = new LocalizedText(textLocator.TextContent.PlainText, "en-GB");
                            logic.Services.InteractionService.RegisterMessage(new FormatedData("#Error").Localize(logic.Lang), new LocalizedText[1] { localizedText }.ToList());
                            lastErrorMessage = DateTime.Now;
                        }
                    }
                    else if ((!flag && diagnosticDeviceResult.ECUJob != null && showErrorPopupForNotOkay) & flag2)
                    {
                        string stringResult = diagnosticDeviceResult.ECUJob.getStringResult((ushort)diagnosticDeviceResult.ECUJob.JobResultSets, "JOB_STATUS");
                        if (!callingModule._DoLoopHandling || lastErrorMessage == DateTime.MinValue || lastErrorMessage.AddSeconds(10.0) < DateTime.Now)
                        {
                            try
                            {
                                string text3 = FindEcuName(diagnosticDeviceResult.ECUJob.EcuName);
                                FormatedData formatedData = new FormatedData("#Error");
                                FormatedData formatedData2 = new FormatedData("#JobStatusError", false, stringResult, text3, diagnosticDeviceResult.ECUJob.JobErrorCode)
                                {
                                    ModuleName = "EDIABAS"
                                };
                                logic.Services.InteractionService.RegisterMessage(formatedData.Localize(logic.Lang), formatedData2.Localize(logic.Lang));
                                lastErrorMessage = DateTime.Now;
                            }
                            catch (UserCanceledException)
                            {
                                Log.Info("EcuKomServiceDlgImpl.DscSynchron()", "dialog was canceled");
                            }
                        }
                    }
                }

                return diagnosticDeviceResult;
            }

            return new EDIABASAdapterDeviceResult();
        }

        private string FindEcuName(string jobEcuName)
        {
            if (!string.IsNullOrEmpty(jobEcuName) && logic.VecInfo != null && logic.VecInfo.ECU != null)
            {
                foreach (ECU item in logic.VecInfo.ECU)
                {
                    if (item.ECU_GRUPPE != null && jobEcuName.Contains(item.ECU_GRUPPE))
                    {
                        return item.TITLE_ECUTREE;
                    }

                    if (jobEcuName.Equals(item.ECU_SGBD))
                    {
                        return item.TITLE_ECUTREE;
                    }
                }
            }

            Log.Error("EcuKomServiceDlgImpl.FindEcuName()", "Did not find ECU name for jobEcuName \"{0}\". Returning \"{1}\".", jobEcuName, "unknown");
            return "unknown";
        }

        private void DoStartStopAsynch()
        {
            Log.Info("EcuKomServiceDlgImpl.DoStartStopAsynch()", "called");
            if (m_bStartPressed)
            {
                m_bStartPressed = false;
                try
                {
                    if (!p_DSCError)
                    {
                        executionTimer.Change(-1, -1);
                    }
                }
                catch
                {
                }
                finally
                {
                    base.Model.CustomButton0Content = __Text("51944459").TextContent.PlainText;
                    if (!m_IOFrage)
                    {
                        SetNextButtonEnabled(value: true);
                    }
                }
            }
            else
            {
                m_bStartPressed = true;
                p_DSCError = false;
                if (!m_IOFrage)
                {
                    SetNextButtonEnabled(value: true);
                }

                if (p_Fehlermeldung)
                {
                    executionTimer.Change(0, 500);
                }
                else
                {
                    executionTimer.Change(0, 500);
                }

                base.Model.CustomButton0Content = __Text("51942795").TextContent.PlainText;
            }

            Log.Info("EcuKomServiceDlgImpl.DoStartStopAsynch()", "_ExitIndex is: {0}", EventKind.T, 0);
        }

        private void ButtonSelection(int buttonIdx)
        {
            Log.Info("EcuKomServiceDlgImpl.ButtonSelection()", "called");
            try
            {
                switch (buttonIdx)
                {
                    case 0:
                        selectionIndex = 0;
                        break;
                    case 1:
                        selectionIndex = 1;
                        break;
                }

                SetNextButtonEnabled(value: true);
            }
            catch (Exception exception)
            {
                Log.WarningException("EcuKomServiceDlgImpl.ButtonSelection()", exception);
            }
            finally
            {
                for (int i = 0; i < base.Model.CheckedInfo.Count; i++)
                {
                    base.Model.CheckedInfo[i] = i == buttonIdx;
                }
            }
        }

        private void ProtocolFasta()
        {
            if (fastaDlg == null || !display)
            {
                return;
            }

            List<LocalizedText> list = new List<LocalizedText>();
            if (concatTxt == null && wertFeld == null)
            {
                list.AddRange(logic.Lang.Select((string x) => new LocalizedText("(empty)", x)));
                fastaDlg.SpecialAction.CreateAndAddMessageText(list);
                return;
            }

            _ = string.Empty;
            if (concatTxt != null && concatTxt.TextContent.PlainText != null)
            {
                list.AddRangeIfNotContains(concatTxt.TextContent.GetTextForUI(logic.Lang));
            }

            if (wertFeld != null && wertFeld.TextContent.PlainText != null)
            {
                list.AddRangeIfNotContains(wertFeld.TextContent.GetTextForUI(logic.Lang));
            }

            fastaDlg.SpecialAction.CreateAndAddMessageText(list);
        }

        private void WaitOnUserInteraction()
        {
            try
            {
                ServiceProgramNavigationAction serviceProgramNavigationAction;
                while (true)
                {
                    ServiceProgramAction serviceProgramAction = base.ServiceProgramController.AwaitUserAction(-1);
                    serviceProgramNavigationAction = serviceProgramAction as ServiceProgramNavigationAction;
                    if (serviceProgramNavigationAction != null)
                    {
                        break;
                    }

                    if (serviceProgramAction is ServiceProgramButtonSelectionAction serviceProgramButtonSelectionAction)
                    {
                        Log.Info("EcuKomServiceDlgImpl.WaitOnUserInteraction()", "Selected button index: {0}", serviceProgramButtonSelectionAction.SelectedIndex);
                        ButtonSelection(serviceProgramButtonSelectionAction.SelectedIndex);
                    }
                }

                Log.Info("EcuKomServiceDlgImpl.WaitOnUserInteraction()", "Navigation action: {0}", serviceProgramNavigationAction.NavigationAction);
            }
            catch (Exception exception)
            {
                Log.WarningException("EcuKomServiceDlgImpl.WaitOnUserInteraction()", exception);
            }
        }
    }
}