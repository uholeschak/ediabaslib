using PsdzClient;
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzTargetSelector))]
    public class PsdzConnection : IPsdzConnection
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public Guid Id { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzTargetSelector TargetSelector { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int Port { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Connection: [Id: {0}, TargetSelector: {1}, Port: {2}],", Id, TargetSelector, Port);
        }
    }
}
