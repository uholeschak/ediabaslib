using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzTalLine : IPsdzTalElement
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        PsdzFscDeploy FscDeploy { get; }

        PsdzBlFlash BlFlash { get; }

        PsdzIbaDeploy IbaDeploy { get; }

        PsdzSwDeploy SwDeploy { get; }

        PsdzIdRestore IdRestore { get; }

        PsdzSFADeploy SFADeploy { get; }

        PsdzTaCategories TaCategories { get; }

        IPsdzTaCategory TaCategory { get; }
    }
}
