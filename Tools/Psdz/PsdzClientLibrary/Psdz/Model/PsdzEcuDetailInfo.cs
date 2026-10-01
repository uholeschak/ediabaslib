using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzEcuDetailInfo : IPsdzEcuDetailInfo
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte ByteValue { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PsdzEcuDetailInfo psdzEcuDetailInfo)
            {
                return ByteValue == psdzEcuDetailInfo.ByteValue;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ByteValue.GetHashCode();
        }
    }
}
