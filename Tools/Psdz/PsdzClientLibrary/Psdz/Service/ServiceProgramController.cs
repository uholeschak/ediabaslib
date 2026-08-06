using BMW.ISPI.IstaOperation.Contract.Document;
using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.ServiceProgram;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Threading;
using PsdzClient;

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

        //private ModuleData currentModule;

        private CancellationTokenSource automaticForwardNavigationCTS;

        public string Identifier => string.Empty;

        public ScreenMode ScreenMode { get; set; }

        public ServiceProgramController()
        {
            //this.logic = logic;
            //this.faultManager = faultManager;
            //this.documentLoader = documentLoader;
            //moduleLauncher = new ModuleLauncher(logic);
        }

        public bool NavigateToDialog(IServiceDialogModel model)
        {
            return false;
        }

        public bool IsNextButtonPressedWithinTimePeriod()
        {
            return DateTime.Now < lastTimeNextButtonPressed.AddMilliseconds(10000.0);
        }

        public ServiceProgramAction AwaitUserAction(int millisecondsTimeout)
        {
            return null;
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
            return false;
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
