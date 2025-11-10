using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [DataContract]
    internal class EcuData
    {
        [DataMember(Name = "BTLD-SGBM-NO")]
        public readonly string btld;
        [DataMember(Name = "CAFD-SGBM-ID")]
        public readonly string[] cafd;
        public List<string> CafdId => cafd.Select((string a) => a.Split('.')[0].Replace("cafd_", "").ToUpper()).ToList();

        public override string ToString()
        {
            string arg = btld;
            string[] array = cafd;
            return string.Format("BTLD-SGBM-NO :{0} - CAFD-SGBM-ID:{1}", arg, (array != null && array.Any()) ? string.Join("/", cafd) : string.Empty);
        }
    }
}