using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class EcuObjDetailInfo : IEcuDetailInfo
    {
        internal EcuObjDetailInfo(byte value)
        {
            this.Value = value;
        }

        public byte Value { get; private set; }
    }
}
