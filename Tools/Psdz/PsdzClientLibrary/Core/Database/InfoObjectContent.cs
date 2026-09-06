using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Text;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class InfoObjectContent : IInfoObjectContent
    {
        public string Doc { get; set; }

        [PreserveSource(Hint = "ICollection<XEP_QUERYOBJECTSEX>", Placeholder = true)]
        public ICollection<PlaceholderType> ListSvgLinks { get; set; }

        public ICollection<LinkType> ListLinks { get; set; }

        public ICollection<GraphicsType> ListGraphics { get; set; }

        public byte[] BinaryDocument { get; set; }

        public ICollection<string> ListIncludes { get; set; }

        public InfoObjectContent()
        {
            ListLinks = new Collection<LinkType>();
            //[-] ListSvgLinks = new Collection<XEP_QUERYOBJECTSEX>();
            ListGraphics = new Collection<GraphicsType>();
            ListIncludes = new Collection<string>();
        }
    }
}
