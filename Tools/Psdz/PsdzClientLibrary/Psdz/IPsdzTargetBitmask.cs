using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzTargetBitmask
    {
        IList<IPsdzEcuFailureResponseCto> FailedEcus { get; }

        byte[] TargetBitmask { get; }
    }
}