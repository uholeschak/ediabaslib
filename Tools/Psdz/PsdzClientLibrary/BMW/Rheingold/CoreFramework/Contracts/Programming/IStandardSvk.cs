using System.Collections.Generic;

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
