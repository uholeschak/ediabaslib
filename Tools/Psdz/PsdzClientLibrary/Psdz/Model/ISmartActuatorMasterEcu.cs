using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISmartActuatorMasterEcu : IEcuObj
    {
        IStandardSvk SmacMasterSVK { get; }

        IList<ISmartActuatorEcu> SmartActuators { get; }
    }
}