using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Programming.API
{
    [DataContract]
    internal class EcuObjDetailInfo : IEcuDetailInfo
    {
        [DataMember]
        public byte Value { get; private set; }

        internal EcuObjDetailInfo(byte value)
        {
            Value = value;
        }
    }
}