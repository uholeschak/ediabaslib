using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    public interface IPsdzKdsFailureResponseCto
    {
        ILocalizableMessageTo Cause { get; }

        IPsdzKdsIdCto KdsId { get; }
    }
}
