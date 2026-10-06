using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Measurement.Common.Contract;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Localization;
using BMW.Rheingold.ISTA.CoreFramework;
using BMW.Rheingold.Measurement;
using BMW.Rheingold.Measurement.Common;
using BMW.Rheingold.Measurement.Common.Data;
using BMW.Rheingold.MeasurementCommunication;

namespace BMW.Rheingold.Module.ISTA
{
    internal class MeasuringServiceDlgImpl : ServiceDlgImplBase<MeasuringServiceDlgModel>
    {
        private string method;
        private ParameterContainer outParameter;
        private ISTAModule callingModule;
        private bool showView;
        private string unit1Configured;
        private string unit1Parameter;
        private string measuringUnit1Fasta;
        private IDmmManager manager;
        private string measuredFunction1;
        private DmmMeasuredValueMinMax valueMeasured;
        private string measuredValue1Fasta;
        private DateTime measuringEnd;
        private bool measuerd;
        private DateTime measuringStart;
        private bool bERROR;
        private IProtocolBasic fasta;
        private IAction<IUiDialog> fastaUiDlgAction;
        private ITextLocator txtAdaptionText;
        private ITextLocator txtToleranzFeldFrageText;
        public MeasuringServiceDlgImpl(ParameterContainer inParam) : base(inParam)
        {
            _globalModuleInParameter = inParam;
            __handleInParameter();
            callingModule = inParam.getParameter("__CallingModule__") as ISTAModule;
        }

        private void ShowErrorMessage(string message, string details = "")
        {
            logic.Services.InteractionService.RegisterMessage(new FormatedData("#Error").Localize(), message, details);
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            txtAdaptionText = inParam.getParameter("AdaptionsText", TextLocator.Empty) as ITextLocator;
            txtToleranzFeldFrageText = inParam.getParameter("ToleranzFeldFrageText", null) as ITextLocator;
            string adaptionsText = "";
            string toleranzFrage = "";
            if (!string.IsNullOrEmpty(txtToleranzFeldFrageText?.TextContent.PlainText))
            {
                toleranzFrage = GetContent(txtToleranzFeldFrageText?.TextContent);
            }

            if (!string.IsNullOrEmpty(txtAdaptionText?.TextContent.PlainText))
            {
                adaptionsText = GetContent(txtAdaptionText?.TextContent);
            }

            InitializeInputFields(adaptionsText, toleranzFrage);
            this.method = method;
            outParameter = outParam;
            ConfigurationContainer config = inParam.getParameter("DSCConfig1") as ConfigurationContainer;
            bool flag = (bool)inParam.getParameter("Display", true);
            MeasuringConfigurationType measuringConfigurationType = new MeasuringConfigurationAdapter(config).ParseParametrization();
            showView = inParam.getParameter("Display", "true").ToString().ToLower().Equals("true");
            IDeviceImib deviceImib = null;
            Model.IsManualInput = false;
            if (MeasurementLauncher != null)
            {
                deviceImib = MeasurementLauncher.ReserveMeasurementDevice();
            }

            if (deviceImib == null)
            {
                Model.IsManualInput = true;
                Log.Warning("MeasuringServiceDlgImpl.Invoke()", "Measurementlauncher is not available.");
            }

            object parameter = inParam.getParameter("BothChannels");
            Model.IsCh2 = parameter != null && "true".Equals(parameter.ToString().ToLower());
            unit1Parameter = inParam.getParameter("Unit1") as string;
            unit1Configured = ConvertFunctionToUnit(measuringConfigurationType.Dmm.Channel[0].Function);
            Model.MeasuringUnit2 = string.Empty;
            if (Model.IsCh2)
            {
                Log.Error("MeasuringServiceDlgImpl.Invoke()", "Service dialog should have two channels, but channel two is not configured.");
            }

            if (Model.IsManualInput)
            {
                Model.MeasuringUnit1 = unit1Parameter;
                measuringUnit1Fasta = unit1Parameter;
                Model.IsCh1Measure = false;
                Model.IsCh1Enter = true;
                if (flag)
                {
                    SetKeyboardEnabled(enable: true);
                }
            }
            else
            {
                Model.MeasuringUnit1 = unit1Configured;
                Model.IsCh1Measure = true;
                Model.IsCh1Enter = false;
                manager = MeasurementLauncher.CreateAndInititalizeDmm();
                manager.ToggleHold(1);
                manager.ToggleHold(2);
                manager.ActivateMinMax(1, activate: true);
                manager.LoadModel(measuringConfigurationType);
                measuredFunction1 = manager.FunctionName(1);
                Log.Info("MeasuringServiceDlgImpl.Invoke()", "Register MeasuredValueChanged.");
                manager.Model[1].PropertyChanged += MeasuredValueChanged;
                try
                {
                    manager.ToggleHold(1);
                    _ = Model.IsCh2;
                    manager.ToggleHold(2);
                    measuringStart = DateTime.Now;
                    manager.Start();
                }
                catch (ImibConfigException ex)
                {
                    Log.ErrorException("MeasuringServiceDlgImpl.Invoke()", ex);
                    if (ex.ErrorCode.Equals("SensorNotConnected") && !string.IsNullOrEmpty(ex.MeasuringSource))
                    {
                        string text = FormatedData.Localize(ex.MeasuringSource, "Measurement", false);
                        FormatedData formatedData = new FormatedData("#SensorMissing", false, text);
                        formatedData.ModuleName = "Measurement";
                        ShowErrorMessage(formatedData.Localize());
                    }
                    else if (ex.ErrorCode.Equals("SensorNotConnected") && !string.IsNullOrEmpty(ex.LanguageId))
                    {
                        string message = FormatedData.Localize(ex.LanguageId, "ISTAGui", false);
                        ShowErrorMessage(message);
                    }

                    bERROR = true;
                }
            }

            string answerText;
            if (showView)
            {
                SetNextButtonEnabled(value: true);
                WaitForContinue();
                answerText = "NEXT button pressed";
            }
            else
            {
                if (deviceImib != null)
                {
                    WaitForValueMeasured();
                }
                else
                {
                    bERROR = true;
                }

                answerText = "--";
            }

            fasta = FastaProtocoler;
            if (fasta == null)
            {
                fasta = RetrieveFasta(inParam);
                if (fasta == null)
                {
                    Log.Error("MeasuringServiceDlgImpl.Invoke()", "FASTA protocoling not possible.");
                }
            }

            FinishDialog(answerText);
        }

        private void InitializeInputFields(string adaptionsText, string toleranzFrage)
        {
            Model.AdaptionsTextFlow = adaptionsText;
            Model.ToleranzFeldFrageText = toleranzFrage;
            bool flag = !string.IsNullOrEmpty(Model.ToleranzFeldFrageText);
            Model.IsToleranzFrage = flag;
            if (flag)
            {
                Model.Button1Text = __Text("51907851").TextContent.PlainText;
                Model.Button2Text = __Text("51910795").TextContent.PlainText;
            }
            else
            {
                Model.Button1Text = string.Empty;
                Model.Button2Text = string.Empty;
            }
        }

        private string ConvertFunctionToUnit(string function)
        {
            if (function == null)
            {
                return string.Empty;
            }

            try
            {
                return function.ParseEnum<MeasuringFunction>().Unit();
            }
            catch (Exception ex)
            {
                Log.Error("MeasuringServiceDlgImpl.ConvertFunctionToUnit()", "Failed to convert function \"{0}\": {1}", function, ex);
                return string.Empty;
            }
        }

        private void MeasuredValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("MeasuredValue".Equals(e.PropertyName))
            {
                valueMeasured = manager.Model[1].MeasuredValue;
                string[] array = DmmChannelData.FormatMeasuringValue4(valueMeasured.Value, valueMeasured.Range, valueMeasured.RangeStatus, unit1Configured);
                Model.MeasuredValue1 = array[0];
                Model.MeasuringUnit1 = array[1];
                measuredValue1Fasta = array[2];
                measuringUnit1Fasta = array[3];
                measuringEnd = DateTime.Now;
                //[-] if (BMW.Rheingold.Measurement.Measurement.DebugLevel > 0)
                {
                    Log.Debug("MeasuringServiceDlgImpl.MeasuredValueChanged()", "Measured value {0}, shown value {1}.", measuredValue1Fasta + " " + measuringUnit1Fasta, Model.MeasuredValue1 + " " + Model.MeasuringUnit1);
                }

                if (Model.IsCh2)
                {
                    Model.MeasuredValue2 = Math.Round(manager.MeasuredValue(2).Value, 3);
                }

                measuerd = true;
            }
        }

        private void WaitForValueMeasured()
        {
            while (!measuerd && !bERROR)
            {
                Thread.Sleep(1000);
            }
        }

        private void FinishDialog(string answerText)
        {
            if (!Model.IsManualInput)
            {
                try
                {
                    if (Model.IsCh2)
                    {
                        manager.Model[2].PropertyChanged -= MeasuredValueChanged;
                    }

                    manager.Model[1].PropertyChanged -= MeasuredValueChanged;
                    Log.Info("MeasuringServiceDlgImpl.SetOutParameters()", "Deregister MeasuredValueChanged.");
                    manager.Stop();
                }
                catch (Exception ex)
                {
                    Log.Error("MeasuringServiceDlgImpl.FinishDialog()", "Failed to stop manager: {0}", ex.ToString());
                }
                finally
                {
                    MeasurementLauncher.ReleaseDmmManagement();
                }
            }

            try
            {
                SetOutParameters(answerText);
            }
            catch (Exception ex2)
            {
                Log.Error("MeasuringServiceDlgImpl.FinishDialog()", "Failed to set output parameter: {0}", ex2.ToString());
            }

            manager = null;
        }

        private void SetOutParameters(string answer)
        {
            if (Model.IsManualInput)
            {
                if (!double.TryParse(Model.MeasuredValue1, out var result))
                {
                    Log.Error("MeasuringServiceDlgImpl.SetOutParameters()", "Failed to parse {0} to double.", Model.MeasuredValue1);
                }

                SetOutParameters("MinValue", 0.0);
                SetOutParameters("MaxValue", 0.0);
                SetOutParameters("Value1", result);
            }
            else
            {
                if (Model.IsCh2)
                {
                    outParameter.setParameter("MinValue", (double)manager.MeasuredValue(2).Min);
                    outParameter.setParameter("MaxValue", (double)manager.MeasuredValue(2).Max);
                }

                double num = UnitHelper.Factor(unit1Parameter);
                SetOutParameters("MinValue", (double)manager.MeasuredValue(1).Min / num);
                SetOutParameters("MaxValue", (double)manager.MeasuredValue(1).Max / num);
                SetOutParameters("Value1", (double)manager.MeasuredValue(1).Value / num);
            }

            outParameter.setParameter("ERROR", bERROR);
            outParameter.setParameter("TimeStamp", DateTime.Now.Ticks);
            outParameter.setParameter("Value2", Model.MeasuredValue2);
            CreateFasta2(answer);
        }

        public static string EscapedUnits(string unit)
        {
            if (!string.IsNullOrEmpty(unit))
            {
                return unit.Replace("Ω", "Ohm").Replace("Ω", "Ohm").Replace("°", "Grad").Replace("µ", "mu");
            }

            return string.Empty;
        }

        private void CreateFasta2(string answer)
        {
            if (fasta == null)
            {
                Log.Error("MeasuringServiceDlgImpl.CreateFasta2", "FASTA 2 is not available.");
                return;
            }

            if (Model.IsManualInput)
            {
                measuringStart = DateTime.Now;
                measuringEnd = measuringStart;
                measuredValue1Fasta = Model.MeasuredValue1;
                if (string.IsNullOrEmpty(measuredValue1Fasta))
                {
                    measuredValue1Fasta = "n/a";
                }
            }

            List<LocalizedText> list = new List<LocalizedText>();
            string adaption = ((txtAdaptionText != null && !string.IsNullOrEmpty(txtAdaptionText.Text)) ? txtAdaptionText.Text : "n/a");
            if (txtAdaptionText != null)
            {
                list.AddRange(txtAdaptionText.TextContent.GetTextForUI(logic.Lang));
            }

            if (!string.IsNullOrEmpty(measuringUnit1Fasta))
            {
            //[-] fasta.AddMeasuringAction(measuringStart, measuringEnd, ActionResult.Success, measuredFunction1 ?? "na", measuredValue1Fasta, EscapedUnits(measuringUnit1Fasta), "na", "na", adaption, "CH1", "IMIB", logic.Lang);
            }

            if (!string.IsNullOrEmpty(Model.MeasuringUnit2))
            {
                string measuredVariable = ((manager != null && !string.IsNullOrEmpty(manager.FunctionName(2))) ? manager.FunctionName(2) : "na");
                //[-] fasta.AddMeasuringAction(measuringStart, measuringEnd, ActionResult.Success, measuredVariable, Model.MeasuredValue2 + string.Empty, EscapedUnits(Model.MeasuringUnit2), "na", "na", adaption, "CH2", "IMIB", logic.Lang);
            }

            if (Model.IsToleranzFrage && txtToleranzFeldFrageText != null)
            {
                list.AddRange(txtToleranzFeldFrageText.TextContent.GetTextForUI(logic.Lang));
            }

            fastaUiDlgAction = fasta.CreateAndAddUiDialogFromServiceProgram("MeasuringServiceDlgImpl", method);
            fastaUiDlgAction.SpecialAction.Display = showView;
            IMessageText messageText = fastaUiDlgAction.SpecialAction.CreateAndAddMessageText(list);
            if (list.Count > 0)
            {
                messageText.AddText(list);
            }

            IList<LocalizedText> list2 = new List<LocalizedText>();
            list2.AddRange(logic.Lang.Select((string x) => new LocalizedText(answer, x)));
            fastaUiDlgAction.SpecialAction.AddAnswer(list2, null);
        }

        private void SetOutParameters(string key, double value)
        {
            Log.Info("MeasuringServiceDlgImpl.SetOutParameters()", string.Format(CultureInfo.InvariantCulture, "{0} = {1} {2}", key, value, unit1Parameter));
            outParameter.setParameter(key, value);
        }

        private void WaitForContinue()
        {
            try
            {
                DisplayWaitCursor(value: false);
                bool nextButtonEnabled = IsNextButtonEnabled();
                while (true)
                {
                    ServiceProgramAction serviceProgramAction = ServiceProgramController.AwaitUserAction(-1);
                    if (serviceProgramAction is ServiceProgramNavigationAction)
                    {
                        break;
                    }

                    if (serviceProgramAction is ServiceProgramMeasuringDlgAction serviceProgramMeasuringDlgAction)
                    {
                        Model.MeasuredValue1 = serviceProgramMeasuringDlgAction.Value1;
                        Model.MeasuredValue2 = serviceProgramMeasuringDlgAction.Value2;
                        Model.IsAnswer1 = serviceProgramMeasuringDlgAction.IsAnswer1;
                        Model.IsAnswer2 = serviceProgramMeasuringDlgAction.IsAnswer2;
                        SetNextButtonEnabled(value: true);
                    }
                }

                if (Model.IsAnswer1)
                {
                    callingModule.ResultSet.CollectiveResult = CollectiveResultSet.Ok;
                }
                else if (Model.IsAnswer2)
                {
                    callingModule.ResultSet.CollectiveResult = CollectiveResultSet.NotOk;
                }

                SetNextButtonEnabled(nextButtonEnabled);
            }
            catch (Exception exception)
            {
                Log.WarningException("MeasuringServiceDlgImpl.WaitForContinue()", exception);
            }
        }
    }
}
