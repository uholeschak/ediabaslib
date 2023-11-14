using PsdzClient.Core;

namespace PsdzClientLibrary.Core
{
    public enum ProgressCancelBehavior
    {
        NonInterruptable,
        Interruptable
    }

    public enum ProgressRequestConfirmationType
    {
        Information,
        Question,
        VehicleIgnition,
        SALAPASelection,
        GWSZInput
    }

    public interface IProgressMonitor
    {
        ProgressCancelBehavior CancelBehavior { get; set; }

        long EndTime { get; set; }

        bool IsAborted { get; set; }

        FormatedData ProcessDescription { get; set; }

        double ProcessProgress { get; set; }

        double ProgressMultiplierCmdList { get; set; }

        FormatedData TaskDescription { get; set; }

        bool IsRunningInBackground { get; set; }

        bool IsMinimizeable { get; set; }

        bool IsMinimizeableToAppHeader { get; set; }

        bool RequestConfirmation(ProgressRequestConfirmationType requestType, params object[] paramList);
    }
}
