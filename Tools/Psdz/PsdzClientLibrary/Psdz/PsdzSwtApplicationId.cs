using PsdzClient;
using System.Globalization;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Swt
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzSwtApplicationId : IPsdzSwtApplicationId
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int ApplicationNumber { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int UpgradeIndex { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PsdzSwtApplicationId psdzSwtApplicationId)
            {
                if (psdzSwtApplicationId.ApplicationNumber == ApplicationNumber)
                {
                    return psdzSwtApplicationId.UpgradeIndex == UpgradeIndex;
                }
                return false;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X4}{1:X4}", ApplicationNumber, UpgradeIndex);
        }
    }
}
