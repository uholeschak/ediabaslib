using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    public interface IPsdzStandardFa
    {
        string AsString { get; }

        IEnumerable<string> EWords { get; }

        string Entwicklungsbaureihe { get; }

        int FaVersion { get; }

        IEnumerable<string> HOWords { get; }

        bool IsValid { get; }

        string Lackcode { get; }

        string Polstercode { get; }

        IEnumerable<string> Salapas { get; }

        string Type { get; }

        string Zeitkriterium { get; }

#if !OLD_PSDZ_FA
#warning OLD_PSDZ_FA activated. Do not use for release builds.
        string Vin { get; }
#endif
    }
}
