using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IServiceHistoryEntry : INotifyPropertyChanged
    {
        string Id { get; }

        DateTime? CreationDate { get; }

        DateTime? CompletionDate { get; }

        string DealerPartnerNumber { get; }

        string SumFlatRateUnits { get; }

        decimal? TotalDistance { get; }

        string MilageUnit { get; }

        ISettlement Settlement { get; }

        IRoadsideAssistanceCause RoadsideAssistanceCause { get; }

        string LocalizedOrderType { get; }
    }
}
