namespace PsdzClientLibrary.Core
{
    public class RuleEvaluationServices : IRuleEvaluationServices
    {
        public ILogger Logger => LoggerRuleEvaluationFactory.Create();

        public IConfigSettingsRuleEvaluation ConfigSettings => ConfigSettingsRuleEvaluationFactory.Create();
    }
}