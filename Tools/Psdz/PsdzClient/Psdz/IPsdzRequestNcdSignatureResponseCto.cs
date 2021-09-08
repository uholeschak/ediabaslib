using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzRequestNcdSignatureResponseCto
    {
        IList<IPsdzSignatureResultCto> SignatureResultCtoList { get; }

        int DurationOfLastRequest { get; }

        IList<IPsdzSecurityBackendRequestFailureCto> Failures { get; }

        PsdzSecurityBackendRequestProgressStatusToEnum ProgressStatus { get; }
    }
}
