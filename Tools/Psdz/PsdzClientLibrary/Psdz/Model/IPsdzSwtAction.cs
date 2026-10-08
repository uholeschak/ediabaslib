using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt
{
    public interface IPsdzSwtAction
    {
        IEnumerable<IPsdzSwtEcu> SwtEcus { get; }
    }
}
