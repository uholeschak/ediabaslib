using PsdzClient;
using System.Collections.Generic;
using Windows.UI.Text;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public interface IInfoObjectContent
    {
        string Doc { get; }

        byte[] BinaryDocument { get; }

        [PreserveSource(Hint = "ICollection<XEP_QUERYOBJECTSEX>", Placeholder = true)]
        ICollection<PlaceholderType> ListSvgLinks { get; }

        ICollection<LinkType> ListLinks { get; }

        ICollection<GraphicsType> ListGraphics { get; }

        ICollection<string> ListIncludes { get; }
    }
}
