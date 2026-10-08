using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzStandardFp
    {
        string AsString { get; }

        IDictionary<int, IList<IPsdzStandardFpCriterion>> Category2Criteria { get; }

        IDictionary<int, string> CategoryId2CategoryName { get; }

        bool IsValid { get; }
    }
}
