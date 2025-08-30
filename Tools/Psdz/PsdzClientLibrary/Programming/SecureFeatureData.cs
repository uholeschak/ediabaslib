using System;
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

        public static string GetSecureFeatureName(long featureIdAsInt)
        {
            //IDatabaseProvider instance = DatabaseProviderFactory.Instance;
            string text = string.Empty;
#if false
            try
            {
                Log.Info(Log.CurrentMethod(), "Trying to resolve feature name for feature '0x{0:X6}'", featureIdAsInt);
                text = instance.GetSwiActivationCodeSFAFeatureNameByIdAndLanguage(featureIdAsInt, instance.AttributeLanguageExtension);
                if (string.IsNullOrEmpty(text) && instance.AttributeLanguageExtension != "ENGB")
                {
                    text = instance.GetSwiActivationCodeSFAFeatureNameByIdAndLanguage(featureIdAsInt, "ENGB");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
            if (string.IsNullOrEmpty(text))
            {
                Log.Warning(Log.CurrentMethod(), $"Could not find Feature Name for ID= {featureIdAsInt} and Language= {instance.AttributeLanguageExtension}. Trying to get fallback value.");
                return "unkown";
            }
            Log.Info(Log.CurrentMethod(), $"Found feature name= {text} for ID= {featureIdAsInt} and Language= {instance.AttributeLanguageExtension}.");
#endif
            return text;
        }
    }
}