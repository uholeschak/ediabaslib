namespace PsdzClient.Core
{
    public interface IRuleEvaluationServices
    {
        ILogger Logger { get; }

        IConfigSettingsRuleEvaluation ConfigSettings { get; }
    }
}