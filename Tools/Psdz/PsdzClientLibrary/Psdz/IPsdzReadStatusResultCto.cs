﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzReadStatusResultCto
    {
        IList<IPsdzFeatureLongStatusCto> FeatureStatusSet { get; set; }

        IList<IPsdzEcuFailureResponseCto> Failures { get; set; }
    }
}
