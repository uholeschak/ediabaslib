using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzSecurityBackendRequestFailureCto
    {
        ILocalizableMessageTo Cause { get; }

        int Retry { get; }

        string Url { get; }
    }
}
