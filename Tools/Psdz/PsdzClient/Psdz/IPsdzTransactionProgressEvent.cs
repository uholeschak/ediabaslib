using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Localization;

namespace BMW.Rheingold.Psdz.Model.Events
{
    public interface IPsdzTransactionProgressEvent : ILocalizableMessage, IPsdzEvent, IPsdzTransactionEvent
    {
        int Progress { get; }

        int TaProgress { get; }
    }
}
