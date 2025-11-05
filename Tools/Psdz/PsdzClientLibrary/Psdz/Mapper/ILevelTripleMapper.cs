using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
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