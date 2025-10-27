using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
{
    internal class TaExecutionStateMapper : MapperBase<PsdzTaExecutionState?, TaExecutionStateModel>
    {
        protected override IDictionary<PsdzTaExecutionState?, TaExecutionStateModel> CreateMap()
        {
            return new Dictionary<PsdzTaExecutionState?, TaExecutionStateModel>
            {
                {
                    PsdzTaExecutionState.Executable,
                    TaExecutionStateModel.Executable
                },
                {
                    PsdzTaExecutionState.Inactive,
                    TaExecutionStateModel.Inactive
                },
                {
                    PsdzTaExecutionState.NotExecutable,
                    TaExecutionStateModel.NotExecutable
                },
                {
                    PsdzTaExecutionState.AbortedByError,
                    TaExecutionStateModel.AbortedByError
                },
                {
                    PsdzTaExecutionState.AbortedByUser,
                    TaExecutionStateModel.AbortedByUser
                },
                {
                    PsdzTaExecutionState.Finished,
                    TaExecutionStateModel.Finished
                },
                {
                    PsdzTaExecutionState.FinishedWithError,
                    TaExecutionStateModel.FinishedWithError
                },
                {
                    PsdzTaExecutionState.FinishedWithWarnings,
                    TaExecutionStateModel.FinishedWithWarnings
                },
                {
                    PsdzTaExecutionState.Running,
                    TaExecutionStateModel.Running
                },
                {
                    PsdzTaExecutionState.Repeat,
                    TaExecutionStateModel.Repeat
                },
                {
                    PsdzTaExecutionState.NotRequired,
                    TaExecutionStateModel.NotRequired
                }
            };
        }
    }
}