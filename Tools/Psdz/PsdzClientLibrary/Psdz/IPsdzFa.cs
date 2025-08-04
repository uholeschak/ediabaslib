using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    public interface IPsdzFa : IPsdzStandardFa
    {
        string AsXml { get; }

#if OLD_PSDZ_FA
#warning OLD_PSDZ_FA activated. Do not use for release builds.
        string Vin { get; }
#endif
    }
}
