using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public enum PsdzSecureEcuModeEtoEnum
    {
        PLANT,
        FIELD,
        ENGINEERING
    }
    
    public interface IPsdzReadSecureEcuModeResultCto
    {
        IDictionary<IPsdzEcuIdentifier, PsdzSecureEcuModeEtoEnum> SecureEcuModes { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> FailureResponse { get; }
    }
}
