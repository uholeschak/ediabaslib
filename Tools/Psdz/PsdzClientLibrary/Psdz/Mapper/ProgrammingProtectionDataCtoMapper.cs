using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class ProgrammingProtectionDataCtoMapper
    {
        public static IPsdzProgrammingProtectionDataCto Map(ProgrammingProtectionDataCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzProgrammingProtectionDataCto
            {
                SWEList = model.SweList?.Select(SgbmIdMapper.Map).ToList(),
                ProgrammingProtectionEcus = model.ProgrammingProtectionEcus?.Select(EcuIdentifierMapper.Map).ToList(),
                SWEData = model.SweData
            };
        }

        public static ProgrammingProtectionDataCtoModel Map(IPsdzProgrammingProtectionDataCto psdzProgrammingProtectionDataCto)
        {
            if (psdzProgrammingProtectionDataCto == null)
            {
                return null;
            }

            return new ProgrammingProtectionDataCtoModel
            {
                SweList = psdzProgrammingProtectionDataCto.SWEList?.Select(SgbmIdMapper.Map).ToList(),
                ProgrammingProtectionEcus = psdzProgrammingProtectionDataCto.ProgrammingProtectionEcus?.Select(EcuIdentifierMapper.Map).ToList(),
                SweData = psdzProgrammingProtectionDataCto.SWEData
            };
        }
    }
}