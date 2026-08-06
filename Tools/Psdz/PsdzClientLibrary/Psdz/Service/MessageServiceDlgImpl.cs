using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PsdzClient;

#pragma warning disable CS0414
namespace BMW.Rheingold.Module.ISTA
{
    internal class MessageServiceDlgImpl : ServiceDlgImplBase<MessageServiceDlgModel>
    {
        private int p_TIMEOUT;

        private bool p_WeiterButtonEnabledStack;

        private ISTAModule callingModule;

        private IList<LocalizedText> text;

        private IList<LocalizedText> value;

        private string textId;

        private decimal? textItemParameterValue;

        public MessageServiceDlgImpl(ParameterContainer inParam)
            : base(inParam)
        {
            p_WeiterButtonEnabledStack = true;
            callingModule = inParam.getParameter("__CallingModule__") as ISTAModule;
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            DateTime now = DateTime.Now;
            if ("HideDialog".Equals(method))
            {
                HideDialog();
                return;
            }
            if ("InitializeDialog".Equals(method))
            {
                ITextLocator txtParam = inParam.getParameter("txtParam", null) as ITextLocator;
                ITextLocator wertFeld = inParam.getParameter("WertFeld", null) as ITextLocator;
                bool quittierung = (bool)inParam.getParameter("Quittierung", false);
                int tIMEOUT = (int)inParam.getParameter("TIMEOUT", 0);
                bool protocol = (bool)inParam.getParameter("Protocol", true);
                bool display = (bool)inParam.getParameter("Display", true);
                bool doLoopHandling = callingModule._DoLoopHandling;
                bool flag = InitializeDialog(now, method, doLoopHandling, txtParam, wertFeld, quittierung, tIMEOUT, protocol, display);
                outParam.setParameter("Quit", flag);
                return;
            }
            throw new NotSupportedException($"Method \"{method}\" is not supported by MessageServiceDlg.");
        }

        private bool InitializeDialog(DateTime startTime, string method, bool loopHandling, ITextLocator txtParam, ITextLocator WertFeld, bool Quittierung, int TIMEOUT, bool Protocol, bool Display)
        {
            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Module.ISTA.MessageServiceDlg.OverrideProtocolFlag", defaultValue: false))
            {
                if (!loopHandling)
                {
                    Log.Info("MessageServiceDlgImpl.InitializeDialog()", "override protocol flag to true; original flag was: {0}", Protocol);
                    Protocol = true;
                }
                else
                {
                    Log.Info("MessageServiceDlgImpl.InitializeDialog()", "override protocol flag not possible due to running loop; original flag was: {0}", Protocol);
                }
            }
            bool flag = DoInitializeDialog(txtParam, WertFeld, Quittierung, TIMEOUT, Protocol, Display);
            if (Protocol)
            {
                WriteFasta(startTime, method, loopHandling, Display, flag);
            }
            return flag;
        }

        [PreserveSource(Cleaned = true)]
        private void WriteFasta(DateTime startTime, string method, bool loopHandling, bool display, bool quit)
        {
        }

        private bool DoInitializeDialog(ITextLocator txtParam, ITextLocator WertFeld, bool Quittierung, int TIMEOUT, bool Protocol, bool Display)
        {
            Log.Info("MessageServiceDlgImpl.DoInitializeDialog()", "called");
            p_TIMEOUT = TIMEOUT;
            ITextLocator textLocator = txtParam ?? __Text();
            text = ((TextContent)textLocator.TextContent).GetTextForUI(logic.Lang);
            XDocument xDocument = null;
            try
            {
                xDocument = XDocument.Parse(txtParam.Text);
                textId = xDocument.Root.Attribute("ID")?.Value;
                SetTextParameterValue(xDocument);
            }
            catch (Exception exception)
            {
                Log.WarningException("DoInitializeDialog", exception);
            }
            if (WertFeld != null)
            {
                value = ((TextContent)WertFeld.TextContent).GetTextForUI(logic.Lang);
            }
            else
            {
                value = null;
            }
            if (Display)
            {
                base.Model.Text = text[0].TextItem;
                if (value != null)
                {
                    base.Model.Value = value[0].TextItem;
                }
                return HandleGui(Quittierung);
            }
            return false;
        }

        private void SetTextParameterValue(XDocument message)
        {
            if (message.Descendants().Count((XElement c) => c.Name.LocalName.Equals("PARAMETER", StringComparison.OrdinalIgnoreCase)) == 1)
            {
                string s = message.Descendants().ToList().FirstOrDefault((XElement c) => c.Name.LocalName.Equals("PARAMETER", StringComparison.OrdinalIgnoreCase))?.Attribute("ID")?.Value;
                decimal result = default(decimal);
                if (decimal.TryParse(s, out result))
                {
                    textItemParameterValue = result;
                }
            }
        }

        private bool HandleGui(bool Quittierung)
        {
            if (!base.Model.IsDialogShown)
            {
                NavigateTo(base.Model);
                base.Model.IsDialogShown = true;
                parentTab.ResetNextButtonLatency();
            }
            SetNextButtonEnabled(value: true);
            bool num = WaitForContinueButton(Quittierung ? (-1) : p_TIMEOUT);
            if (num)
            {
                SetNextButtonEnabled(value: false);
            }
            return num;
        }

        private void HideDialog()
        {
            Log.Info("MessageServiceDlgImpl.HideDialog()", "called");
            if (base.Model.IsDialogShown)
            {
                base.Model.Text = null;
                base.Model.Value = null;
                base.Model.IsDialogShown = false;
            }
            parentTab.ResetNextButtonLatency();
            p_TIMEOUT = 5000;
            FastaProtocoler.ResetCyclicJournalize();
            Log.Info("MessageServiceDlgImpl.HideDialog()", "ended");
        }
    }
}
