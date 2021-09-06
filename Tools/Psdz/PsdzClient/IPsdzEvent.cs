using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzEvent : ILocalizableMessage
    {
        IPsdzEcuIdentifier EcuId { get; }

        string EventId { get; }

        string Message { get; }

        long Timestamp { get; }
    }
}
