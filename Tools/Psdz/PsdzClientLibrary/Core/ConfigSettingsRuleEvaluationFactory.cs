namespace PsdzClientLibrary.Core
{
    public class ConfigSettingsRuleEvaluationFactory
    {
        public static IConfigSettingsRuleEvaluation Create()
        {
            return new ConfigSettingsRuleEvaluation(ConfigSettings.SelectedBrand);
        }
    }
}