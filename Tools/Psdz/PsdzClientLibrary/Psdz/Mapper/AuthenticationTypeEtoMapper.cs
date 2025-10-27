using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.SecureCoding;

namespace BMW.Rheingold.Psdz
{
    internal class AuthenticationTypeEtoMapper : MapperBase<PsdzAuthenticationTypeEto, AuthenticationTypeEto>
    {
        protected override IDictionary<PsdzAuthenticationTypeEto, AuthenticationTypeEto> CreateMap()
        {
            return new Dictionary<PsdzAuthenticationTypeEto, AuthenticationTypeEto>
            {
                {
                    PsdzAuthenticationTypeEto.SSL,
                    AuthenticationTypeEto.SSL
                },
                {
                    PsdzAuthenticationTypeEto.BASIC,
                    AuthenticationTypeEto.BASIC
                },
                {
                    PsdzAuthenticationTypeEto.BEARER,
                    AuthenticationTypeEto.BEARER
                },
                {
                    PsdzAuthenticationTypeEto.UNKNOWN,
                    AuthenticationTypeEto.UNKNOWN
                }
            };
        }
    }
}