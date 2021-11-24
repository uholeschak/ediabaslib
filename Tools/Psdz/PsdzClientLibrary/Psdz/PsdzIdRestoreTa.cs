using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    public class PsdzIdRestoreTa : PsdzTa
    {
        [DataMember]
        public string BackupFile { get; set; }
    }
}
