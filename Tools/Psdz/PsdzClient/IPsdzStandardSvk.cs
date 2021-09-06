using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzStandardSvk
    {
        byte ProgDepChecked { get; }

        IEnumerable<IPsdzSgbmId> SgbmIds { get; }

        byte SvkVersion { get; }
    }
}
