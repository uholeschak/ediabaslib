using System.Collections.Generic;

#pragma warning disable CS0618
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
