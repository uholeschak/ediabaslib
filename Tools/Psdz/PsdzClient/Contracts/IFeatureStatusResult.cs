using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Contracts
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFeatureStatusResult
    {
        long FeatureId { get; }

        string FeatureStatus { get; }

        IBoolResultObject ErrorResult { get; }
    }
}
