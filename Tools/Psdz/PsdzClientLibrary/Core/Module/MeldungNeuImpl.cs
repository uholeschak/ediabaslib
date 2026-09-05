using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BMW.Rheingold.Module.ISTA
{
    internal class MeldungNeuImpl : ServiceDlgImplBase<MeldungNeuModel>
    {
        private ISTAModule callingModule;

        private bool p_Quittierung;

        private bool p_Quitted;

        private ParameterContainer outParameter = new ParameterContainer();

        private ITextLocator txtParam;

        private bool m_bDisplayed;

        private bool p_Protocol = true;

        private int p_TIMEOUT = 5000;

        private ITextLocator WertFeld;

        private ParameterContainer fastaParameter = new ParameterContainer();

        private IProtocolBasic fasta;

        private string callingMethod;

        private IAction<IUiDialog> fastaUiAction;

        public MeldungNeuImpl(ParameterContainer inParam)
            : base(inParam)
        {
            callingModule = inParam.getParameter("__CallingModule__") as ISTAModule;
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            InitDialog(inParam, inoutParam);
            WaitForContinue(-1);
            FinishDialog(inoutParam);
        }

        protected override ParameterContainer AfterInvoke(string method)
        {
            return outParameter;
        }

        public void InitDialog(ParameterContainer inParam, ParameterContainer inoutParam)
        {
            fasta = RetrieveFasta(inParam);
            callingMethod = inParam.getParameter("methodname") as string;
            if (fasta != null)
            {
                fastaUiAction = fasta.CreateAndAddUiDialogFromServiceProgram("Meldung_NeuDlg", callingMethod);
            }
            else
            {
                Log.Warning("MeldungNeuImpl.InitDialog()", "No FASTA available.");
            }
            object parameter = inParam.getParameter("txtParam");
            if (parameter != null)
            {
                txtParam = (ITextLocator)parameter;
            }
            else
            {
                txtParam = new TextLocator("MessageServiceDlg txtParam was empty.");
            }
            parameter = inParam.getParameter("WertFeld");
            if (parameter != null)
            {
                WertFeld = (ITextLocator)parameter;
            }
            parameter = inParam.getParameter("Quittierung");
            if (parameter != null)
            {
                p_Quittierung = (bool)parameter;
            }
            parameter = inParam.getParameter("Display");
            if (parameter != null)
            {
                m_bDisplayed = (bool)parameter;
                if (fastaUiAction != null)
                {
                    fastaUiAction.SpecialAction.Display = m_bDisplayed;
                }
            }
            parameter = inParam.getParameter("Protocol");
            if (parameter != null)
            {
                p_Protocol = (bool)parameter;
            }
            parameter = inParam.getParameter("TIMEOUT");
            if (parameter != null)
            {
                p_TIMEOUT = (int)parameter;
            }
            base.Model.TxtParamFlow = ((TextContent)txtParam.TextContent).GetTextForUI(logic.Lang)[0].TextItem;
            if (WertFeld != null)
            {
                base.Model.WertFeldFlow = ((TextContent)WertFeld.TextContent).GetTextForUI(logic.Lang)[0].TextItem;
            }
            List<LocalizedText> list = new List<LocalizedText>();
            if (fastaUiAction != null)
            {
                if (!string.IsNullOrEmpty(txtParam.TextContent.PlainText))
                {
                    list.AddRange(txtParam.TextContent.GetTextForUI(logic.Lang));
                }
                else
                {
                    list.AddRangeIfNotContains(new TextContent("n/a").GetTextForUI(logic.Lang));
                }
                fastaUiAction.SpecialAction.CreateAndAddMessageText(list);
            }
            outParameter.setParameter("Quit", p_Quitted);
            outParameter.setParameter("_FASTA", fastaParameter);
        }

        public bool WaitForContinue(int timeout)
        {
            try
            {
                if (p_Quittierung)
                {
                    p_Quitted = WaitForContinueButton(timeout);
                }
                else
                {
                    p_Quitted = parentTab.NextButtonPressedWithinLastSecond;
                }
                string answer = (p_Quitted ? "NEXT button pressed" : "timeout reached");
                List<LocalizedText> list = new List<LocalizedText>();
                list.AddRange(logic.Lang.Select((string x) => new LocalizedText(answer, x)));
                fastaUiAction.SpecialAction.AddAnswer(list, null);
                fastaUiAction.EndTime = DateTime.Now;
                if (txtParam != null && !string.IsNullOrEmpty(txtParam.TextContent.PlainText))
                {
                    Match match = new Regex("D....(_|__)........(_|__)..(_|__)...").Match(txtParam.TextContent.PlainText);
                    if (match.Success)
                    {
                        Log.Info("MeldungNeuImpl.WaitForContinue()", "found diagcode: {0}", match.Value);
                        if (!string.IsNullOrEmpty(match.Value))
                        {
                            string diagCodeString = match.Value.Replace("__", "_");
                            if (logic.VecInfo != null)
                            {
                                logic.VecInfo.AddDiagCode(diagCodeString, null, FindIdentifierInfoObjStarted(), null);
                            }
                        }
                        if (!p_Quittierung)
                        {
                            p_Quitted = WaitForContinueButton(-1);
                        }
                    }
                }
                outParameter.setParameter("Quit", p_Quitted);
                outParameter.setParameter("_FASTA", fastaParameter);
                return p_Quitted;
            }
            catch (Exception exception)
            {
                Log.WarningException("MeldungNeuImpl.WaitForContinue()", exception);
            }
            p_Quitted = false;
            outParameter.setParameter("Quit", p_Quitted);
            outParameter.setParameter("_FASTA", fastaParameter);
            return false;
        }

        public ParameterContainer FinishDialog(ParameterContainer inoutParam)
        {
            return outParameter;
        }
    }
}
