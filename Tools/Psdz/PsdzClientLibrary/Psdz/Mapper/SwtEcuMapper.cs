using BMW.Rheingold.Psdz.Model.Swt;
using System.Linq;
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class SwtEcuMapper
    {
        private static RootCertStateMapper _rootCertStateMapper = new RootCertStateMapper();
        private static SoftwareSigStateEnumMapper _softwareSigStateEnumMapper = new SoftwareSigStateEnumMapper();
        internal static IPsdzSwtEcu Map(SwtEcuModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzSwtEcu
            {
                EcuIdentifier = EcuIdentifierMapper.Map(model.EcuIdentifier),
                RootCertState = _rootCertStateMapper.GetValue(model.RootCertState),
                SoftwareSigState = _softwareSigStateEnumMapper.GetValue(model.SoftwareSigState).Value,
                SwtApplications = model.SwtApplications?.Select(SwtApplicationMapper.Map),
                Vin = model.Vin
            };
        }

        internal static SwtEcuModel Map(IPsdzSwtEcu psdzSwtEcu)
        {
            if (psdzSwtEcu == null)
            {
                return null;
            }

            return new SwtEcuModel
            {
                EcuIdentifier = EcuIdentifierMapper.Map(psdzSwtEcu.EcuIdentifier),
                RootCertState = _rootCertStateMapper.GetValue(psdzSwtEcu.RootCertState),
                SoftwareSigState = _softwareSigStateEnumMapper.GetValue(psdzSwtEcu.SoftwareSigState),
                SwtApplications = psdzSwtEcu.SwtApplications?.Select(SwtApplicationMapper.Map).ToList(),
                Vin = psdzSwtEcu.Vin
            };
        }
    }
}