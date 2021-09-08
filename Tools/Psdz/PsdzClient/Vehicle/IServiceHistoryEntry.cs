using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient.Vehicle
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
