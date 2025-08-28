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
    [KnownType(typeof(PsdzTargetSelector))]
    public class PsdzConnection : IPsdzConnection
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public IPsdzTargetSelector TargetSelector { get; set; }

        [DataMember]
        public int Port { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Connection: [Id: {0}, TargetSelector: {1}, Port: {2}],", Id, TargetSelector, Port);
        }
    }
}
