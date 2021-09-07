using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IStandardSvk
    {
        IEnumerable<ISgbmId> SgbmIds { get; }

        byte SvkVersion { get; }

        byte ProgDepChecked { get; }
    }
}
