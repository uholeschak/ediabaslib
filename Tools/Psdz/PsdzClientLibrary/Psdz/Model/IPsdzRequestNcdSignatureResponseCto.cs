using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding.SignatureResultCtos;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.RequestNcdSignatureResponseCto
{
    public interface IPsdzRequestNcdSignatureResponseCto
    {
        IList<IPsdzSignatureResultCto> SignatureResultCtoList { get; }

        int DurationOfLastRequest { get; }

        IList<IPsdzSecurityBackendRequestFailureCto> Failures { get; }

        PsdzSecurityBackendRequestProgressStatusToEnum ProgressStatus { get; }
    }
}
