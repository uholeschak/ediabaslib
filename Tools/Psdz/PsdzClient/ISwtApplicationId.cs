using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwtApplicationId
    {
        int AppNo { get; }

        int UpgradeIdx { get; }
    }
}
