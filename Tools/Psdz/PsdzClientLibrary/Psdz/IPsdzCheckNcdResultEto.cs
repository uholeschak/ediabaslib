using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public interface IPsdzCheckNcdResultEto
    {
        IList<IPsdzDetailedNcdInfoEto> DetailedNcdStatus { get; }

        bool isEachNcdSigned { get; }
    }
}
