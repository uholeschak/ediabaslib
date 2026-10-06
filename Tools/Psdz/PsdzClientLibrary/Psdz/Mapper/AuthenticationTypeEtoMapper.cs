using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

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