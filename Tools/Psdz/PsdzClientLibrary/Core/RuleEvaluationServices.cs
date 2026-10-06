using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.ISTA.CoreFramework;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.RuleVariantHandling
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