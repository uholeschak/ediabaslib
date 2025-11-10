using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
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