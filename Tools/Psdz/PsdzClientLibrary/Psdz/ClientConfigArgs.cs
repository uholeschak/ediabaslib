using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    public class ClientConfigArgs
    {
        [DataMember]
        public string DealerID { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}:             {1}\n", "DealerID", DealerID);
            return stringBuilder.ToString();
        }
    }
}
