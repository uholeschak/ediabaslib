using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzSecurityBackendRequestFailureCto
    {
        ILocalizableMessageTo Cause { get; }

        int Retry { get; }

        string Url { get; }
    }
}
