using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PBMW.Rheingold.CoreFramework.Contracts;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using BMW.Rheingold.ISTA.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    internal class AdapterServiceDlgImpl : ServiceDlgImplBase<AdapterServiceDlgModel>
    {
        private IProtocolBasic fasta;

        private readonly VehicleAdapters installedAdapters;

        private string callingMethod;

        public AdapterServiceDlgImpl(ParameterContainer inParam)
            : base(inParam)
        {
            installedAdapters = new VehicleAdapters(logic.VecInfo);
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            DateTime now = DateTime.Now;
            callingMethod = method;
            fasta = RetrieveFasta(inParam);
            List<ITextLocator> txtParam = inParam.getParameter("txtParam", new List<ITextLocator>()) as List<ITextLocator>;
            IVehicleAdapterLocator adapter = inParam.getParameter("Adapter", null) as IVehicleAdapterLocator;
            bool install = (bool)inParam.getParameter("Install", true);
            bool flag = (bool)inParam.getParameter("Display", true);
            bool nextButtonEnabled = IsNextButtonEnabled();
            if ("InitializeDialog".Equals(method))
            {
                InitializeDialog(txtParam, adapter, install, flag);
                WriteFasta(now, txtParam, adapter, flag);
                if (flag)
                {
                    NavigateTo(base.Model);
                    SetNextButtonEnabled(value: true);
                    WaitForContinueButton();
                }
            }
            SetNextButtonEnabled(nextButtonEnabled);
        }

        private void InitializeDialog(List<ITextLocator> txtParam, IVehicleAdapterLocator adapter, bool install, bool display)
        {
            Log.Info("AdapterServiceDialog.InitializeDialog()", "called");
            ITextLocator textLocator2;
            if (txtParam == null)
            {
                ITextLocator textLocator = new TextLocator();
                textLocator2 = textLocator;
            }
            else
            {
                textLocator2 = new TextLocator().Concat(txtParam, theAddAfterLineBreak: true);
            }
            ITextLocator ezText = textLocator2;
            TextParser(ref ezText);
            if (display)
            {
                SetNextButtonEnabled(value: true);
                base.Model.Text = GetContent(ezText.TextContent);
            }
            if (install)
            {
                AddAdapter(adapter);
            }
            else
            {
                RemoveAdapter(adapter);
            }
            if (ezText.TextContent != null && ezText.TextContent.PlainText != null)
            {
                Log.Info("AdapterServiceDialogImpl.InitializeDialog()", "AdapterServiceDlg called with text: {0}", ezText.TextContent.PlainText);
            }
        }

        private void TextParser(ref ITextLocator ezText)
        {
            Log.Info("AdapterServiceDialog.TextParser()", "called");
            try
            {
                if (ezText == null)
                {
                    return;
                }
                ITextLocator obj = ezText;
                ITextLocator textLocator = new TextLocator();
                string formattedText = obj.TextContent.FormattedText;
                string text = string.Empty;
                int length = formattedText.Length;
                bool flag = true;
                for (int i = 0; i < length; i++)
                {
                    string text2 = formattedText.Substring(i, 1);
                    if (flag)
                    {
                        if (text2 == "\n" && i + 2 < length && formattedText.Substring(i + 1, 2) == "  ")
                        {
                            flag = false;
                        }
                        else if (!(text2 == " ") || i <= 3 || !(formattedText.Substring(i - 3, 3).ToLower() == "<br"))
                        {
                            text += text2;
                        }
                    }
                    else if (text2 != " ")
                    {
                        flag = true;
                        text += text2;
                    }
                }
                textLocator.TextContent = textLocator.TextContent.Concat(text);
                ezText = textLocator;
            }
            catch (Exception exception)
            {
                Log.WarningException("AdapterServiceDlg.TextParser()", exception);
            }
        }

        private void AddAdapter(IVehicleAdapterLocator adapter)
        {
            Log.Info("AdapterServiceDialog.AddAdapter()", "called");
            if (!installedAdapters.IsInstalled(adapter))
            {
                installedAdapters.Install(adapter);
            }
        }

        private void RemoveAdapter(IVehicleAdapterLocator adapter)
        {
            Log.Info("AdapterServiceDialog.RemoveAdapter()", "called");
            if (installedAdapters.IsInstalled(adapter))
            {
                installedAdapters.Uninstall(adapter);
            }
        }

        private void WriteFasta(DateTime startTime, List<ITextLocator> txtParam, IVehicleAdapterLocator adapter, bool display)
        {
            string text = ((adapter != null) ? $"{adapter.Id}*{adapter.Title}" : "*");
            IMessageText messageText = null;
            if (fasta != null)
            {
                IAction<IUiDialog> action = fasta.CreateAndAddUiDialogFromServiceProgram("AdapterServiceDlg", callingMethod);
                action.StartTime = startTime;
                action.SpecialAction.Display = display;
                IList<LocalizedText> textForUI = new TextContent(text).GetTextForUI(logic.Lang);
                messageText = action.SpecialAction.CreateAndAddMessageText(textForUI);
            }
            if (adapter == null)
            {
                Log.Warning("AdapterServiceDlg.InitializeGUI()", "adaptert was null");
            }
            IList<LocalizedText> list = new List<LocalizedText>();
            foreach (ITextLocator item in txtParam)
            {
                if (!string.IsNullOrEmpty(item.TextContent.PlainText))
                {
                    if (messageText != null)
                    {
                        list.AddRangeIfNotContains(item.TextContent.GetTextForUI(logic.Lang));
                    }
                }
                else if (messageText != null)
                {
                    list.AddRangeIfNotContains(new TextContent("(empty)").GetTextForUI(logic.Lang));
                }
            }
            messageText?.AddText(list);
        }
    }
}
