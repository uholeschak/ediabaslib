using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwtStatus : INotifyPropertyChanged
    {
        string OrderingStatus { get; }

        string STAT_FSCS_CERT_STATUS_CODE { get; }

        string STAT_FSC_STATUS_CODE { get; }

        string STAT_ROOT_CERT_STATUS_CODE { get; }

        string STAT_SIGS_CERT_STATUS_CODE { get; }

        string STAT_SW_ID { get; }

        string STAT_SW_SIG_STATUS_CODE { get; }

        string Title { get; }

        uint applicationNoUI { get; }

        uint upgradeIndexUI { get; }
    }
}
