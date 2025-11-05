using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class ProgrammingTokenCtoMapper
    {
        public static ProgrammingTokenCtoModel Map(IPsdzProgrammingTokenCto psdzObj)
        {
            if (psdzObj == null)
            {
                return null;
            }

            return new ProgrammingTokenCtoModel
            {
                TokenVersion = psdzObj.TokenVersion,
                Vin = VinMapper.Map(psdzObj.Vin),
                EcuIdentifier = EcuIdentifierCtoMapper.Map(psdzObj.EcuIdentifier),
                EcuUidCto = EcuUidCtoMapper.Map(psdzObj.EcuUidCto),
                ActiveSGBMIDs = psdzObj.ActiveSGBMIDs?.Select(SgbmIdMapper.Map).ToList(),
                NewSGBMIDs = psdzObj.NewSGBMIDs?.Select(SgbmIdMapper.Map).ToList(),
                ActiveSGBMIDsHash = psdzObj.ActiveSGBMIDsHash,
                ValidityStartTime = psdzObj.ValidityStartTime,
                ValidityEndTime = psdzObj.ValidityEndTime,
                IsSigned = psdzObj.IsSigned,
                ProgrammingTokenAsBytes = psdzObj.ProgrammingTokenAsBytes
            };
        }

        public static IPsdzProgrammingTokenCto Map(ProgrammingTokenCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzProgrammingTokenCto
            {
                TokenVersion = model.TokenVersion,
                Vin = VinMapper.Map(model.Vin),
                EcuIdentifier = EcuIdentifierCtoMapper.Map(model.EcuIdentifier),
                EcuUidCto = EcuUidCtoMapper.Map(model.EcuUidCto),
                ActiveSGBMIDs = model.ActiveSGBMIDs?.Select(SgbmIdMapper.Map).ToList(),
                NewSGBMIDs = model.NewSGBMIDs?.Select(SgbmIdMapper.Map).ToList(),
                ActiveSGBMIDsHash = model.ActiveSGBMIDsHash,
                ValidityStartTime = model.ValidityStartTime,
                ValidityEndTime = model.ValidityEndTime,
                IsSigned = model.IsSigned,
                ProgrammingTokenAsBytes = model.ProgrammingTokenAsBytes
            };
        }
    }
}