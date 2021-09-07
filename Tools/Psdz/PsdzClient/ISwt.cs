using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwt
    {
        IEnumerable<ISwtEcu> Ecus { get; }

        ISwtApplication GetSwtApplication(int diagAddrAsInt, ISwtApplicationId swtApplicationId);
    }
}
