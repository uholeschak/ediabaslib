using BMW.ISPI.IstaOperation.Contract.Document;
using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.ServiceProgram;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable CS0169, CS0649
namespace BMW.ISPI.IstaOperation.Impl
{
    [PreserveSource(Hint="No update", SuppressWarning = true)]
    internal class ServiceProgramController : IServiceProgramController
    {
        private const int NextButtonPressedTimePeriodInMillis = 10000;

        private const int NodeClassIdes = 41153666;

        //private readonly IstaOperationLogic logic;

        //private readonly IDocumentLoader documentLoader;

        //private readonly FaultManager faultManager;

        //private readonly ModuleLauncher moduleLauncher;

        private readonly AutoResetEvent resumeEvent = new AutoResetEvent(initialState: false);

        private ServiceProgramAction userAction;

        //private IModuleExecutionHandle executingModuleHandle;

        private DateTime lastTimeNextButtonPressed = DateTime.MinValue;

        //private IDocument currentDocument;

        //private ServiceProgramDocumentItem currenTestModuleDocument;

        private ModuleData currentModule;

        private CancellationTokenSource automaticForwardNavigationCTS;

        public string Identifier => string.Empty;

        public ScreenMode ScreenMode { get; set; }

        public ServiceProgramController(ModuleData moduleData)
        {
            //this.logic = logic;
            //this.faultManager = faultManager;
            //this.documentLoader = documentLoader;
            //moduleLauncher = new ModuleLauncher(logic);
            currentModule = moduleData;
        }

        public bool NavigateToDialog(IServiceDialogModel model)
        {
            return false;
        }

        public bool IsNextButtonPressedWithinTimePeriod()
        {
            //[-] return DateTime.Now < lastTimeNextButtonPressed.AddMilliseconds(10000.0);
            //[+] return true;
            return true;
        }

        public ServiceProgramAction AwaitUserAction(int millisecondsTimeout)
        {
            try
            {
                Log.Info("ServiceProgramController.AwaitUserAction()", "Wait on user input. Timeout: {0} ms", millisecondsTimeout);
                if (IsNextButtonEnabled() && IsNextButtonPressedWithinTimePeriod())
                {
                    Log.Info("ServiceProgramController.AwaitUserAction()", "Next button was pressed within the defined time period ({0} ms). Continue immediately.", 10000);
                    userAction = new ServiceProgramNavigationAction(NavigationAction.Next);
                    return userAction;
                }
                if (millisecondsTimeout < 0)
                {
                    currentModule.ModuleState = ModuleExecutionStateType.idle;
                }
                userAction = null;
                resumeEvent.Reset();
                if ((millisecondsTimeout < 0) ? resumeEvent.WaitOne() : resumeEvent.WaitOne(millisecondsTimeout, exitContext: true))
                {
                    Log.Info("ServiceProgramController.AwaitUserAction()", "User input received.");
                    return userAction;
                }
                Log.Info("ServiceProgramController.AwaitUserAction()", "Timeout reached.");
                return null;
            }
            finally
            {
                lastTimeNextButtonPressed = DateTime.MinValue;
                currentModule.ModuleState = ((currentModule.ModuleState != ModuleExecutionStateType.aborted && currentModule.ModuleState != ModuleExecutionStateType.error && currentModule.ModuleState != ModuleExecutionStateType.finished) ? ModuleExecutionStateType.running : currentModule.ModuleState);
            }
        }

        public void ResetLastTimeNextButtonPressed()
        {
            lastTimeNextButtonPressed = DateTime.MinValue;
        }

        public void AddDocInfoObjects(IList<InfoObject> docs, int slot, IProtocolBasic fasta)
        {
        }

        public void HandleRDCToolDataAction()
        {
        }

        public void AbortServiceProgram()
        {
        }

        public void RemoveDocInfoObjects(IList<InfoObject> docs, int slot)
        {
        }

        public void RemoveDocInfoObjectsAll()
        {
        }

        public void AddSuspiciousDiagObject(string grobzeichen)
        {
        }

        public void SetNextButtonEnabled(bool enable)
        {
        }

        public bool IsNextButtonEnabled()
        {
            return true;
        }

        public void NavigateForward(int milliseconds = 0)
        {
        }

        internal void PerformUserAction(ServiceProgramAction userAction)
        {
        }

        public void SetDisplayMode(DisplayMode mode)
        {
        }
    }
}
