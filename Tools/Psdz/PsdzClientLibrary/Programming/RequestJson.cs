using BMW.Rheingold.Programming.Common;
using PsdzClient.Programming;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [PreserveSource(Hint = "Changed to public", AccessModified = true)]
    [DataContract]
    public class RequestJson
    {
        [DataMember(Name = "FA")]
        public readonly string fa;
        [DataMember(Name = "testerNo")]
        public readonly string testerNo;
        [DataMember(Name = "vpc")]
        public readonly string vpc;
        [DataMember(Name = "calcEcuData")]
        public readonly EcuDataGroup calcEcuData;
        [DataMember(Name = "currEcuData")]
        public readonly EcuDataGroup currEcuData;
        internal string FaAsXml => Encoding.Default.GetString(Convert.FromBase64String(fa));

        public RequestJson(string fa)
        {
            this.fa = fa;
        }

        public override string ToString()
        {
            return $"FA: {ProgrammingUtils.NormalizeXmlText(FaAsXml)} - vpc:{vpc} - CurrEcuData:{currEcuData?.ToString() ?? string.Empty} - CalcEcuData:{calcEcuData?.ToString() ?? string.Empty}";
        }

        internal bool CompareFA(string faAsXmlToBeCompared)
        {
            string x = CleanHeaderAttibutes(FaAsXml);
            string y = CleanHeaderAttibutes(faAsXmlToBeCompared);
            return StringComparer.Create(CultureInfo.InvariantCulture, ignoreCase: true).Equals(x, y);
        }

        private string CleanHeaderAttibutes(string xmlContent)
        {
            XDocument xDocument = XDocument.Parse(xmlContent);
            (
                from x in xDocument.Descendants().FirstOrDefault((XElement p) => p.Name.LocalName == "header").Attributes()
                where x.Name == "date" || x.Name == "time" || x.Name == "createdBy"
                select x).Select(delegate (XAttribute x)
            {
                x.Value = string.Empty;
                return x;
            }).ToList();
            return xDocument.ToString();
        }

        [PreserveSource(Hint = "Added for backward compatibility")]
        [DataMember(Name = "ecuData")]
        public readonly EcuData[] ecuData;
    }
}