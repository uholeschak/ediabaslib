using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [DataContract]
    public class RequestJson
    {
        public RequestJson(string fa)
        {
            this.fa = fa;
        }

        internal string FaAsXml
        {
            get
            {
                return Encoding.Default.GetString(Convert.FromBase64String(this.fa));
            }
        }

        internal new string ToString
        {
            get
            {
                return "FA: " + ProgrammingUtils.NormalizeXmlText(this.FaAsXml) + " - EcuData:" + string.Join("/", (from r in this.ecuData
                    select r.ToString).ToArray<string>());
            }
        }

        internal bool CompareFA(string faAsXmlToBeCompared)
        {
            string x = this.CleanHeaderAttibutes(this.FaAsXml);
            string y = this.CleanHeaderAttibutes(faAsXmlToBeCompared);
            return StringComparer.Create(CultureInfo.InvariantCulture, true).Equals(x, y);
        }

        private string CleanHeaderAttibutes(string xmlContent)
        {
            XDocument xdocument = XDocument.Parse(xmlContent);
            (from x in xdocument.Descendants().FirstOrDefault((XElement p) => p.Name.LocalName == "header").Attributes()
                where x.Name == "date" || x.Name == "time" || x.Name == "createdBy"
                select x).Select(delegate (XAttribute x)
            {
                x.Value = string.Empty;
                return x;
            }).ToList<XAttribute>();
            return xdocument.ToString();
        }

        [DataMember(Name = "FA")]
        public readonly string fa;

        [DataMember(Name = "ecuData")]
        public readonly EcuData[] ecuData;
    }
}
