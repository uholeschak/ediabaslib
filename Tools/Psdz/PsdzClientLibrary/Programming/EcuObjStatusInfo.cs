using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming.API
{
    [DataContract]
    internal class EcuObjStatusInfo : IEcuStatusInfo
    {
        [DataMember]
        public byte Value { get; private set; }

        [DataMember]
        public bool HasIndividualData { get; private set; }

        public EcuObjStatusInfo()
        {
        }

        internal EcuObjStatusInfo(byte value, bool hasIndividualData)
        {
            Value = value;
            HasIndividualData = hasIndividualData;
        }
    }
}