using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public interface IEcuLogisticsEntry
    {
        int DiagAddress { get; }

        string Name { get; }

        string GroupSgbd { get; }

        BusType Bus { get; }

        int Column { get; }

        int Row { get; }

        string ShortName { get; }

        long? SubDiagAddress { get; }

        BusType[] SubBusList { get; }
    }
}
