using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IIstufenTriple
    {
        string Current { get; }

        string Last { get; }

        string Shipment { get; }
    }
}
