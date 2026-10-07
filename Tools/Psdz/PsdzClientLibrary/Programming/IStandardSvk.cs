using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IStandardSvk
    {
        IEnumerable<ISgbmId> SgbmIds { get; }

        byte SvkVersion { get; }

        byte ProgDepChecked { get; }
    }
}
