using BMW.Rheingold.ISTA.CoreFramework;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.RuleVariantHandling
{
    public class LoggerRuleEvaluationFactory
    {
        public static ILogger Create()
        {
            return new NugetLogger();
        }
    }
}