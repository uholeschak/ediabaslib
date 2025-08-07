using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public enum PsdzSecurityBackendRequestProgressStatusToEnum
    {
        ERROR,
        RUNNING,
        SUCCESS,
        UNKNOWN_REQUEST_ID
    }

    public enum PsdzTokenOverallStatusEtoEnum
    {
        AUTH_FAILED,
        CONFLICT,
        DETAILED,
        DNS_NOT_AVAILABLE,
        EMPTY,
        ERROR,
        HOSTNAME_NOT_CORRECT,
        INVALID_HOSTNAME,
        MALFORMED,
        NOT_IN_WHITELIST,
        OK,
        OTHER_ERROR,
        UNKNOWN_HOSTNAME,
        UNKNOWN_VERSION,
        VIN_MALFORMED,
        WRONG_FORMAT,
        NULL,
        TOKENPACKAGE_REBUILD_ERROR,
        FEATURE_SET_REBUILD_ERROR,
        NO_RIGHTS_ASSIGNED,
        LINK_TO_ID_UNKNOWN,
        UNDEFINED
    }

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
