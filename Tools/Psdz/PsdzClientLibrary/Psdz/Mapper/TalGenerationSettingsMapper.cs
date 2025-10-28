using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class TalGenerationSettingsMapper
    {
        public static TalGenerationSettings Map(TalGenerationSettingsModel talGenerationSettingsModel)
        {
            if (talGenerationSettingsModel == null)
            {
                return default(TalGenerationSettings);
            }
            return new TalGenerationSettings
            {
                ECUsToSuppress = talGenerationSettingsModel.EcuToSuppress?.Select(DiagAddressMapper.Map).ToList(),
                AllAllowedIntelligentSensors = talGenerationSettingsModel.AllAllowedIntelligentSensors?.Select(DiagAddressMapper.Map).ToList(),
                FA = FaMapper.Map(talGenerationSettingsModel.Fa),
                VehicleVPC = talGenerationSettingsModel.VehicleVPC,
                IsCheckProgrammingDeps = talGenerationSettingsModel.CheckProgrammingDeps,
                IsFilterIntelligentSensors = talGenerationSettingsModel.FilterIntelligentSensors,
                IsPreventIncosistendSwFlash = talGenerationSettingsModel.PreventInconsistentSwFlash,
                IsUseMirrorProtocol = talGenerationSettingsModel.UseMirrorProtocol
            };
        }

        public static TalGenerationSettingsModel Map(TalGenerationSettings talGenerationSettings)
        {
            return new TalGenerationSettingsModel
            {
                EcuToSuppress = talGenerationSettings.ECUsToSuppress?.Select(DiagAddressMapper.Map).ToList(),
                AllAllowedIntelligentSensors = talGenerationSettings.AllAllowedIntelligentSensors?.Select(DiagAddressMapper.Map).ToList(),
                Fa = FaMapper.Map(talGenerationSettings.FA),
                VehicleVPC = talGenerationSettings.VehicleVPC,
                CheckProgrammingDeps = talGenerationSettings.IsCheckProgrammingDeps,
                FilterIntelligentSensors = talGenerationSettings.IsFilterIntelligentSensors,
                PreventInconsistentSwFlash = talGenerationSettings.IsPreventIncosistendSwFlash,
                UseMirrorProtocol = talGenerationSettings.IsUseMirrorProtocol
            };
        }
    }
}