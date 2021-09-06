using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzStandardFp
    {
        string AsString { get; }

        IDictionary<int, IList<IPsdzStandardFpCriterion>> Category2Criteria { get; }

        IDictionary<int, string> CategoryId2CategoryName { get; }

        bool IsValid { get; }
    }
}
