using System.Collections.Generic;

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
