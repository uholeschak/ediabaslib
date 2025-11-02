using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzFetchCalculationSecureTokensResultCto
    {
        IList<IPsdzDetailedStatusCto> DetailedStatus { get; }

        int DurationOfLastRequest { get; }

        IList<IPsdzSecurityBackendRequestFailureCto> Failures { get; }

        string FeatureSetReference { get; }

        string JsonString { get; }

        PsdzSecurityBackendRequestProgressStatusToEnum ProgressStatus { get; }

        IList<IPsdzSecureTokenEto> SecureTokens { get; set; }

        IPsdzSecureTokenForVehicleEto SecureTokenForVehicle { get; }

        PsdzTokenOverallStatusEtoEnum TokenOverallStatusEto { get; }

        string TokenPackageReference { get; }
    }
}
