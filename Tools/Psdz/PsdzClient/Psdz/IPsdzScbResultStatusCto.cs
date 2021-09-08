using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzScbResultStatusCto
    {
        string AppErrorId { get; }

        string Code { get; }

        string ErrorMessage { get; }
    }
}
