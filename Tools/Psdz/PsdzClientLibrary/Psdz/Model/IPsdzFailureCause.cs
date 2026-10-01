using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Localization;

namespace BMW.Rheingold.Psdz.Model.Tal.TalStatus
{
    public interface IPsdzFailureCause : ILocalizableMessage
    {
        string Id { get; }

        string IdReference { get; }

        string Message { get; }

        IPsdzTalElement TalElement { get; }

        long Timestamp { get; }
    }
}
