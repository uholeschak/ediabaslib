using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
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
            PsdzSwtApplicationId psdzSwtApplicationId = obj as PsdzSwtApplicationId;
            return psdzSwtApplicationId != null && psdzSwtApplicationId.ApplicationNumber == this.ApplicationNumber && psdzSwtApplicationId.UpgradeIndex == this.UpgradeIndex;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X4}{1:X4}", this.ApplicationNumber, this.UpgradeIndex);
        }
    }
}
