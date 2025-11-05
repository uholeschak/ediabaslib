using PsdzClient;
using PsdzClient.Core;

namespace PsdzClient.Core
{
    public class RuleEvaluationServices : IRuleEvaluationServices
    {
        public Vehicle Vec { get; }

        public IConfigSettingsRuleEvaluation ConfigSettings { get; }

        public RuleEvaluationServices(Vehicle vec)
        {
            // [UH] Using ClientContext
            this.Vec = vec;
            ConfigSettings = new ConfigSettingsRuleEvaluation(ClientContext.GetBrand(vec));
        }

        public ILogger Logger => LoggerRuleEvaluationFactory.Create();
    }
}