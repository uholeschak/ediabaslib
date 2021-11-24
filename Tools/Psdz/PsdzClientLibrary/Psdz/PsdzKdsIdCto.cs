﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [DataContract]
    public class PsdzKdsIdCto : IPsdzKdsIdCto
    {
        [DataMember]
        public string IdAsHex { get; set; }

        [DataMember]
        public int Id { get; set; }
    }
}
