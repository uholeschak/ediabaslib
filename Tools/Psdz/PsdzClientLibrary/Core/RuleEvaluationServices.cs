namespace PsdzClient.Core
{
    public class RuleEvaluationServices : IRuleEvaluationServices
    {
        public ILogger Logger => LoggerRuleEvaluationFactory.Create();

        [PreserveSource(Hint = "Modified")]
        public IConfigSettingsRuleEvaluation ConfigSettings { get; }

        [PreserveSource(Hint = "Added")]
        public Vehicle Vec { get; }

        [PreserveSource(Hint = "Constructor added, using ClientContext")]
        public RuleEvaluationServices(Vehicle vec)
        {
            this.Vec = vec;
            ConfigSettings = new ConfigSettingsRuleEvaluation(ClientContext.GetBrand(vec));
        }
    }
}