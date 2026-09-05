using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Rheingold.Module.ISTA
{
    internal class QuestionServiceDlgImpl : ServiceDlgImplBase<QuestionServiceDlgModel>
    {
        private readonly IProtocolBasic fasta;

        private readonly ParameterContainer fastaParameter = new ParameterContainer();

        private readonly ParameterContainer outParameter = new ParameterContainer();

        private IAction<IUiDialog> fastaAction;

        private IUiDialog fastaUiDialog;

        private IMessageText messageText;

        private bool p_WeiterButtonEnabledStack;

        private int result = -1;

        public int Result => result;

        public bool Quit => result >= 0;

        public QuestionServiceDlgImpl(ParameterContainer inParam)
            : base(inParam)
        {
            fasta = RetrieveFasta(inParam);
            if (fasta == null)
            {
                Log.Error("QuestionServiceDlg.ctor", "FASTA protocoling not possible.");
            }
        }

        private void JournalizeButtonSelectionInFasta()
        {
            switch (result)
            {
                case 1:
                    if (fastaUiDialog != null)
                    {
                        ISelectable selectable2 = messageText.CreateAndAddSelectable("SelectButtons");
                        selectable2.AddEntry(selectionState: true, null, null);
                        selectable2.AddEntry(selectionState: false, null, null);
                    }
                    else
                    {
                        Log.Error("QuestionServiceDlgImpl.ButtonSelection()", "No FASTA available.");
                    }
                    break;
                case 2:
                    if (fastaUiDialog != null)
                    {
                        ISelectable selectable = messageText.CreateAndAddSelectable("SelectButtons");
                        selectable.AddEntry(selectionState: false, null, null);
                        selectable.AddEntry(selectionState: true, null, null);
                    }
                    else
                    {
                        Log.Error("QuestionServiceDlgImpl.ButtonSelection()", "No FASTA available.");
                    }
                    break;
                default:
                    Log.Error("JournalizeButtonSelection()", "Selected index is out of range so nothing is journalized in FASTA.");
                    break;
            }
        }

        public ParameterContainer FinishDialog(ParameterContainer inoutParam)
        {
            SetNextButtonEnabled(p_WeiterButtonEnabledStack);
            if (fastaUiDialog != null)
            {
                if (result > 0)
                {
                    List<LocalizedText> list = new List<LocalizedText>();
                    list.AddRange(logic.Lang.Select((string x) => new LocalizedText(result.ToString(), x)));
                    fastaUiDialog.AddAnswer(list, null);
                    JournalizeButtonSelectionInFasta();
                }
                else
                {
                    Log.Warning("QuestionServiceDlgImpl.ButtonSelection()", "No result to protocolize in FASTA. (value is {0})", result.ToString());
                }
            }
            else
            {
                Log.Error("QuestionServiceDlgImpl.ButtonSelection()", "No FASTA available.");
            }
            outParameter.setParameter("Result", result);
            outParameter.setParameter("_FASTA", fastaParameter);
            if (fastaAction != null)
            {
                fastaAction.EndTime = DateTime.Now;
            }
            return outParameter;
        }

        public void InitDialog(string callingMethod, ParameterContainer InParameters, ParameterContainer inoutParam)
        {
            if (fasta != null)
            {
                fastaAction = fasta.CreateAndAddUiDialogFromServiceProgram("QuestionServiceDialog", string.IsNullOrEmpty(callingMethod) ? "n/a" : callingMethod);
                fastaUiDialog = fastaAction.SpecialAction;
            }
            else
            {
                Log.Error("QuestionServiceDlg.InitDialog()", "No FASTA available.");
            }
            p_WeiterButtonEnabledStack = IsNextButtonEnabled();
            SetNextButtonEnabled(value: false);
            ITextLocator textLocator = InParameters.getParameter("txtParam", TextLocator.Empty) as ITextLocator;
            IList<LocalizedText> list = ((TextContent)__Text("51882507").TextContent).CreatePlainText(logic.Lang);
            TextContent textContent;
            if (list != null)
            {
                base.Model.TextInfo[0] = list[0].TextItem;
                textContent = new TextContent(list);
                IList<LocalizedText> list2 = new List<LocalizedText>();
                list2.AddRange(logic.Lang.Select((string x) => new LocalizedText(" -1- ", x)));
                textContent = textContent.ConcatPlainText(list2, inFront: true);
            }
            else
            {
                base.Model.TextInfo[0] = "Yes";
                textContent = new TextContent(" -1- " + base.Model.TextInfo[0]);
            }
            IList<LocalizedText> list3 = ((TextContent)__Text("51883659").TextContent).CreatePlainText(logic.Lang);
            if (list3 != null)
            {
                base.Model.TextInfo[1] = list3[0].TextItem;
                TextContent textContent2 = new TextContent(list3);
                IList<LocalizedText> list4 = new List<LocalizedText>();
                list4.AddRange(logic.Lang.Select((string x) => new LocalizedText(" -2- ", x)));
                textContent.Concat(textContent2.ConcatPlainText(list4, inFront: true));
            }
            else
            {
                base.Model.TextInfo[1] = "No";
                IList<LocalizedText> list5 = new List<LocalizedText>();
                list5.AddRange(logic.Lang.Select((string x) => new LocalizedText(" -2- " + base.Model.TextInfo[1], x)));
                textContent.Concat(new TextContent(list5));
            }
            IList<LocalizedText> list6 = ((textLocator == null) ? null : ((TextContent)textLocator.TextContent).GetTextForUI(logic.Lang));
            if (textLocator != null && !string.IsNullOrEmpty(textLocator.TextContent.PlainText))
            {
                base.Model.Title = textLocator.TextContent.PlainText;
            }
            else
            {
                base.Model.Title = ToString();
            }
            if (list6 != null)
            {
                base.Model.Text = list6[0].TextItem;
            }
            else
            {
                base.Model.Text = "(empty)";
            }
            if (fastaUiDialog != null && list6 != null)
            {
                messageText = fastaUiDialog.CreateAndAddMessageText(list6);
            }
            if (fastaUiDialog != null)
            {
                if (messageText == null)
                {
                    messageText = fastaUiDialog.CreateAndAddMessageText(textContent.GetTextForUI(logic.Lang));
                }
                else
                {
                    messageText.AddText(textContent.GetTextForUI(logic.Lang));
                }
            }
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
                        Log.Info("QuestionServiceDlg.WaitOnUserInteraction()", "Selected button index: {0}", serviceProgramButtonSelectionAction.SelectedIndex);
                        ButtonSelection(serviceProgramButtonSelectionAction.SelectedIndex);
                    }
                }
                Log.Info("QuestionServiceDlg.WaitOnUserInteraction()", "Navigation action: {0}", serviceProgramNavigationAction.NavigationAction);
            }
            catch (Exception exception)
            {
                Log.WarningException("QuestionServiceDlg.WaitOnUserInteraction()", exception);
            }
        }

        private void ButtonSelection(int buttonIdx)
        {
            Log.Info("QuestionServiceDlgImpl.ButtonSelection()", "called.");
            try
            {
                switch (buttonIdx)
                {
                    case 0:
                        result = 1;
                        break;
                    case 1:
                        result = 2;
                        break;
                    default:
                        Log.Error("QuestionServiceDlgImpl.ButtonSelection()", "Button index out of range.");
                        break;
                }
                SetNextButtonEnabled(value: true);
            }
            catch (Exception exception)
            {
                Log.WarningException("QuestionServiceDlgImpl.ButtonSelection()", exception);
            }
            finally
            {
                for (int i = 0; i < base.Model.CheckedInfo.Count; i++)
                {
                    base.Model.CheckedInfo[i] = i == buttonIdx;
                }
            }
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            InitDialog(method, inParam, inoutParam);
            WaitOnUserInteraction();
            FinishDialog(inoutParam);
        }

        protected override ParameterContainer AfterInvoke(string method)
        {
            return outParameter;
        }
    }
}
