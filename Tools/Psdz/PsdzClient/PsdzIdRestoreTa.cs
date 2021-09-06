using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class PsdzIdRestoreTa : PsdzTa
    {
        [DataMember]
        public string BackupFile { get; set; }
    }
}
