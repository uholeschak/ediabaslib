namespace PsdzClient.Core
{
    public class LoggerRuleEvaluationFactory
    {
        public static ILogger Create()
        {
            return new NugetLogger();
        }
    }
}