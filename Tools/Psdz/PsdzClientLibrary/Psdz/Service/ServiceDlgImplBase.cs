using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using BMW.ISPI.IstaOperation.Contract.ServiceProgram;

namespace BMW.Rheingold.Module.ISTA
{
    internal abstract class ServiceDlgImplBase<TModel> : ISTAServiceDialog, IServiceDlgImplBase<TModel> where TModel : ServiceDialogModelBase, new()
    {
        protected readonly IModuleExecutionParent parentTab;

        private ScreenMode screenmode;

        protected IServiceProgramController ServiceProgramController { get; }

        public TModel Model { get; }

        protected ServiceDlgImplBase(ParameterContainer inParam)
        {
            _globalModuleInParameter = inParam;
            __handleInParameter();
            parentTab = _globalTabModuleISTA;
            Model = new TModel();
            ServiceProgramController = __RheinGoldCoreModuleParameters__.getParameter(ModuleParameter.ParameterName.ServiceProgramController) as IServiceProgramController;
            if (ServiceProgramController == null)
            {
                throw new InvalidOperationException("State controller must not be null.");
            }
        }

        protected void SetNextButtonEnabled(bool value)
        {
            parentTab.SetNextButtonEnabled(value);
        }

        protected bool IsNextButtonEnabled()
        {
            return parentTab.IsNextEnabled;
        }

        public bool IsNextButtonPressedWithinTimePeriod()
        {
            return ServiceProgramController.IsNextButtonPressedWithinTimePeriod();
        }

        public void ResetLastTimeNextButtonPressed()
        {
            ServiceProgramController.ResetLastTimeNextButtonPressed();
        }

        protected void ResetNextButtonLatency()
        {
            Log.Debug("ServiceDlgImplBase.ResetNextButtonLatency()", "Not yet implemented.");
        }

        protected bool WaitForContinueButton()
        {
            return WaitForContinueButton(-1);
        }

        protected bool ShowQuestionDialog(IList<LocalizedText> title, IList<LocalizedText> question)
        {
            //[-] InteractionButtonResponse response = logic.Services.InteractionService.RegisterQuestion(title, question).Response;
            //[-] if (response == null)
            //[-] {
            //[-] return false;
            //[-] }
            //[-] return response.Action == InteractionButton.Yes;
            //[+] return true;
            return true;
        }

        protected void AbortTestModule()
        {
            ServiceProgramController.AbortServiceProgram();
        }

        protected bool WaitForContinueButton(int timeOut)
        {
            Log.Info("ServiceDlgImplBase.WaitForContinueButton()", "called with TimeOut: {0}", timeOut);
            ServiceProgramAction serviceProgramAction = ServiceProgramController.AwaitUserAction(timeOut);
            Log.Info("DiagnosticsModuleCoreTabForeground.WaitForContinueButton()", "Result: {0}", serviceProgramAction != null);
            return serviceProgramAction != null;
        }

        protected void NavigateTo(IModuleExecutionStep step)
        {
            parentTab.NavigateTo(step);
        }

        protected void DisplayWaitCursor(bool value)
        {
            Log.Warning("ServiceDlgImplBase.DisplayWaitCursor()", "Not yet implemented. Value to set: {0}", value);
        }

        protected void StoreKeyboardEnabled()
        {
            Log.Warning("ServiceDlgImplBase.StoreKeyboardEnabled()", "Not yet implemented");
        }

        protected void RestoreKeyboardEnabled()
        {
            Log.Warning("ServiceDlgImplBase.RestoreKeyboardEnabled()", "Not yet implemented");
        }

        protected void SetKeyboardEnabled(bool enable)
        {
            Log.Warning("ServiceDlgImplBase.SetKeyboardEnabled()", "Not yet implemented");
        }

        protected void StoreScreenMode()
        {
            screenmode = ServiceProgramController.ScreenMode;
            Log.Info("ServiceDlgImplBase.StoreScreenMode()", "Screenmode '{0}' stored.", screenmode.ToString());
        }

        public void ResetScreenMode()
        {
            SetScreenMode(screenmode);
            Log.Info("ServiceDlgImplBase.ResetScreenMode()", "Screenmode reset to {0}", screenmode.ToString());
        }

        protected void SetScreenMode(ScreenMode screenModeValue)
        {
            ServiceProgramController.ScreenMode = screenModeValue;
            Log.Info("ServiceDlgImplBase.SetScreenMode()", "Screenmode set to {0}", screenModeValue.ToString());
        }

        protected ScreenMode GetScreenMode()
        {
            return screenmode;
        }

        protected void ResetPageTitle()
        {
            Log.Warning("ServiceDlgImplBase.ResetPageTitle()", "Not yet implemented");
        }

        protected void ShowProgressDialog(FormatedData taskDescription = null)
        {
            Log.Warning("ServiceDlgImplBase.ShowProgressDialog()", "Not yet implemented");
        }

        protected void CloseProgressDialog()
        {
            Log.Warning("ServiceDlgImplBase.CloseProgressDialog()", "Not yet implemented");
        }

        protected string FindIdentifierInfoObjStarted()
        {
            return ServiceProgramController.Identifier;
        }
    }
}
