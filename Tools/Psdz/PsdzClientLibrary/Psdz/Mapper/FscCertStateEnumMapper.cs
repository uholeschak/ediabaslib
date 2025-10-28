using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Swt;

namespace BMW.Rheingold.Psdz
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