using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzFsaTa : IPsdzTa, IPsdzTalElement
    {
        long EstimatedExecutionTime { get; set; }

        long FeatureId { get; set; }
    }
}
