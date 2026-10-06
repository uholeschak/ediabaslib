using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzCheckNcdResultEto
    {
        IList<IPsdzDetailedNcdInfoEto> DetailedNcdStatus { get; }

        bool isEachNcdSigned { get; }
    }
}
