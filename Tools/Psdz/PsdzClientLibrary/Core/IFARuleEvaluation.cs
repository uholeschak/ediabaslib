using System.Collections.Generic;

namespace PsdzClientLibrary.Core
{
    public interface IFARuleEvaluation
    {
        IEnumerable<string> SA { get; }

        IEnumerable<string> E_WORT { get; }

        IEnumerable<string> HO_WORT { get; }
    }
}