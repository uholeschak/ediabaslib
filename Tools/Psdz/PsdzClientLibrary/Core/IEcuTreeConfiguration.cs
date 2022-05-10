using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PsdzClient.Core
{
    public interface IEcuTreeConfiguration
    {
        string SchemaVersion { get; }

        string MainSeriesSgbd { get; }

        string CompatibilityInfo { get; }

        string SitInfo { get; }

        double? RootHorizontalBusStep { get; }

        ReadOnlyCollection<IEcuLogisticsEntry> EcuLogisticsList { get; }

        ReadOnlyCollection<IBusLogisticsEntry> BusLogisticsList { get; }

        ReadOnlyCollection<IBusInterConnectionEntry> BusInterConnectionList { get; }

        ReadOnlyCollection<ICombinedEcuHousingEntry> CombinedEcuHousingList { get; }

        ReadOnlyCollection<ISGBDBusLogisticsEntry> SGBDBusLogisticsList { get; }

        ReadOnlyCollection<IBusNameEntry> BusNameList { get; }

        ReadOnlyCollection<IXGBMBusLogisticsEntry> XGBMBusLogisticsList { get; }

        List<int> MinimalConfigurationList { get; }

        List<int> ExcludedConfigurationList { get; }

        List<int> OptionalConfigurationList { get; }

        List<int> UnsureConfigurationList { get; }

        List<int[]> XorConfigurationList { get; }
    }
}
