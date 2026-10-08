using System.Collections;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IEcuProgrammingInfos : IEnumerable<IEcuProgrammingInfo>, IEnumerable
    {
        IEcuProgrammingInfosData DataContext { get; }

        IEcuProgrammingInfo GetItem(IEcu ecu);
        IEcuProgrammingInfo GetItem(IEcu ecu, string category);
        void SelectProgrammingForIndustrialCustomer(IEcu ecu, string category, bool value);
        void SelectCodingForIndustrialCustomer(IEcu ecu, string category, bool value);
        void SelectReplacementForIndustrialCustomer(IEcu ecu, string category, bool value);
        void EstablishSelection();
    }
}