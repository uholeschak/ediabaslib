using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Contracts
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFscType
    {
        string Code { get; set; }

        string Id { get; set; }

        string Value { get; set; }

        byte[] GetBinaryValue();
    }
}
