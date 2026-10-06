using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class SoftwareSigStateEnumMapper : NullableEnumMapper<PsdzSoftwareSigState, SoftwareSigStateModel>
    {
        protected override IDictionary<PsdzSoftwareSigState?, SoftwareSigStateModel?> CreateMap()
        {
            return new Dictionary<PsdzSoftwareSigState?, SoftwareSigStateModel?>
            {
                {
                    PsdzSoftwareSigState.Accepted,
                    SoftwareSigStateModel.Accepted
                },
                {
                    PsdzSoftwareSigState.Invalid,
                    SoftwareSigStateModel.Invalid
                },
                {
                    PsdzSoftwareSigState.Imported,
                    SoftwareSigStateModel.Imported
                },
                {
                    PsdzSoftwareSigState.Rejected,
                    SoftwareSigStateModel.Rejected
                },
                {
                    PsdzSoftwareSigState.NotAvailable,
                    SoftwareSigStateModel.NotAvailable
                }
            };
        }
    }
}