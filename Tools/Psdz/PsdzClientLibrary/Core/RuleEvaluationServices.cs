namespace PsdzClient.Core
{
    public class RuleEvaluationServices : IRuleEvaluationServices
    {
        public ILogger Logger => LoggerRuleEvaluationFactory.Create();

        [PreserveSource(Hint = "Initialized in constructor", OriginalHash = "8EB361A838945350E54E3512E44D57A0")]
        public IConfigSettingsRuleEvaluation ConfigSettings { get; }

        [PreserveSource(Hint = "Added")]
        public Vehicle Vec { get; }

        [PreserveSource(Hint = "Constructor added, store vec, using ClientContext")]
        public RuleEvaluationServices(Vehicle vec)
        {
            this.Vec = vec;
            ConfigSettings = new ConfigSettingsRuleEvaluation(ClientContext.GetBrand(vec));
        }
    }
}