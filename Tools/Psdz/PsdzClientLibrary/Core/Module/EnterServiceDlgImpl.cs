using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using PsdzClient;

#pragma warning disable CS0649
namespace BMW.Rheingold.Module.ISTA
{
    internal class EnterServiceDlgImpl : ServiceDlgImplBase<EnterServiceDlgModel>
    {
        [PreserveSource(SuppressWarning = true)]
        private enum DataType
        {
            Numerical,
            AlphaNumerical,
            Decimal,
            Hex,
            RDCTriggerTool
        }

        private readonly ParameterContainer fastaParameter = new ParameterContainer();
        private readonly ParameterContainer outParameter;
        private DataType dataType = DataType.AlphaNumerical;
        private ITextLocator txtParam;
        private int maxTextLength = 1024;
        private IProtocolBasic fasta;
        private string rdcTriggerToolReport;
        private IAction<IUiDialog> fastaDialog;
        private string callingMethod;
        private ISTAModule callingModule;
        private DateTime startTime;
        private bool rdcResponseReceived;
        private readonly decimal hintIDNumeric = 2000050451914m;
        private readonly decimal hintIDAlphaNumeric = 2000050451915m;
        private readonly decimal hintIDDecimal = 2000050451916m;
        private readonly decimal hintIDHex = 2000050451917m;
        public EnterServiceDlgImpl(ParameterContainer inParam) : base(inParam)
        {
            Log.Info("EnterServiceDlg.EnterServiceDlgImpl()", "called");
            _globalModuleInParameter = inParam;
            fasta = RetrieveFasta(inParam);
            callingModule = inParam.getParameter("__CallingModule__") as ISTAModule;
            outParameter = new ParameterContainer();
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            Log.Info("EnterServiceDlg.Invoke()", "called with parameter: {0}", inParam);
            SetUpInParameters(method, inParam);
            if ("ServiceCodeProtokoll".Equals(method))
            {
                string name = ((inParam.getParameter("ServiceCodeName", null)is ITextLocator textLocator) ? textLocator.TextContent.PlainText : "n/a");
                string value = inParam.getParameter("ServiceCodeWert", "n/a") as string;
                ParameterContainer parameter = new ParameterContainer();
                outParam.setParameter("_FASTA", parameter);
                if (FastaProtocoler != null)
                {
                    DetermineLayoutGroup(GetInfoObjStarted());
                    FastaProtocoler.AddServiceCode(name, value, LayoutGroup.X);
                }
                else
                {
                    Log.Warning("EnterServiceDlgCmd.DoInvoke()", "FASTA is not available.");
                }
            }
            else
            {
                InitDialog(inParam, inoutParam);
                WaitForContinue(-1);
                FinishDialog(inoutParam);
            }
        }

        protected override ParameterContainer AfterInvoke(string method)
        {
            return outParameter;
        }

        public ParameterContainer FinishDialog(ParameterContainer inoutParam)
        {
            outParameter.setParameter("Result", base.Model.TextInput);
            outParameter.setParameter("_FASTA", fastaParameter);
            if (fasta != null)
            {
                fastaDialog = fasta.CreateAndAddUiDialogFromServiceProgram("EnterServiceDlg", callingMethod);
                fastaDialog.StartTime = startTime;
                List<LocalizedText> list = new List<LocalizedText>();
                list.AddRange(logic.Lang.Select((string x) => new LocalizedText("(empty)", x)));
                if (txtParam != null && !string.IsNullOrEmpty(txtParam.TextContent.Text))
                {
                    list = new List<LocalizedText>();
                    list.AddRange(txtParam.TextContent.GetTextForUI(logic.Lang));
                }

                fastaDialog.SpecialAction.CreateAndAddMessageText(list);
                string answerText = ((string.IsNullOrEmpty(base.Model.TextInput) || string.IsNullOrEmpty(base.Model.TextInput.Trim())) ? "(empty)" : base.Model.TextInput);
                List<LocalizedText> list2 = new List<LocalizedText>();
                list2.AddRange(logic.Lang.Select((string x) => new LocalizedText(answerText, x)));
                fastaDialog.SpecialAction.AddAnswer(list2, null);
            }
            else
            {
                Log.Error("EnterServiceDlg.FinishDialog()", "FASTA 2 protocoling not possible.");
            }

            return outParameter;
        }

        public void InitDialog(ParameterContainer inParameters, ParameterContainer inoutParam)
        {
            fasta = RetrieveFasta(inParameters);
            startTime = DateTime.Now;
            callingMethod = inParameters.getParameter("methodname") as string;
            SetNextButtonEnabled(value: false);
            if (inParameters == null)
            {
                Log.Warning("EnterServiceDlg.InitializeGUI()", "InParameters was null");
                return;
            }

            bool num = (bool)inParameters.getParameter("MaxTextLengthUsed", false);
            txtParam = inParameters.getParameter("txtParam", TextLocator.Empty) as ITextLocator;
            base.Model.TxtParamFlow = GetContent(txtParam.TextContent);
            if (num)
            {
                object parameter = inParameters.getParameter("MaxTextLength");
                if (parameter != null)
                {
                    maxTextLength = (int)parameter;
                    base.Model.TextInputMaxLength = maxTextLength;
                }
            }
            else
            {
                base.Model.TextInputMaxLength = 0;
            }

            dataType = (DataType)inParameters.getParameter("Datentyp", 1);
            SetInputTypeHint(dataType);
            if (dataType == DataType.RDCTriggerTool)
            {
                SetUpRDCTriggertool();
            }

            SetKeyboardEnabled(enable: true);
        }

        [PreserveSource(Cleaned = true)]
        private void SetUpRDCTriggertool()
        {
        }

        public void WaitForContinue(int timeout)
        {
            try
            {
                DisplayWaitCursor(value: false);
                bool flag = false;
                ServiceProgramNavigationAction serviceProgramNavigationAction;
                do
                {
                    if (rdcResponseReceived && !flag)
                    {
                        base.Model.TextInput = rdcTriggerToolReport;
                        flag = true;
                        TextChangedCommand();
                    }

                    ServiceProgramAction serviceProgramAction = base.ServiceProgramController.AwaitUserAction(1000);
                    if (serviceProgramAction is ServiceProgramTextChangedAction serviceProgramTextChangedAction)
                    {
                        Log.Info("EnterServiceDlg.WaitForContinue()", "Keyboard action: {0}", serviceProgramTextChangedAction.NewText);
                        base.Model.TextInput = serviceProgramTextChangedAction.NewText;
                        TextChangedCommand();
                    }

                    serviceProgramNavigationAction = serviceProgramAction as ServiceProgramNavigationAction;
                }
                while (serviceProgramNavigationAction == null);
                Log.Info("EnterServiceDlg.WaitForContinue()", "Navigation action: {0}", serviceProgramNavigationAction.NavigationAction);
            }
            catch (Exception exception)
            {
                Log.WarningException("EnterServiceDlg.WaitForContinue()", exception);
            }
        }

        private void SetUpInParameters(string method, ParameterContainer inParam)
        {
            Log.Info("EnterServiceDlgImpl.SetUpInParameters", "Set up InParameters for method {0}", method);
            inParam.setParameter("methodname", method);
            switch (method)
            {
                case "InitializeDialog":
                    inParam.setParameter("MaxTextLengthUsed", false);
                    break;
                case "InitializeDialog2":
                    inParam.setParameter("MaxTextLengthUsed", true);
                    break;
                case "ServiceCodeEingabe_und_Protokoll":
                {
                    ITextLocator parameter = (ITextLocator)inParam.getParameter("AnzeigeText", TextLocator.Empty);
                    inParam.setParameter("AnzeigeText", parameter);
                    inParam.setParameter("MaxTextLengthUsed", true);
                    break;
                }

                case "ServiceCodeProtokoll":
                    Log.Info("EnterServiceDlgImpl.SetUpInParameters()", "nothing to do here");
                    break;
                default:
                    throw new ServiceDialogMethodUnsupportedException();
            }
        }

        private EnumKeyboardDataType GetDataTypeEnum(int dataType)
        {
            switch (dataType)
            {
                case 0:
                    return EnumKeyboardDataType.numeric10;
                case 1:
                    return EnumKeyboardDataType.alphanumeric;
                case 2:
                    return EnumKeyboardDataType.numeric14;
                case 3:
                    return EnumKeyboardDataType.numericHex;
                case 4:
                    return EnumKeyboardDataType.barcode;
                default:
                    return EnumKeyboardDataType.alphanumeric;
            }
        }

        private void TextChangedCommand()
        {
            string textInput = base.Model.TextInput;
            try
            {
                if (string.IsNullOrEmpty(textInput) || textInput.Trim().Length > maxTextLength)
                {
                    SetNextButtonEnabled(value: false);
                    return;
                }

                long result;
                switch (dataType)
                {
                    case DataType.Numerical:
                        SetNextButtonEnabled(long.TryParse(textInput.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result));
                        break;
                    case DataType.AlphaNumerical:
                        SetNextButtonEnabled(value: true);
                        break;
                    case DataType.Decimal:
                    {
                        SetNextButtonEnabled(decimal.TryParse(textInput.Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, new CultureInfo(ConfigSettings.CurrentUICulture, useUserOverride: false), out var _));
                        break;
                    }

                    case DataType.Hex:
                        SetNextButtonEnabled(long.TryParse(textInput.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result));
                        break;
                    default:
                        SetNextButtonEnabled(value: true);
                        break;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("EnterServiceDlg.OnTextChanged()", exception);
                SetNextButtonEnabled(value: false);
            }
        }

        private void SetInputTypeHint(DataType dataType)
        {
            try
            {
                decimal num = hintIDAlphaNumeric;
                switch (dataType)
                {
                    case DataType.Numerical:
                        num = hintIDNumeric;
                        break;
                    case DataType.AlphaNumerical:
                        num = hintIDAlphaNumeric;
                        break;
                    case DataType.Decimal:
                        num = hintIDDecimal;
                        break;
                    case DataType.Hex:
                        num = hintIDHex;
                        break;
                    default:
                        num = hintIDAlphaNumeric;
                        Log.Error("EnterServiceDlgImpl.SetInputTypeHint()", "DataType not recognized. Setting InputHintType to AlphaNumeric by default.", dataType);
                        break;
                }

                base.Model.InputTypeHint = __Text(num.ToString()).TextContent.PlainText;
            }
            catch (Exception ex)
            {
                Log.Error("EnterServiceDlgImpl.InitDialog()", "Exception was thrown", ex);
            }
        }
    }
}