using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Programming;

namespace PsdzClient.Contracts
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface INavFSCProvided
    {
        string NavMapName { get; set; }

        string FscByteString { get; }

        IFSCProvided FscObject { get; set; }
    }
}
