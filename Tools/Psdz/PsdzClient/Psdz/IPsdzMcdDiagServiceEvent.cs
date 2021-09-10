using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Localization;

namespace BMW.Rheingold.Psdz.Model.Events
{
    public interface IPsdzMcdDiagServiceEvent : ILocalizableMessage, IPsdzEvent
    {
        int ErrorId { get; }

        string ErrorName { get; }

        string JobName { get; }

        string LinkName { get; }

        string ServiceName { get; }

        string ResponseType { get; }

        bool IsTimingEvent { get; }
    }
}
