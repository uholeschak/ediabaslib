using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzEcuStatusInfo : IPsdzEcuStatusInfo
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte ByteValue { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool HasIndividualData { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PsdzEcuStatusInfo psdzEcuStatusInfo)
            {
                return ByteValue == psdzEcuStatusInfo.ByteValue;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ByteValue.GetHashCode();
        }
    }
}