using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum FscState
    {
        Accepted,
        Cancelled,
        Imported,
        Invalid,
        NotAvailable,
        Rejected
    }

    public interface ISwtApplicationReport
    {
        int DiagAddrAsInt { get; set; }

        FscState FscState { get; }

        ISwtApplicationId Id { get; }

        string Title { get; set; }

        IDictionary<string, string> TitleDictionary { get; set; }
    }
}
