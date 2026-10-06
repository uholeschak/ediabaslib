using BMW.Authoring;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ICCM : IHideObjectMembers
    {
        double Timestamp { get; }

        string Description { get; }

        double Mileage { get; }

        long Code { get; }

        string SGVariante { get; }

        string Cause { get; }

        string Longtext { get; }

        int Timestamp_NS { get; }

        int Occurence { get; }

        int PWF_STATE { get; }

        string Diag_Addr_Sender_Hex { get; }

        IEnumerable<IFault> SpecifiedExistingFaultsCausingThisCcm { get; }
    }
}
