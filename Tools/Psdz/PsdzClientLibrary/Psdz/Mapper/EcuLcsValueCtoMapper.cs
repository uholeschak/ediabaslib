using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuLcsValueCtoMapper
    {
        public static IPsdzEcuLcsValueCto Map(EcuLcsValueCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzEcuLcsValueCto
            {
                LcsNumber = model.LcsNumber,
                LcsValue = model.LcsValue,
                EcuIdentifier = EcuIdentifierMapper.Map(model.EcuIdentifier)
            };
        }

        public static EcuLcsValueCtoModel Map(IPsdzEcuLcsValueCto psdzEcuLcsValueCto)
        {
            if (psdzEcuLcsValueCto == null)
            {
                return null;
            }

            return new EcuLcsValueCtoModel
            {
                LcsNumber = psdzEcuLcsValueCto.LcsNumber,
                LcsValue = psdzEcuLcsValueCto.LcsValue,
                EcuIdentifier = EcuIdentifierMapper.Map(psdzEcuLcsValueCto.EcuIdentifier)
            };
        }
    }
}