using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Swt;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt
{
    public interface IPsdzSwtAction
    {
        IEnumerable<IPsdzSwtEcu> SwtEcus { get; }
    }
}
