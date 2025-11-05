using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuContextInfoMapper
    {
        public static IPsdzEcuContextInfo Map(EcuContextInfoModel ecuContextInfoModel)
        {
            if (ecuContextInfoModel == null)
            {
                return null;
            }

            return new PsdzEcuContextInfo
            {
                EcuId = EcuIdentifierMapper.Map(ecuContextInfoModel.EcuId),
                LastProgrammingDate = ecuContextInfoModel.LastProgrammingDate,
                ManufacturingDate = ecuContextInfoModel.ManufacturingDate,
                PerformedFlashCycles = ecuContextInfoModel.PerformedFlashCycles,
                ProgramCounter = ecuContextInfoModel.ProgramCounter,
                RemainingFlashCycles = ecuContextInfoModel.RemainingFlashCycles
            };
        }
    }
}