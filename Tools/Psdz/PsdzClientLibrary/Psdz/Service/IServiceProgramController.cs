using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.ServiceProgram;
using PsdzClient.Core.Container;
using System.Collections.Generic;
using BMW.ISPI.IstaOperation.Contract.Document;

namespace BMW.ISPI.IstaOperation.Contract.ServiceProgram
{
    public interface IServiceProgramController
    {
        ScreenMode ScreenMode { get; set; }

        string Identifier { get; }

        bool NavigateToDialog(IServiceDialogModel model);

        ServiceProgramAction AwaitUserAction(int millisecondsTimeout);

        void AddDocInfoObjects(IList<InfoObject> doc, int slot, IProtocolBasic fasta);

        void AbortServiceProgram();

        void RemoveDocInfoObjects(IList<InfoObject> doc, int slot);

        void RemoveDocInfoObjectsAll();

        void AddSuspiciousDiagObject(string grobzeichen);

        void SetNextButtonEnabled(bool enable);

        bool IsNextButtonEnabled();

        bool IsNextButtonPressedWithinTimePeriod();

        void ResetLastTimeNextButtonPressed();

        void NavigateForward(int milliseconds = 0);

        void SetDisplayMode(DisplayMode mode);

        void HandleRDCToolDataAction();
    }
}
