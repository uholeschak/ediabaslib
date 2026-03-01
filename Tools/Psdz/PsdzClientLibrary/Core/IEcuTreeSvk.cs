using System.Collections.Generic;

namespace PsdzClient.Core
{
    public interface IEcuTreeSvk
    {
        IEnumerable<string> XWE_SGBMID { get; }
    }
}