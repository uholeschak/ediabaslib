using PsdzClient;
using System.Collections.Generic;
using Windows.UI.Text;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public interface IInfoObjectContent
    {
        string Doc { get; }

        byte[] BinaryDocument { get; }

        ICollection<XEP_QUERYOBJECTSEX> ListSvgLinks { get; }

        ICollection<LinkType> ListLinks { get; }

        ICollection<GraphicsType> ListGraphics { get; }

        ICollection<string> ListIncludes { get; }
    }
}
