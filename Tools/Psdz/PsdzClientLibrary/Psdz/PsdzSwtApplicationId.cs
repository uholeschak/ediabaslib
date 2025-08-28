using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Swt
{
    [DataContract]
    public class PsdzSwtApplicationId : IPsdzSwtApplicationId
    {
        [DataMember]
        public int ApplicationNumber { get; set; }

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
