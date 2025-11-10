using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Programming
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