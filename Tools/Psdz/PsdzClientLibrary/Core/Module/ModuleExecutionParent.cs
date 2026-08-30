using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.ServiceProgram;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using BMW.Rheingold.RheingoldSessionController.Module;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    public class ModuleExecutionParent : IModuleExecutionParent
    {
        private readonly IServiceProgramController serviceProgramController;

        private readonly ILogic logic;

        private readonly IModule moduleData;

        public IFastaGrouping FastaGrouping { get; set; }

        public bool IsKeyboardEnabled
        {
            get
            {
                Log.Error("ModuleExecutionParent.IsKeyboardEnabled_get", "Returning false.");
                return false;
            }
            set
            {
                Log.Error("ModuleExecutionParent.IsKeyboardEnabled_set", "Will be ignored.");
            }
        }

        public string Title { get; private set; }

        public bool IsNextEnabled
        {
            get
            {
                return serviceProgramController.IsNextButtonEnabled();
            }
            set
            {
                serviceProgramController.SetNextButtonEnabled(value);
            }
        }

        public DateTime LastTimeNextButtonPressed
        {
            get
            {
                DateTime now = DateTime.Now;
                Log.Error("ModuleExecutionParent.IsNextEnabled_get", "Returning \"{0}\"", now);
                return now;
            }
        }

        public bool NextButtonPressedWithinLastSecond
        {
            get
            {
                Log.Error("ModuleExecutionParent.NextButtonPressedWithinLastSecond_get", "Returning false");
                return false;
            }
        }

        public IModule ModuleData => moduleData;

        internal ModuleExecutionParent(ModuleImpl module, string runParameter, ModuleParameter parameters)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }
            moduleData = module.Data;
            if (parameters != null)
            {
                logic = (ILogic)parameters.getParameter(ModuleParameter.ParameterName.Logic);
                serviceProgramController = parameters.getParameter(ModuleParameter.ParameterName.ServiceProgramController) as IServiceProgramController;
                Title = runParameter;
            }
            module.setFASTAAblaufName(runParameter);
        }

        public void SetScreenMode(uint mode)
        {
            Log.Error("ModuleExecutionParent.SetScreenMode_set", "Will be ignored. Mode: {0}", mode);
        }

        public void ResetPageTitle()
        {
            Log.Error("ModuleExecutionParent.ResetPageTitle_set", "Will be ignored.");
        }

        public uint GetScreenMode()
        {
            Log.Error("ModuleExecutionParent.GetScreenMode_get", "Returning 0.");
            return 0u;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public void Close()
        {
            Close(abort: true);
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public void Close(bool abort)
        {
            if (logic.VecInfo.DiagCodes == null || !logic.VecInfo.DiagCodes.Any())
            {
                return;
            }
            //[-] using (IIstaPukService istaPukService = ((Logic)logic).CreatePukServiceClient())
            //[-] {
            //[-] if (istaPukService.IsAvailable() && logic.VecInfo.DiagCodes.Any())
            //[-] {
            //[-] List<PukDiagnosisCode> list = new List<PukDiagnosisCode>();
            //[-] list.AddRange(logic.VecInfo.DiagCodes.Select((typeDiagCode x) => new PukDiagnosisCode
            //[-] {
            //[-] Code = x.DiagnoseCode
            //[-] }));
            //[-] if (((Logic)logic).VehicleCase != null)
            //[-] {
            //[-] istaPukService.StoreDiagnosisCodes(list, ((Logic)logic).VehicleCase).Wait();
            //[-] }
            //[-] }
            //[-] }
        }

        public void SetNextButtonEnabled(bool enable)
        {
            serviceProgramController.SetNextButtonEnabled(enable);
        }

        public string FindIdentifierInfoObjStarted()
        {
            Log.Error("ModuleExecutionParent.FindIdentifierInfoObjStarted()", "Returning null.");
            return null;
        }

        public void AddDocInfoObjects(IList<InfoObject> doc, int slot, IProtocolBasic fasta)
        {
            if (serviceProgramController != null)
            {
                serviceProgramController.AddDocInfoObjects(doc, slot, fasta);
            }
            else
            {
                Log.Error("ModuleExecutionParent.AddDocInfoObjects()", "Will be ignored.");
            }
        }

        public void AddSuspiciousObject(string grobzeichen)
        {
            if (serviceProgramController != null)
            {
                serviceProgramController.AddSuspiciousDiagObject(grobzeichen);
            }
            else
            {
                Log.Error("ModuleExecutionParent.AddSuspiciousObject()", "Will be ignored.");
            }
        }

        public void DisplayWaitCursor(bool bWaitCursor)
        {
            Log.Error("ModuleExecutionParent.DisplayWaitCursor()", "Will be ignored.");
        }

        public bool IsWaitCursor()
        {
            Log.Error("ModuleExecutionParent.IsWaitCursor()", "Returning false.");
            return false;
        }

        public void NavigateTo(IModuleExecutionStep step)
        {
            IServiceDialogModel model = step as IServiceDialogModel;
            if (serviceProgramController != null)
            {
                serviceProgramController.NavigateToDialog(model);
            }
            else
            {
                Log.Error("ModuleExecutionParent.NavigateTo()", "Will be ignored because serviceProgramController is null.");
            }
        }

        public void RemoveDocInfoObjects(IList<InfoObject> doc, int slot)
        {
            if (serviceProgramController != null)
            {
                serviceProgramController.RemoveDocInfoObjects(doc, slot);
            }
            else
            {
                Log.Error("ModuleExecutionParent.RemoveDocInfoObjects()", "Will be ignored.");
            }
        }

        public void RemoveDocInfoObjectsAll()
        {
            if (serviceProgramController != null)
            {
                serviceProgramController.RemoveDocInfoObjectsAll();
            }
            else
            {
                Log.Error("ModuleExecutionParent.RemoveDocInfoObjectsAll()", "Will be ignored.");
            }
        }

        public void ResetNextButtonLatency()
        {
            Log.Error("ModuleExecutionParent.ResetNextButtonLatency()", "Will be ignored.");
        }

        public bool WaitForContinueButton(int TimeOut)
        {
            Log.Error("ModuleExecutionParent.WaitForContinueButton(int)", "Will be ignored, returning true.");
            return true;
        }

        public void WaitForContinueButton()
        {
            Log.Error("ModuleExecutionParent.WaitForContinueButton()", "Will be ignored.");
        }

        public void ShowErrorMessage(string message, string details)
        {
            string title = FormatedData.Localize("#Error");
            string message2 = FormatedData.Localize(message);
            logic.Services.InteractionService.RegisterMessage(title, message2, details);
        }

        public void ShowInfoMessage(string message, string details)
        {
            string title = FormatedData.Localize("#Info");
            string message2 = FormatedData.Localize(message);
            logic.Services.InteractionService.RegisterMessage(title, message2, details);
        }
    }
}
