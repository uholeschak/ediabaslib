using PsdzClientLibrary;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(Removed = true)]
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
