using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model
{
    public interface IPsdzStandardFp
    {
        string AsString { get; }

        IDictionary<int, IList<IPsdzStandardFpCriterion>> Category2Criteria { get; }

        IDictionary<int, string> CategoryId2CategoryName { get; }

        bool IsValid { get; }
    }
}
