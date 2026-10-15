using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.EcuTree
{
    public interface IEcuTreeSvk
    {
        IEnumerable<string> XWE_SGBMID { get; }
    }
}