using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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