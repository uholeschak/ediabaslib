using System;
using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class NcdStatusEtoEnumMapper : MapperBase<PsdzNcdStatusEtoEnum, NcdStatusEto>
    {
        internal override PsdzNcdStatusEtoEnum GetValue(NcdStatusEto key)
        {
            try
            {
                return base.GetValue(key);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override IDictionary<PsdzNcdStatusEtoEnum, NcdStatusEto> CreateMap()
        {
            return new Dictionary<PsdzNcdStatusEtoEnum, NcdStatusEto>
            {
                {
                    PsdzNcdStatusEtoEnum.NO_NCD,
                    NcdStatusEto.NO_NCD
                },
                {
                    PsdzNcdStatusEtoEnum.SIGNED,
                    NcdStatusEto.SIGNED
                },
                {
                    PsdzNcdStatusEtoEnum.UNSIGNED,
                    NcdStatusEto.UNSIGNED
                },
                {
                    PsdzNcdStatusEtoEnum.CPS_INVALID,
                    NcdStatusEto.CPS_INVALID
                }
            };
        }
    }
}