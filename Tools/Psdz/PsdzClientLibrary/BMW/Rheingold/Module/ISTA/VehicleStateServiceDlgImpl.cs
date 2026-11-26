using BMW.Authoring.Session;
using BMW.ISPI.TRIC.ISTA.Configuration.Constants;
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.VehicleCommunication;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BMW.Rheingold.ISTA.CoreFramework;
using BMW.Rheingold.ISTA.CoreFramework.Module;
using PsdzClient;

#pragma warning disable CS0649
namespace BMW.Rheingold.Module.ISTA
{
    internal class VehicleStateServiceDlgImpl : ServiceDlgImplBase<VehicleStateServiceDlgModel>
    {
        private bool p_WeiterButtonEnabledStack;

        private bool display;

        private IProtocolBasic fasta;

        private string callingMethod;

        private IAction<IUiDialog> fastaUiAction;

        private bool p_Quitted;

        private bool outStateZuendungMet;

        private bool outStateMotorMet;

        private bool outStateZuendungOn;

        private bool outStateMotorOn;

        private VehicleState VehicleStates = new VehicleState();

        private bool pMotorSollZustandEin;

        private bool pbFormAutomaticInstructionDisplayed;

        private IVehiclePartLocator[] vehiclePartArray;

        private IVehicleStateLocator[] vehicleStateArray;

        private string[] verificationMethodArray;

        private bool pbPTT_Device_Detected;

        private int pMotorIndex = -1;

        public int pStandlichtIndex = -1;

        public int pTesterIndex = -1;

        public int pZuendungIndex = -1;

        private ITextLocator wertFeld;

        private bool pb_ErrorJob_Status_TD_Signal;

        private bool pZuendungSollZustandEin;

        private ITextLocator txtTraceAutomaticStates = new TextLocator();

        private ITextLocator txtTracePTTStates = new TextLocator();

        private ITextLocator traceTxt = new TextLocator();

        private bool ibStateZuendungMet;

        private bool ibStateMotorMet;

        private string[] pArrayCharacteristics_PTT_EBezeichnung = new string[22]
        {
        "E93", "E38", "E52", "E39", "E90", "E92", "R50", "E86", "E83", "E60",
        "E53", "E65", "R52", "E87", "E91", "E46", "R53", "E63", "E85", "E64",
        "E61", "E66"
        };

        private string[] pArrayProductionDate_PTT_EBezeichnung = new string[22]
        {
        null, null, null, null, null, null, null, null, null, null,
        null, "2006/09", null, null, "2007/03", "2007/03", "2007/03", "2007/03", "2007/03", null,
        null, null
        };

        private bool pbIs_PTT_Case;

        public VehicleStateServiceDlgImpl(ParameterContainer inParam)
            : base(inParam)
        {
            //[+] VehicleStates = new VehicleState(logic.ClientContext);
            VehicleStates = new VehicleState(logic.ClientContext);
        }

        public override void Invoke(string method, ParameterContainer inParameters, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            p_WeiterButtonEnabledStack = IsNextButtonEnabled();
            if ("InitializeDialog".Equals(method))
            {
                InitializeGUI(inParameters);
                if (display)
                {
                    NavigateTo(base.Model);
                }
                WaitForContinue(parentTab, -1);
            }
            SetNextButtonEnabled(p_WeiterButtonEnabledStack);
        }

        public bool WaitForContinue(IModuleExecutionParent parent, int i)
        {
            try
            {
                if (pZuendungIndex >= 0 && pbIs_PTT_Case)
                {
                    WaitForContinueButton(-1);
                }
                else
                {
                    _ = pTesterIndex;
                    _ = 0;
                    ProcessAutomaticStates(ref outStateZuendungMet, ref outStateMotorMet, ref outStateZuendungOn, ref outStateMotorOn);
                    ProcessPTTStates();
                    ProcessNonAutomaticStates();
                }
                return p_Quitted;
            }
            catch (Exception exception)
            {
                Log.WarningException("VehicleStateServiceDlg.WaitForContinue()", exception);
            }
            p_Quitted = false;
            if (fastaUiAction != null)
            {
                List<LocalizedText> list = new List<LocalizedText>();
                list.AddRange(logic.Lang.Select((string x) => new LocalizedText("timout reached", x)));
                fastaUiAction.SpecialAction.AddAnswer(list, null);
            }
            return false;
        }

        private void ProcessNonAutomaticStates()
        {
            Log.Info("VehicleStateServiceDlg.ProcessNonAutomaticStates()", "called");
            ITextLocator textLocator = new TextLocator();
            ITextLocator textLocator2 = new TextLocator();
            if (!display)
            {
                return;
            }
            int num = 0;
            textLocator.TextContent = textLocator.TextContent.Concat(__Text("49952741259").TextContent);
            for (int i = 0; i < vehicleStateArray.Length; i++)
            {
                if (i != pZuendungIndex && i != pMotorIndex && !pb_ErrorJob_Status_TD_Signal && i != pStandlichtIndex)
                {
                    IVehiclePartLocator vehiclePartLocator = vehiclePartArray[i];
                    if (vehiclePartLocator != null)
                    {
                        ITextContent value = __Text(string.Empty).TextContent.Concat(vehiclePartLocator.GetDataValue("Title"));
                        ITextContent textContent = __Text("55899027083", new __TextParameter[1]
                        {
                        new __TextParameter("parameter1", value)
                        }).TextContent;
                        textLocator.TextContent = textLocator.TextContent.Concat(textContent);
                        num++;
                    }
                }
            }
            if (num > 0)
            {
                textLocator.TextContent = textLocator.TextContent.Concat(__Text("49952744587").TextContent);
                base.Model.MainText = GetContent(textLocator.TextContent);
                textLocator2.TextContent = textLocator2.TextContent.Concat(textLocator.TextContent);
                DisplayWaitCursor(value: false);
                bool isNextEnabled = parentTab.IsNextEnabled;
                SetNextButtonEnabled(value: true);
                WaitForContinueButton(-1);
                DisplayWaitCursor(value: true);
                SetNextButtonEnabled(isNextEnabled);
                SetNonAutomaticStates();
            }
            if (pStandlichtIndex < 0)
            {
                SetStateStandlicht();
            }
        }

        private void SetStateStandlicht()
        {
            Log.Info("VehicleStateServiceDlg.SetStateStandlicht()", "called");
            if (pStandlichtIndex >= 0)
            {
                if (vehicleStateArray[pStandlichtIndex].SignedId == __State("49670411").SignedId && VehicleStates.GetState(__Part("49668107")) != __State("49670411"))
                {
                    VehicleStates.SetState(vehiclePartArray[pStandlichtIndex], vehicleStateArray[pStandlichtIndex]);
                    VehicleStates.SetState(__Part("49675019"), __State("49679627"));
                    VehicleStates.SetState(__Part("49681931"), __State("49686539"));
                }
                else
                {
                    VehicleStates.SetState(vehiclePartArray[pStandlichtIndex], vehicleStateArray[pStandlichtIndex]);
                }
            }
        }

        private void SetNonAutomaticStates()
        {
            Log.Info("VehicleStateServiceDlg.SetNonAutomaticStates()", "called");
            for (int i = 0; i < vehiclePartArray.Length; i++)
            {
                if (vehiclePartArray[i].Id.ToString(CultureInfo.InvariantCulture).Equals(__Part("49633547").Id) && !pb_ErrorJob_Status_TD_Signal && vehiclePartArray[i].Id.ToString(CultureInfo.InvariantCulture).Equals(__Part("49624331").Id) && vehiclePartArray[i].Id.ToString(CultureInfo.InvariantCulture).Equals(__Part("49668107").Id) && VehicleStates.GetState(vehiclePartArray[i]) != vehicleStateArray[i])
                {
                    VehicleStates.SetState(vehiclePartArray[i], vehicleStateArray[i]);
                }
            }
        }

        private void ProcessPTTStates()
        {
            Log.Info("VehicleStateServiceDlg.ProcessPTTStates()", "called");
            ITextLocator textLocator = __Text();
            txtTracePTTStates = __Text();
            if (pbIs_PTT_Case)
            {
                textLocator = ((!pZuendungSollZustandEin) ? __Text("62193171723") : __Text("62193169035"));
                base.Model.MainText = GetContent(textLocator.TextContent);
                txtTracePTTStates.TextContent = txtTracePTTStates.TextContent.Concat(textLocator.TextContent);
                DisplayWaitCursor(value: false);
                SetNextButtonEnabled(value: true);
                parentTab.WaitForContinueButton(-1);
                DisplayWaitCursor(value: true);
                SetNextButtonEnabled(value: false);
                VehicleStates.SetState(vehiclePartArray[pZuendungIndex], vehicleStateArray[pZuendungIndex]);
                base.ResultSet.CollectiveResult = CollectiveResultSet.Ok;
            }
        }

        private void ProcessAutomaticStates(ref bool outStateZuendungMet, ref bool outStateMotorMet, ref bool outStateZuendungOn, ref bool outStateMotorOn)
        {
            Log.Info("VehicleStateServiceDlg.ProcessAutomaticStates()", "called");
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            AreAutomaticStatesMet(ref outStateZuendungMet, ref outStateMotorMet, ref outStateZuendungOn, ref outStateMotorOn);
            if (pZuendungIndex >= 0 && !pbIs_PTT_Case && (verificationMethodArray[pZuendungIndex].ToUpper() == "TEST" || verificationMethodArray[pZuendungIndex].ToUpper() == "CHECK") && flag)
            {
                VehicleStates.SetState(vehiclePartArray[pZuendungIndex], vehicleStateArray[pZuendungIndex]);
            }
            if (pMotorIndex >= 0 && (verificationMethodArray[pMotorIndex].ToUpper() == "TEST" || verificationMethodArray[pMotorIndex].ToUpper() == "CHECK") && flag2)
            {
                VehicleStates.SetState(vehiclePartArray[pMotorIndex], vehicleStateArray[pMotorIndex]);
            }
            if (pZuendungIndex >= 0 && pbIs_PTT_Case)
            {
                flag = true;
            }
            if (pZuendungIndex >= 0 && (verificationMethodArray[pZuendungIndex].ToUpper() == "TEST" || verificationMethodArray[pZuendungIndex].ToUpper() == "CHECK") && pMotorIndex < 0 && !pbIs_PTT_Case)
            {
                if (flag)
                {
                    base.ResultSet.CollectiveResult = CollectiveResultSet.Ok;
                }
                else
                {
                    base.ResultSet.CollectiveResult = CollectiveResultSet.NotOk;
                }
            }
            if (pZuendungIndex < 0 && pMotorIndex >= 0 && (verificationMethodArray[pMotorIndex].ToUpper() == "TEST" || verificationMethodArray[pMotorIndex].ToUpper() == "CHECK"))
            {
                if (flag2)
                {
                    base.ResultSet.CollectiveResult = CollectiveResultSet.Ok;
                }
                else
                {
                    base.ResultSet.CollectiveResult = CollectiveResultSet.NotOk;
                }
            }
            if (pZuendungIndex >= 0 && (verificationMethodArray[pZuendungIndex].ToUpper() == "TEST" || verificationMethodArray[pZuendungIndex].ToUpper() == "CHECK") && pMotorIndex >= 0 && (verificationMethodArray[pMotorIndex].ToUpper() == "TEST" || verificationMethodArray[pMotorIndex].ToUpper() == "CHECK"))
            {
                if (flag && flag2)
                {
                    base.ResultSet.CollectiveResult = CollectiveResultSet.Ok;
                }
                else
                {
                    base.ResultSet.CollectiveResult = CollectiveResultSet.NotOk;
                }
            }
            outStateZuendungMet = flag;
            outStateMotorMet = flag2;
            outStateZuendungOn = flag3;
            outStateMotorOn = flag4;
        }

        private void AreAutomaticStatesMet(ref bool outStateZuendungMet, ref bool outStateMotorMet, ref bool outStateZuendungOn, ref bool outStateMotorOn)
        {
            Log.Info("VehicleStateServiceDlg.AreAutomaticStatesMet()", "called");
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            if (pZuendungIndex >= 0)
            {
                if (!pbIs_PTT_Case)
                {
                    flag5 = true;
                }
                if (vehicleStateArray[pZuendungIndex].SignedId == 49626635m)
                {
                    pZuendungSollZustandEin = true;
                }
                else
                {
                    pZuendungSollZustandEin = false;
                }
            }
            if (pMotorIndex >= 0)
            {
                flag6 = true;
                if (vehicleStateArray[pMotorIndex].SignedId == 49635851m)
                {
                    pMotorSollZustandEin = true;
                }
                else
                {
                    pMotorSollZustandEin = false;
                }
            }
            base._DoLoopHandling = true;
            while (flag5 || flag6)
            {
                ReadAutomaticStatesOnce(ref outStateZuendungMet, ref outStateMotorMet, ref outStateZuendungOn, ref outStateMotorOn);
                if ((pZuendungIndex >= 0) & outStateZuendungMet)
                {
                    flag5 = false;
                }
                if (pMotorIndex >= 0)
                {
                    if (outStateMotorMet)
                    {
                        flag6 = false;
                    }
                    if (pb_ErrorJob_Status_TD_Signal)
                    {
                        flag6 = false;
                    }
                    else if (outStateMotorMet)
                    {
                        flag6 = false;
                    }
                }
                if (pZuendungIndex >= 0)
                {
                    if (verificationMethodArray[pZuendungIndex].ToUpper() != "CHECK")
                    {
                        flag5 = false;
                    }
                }
                else
                {
                    flag5 = false;
                }
                if (pMotorIndex >= 0)
                {
                    if (verificationMethodArray[pMotorIndex].ToUpper() != "CHECK")
                    {
                        flag6 = false;
                    }
                }
                else
                {
                    flag6 = false;
                }
                if (display && (flag5 || flag6))
                {
                    if ((pZuendungIndex >= 0 || pMotorIndex >= 0) && ((pZuendungIndex >= 0 && verificationMethodArray[pZuendungIndex] == "CHECK") || (pMotorIndex >= 0 && verificationMethodArray[pMotorIndex] == "CHECK")) && !pbFormAutomaticInstructionDisplayed)
                    {
                        ShowInstructionForAutomaticCheck(ibStateZuendungMet, ibStateMotorMet);
                        pbFormAutomaticInstructionDisplayed = true;
                    }
                }
                else
                {
                    flag5 = false;
                    flag6 = false;
                }
            }
            base._DoLoopHandling = false;
            if ((pZuendungIndex >= 0 && verificationMethodArray[pZuendungIndex] == "TEST" && !outStateZuendungMet) || (pMotorIndex >= 0 && verificationMethodArray[pMotorIndex] == "TEST" && !outStateMotorMet))
            {
                bool isNextEnabled = parentTab.IsNextEnabled;
                SetNextButtonEnabled(value: true);
                WaitForContinueButton(-1);
                SetNextButtonEnabled(isNextEnabled);
                ReadAutomaticStatesOnce(ref outStateZuendungMet, ref outStateMotorMet, ref outStateZuendungOn, ref outStateMotorOn);
            }
            outStateZuendungMet = flag;
            outStateMotorMet = flag2;
            outStateZuendungOn = flag3;
            outStateMotorOn = flag4;
        }

        private void ShowInstructionForAutomaticCheck(bool ibStateZuendungMet, bool ibStateMotorMet)
        {
            Log.Info("VehicleSteSeviceDlg.ShowInstructionForAutomaticCheck()", "called");
            ITextLocator textLocator = __Text();
            ITextContent textContent = __Text().TextContent;
            ITextContent textContent2 = __Text().TextContent;
            if (!pbFormAutomaticInstructionDisplayed)
            {
                textLocator.TextContent = textLocator.TextContent.Concat(__Text("49952741259").TextContent);
                if (pZuendungIndex >= 0 && !pbIs_PTT_Case && (verificationMethodArray[pZuendungIndex] == "CHECK" || verificationMethodArray[pMotorIndex] == "CHECK"))
                {
                    ITextContent theTextContent = new TextContent(vehicleStateArray[pZuendungIndex].Title);
                    textContent2 = __Text(string.Empty).TextContent.Concat(theTextContent);
                    textContent = __Text("55899027083", new __TextParameter[1]
                    {
                    new __TextParameter("parameter1", textContent2)
                    }).TextContent;
                    textLocator.TextContent = textLocator.TextContent.Concat(textContent);
                }
                if (pMotorIndex >= 0 && !pb_ErrorJob_Status_TD_Signal && (verificationMethodArray[pMotorIndex] == "CHECK" || verificationMethodArray[pZuendungIndex] == "CHECK"))
                {
                    ITextContent theTextContent2 = new TextContent(vehicleStateArray[pMotorIndex].Title);
                    textContent2 = __Text(string.Empty).TextContent.Concat(theTextContent2);
                    textContent = __Text("55899027083", new __TextParameter[1]
                    {
                    new __TextParameter("parameter1", textContent2)
                    }).TextContent;
                    textLocator.TextContent = textLocator.TextContent.Concat(textContent);
                }
                if (pTesterIndex >= 0)
                {
                    ITextContent theTextContent3 = new TextContent(vehicleStateArray[pTesterIndex].Title);
                    textContent2 = __Text(string.Empty).TextContent.Concat(theTextContent3);
                    textContent = __Text("55899027083", new __TextParameter[1]
                    {
                    new __TextParameter("parameter1", textContent2)
                    }).TextContent;
                    textLocator.TextContent = textLocator.TextContent.Concat(textContent);
                }
                base.Model.MainText = GetContent(textLocator.TextContent);
                txtTraceAutomaticStates.TextContent = txtTraceAutomaticStates.TextContent.Concat(textLocator.TextContent);
                pbFormAutomaticInstructionDisplayed = true;
            }
        }

        private void ReadAutomaticStatesOnce(ref bool outStateZuendungMet, ref bool outStateMotorMet, ref bool outStateZuendungOn, ref bool outStateMotorOn)
        {
            Log.Info("VehiclestateServiceDlg.ReadAutomaticStatesOnce", "called");
            int num = -9;
            int num2 = -9;
            sbyte defaultRes = 0;
            string text = null;
            string text2 = null;
            outStateZuendungMet = false;
            outStateMotorMet = false;
            outStateZuendungOn = false;
            outStateMotorOn = false;
            pb_ErrorJob_Status_TD_Signal = false;
            if (pZuendungIndex >= 0)
            {
                text = "";
                EcuKomStatement.ShowErrors = false;
                if (HasVehicleVariant("UNKNOWNGROUP", "DIA_DOSE"))
                {
                    EcuKomStatement.ProtocolResults = new List<string>();
                    EcuKomStatement.ProtocolResults.Add(",JOB_STATUS");
                    EcuKomStatement.ProtocolResults.Add("1,SPANNUNG_V");
                    string text3 = "";
                    text3 += "KL15";
                    IEcuJob ecuJob = EcuKomStatement.Execute("DIA_DOSE", "GET_VOLTAGE", text3, 0, FastaProtocoler);
                    if (ecuJob != null && ecuJob.IsDone())
                    {
                        text = ecuJob.getResultAs("JOB_STATUS", text, getLast: true);
                        num = ecuJob.getResultAs(1, "SPANNUNG_V", num);
                    }
                }
                else
                {
                    AddServiceCodeToProtocol(ServiceCodes.SKIPPEDECUKOM_UNKNOWNGROUP, CallingModule);
                }
                if (text == "OKAY")
                {
                    if (num >= 9000)
                    {
                        outStateZuendungOn = true;
                        if (pZuendungSollZustandEin)
                        {
                            outStateZuendungMet = true;
                        }
                    }
                    else if (!pZuendungSollZustandEin)
                    {
                        outStateZuendungMet = true;
                    }
                }
            }
            if (pMotorIndex < 0)
            {
                return;
            }
            EcuKomStatement.ShowErrors = true;
            if (HasVehicleVariant("UNKNOWNGROUP", "DIA_DOSE"))
            {
                EcuKomStatement.ProtocolResults = new List<string>();
                EcuKomStatement.ProtocolResults.Add(",JOB_STATUS");
                EcuKomStatement.ProtocolResults.Add("1,TDVALID");
                EcuKomStatement.ProtocolResults.Add("1,TDVALUE");
                string param = "";
                IEcuJob ecuJob2 = EcuKomStatement.Execute("DIA_DOSE", "STATUS_TD_SIGNAL", param, 0, FastaProtocoler);
                if (ecuJob2 != null && ecuJob2.IsDone())
                {
                    text2 = ecuJob2.getResultAs("JOB_STATUS", text2, getLast: true);
                    defaultRes = ecuJob2.getResultAs(1, "TDVALID", defaultRes);
                    num2 = ecuJob2.getResultAs(1, "TDVALUE", num2);
                }
            }
            else
            {
                AddServiceCodeToProtocol(ServiceCodes.SKIPPEDECUKOM_UNKNOWNGROUP, CallingModule);
            }
            if (text2 == "OKAY")
            {
                if (num2 > 0)
                {
                    outStateMotorOn = true;
                    if (pMotorSollZustandEin)
                    {
                        outStateMotorMet = true;
                    }
                }
                else if (!pMotorSollZustandEin)
                {
                    outStateMotorMet = true;
                }
            }
            else
            {
                pb_ErrorJob_Status_TD_Signal = true;
            }
        }

        private void InitializeGUI(ParameterContainer inParameters)
        {
            fasta = RetrieveFasta(inParameters);
            if (fasta == null)
            {
                Log.Error("VehicleStateServiceDlgImpl.ctor", "FASTA protocoling not possible.");
            }
            if (string.IsNullOrEmpty(callingMethod))
            {
                callingMethod = inParameters.getParameter("methodname") as string;
            }
            int num = 0;
            foreach (KeyValuePair<string, object> item in inParameters.Parameter)
            {
                if (!string.IsNullOrEmpty(item.Key) && Regex.Match(item.Key, "/WurzelIn/Vehicle/VehicleParts\\[\\d*\\]/VehiclePart").Success)
                {
                    num++;
                }
            }
            vehiclePartArray = new IVehiclePartLocator[num];
            vehicleStateArray = new IVehicleStateLocator[num];
            verificationMethodArray = new string[num];
            ITextLocator textLocator = new TextLocator();
            for (int i = 0; i < num; i++)
            {
                if (inParameters.getParameter($"/WurzelIn/Vehicle/VehicleParts[{i}]/VehiclePart") is IVehiclePartLocator vehiclePartLocator)
                {
                    vehiclePartArray[i] = vehiclePartLocator;
                }
                if (inParameters.getParameter($"/WurzelIn/Vehicle/VehicleParts[{i}]/VehicleState") is IVehicleStateLocator vehicleStateLocator)
                {
                    vehicleStateArray[i] = vehicleStateLocator;
                    ITextLocator theTextLocator = __Text("55899027083", new __TextParameter[1]
                    {
                    new __TextParameter("parameter1", new TextLocator($"<spe:TEXTITEM xmlns:spe='http://bmw.com/2014/Spe_Text_2.0'><spe:PARAGRAPH>{vehicleStateArray[i].Title}</spe:PARAGRAPH></spe:TEXTITEM>"))
                    });
                    textLocator = textLocator.Concat(theTextLocator);
                }
                string text = inParameters.getParameter($"/WurzelIn/Vehicle/VehicleParts[{i}]/VerificationMethod") as string;
                verificationMethodArray[i] = (string.IsNullOrEmpty(text) ? "CHECK" : text);
            }
            display = (bool)inParameters.getParameter("Display", true);
            ReadPositions(num);
            PttHandling();
            if (pbIs_PTT_Case && pZuendungIndex >= 0)
            {
                if (pZuendungSollZustandEin)
                {
                    traceTxt = __Text("62193169035");
                }
                else
                {
                    traceTxt = __Text("62193171723");
                }
            }
            else
            {
                traceTxt = __Text("49952741259");
                traceTxt = traceTxt.Concat(textLocator);
                wertFeld = __Text("49952744587");
            }
            if (display && traceTxt != null)
            {
                base.Model.MainText = GetContent(traceTxt.TextContent);
                if (wertFeld != null)
                {
                    base.Model.ValueField = GetContent(wertFeld.TextContent);
                }
            }
            if (fasta == null)
            {
                return;
            }
            fastaUiAction = fasta.CreateAndAddUiDialogFromServiceProgram("VehicleStateServiceDlg", callingMethod);
            fastaUiAction.SpecialAction.Display = display;
            List<LocalizedText> list = new List<LocalizedText>();
            if (traceTxt != null && !string.IsNullOrEmpty(traceTxt.TextContent.PlainText))
            {
                list.AddRange(traceTxt.TextContent.GetTextForUI(logic.Lang));
                fastaUiAction.SpecialAction.CreateAndAddMessageText(list);
                return;
            }
            list.AddRange(logic.Lang.Select((string x) => new LocalizedText("(empty)", x)));
            fastaUiAction.SpecialAction.CreateAndAddMessageText(list);
        }

        private void ReadPositions(int parameterCount)
        {
            Log.Info("VehicleStateServiceDlg.ReadPositions", "called");
            for (int i = 0; i < parameterCount; i++)
            {
                switch (vehiclePartArray[i].Id.ToString(CultureInfo.InvariantCulture))
                {
                    case "49633547":
                        pMotorIndex = i;
                        break;
                    case "49624331":
                        pZuendungIndex = i;
                        break;
                    case "49668107":
                        pStandlichtIndex = i;
                        break;
                    case "49617419":
                        pTesterIndex = i;
                        break;
                }
            }
        }

        private void PttHandling()
        {
            bool resultOK = true;
            string interfaceType = null;
            bool flag = false;
            long num = -1L;
            int num2 = -1;
            string text = null;
            int num3 = 0;
            int num4 = 0;
            string text2 = null;
            if (pZuendungIndex >= 0)
            {
                GetCommunicationInterfaceType(ref interfaceType, ref resultOK);
                if (interfaceType == VCIDeviceType.EDIABAS.ToString() && resultOK && pArrayCharacteristics_PTT_EBezeichnung != null && num >= 0)
                {
                    for (int i = 0; i < pArrayCharacteristics_PTT_EBezeichnung.Length; i++)
                    {
                        if (logic.VecInfo.Ereihe == pArrayCharacteristics_PTT_EBezeichnung[i])
                        {
                            flag = true;
                            num2 = i;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    num3 = Convert.ToInt32(logic.VecInfo.Modelljahr, CultureInfo.InvariantCulture);
                    if (num3 <= 1900 || num3 <= DateTime.Now.Year)
                    {
                        Log.Error("VehicleStetDlg", " invalid value for vehicle production year:" + num3);
                        num3 = 0;
                    }
                    num4 = Convert.ToInt32(logic.VecInfo.Modellmonat, CultureInfo.InvariantCulture);
                    if (num4 < 1 && num4 > 12)
                    {
                        Log.Error("VehiclestateDlg", " invalid value for vehicle production month:" + num4);
                        num4 = 0;
                    }
                }
                if (flag)
                {
                    if (pArrayProductionDate_PTT_EBezeichnung[num2] != null)
                    {
                        text = pArrayProductionDate_PTT_EBezeichnung[num2];
                        if (text != null)
                        {
                            string text3 = null;
                            string text4 = null;
                            int num5 = 0;
                            int num6 = 0;
                            int num7 = 0;
                            try
                            {
                                text2 = "AnalyseProductionDate";
                                num7 = text.IndexOf("/", StringComparison.Ordinal);
                                if (num7 > 0)
                                {
                                    text3 = text.Substring(0, num7);
                                    text4 = text.Substring(num7 + 1, text.Length - num7 - 1);
                                    if (text3 != null)
                                    {
                                        num5 = Convert.ToInt32(text3, CultureInfo.InvariantCulture);
                                    }
                                    if (text4 != null)
                                    {
                                        num6 = Convert.ToInt32(text4, CultureInfo.InvariantCulture);
                                    }
                                }
                                else
                                {
                                    Log.Error("VehicleStateDlg", "The configured PTT date has wrong format '" + text + "'. Expected=YYYY/MM");
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("VehicleStateDlg", " Exception occured in  InitializeDialog() Label=" + text2 + ":" + ex);
                            }
                            if (num7 > 0)
                            {
                                if (num3 > 0 && num3 < num5)
                                {
                                    pbIs_PTT_Case = true;
                                }
                                else if (num3 > 0 && num3 == num5 && num4 > 0 && num4 <= num6)
                                {
                                    pbIs_PTT_Case = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        pbIs_PTT_Case = true;
                    }
                }
            }
            Log.Debug("VehicleStateDlg()", " Result of PTT Determination PTT-Device-detected=" + pbPTT_Device_Detected + " IsPTTCase=" + pbIs_PTT_Case + " ResultOK=" + resultOK + " FoundInArrayEBez=" + flag + " Index of CharacteristicEBez=" + num2);
        }

        private void GetCommunicationInterfaceType(ref string interfaceType, ref bool resultOK)
        {
            Log.Info("GetCommunicationInterfaceType", "called");
            resultOK = true;
            interfaceType = null;
            if (logic.VecInfo != null && logic.VecInfo.VCI != null && logic.VecInfo.VCI.DevType != null)
            {
                interfaceType = logic.VecInfo.VCI.VCIType.ToString();
            }
            else
            {
                resultOK = false;
            }
        }

        [PreserveSource(Hint = "static removed", SignatureModified = true)]
        private new IVehiclePartLocator __Part(string refText)
        {
            return new VehiclePart(refText, logic.ClientContext);
        }
    }
}
