using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzScbResultStatusCto
    {
        string AppErrorId { get; }

        string Code { get; }

        string ErrorMessage { get; }
    }
}
