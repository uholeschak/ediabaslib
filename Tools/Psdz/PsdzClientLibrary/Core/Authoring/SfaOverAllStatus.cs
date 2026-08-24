using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Authoring.API.Implementation.Sfa.Models
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public class SfaOverAllStatus
    {
        public int Overall { get; set; }

        public List<SfaStatusInfo> Detailed { get; set; }
    }
}
