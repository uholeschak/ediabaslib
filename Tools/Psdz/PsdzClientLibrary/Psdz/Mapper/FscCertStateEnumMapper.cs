using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class FscCertStateEnumMapper : MapperBase<FscCertStateModel, PsdzFscCertificateState>
    {
        protected override IDictionary<FscCertStateModel, PsdzFscCertificateState> CreateMap()
        {
            return new Dictionary<FscCertStateModel, PsdzFscCertificateState>
            {
                {
                    FscCertStateModel.Accepted,
                    PsdzFscCertificateState.Accepted
                },
                {
                    FscCertStateModel.Imported,
                    PsdzFscCertificateState.Imported
                },
                {
                    FscCertStateModel.Invalid,
                    PsdzFscCertificateState.Invalid
                },
                {
                    FscCertStateModel.Rejected,
                    PsdzFscCertificateState.Rejected
                },
                {
                    FscCertStateModel.NotAvailable,
                    PsdzFscCertificateState.NotAvailable
                }
            };
        }
    }
}