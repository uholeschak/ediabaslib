using System.Globalization;
using System.Runtime.Serialization;

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
            return string.Format(CultureInfo.InvariantCulture, "TargetSelector: Project={0}, VehicleInfo={1}", Project, VehicleInfo);
        }
    }
}
