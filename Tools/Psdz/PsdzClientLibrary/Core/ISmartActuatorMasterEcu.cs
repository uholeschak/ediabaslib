using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.CoreFramework.Programming.Data.Ecu
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISmartActuatorMasterEcu : IEcuObj
    {
        IStandardSvk SmacMasterSVK { get; }

        IList<ISmartActuatorEcu> SmartActuators { get; }
    }
}