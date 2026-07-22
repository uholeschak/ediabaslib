using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework
{
    public interface IModuleExecutionParent
    {
        string Title { get; }

        bool IsNextEnabled { get; set; }

        bool IsKeyboardEnabled { get; set; }

        DateTime LastTimeNextButtonPressed { get; }

        bool NextButtonPressedWithinLastSecond { get; }

        IFastaGrouping FastaGrouping { get; set; }

        IModule ModuleData { get; }

        void AddDocInfoObjects(IList<InfoObject> doc, int slot, IProtocolBasic fasta);

        void DisplayWaitCursor(bool bWaitCursor);

        bool IsWaitCursor();

        void NavigateTo(IModuleExecutionStep step);

        void RemoveDocInfoObjects(IList<InfoObject> doc, int slot);

        void RemoveDocInfoObjectsAll();

        void ResetNextButtonLatency();

        bool WaitForContinueButton(int TimeOut);

        void WaitForContinueButton();

        void SetNextButtonEnabled(bool enabled);

        string FindIdentifierInfoObjStarted();

        uint GetScreenMode();

        void SetScreenMode(uint screenmode);

        void ResetPageTitle();

        void AddSuspiciousObject(string grobzeichen);

        void Close(bool abort);

        void ShowErrorMessage(string message, string details);

        void ShowInfoMessage(string message, string details);
    }
}
