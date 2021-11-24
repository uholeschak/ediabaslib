using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzTargetSelector : IPsdzTargetSelector
    {
        [DataMember]
        public string Baureihenverbund { get; set; }

        [DataMember]
        public bool IsDirect { get; set; }

        [DataMember]
        public string Project { get; set; }

        [DataMember]
        public string VehicleInfo { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "TargetSelector: Project={0}, VehicleInfo={1}", this.Project, this.VehicleInfo);
        }
    }
}
