using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class DocumentLink
    {
        public string Id { get; private set; }

        public string Url { get; private set; }

        public string ZoomUrl { get; private set; }

        public DocumentLink(string id, string url)
            : this(id, url, null)
        {
        }

        public DocumentLink(string id, string url, string zoomUrl)
        {
            Id = id;
            Url = url;
            ZoomUrl = zoomUrl;
        }

        public string ToHtml()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<link linkid=\"").Append(Id).Append("\" url=\"")
                .Append(Url)
                .Append("\"");
            if (ZoomUrl != null)
            {
                stringBuilder.Append(" zoomurl=\"").Append(ZoomUrl).Append("\"");
            }
            stringBuilder.Append("/>");
            return stringBuilder.ToString();
        }

        public XElement ToXml()
        {
            List<XAttribute> list = new List<XAttribute>();
            list.Add(new XAttribute("linkid", Id));
            list.Add(new XAttribute("url", Url));
            if (ZoomUrl != null)
            {
                list.Add(new XAttribute("zoomurl", ZoomUrl));
            }
            return new XElement("link", list);
        }
    }
}
