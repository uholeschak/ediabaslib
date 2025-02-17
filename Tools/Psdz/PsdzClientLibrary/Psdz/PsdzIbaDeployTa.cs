﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    [KnownType(typeof(PsdzProtocol))]
    public class PsdzIbaDeployTa : PsdzTa
    {
        [DataMember]
        public PsdzProtocol? ActualProtocol { get; set; }

        [DataMember]
        public PsdzProtocol? PreferredProtocol { get; set; }
    }
}
