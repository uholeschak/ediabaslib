using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISmartActuatorMasterEcu : IEcuObj
    {
        IStandardSvk SmacMasterSVK { get; }

        IList<ISmartActuatorEcu> SmartActuators { get; }
    }
}