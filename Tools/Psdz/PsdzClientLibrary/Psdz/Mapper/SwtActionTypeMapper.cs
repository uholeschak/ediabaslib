using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class SwtActionTypeMapper : NullableEnumMapper<PsdzSwtActionType, SwtActionTypeModel>
    {
        protected override IDictionary<PsdzSwtActionType?, SwtActionTypeModel?> CreateMap()
        {
            return new Dictionary<PsdzSwtActionType?, SwtActionTypeModel?>
            {
                {
                    PsdzSwtActionType.ActivateStore,
                    SwtActionTypeModel.ActivateStore
                },
                {
                    PsdzSwtActionType.ActivateUpdate,
                    SwtActionTypeModel.ActivateUpdate
                },
                {
                    PsdzSwtActionType.ActivateUpgrade,
                    SwtActionTypeModel.ActivateUpgrade
                },
                {
                    PsdzSwtActionType.Deactivate,
                    SwtActionTypeModel.Deactivate
                },
                {
                    PsdzSwtActionType.ReturnState,
                    SwtActionTypeModel.ReturnState
                },
                {
                    PsdzSwtActionType.WriteVin,
                    SwtActionTypeModel.WriteVin
                }
            };
        }
    }
}