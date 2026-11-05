using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.EcuManager
{
    public interface IEcuListManager
    {
        IReadOnlyList<IIdentEcu> GetEcus();

        IIdentEcu GetEcuByIdSgAdr(long idSgAdr);

        bool ContainsEcu(long idSgAdr);

        void AddEcu(IIdentEcu ecu, EcuDataSource source);

        void UpdateEcu(IIdentEcu ecu, EcuDataSource source);

        void RemoveEcu(long idSgAdr, EcuDataSource source);

        void ReplaceEcus(IEnumerable<IIdentEcu> ecus, EcuDataSource source);
    }
}
