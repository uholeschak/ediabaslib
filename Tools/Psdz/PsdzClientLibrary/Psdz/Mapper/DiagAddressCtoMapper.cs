using BMW.Rheingold.Psdz.Model.SecureCoding;

namespace BMW.Rheingold.Psdz
{
    internal static class DiagAddressCtoMapper
    {
        public static IPsdzDiagAddressCto Map(DiagAddressCtoModel diagAddressModel)
        {
            if (diagAddressModel == null)
            {
                return null;
            }
            return new PsdzDiagAddressCto
            {
                INVALID_OFFSET = diagAddressModel.InvalidOffset,
                IsValid = diagAddressModel.Valid,
                MAX_OFFSETT = diagAddressModel.MaxOffset,
                MIN_OFFSET = diagAddressModel.MinOffset,
                OffsetSetAsHex = diagAddressModel.OffsetAsHex,
                OffsetSetAsInt = diagAddressModel.OffsetAsInt,
                OffsetSetAsString = diagAddressModel.OffsetAsString
            };
        }

        public static DiagAddressCtoModel Map(IPsdzDiagAddressCto diagAddress)
        {
            if (diagAddress == null)
            {
                return null;
            }
            return new DiagAddressCtoModel
            {
                InvalidOffset = diagAddress.INVALID_OFFSET,
                Valid = diagAddress.IsValid,
                MaxOffset = diagAddress.MAX_OFFSETT,
                MinOffset = diagAddress.MIN_OFFSET,
                OffsetAsHex = diagAddress.OffsetSetAsHex,
                OffsetAsInt = diagAddress.OffsetSetAsInt,
                OffsetAsString = diagAddress.OffsetSetAsString
            };
        }
    }
}