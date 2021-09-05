using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class ClientConfigArgs
    {
        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000001 RID: 1 RVA: 0x000020D0 File Offset: 0x000002D0
        // (set) Token: 0x06000002 RID: 2 RVA: 0x000020D8 File Offset: 0x000002D8
        [DataMember]
        public string DealerID { get; set; }

        // Token: 0x06000003 RID: 3 RVA: 0x000020E1 File Offset: 0x000002E1
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}:             {1}\n", "DealerID", this.DealerID);
            return stringBuilder.ToString();
        }
    }
}
