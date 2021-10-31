using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuVariantLocator : ISPELocator
    {
    }
}
