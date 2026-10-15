using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzStandardSvk
    {
        byte ProgDepChecked { get; }

        IEnumerable<IPsdzSgbmId> SgbmIds { get; set; }

        byte SvkVersion { get; }
    }
}