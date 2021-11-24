using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IDtcUmweltDisplay
    {
        string Current_F_UW_EINH { get; }

        object Current_F_UW_WERT { get; }

        string F_UW_TEXT { get; }

        string First_F_UW_EINH { get; }

        object First_F_UW_WERT { get; }
    }
}
