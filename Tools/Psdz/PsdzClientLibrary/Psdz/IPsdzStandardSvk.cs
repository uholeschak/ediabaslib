using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    public interface IPsdzStandardSvk
    {
        byte ProgDepChecked { get; }

        IEnumerable<IPsdzSgbmId> SgbmIds { get; set; }

        byte SvkVersion { get; }
    }
}