using System.Collections.Generic;
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.VinValidator
{
    public class VINResolverResult
    {
        public IEnumerable<string> PossibleVINs { get; set; }

        public VINResolverResultType ResultType { get; set; }

        public BackendStatus FDLGateStatus { get; set; }

        public BackendStatus SVMDStatus { get; set; }

        public bool VINResolvedSuccessfully { get; set; }
    }
}
