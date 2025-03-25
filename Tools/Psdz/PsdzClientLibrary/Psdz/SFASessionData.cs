using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;
using System.Linq;

namespace PsdzClient.Programming
{
    public class SFASessionData
    {
        public IList<IPsdzFeatureLongStatusCto> CurrentFeatures { get; set; }

        public bool IsValid
        {
            get
            {
                if (CurrentFeatures != null && CurrentFeatures.Any() && SecureTokens != null && SecureTokens.Any() && TargetFeatures != null)
                {
                    return TargetFeatures.Any();
                }
                return false;
            }
        }

        public IList<IPsdzSecureTokenEto> SecureTokens { get; set; }

        public IList<IPsdzEcuFeatureTokenRelationCto> TargetFeatures { get; set; }
    }
}