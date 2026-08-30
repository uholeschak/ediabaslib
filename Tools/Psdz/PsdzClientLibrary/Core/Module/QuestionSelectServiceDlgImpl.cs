using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BMW.Rheingold.Module.ISTA
{
    internal abstract class QuestionSelectServiceDlgImpl<TModel> : ServiceDlgImplBase<TModel> where TModel : QuestionSelectServiceDlgModel, new()
    {
        private string kindOfDialog;

        private bool disposed;

        private IProtocolBasic fasta;

        private IMessageText fastaMsgText;

        private IAction<IUiDialog> fastaUiDlgAction;

        private bool isMultiSelect;

        private string calledMethod;

        private int buttonCount = 25;

        private string[] buttonTexts;

        private DateTime startTime;

        public UiBrand CurrentBrand { get; private set; }

        private int Result => base.Model.Buttons.FirstOrDefault((QuestionSelectButtonModel item) => item.IsChecked)?.Result ?? 0;

        private string PageTitle
        {
            get
            {
                return base.Model.PageTitle;
            }
            set
            {
                base.Model.PageTitle = value;
            }
        }

        public QuestionSelectServiceDlgImpl(ParameterContainer inParameters)
            : base(inParameters)
        {
            Initialize(inParameters);
        }

        ~QuestionSelectServiceDlgImpl()
        {
            Dispose();
        }

        private void Initialize(ParameterContainer inParameters)
        {
            if (inParameters == null)
            {
                throw new ArgumentException("InParameters are null.", "InParameters");
            }
            base.Model.PriorText = string.Empty;
            base.Model.SuccessorText = string.Empty;
            kindOfDialog = inParameters.getParameter("__DialogName__") as string;
            fasta = RetrieveFasta(inParameters);
        }

        protected int[] GetSelectionSettings()
        {
            return base.Model.Buttons.Select((QuestionSelectButtonModel item) => item.SelectionState).ToArray();
        }

        private ITextLocator RetrievePrioTxt(ParameterContainer inParameters)
        {
            ITextLocator textLocator = inParameters.getParameter("_priorText", null) as ITextLocator;
            if (textLocator == null)
            {
                textLocator = inParameters.getParameter("priorText", null) as ITextLocator;
            }
            if (textLocator == null)
            {
                textLocator = inParameters.getParameter("PriorText", null) as ITextLocator;
            }
            if (textLocator == null)
            {
                textLocator = inParameters.getParameter("AnfangText", null) as ITextLocator;
            }
            return textLocator;
        }

        private ITextLocator RetrievePostTxt(ParameterContainer inParameters)
        {
            ITextLocator textLocator = inParameters.getParameter("_pastText", null) as ITextLocator;
            if (textLocator == null)
            {
                textLocator = inParameters.getParameter("pastText", null) as ITextLocator;
            }
            if (textLocator == null)
            {
                textLocator = inParameters.getParameter("EndeText", null) as ITextLocator;
            }
            if (textLocator == null)
            {
                textLocator = inParameters.getParameter("PastText", null) as ITextLocator;
            }
            if (textLocator == null)
            {
                textLocator = new TextLocator(string.Empty);
            }
            return textLocator;
        }

        private int[] RetrieveSelectionVorgabe(ParameterContainer inParameters)
        {
            int[] array = ((!(inParameters.getParameter("SelektionVorgabe", null) is int[] array2)) ? (inParameters.getParameter("ButtonSelect", null) as int[]) : array2);
            if (array == null)
            {
                array = new int[0];
            }
            if (array.Length < buttonCount)
            {
                List<int> list = new List<int>(array);
                for (int i = 0; i < buttonCount - array.Length; i++)
                {
                    if (isMultiSelect)
                    {
                        list.Add(0);
                    }
                    else
                    {
                        list.Add(-1);
                    }
                }
                array = list.ToArray();
            }
            return array;
        }

        private TextContent BuildButtonReport(TextContent buttonLabel, TextContent buttonTextContent, IList<string> lang)
        {
            IList<LocalizedText> list;
            if (buttonLabel != null)
            {
                list = buttonLabel.CreatePlainText(lang);
                list.ForEach(delegate (LocalizedText x)
                {
                    x.TextItem = " -" + x.TextItem + "- ";
                });
            }
            else
            {
                list = new List<LocalizedText>();
                list.AddRange(lang.Select((string x) => new LocalizedText(" -- ", x)));
            }
            if (buttonTextContent != null)
            {
                return buttonTextContent.ConcatPlainText(list, inFront: true);
            }
            return new TextContent(list);
        }

        private void InitButtonCount(ParameterContainer inParameters)
        {
            if (!isMultiSelect)
            {
                buttonCount = (int)inParameters.getParameter("ButtonCount", 0);
                if (buttonCount == 0)
                {
                    buttonCount = (int)inParameters.getParameter("AnzahlTexte", 0);
                }
            }
        }

        internal virtual void Init(ParameterContainer inParameters, ParameterContainer inoutParam, string method = "")
        {
            string methodName = inParameters.getParameter("methodname") as string;
            bool flag = (bool)inParameters.getParameter("Display", true);
            fasta = RetrieveFasta(inParameters);
            startTime = DateTime.Now;
            CurrentBrand = logic.Brand;
            isMultiSelect = CallingModule == "MultiselectServiceDlgImpl";
            if (fasta != null && !"WithButtonLabel_25".Equals(method) && !"OnlyButtonText_25".Equals(method))
            {
                fastaUiDlgAction = fasta.CreateAndAddUiDialogFromServiceProgram(kindOfDialog, methodName);
                fastaUiDlgAction.SpecialAction.Display = flag;
            }
            else if (!"WithButtonLabel_25".Equals(method) && !"OnlyButtonText_25".Equals(method))
            {
                Log.Error("QuestionSelectServiceDlgImpl.Init()", "FASTA protocoling not available.");
            }
            ITextLocator textLocator = RetrievePrioTxt(inParameters);
            if (fastaUiDlgAction != null)
            {
                fastaMsgText = Protocol4Fasta(textLocator, fastaUiDlgAction);
            }
            ITextLocator textLocator2 = RetrievePostTxt(inParameters);
            PageTitle = ((textLocator != null && !string.IsNullOrEmpty(textLocator.TextContent.PlainText)) ? textLocator.TextContent.PlainText : string.Empty);
            if (flag)
            {
                if (textLocator != null)
                {
                    base.Model.PriorText = GetContent(textLocator.TextContent);
                }
                if (textLocator2 != null)
                {
                    base.Model.SuccessorText = GetContent(textLocator2.TextContent);
                }
            }
            InitButtonCount(inParameters);
            int buttonBeschriftung = (int)inParameters.getParameter("ButtonBeschriftung", 0);
            int[] selectionSettings = RetrieveSelectionVorgabe(inParameters);
            buttonTexts = new string[buttonCount];
            InitButtons(buttonBeschriftung, inParameters, selectionSettings);
            PrtotcolPastTxt4Fasta(textLocator2, fastaMsgText);
        }

        private void InitButtons(int buttonBeschriftung, ParameterContainer buttonTextLabel, int[] selectionSettings)
        {
            ISelectable selectable = null;
            if (buttonCount > 0)
            {
                if (fastaMsgText == null)
                {
                    if (fastaUiDlgAction != null)
                    {
                        List<LocalizedText> list = new List<LocalizedText>();
                        list.AddRange(logic.Lang.Select((string x) => new LocalizedText("n/a", x)));
                        fastaMsgText = fastaUiDlgAction.SpecialAction.CreateAndAddMessageText(list);
                    }
                    else
                    {
                        Log.Error("QuestionSelectServiceDlgImpl.InitButtons()", "No FASTA available.");
                    }
                }
                if (fastaMsgText != null)
                {
                    selectable = fastaMsgText.CreateAndAddSelectable(kindOfDialog);
                }
                else
                {
                    Log.Warning("QuestionSelectServiceDlgImpl.InitButtons()", "No FASTA 2 available.");
                }
            }
            bool[] array = new bool[buttonCount];
            bool[] array2 = base.Model.Buttons.Select((QuestionSelectButtonModel questionSelectButtonModel) => questionSelectButtonModel.IsMarked).ToArray();
            Array.Copy(array2, array, array2.Length);
            base.Model.Buttons.Clear();
            for (int num = 0; num < buttonCount; num++)
            {
                int num2 = num;
                if (!isMultiSelect)
                {
                    num2++;
                }
                string name = string.Format(CultureInfo.InvariantCulture, "ButtonLabel{0}", num2);
                string name2 = string.Format(CultureInfo.InvariantCulture, "ButtonText{0}", num2);
                string name3 = string.Format(CultureInfo.InvariantCulture, "ButtonText_{0:00}", num2);
                string name4 = string.Format(CultureInfo.InvariantCulture, "ButtonLabel_{0:00}", num2);
                ITextLocator textLocator = ((!(buttonTextLabel.getParameter(name) is ITextLocator textLocator2)) ? (buttonTextLabel.getParameter(name4) as ITextLocator) : textLocator2);
                if (textLocator == null)
                {
                    name = string.Format(CultureInfo.InvariantCulture, "AuswahlText_{0:00}", num2);
                    textLocator = buttonTextLabel.getParameter(name) as ITextLocator;
                }
                object parameter = buttonTextLabel.getParameter(name2);
                if (parameter == null)
                {
                    parameter = buttonTextLabel.getParameter(name3);
                }
                if (buttonBeschriftung == 1)
                {
                    textLocator = new TextLocator(num2.ToString(CultureInfo.InvariantCulture));
                    Log.Info("QuestionSelectServiceDlgImpl.InitDialog()", "Text on Button was set to number {0}.", textLocator);
                }
                ITextLocator textLocator3 = parameter as ITextLocator;
                QuestionSelectButtonModel item = InitButton(array[num], textLocator?.TextContent, textLocator3?.TextContent as TextContent, num2, fastaMsgText, selectable, selectionSettings[num], logic.Lang);
                buttonTexts[num] = textLocator3?.TextContent?.PlainText;
                base.Model.Buttons.Add(item);
            }
        }

        private QuestionSelectButtonModel InitButton(bool isMarked, ITextContent buttonLabel, TextContent buttonTextContent, int result, IMessageText fastaMsgText, ISelectable selectable, int selectionState, IList<string> lang)
        {
            string buttonText = ((buttonLabel == null) ? string.Empty : buttonLabel.Text);
            string plainText = ((buttonLabel == null) ? string.Empty : buttonLabel.PlainText);
            IList<LocalizedText> list = new List<LocalizedText>();
            string label = ((buttonTextContent == null) ? string.Empty : buttonTextContent.GetTextForUI(lang)[0].TextItem);
            if (!isMultiSelect)
            {
                buttonTexts[result - 1] = ((buttonTextContent == null) ? string.Empty : buttonTextContent.PlainText);
            }
            else
            {
                buttonTexts[result] = ((buttonTextContent == null) ? string.Empty : buttonTextContent.PlainText);
            }
            if (fastaMsgText != null)
            {
                TextContent textContent = BuildButtonReport(buttonLabel as TextContent, buttonTextContent, lang);
                fastaMsgText.AddText(textContent.GetTextForUI(lang));
            }
            if (buttonLabel != null)
            {
                list.AddRange(buttonLabel.GetTextForUI(lang));
            }
            ISelectableEntry fastaEntry = null;
            if (selectable != null)
            {
                fastaEntry = selectable.AddEntry(selectionState: false, list, null);
            }
            if (calledMethod != null && calledMethod.Equals("OnlyButtonText_25"))
            {
                plainText = "";
            }
            return new QuestionSelectButtonModel(isMarked, selectionState, buttonText, plainText, label, result, fastaEntry, isMultiSelect, CurrentBrand.ToString());
        }

        private IMessageText Protocol4Fasta(ITextLocator text, IAction<IUiDialog> uiDlgAction)
        {
            IMessageText result = null;
            List<LocalizedText> list = new List<LocalizedText>();
            if (text != null && text.TextContent != null)
            {
                if (uiDlgAction != null)
                {
                    ITextContent textContent = new TextContent(string.Empty).Concat(text.TextContent).Concat("<br/>");
                    list.AddRange(textContent.GetTextForUI(logic.Lang));
                    result = uiDlgAction.SpecialAction.CreateAndAddMessageText(list);
                }
            }
            else if ("QuestionSelectServiceDlg".Equals(kindOfDialog, StringComparison.Ordinal) && uiDlgAction != null)
            {
                list.AddRange(logic.Lang.Select((string x) => new LocalizedText("-<br/><br/>", x)));
                result = uiDlgAction.SpecialAction.CreateAndAddMessageText(list);
            }
            return result;
        }

        private void PrtotcolPastTxt4Fasta(ITextLocator text, IMessageText msgText)
        {
            List<LocalizedText> list = new List<LocalizedText>();
            if (text != null && text.TextContent != null)
            {
                list.AddRange(text.TextContent.GetTextForUI(logic.Lang));
                if (msgText == null && fastaUiDlgAction != null)
                {
                    fastaUiDlgAction.SpecialAction.CreateAndAddMessageText(list);
                }
                else
                {
                    msgText?.AddText(list);
                }
            }
        }

        private void ButtonSelection(QuestionSelectButtonModel btn)
        {
            if (base.Model is MehrfachAuswahlDlgModel || base.Model is MultiselectServiceDlgModel || isMultiSelect)
            {
                btn.IsChecked = !btn.IsChecked;
            }
            else
            {
                foreach (QuestionSelectButtonModel button in base.Model.Buttons)
                {
                    button.IsChecked = button == btn;
                }
            }
            if (btn.FastaEntry != null)
            {
                btn.FastaEntry.SelectionState = btn.IsChecked;
            }
            if (btn.IsChecked)
            {
                SetNextButtonEnabled(value: true);
            }
        }

        public new void Dispose()
        {
            CleanUp(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void CleanUp(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
            }
        }

        protected void DisableDisplay()
        {
            base.Model.Buttons.ForEach(delegate (QuestionSelectButtonModel x)
            {
                x.IsEnabled = false;
            });
        }

        protected virtual bool WaitForContinue()
        {
            try
            {
                DisplayWaitCursor(value: false);
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
                        int num = serviceProgramButtonSelectionAction.SelectedIndex;
                        if (isMultiSelect)
                        {
                            num++;
                        }
                        Log.Info("QuestionSelectServiceDlg.WaitOnUserInteraction()", "Selected button index: {0}", num);
                        QuestionSelectButtonModel btn = base.Model.Buttons[num];
                        ButtonSelection(btn);
                    }
                }
                Log.Info("QuestionSelectServiceDlg.WaitOnUserInteraction()", "Navigation action: {0}", serviceProgramNavigationAction.NavigationAction);
                foreach (QuestionSelectButtonModel button in base.Model.Buttons)
                {
                    if (button.IsChecked)
                    {
                        button.IsMarked = true;
                    }
                }
                base.Model.IsDialogShown = false;
                DisplayWaitCursor(value: true);
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("QuestionSelectServiceDlgBase.WaitForContinue()", exception);
            }
            return false;
        }

        public virtual ParameterContainer FinishDialog(ParameterContainer inoutParam)
        {
            ParameterContainer parameterContainer = new ParameterContainer();
            parameterContainer.setParameter("Result", Result);
            return parameterContainer;
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            Invoke(method, inParam, outParam, inoutParam, isNextButtonEnabled: false);
        }

        protected void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam, bool isNextButtonEnabled)
        {
            bool nextButtonEnabled = IsNextButtonEnabled();
            if ("ButtonLabel_Vorbelegung".Equals(method))
            {
                SetNextButtonEnabled(isNextButtonEnabled);
                Init(inParam, inoutParam, method);
            }
            else if ("InitializeDialog2".Equals(method))
            {
                SetNextButtonEnabled(isNextButtonEnabled);
                Init(inParam, inoutParam, method);
            }
            else if ("WithButtonLabel_25".Equals(method) || "OnlyButtonText_25".Equals(method))
            {
                calledMethod = method;
                SetNextButtonEnabled(isNextButtonEnabled);
                Init(inParam, inoutParam, method);
                ProtocolButtonsFasta();
            }
            if ("ButtonLabel_Vorbelegung".Equals(method) || "InitializeDialog2".Equals(method) || "WithButtonLabel_25".Equals(method) || "OnlyButtonText_25".Equals(method))
            {
                WaitForContinue();
                if (fastaUiDlgAction != null)
                {
                    List<LocalizedText> list = new List<LocalizedText>();
                    string answerText = string.Empty;
                    if ("MehrfachAuswahlDlg".Equals(kindOfDialog, StringComparison.OrdinalIgnoreCase) || "WithButtonLabel_25".Equals(method) || "OnlyButtonText_25".Equals(method))
                    {
                        answerText = (from x in GetSelectionSettings()
                                      select x.ToString(CultureInfo.InvariantCulture)).Aggregate((string a, string b) => string.Format(CultureInfo.InvariantCulture, "{0}, {1}", a, b));
                    }
                    else
                    {
                        answerText = Result.ToString(CultureInfo.InvariantCulture);
                    }
                    list.AddRange(logic.Lang.Select((string x) => new LocalizedText(answerText, x)));
                    fastaUiDlgAction.SpecialAction.AddAnswer(list, null);
                }
                outParam.setParameter("Result", Result);
                outParam.setParameter("SelektionAuswahl", GetSelectionSettings());
                outParam.setParameter("ButtonSelectReturn", GetSelectionSettings());
            }
            if (fastaUiDlgAction != null)
            {
                fastaUiDlgAction.EndTime = DateTime.Now;
            }
            if ("WithButtonLabel_25".Equals(method) || "OnlyButtonText_25".Equals(method))
            {
                ProtocolButtonsFasta();
            }
            SetNextButtonEnabled(nextButtonEnabled);
            DisableDisplay();
        }

        private void ProtocolButtonsFasta()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(50);
            int num = 0;
            for (num = 0; num < buttonCount; num++)
            {
                string key = $"Button {num} -&gt; {base.Model.Buttons[num].SelectionState}";
                string value = buttonTexts[num];
                dictionary.Add(key, value);
            }
            fasta?.AddLogStatement(kindOfDialog, dictionary, startTime);
        }
    }
}
