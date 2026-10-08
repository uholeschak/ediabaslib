using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class ILevelTripleMapper
    {
        public static IPsdzIstufenTriple Map(ILevelTripleModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzIstufenTriple
            {
                Current = model.Current,
                Shipment = model.Shipment,
                Last = model.Last
            };
        }

        public static ILevelTripleModel Map(IPsdzIstufenTriple psdzILevelTriple)
        {
            if (psdzILevelTriple == null)
            {
                return null;
            }

            return new ILevelTripleModel
            {
                Current = psdzILevelTriple.Current,
                Shipment = psdzILevelTriple.Shipment,
                Last = psdzILevelTriple.Last
            };
        }
    }
}