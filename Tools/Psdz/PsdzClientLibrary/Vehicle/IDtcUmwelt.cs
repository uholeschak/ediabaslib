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
    public interface IDtcUmwelt : INotifyPropertyChanged
    {
        byte[] F_UW_DATA { get; }

        string F_UW_EINH { get; }

        string F_UW_NAME { get; }

        long? F_UW_NR { get; }

        object F_UW_RAW { get; }

        string F_UW_TEXT { get; }

        UwType F_UW_TYP { get; }

        object F_UW_WERT { get; }
    }
}