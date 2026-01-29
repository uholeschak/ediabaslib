namespace PsdzClient.Core
{
    public class RuleEvaluationServices : IRuleEvaluationServices
    {
        public ILogger Logger => LoggerRuleEvaluationFactory.Create();

        [PreserveSource(Hint = "Initialized in constructor", SuppressWarning = true)]
        public IConfigSettingsRuleEvaluation ConfigSettings { get; }

        [PreserveSource(Added = true)]
        public Vehicle Vec { get; }

        [PreserveSource(Hint = "Constructor added, store vec, using ClientContext", Added = true)]
        public RuleEvaluationServices(Vehicle vec)
        {
            Vec = vec;
            ConfigSettings = new ConfigSettingsRuleEvaluation(ClientContext.GetBrand(vec));
        }
    }
}