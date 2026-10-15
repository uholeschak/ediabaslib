using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzCheckNcdResultEto
    {
        IList<IPsdzDetailedNcdInfoEto> DetailedNcdStatus { get; }

        bool isEachNcdSigned { get; }
    }
}
