using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Programming;

namespace PsdzClient.Contracts
{
    // [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFSCProvided
    {
        ICertType Certificate { get; set; }

        string CustOrderID { get; set; }

        string DealerNo { get; set; }

        IFscItemType FscItem { get; set; }

        string OrderID { get; set; }

        string PartNo { get; set; }

        string RequestID { get; set; }

        ICertType RootCertificate { get; set; }

        string VinShort { get; set; }
    }
}
