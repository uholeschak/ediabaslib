using System;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    public class SecureFeatureData
    {
        public string Description { get; set; }

        public int DiagAddrAsInt { get; set; }

        public int FeatureIdAsInt { get; set; }

        public PsdzFeatureStatusEtoEnum Status { get; set; }

        public SecureFeatureData()
        {
        }

        public SecureFeatureData(IPsdzFeatureLongStatusCto feature)
        {
            FeatureIdAsInt = (int)feature.FeatureId.Value;
            DiagAddrAsInt = feature.EcuIdentifierCto.DiagAddrAsInt;
            if (!IsSystemFeature(FeatureIdAsInt))
            {
                Description = GetSecureFeatureName(feature.FeatureId.Value);
            }
            Status = feature.FeatureStatusEto;
        }

        public static bool IsSystemFeature(long featureIdAsInt)
        {
            return featureIdAsInt < 1048576;
        }

        public SecureFeatureData(IPsdzEcuFeatureTokenRelationCto feature)
        {
            FeatureIdAsInt = (int)feature.FeatureId.Value;
            DiagAddrAsInt = feature.ECUIdentifier.DiagAddrAsInt;
            if (IsSystemFeature(FeatureIdAsInt))
            {
                Description = GetSecureFeatureName(feature.FeatureId.Value);
            }
            Status = PsdzFeatureStatusEtoEnum.ENABLED;
        }

        public override string ToString()
        {
            return $"ECU: 0x{DiagAddrAsInt:X2}, FeatureId: 0X{FeatureIdAsInt:X6}, Status: {Status.ToString()}";
        }

        [PreserveSource(Hint = "Cleaned")]
        public static string GetSecureFeatureName(long featureIdAsInt)
        {
            return string.Empty;
        }
    }
}