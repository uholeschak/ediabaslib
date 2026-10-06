using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class FscStateEnumMapper : MapperBase<PsdzFscState, FscStateModel>
    {
        protected override IDictionary<PsdzFscState, FscStateModel> CreateMap()
        {
            return new Dictionary<PsdzFscState, FscStateModel>
            {
                {
                    PsdzFscState.Accepted,
                    FscStateModel.Accepted
                },
                {
                    PsdzFscState.Invalid,
                    FscStateModel.Invalid
                },
                {
                    PsdzFscState.Cancelled,
                    FscStateModel.Cancelled
                },
                {
                    PsdzFscState.Imported,
                    FscStateModel.Imported
                },
                {
                    PsdzFscState.Rejected,
                    FscStateModel.Rejected
                },
                {
                    PsdzFscState.NotAvailable,
                    FscStateModel.NotAvailable
                }
            };
        }
    }
}