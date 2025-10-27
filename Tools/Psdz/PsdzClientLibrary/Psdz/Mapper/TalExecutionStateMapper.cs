using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
{
    internal class TalExecutionStateMapper : MapperBase<PsdzTalExecutionState, TalExecutionStateModel>
    {
        protected override IDictionary<PsdzTalExecutionState, TalExecutionStateModel> CreateMap()
        {
            return new Dictionary<PsdzTalExecutionState, TalExecutionStateModel>
            {
                {
                    PsdzTalExecutionState.AbortedByError,
                    TalExecutionStateModel.AbortedByError
                },
                {
                    PsdzTalExecutionState.AbortedByUser,
                    TalExecutionStateModel.AbortedByUser
                },
                {
                    PsdzTalExecutionState.Executable,
                    TalExecutionStateModel.Executable
                },
                {
                    PsdzTalExecutionState.Finished,
                    TalExecutionStateModel.Finished
                },
                {
                    PsdzTalExecutionState.FinishedForHardwareTransactions,
                    TalExecutionStateModel.FinishedForHardwareTransactions
                },
                {
                    PsdzTalExecutionState.FinishedForHardwareTransactionsWithError,
                    TalExecutionStateModel.FinishedForHardwareTransactionsWithError
                },
                {
                    PsdzTalExecutionState.FinishedForHardwareTransactionsWithWarnings,
                    TalExecutionStateModel.FinishedForHardwareTransactionsWithWarnings
                },
                {
                    PsdzTalExecutionState.FinishedWithError,
                    TalExecutionStateModel.FinishedWithError
                },
                {
                    PsdzTalExecutionState.FinishedWithWarnings,
                    TalExecutionStateModel.FinishedWithWarnings
                },
                {
                    PsdzTalExecutionState.Running,
                    TalExecutionStateModel.Running
                }
            };
        }
    }
}