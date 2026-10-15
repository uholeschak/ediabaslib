using System;
using System.Runtime.Serialization;

#pragma warning disable CS0649
namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [DataContract]
    internal class SignedNcd
    {
        [DataMember(Name = "ncd")]
        public readonly string ncd;

        [DataMember(Name = "BTLD-SGBM-NO")]
        public readonly string btld;

        [DataMember(Name = "CAFD-SGBM-ID")]
        public readonly string cafd;

        public byte[] NcdConvertedFromBase64 => Convert.FromBase64String(ncd);

        public override string ToString()
        {
            return "BTLD-SGBM-NO :" + btld + " - CAFD-SGBM-ID:" + cafd + " - Ncd: " + ncd;
        }
    }
}
