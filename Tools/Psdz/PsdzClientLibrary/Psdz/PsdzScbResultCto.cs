﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [KnownType(typeof(PsdzScbResultStatusCto))]
    [DataContract]
    [KnownType(typeof(PsdzSecurityBackendRequestFailureCto))]
    [KnownType(typeof(PsdzNcdCalculationRequestIdEto))]
    public class PsdzScbResultCto : IPsdzScbResultCto
    {
        public int ScbDurationOfLastRequest { get; set; }

        public IList<IPsdzSecurityBackendRequestFailureCto> ScbFailures { get; set; }

        public PsdzSecurityBackendRequestProgressStatusToEnum ScbProgressStatus { get; set; }

        public IPsdzNcdCalculationRequestIdEto ScbRequestId { get; set; }

        public IPsdzScbResultStatusCto ScbResultStatusCto { get; set; }
    }
}
