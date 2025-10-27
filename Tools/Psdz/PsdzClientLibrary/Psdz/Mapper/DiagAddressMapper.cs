using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    internal static class DiagAddressMapper
    {
        public static IPsdzDiagAddress Map(DiagAddressModel diagAddressModel)
        {
            if (diagAddressModel == null)
            {
                return null;
            }
            return new PsdzDiagAddress
            {
                Offset = diagAddressModel.Offset
            };
        }

        public static IPsdzDiagAddress Map(DiagAddressCtoModel diagAddressModel)
        {
            if (diagAddressModel == null)
            {
                return null;
            }
            return new PsdzDiagAddress
            {
                Offset = diagAddressModel.OffsetAsInt
            };
        }

        public static DiagAddressModel Map(IPsdzDiagAddress diagAddress)
        {
            if (diagAddress == null)
            {
                return null;
            }
            return new DiagAddressModel
            {
                Offset = diagAddress.Offset
            };
        }

        public static DiagAddressCtoModel MapCto(IPsdzDiagAddress diagAddress)
        {
            if (diagAddress == null)
            {
                return null;
            }
            return new DiagAddressCtoModel
            {
                OffsetAsInt = diagAddress.Offset
            };
        }
    }
}