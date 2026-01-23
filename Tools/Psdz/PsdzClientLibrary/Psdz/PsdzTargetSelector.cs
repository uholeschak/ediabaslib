using PsdzClient;
using System.Globalization;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzTargetSelector : IPsdzTargetSelector
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Baureihenverbund { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsDirect { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Project { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string VehicleInfo { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "TargetSelector: Project={0}, VehicleInfo={1}", Project, VehicleInfo);
        }
    }
}
