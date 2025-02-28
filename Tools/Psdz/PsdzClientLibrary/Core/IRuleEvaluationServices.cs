namespace PsdzClientLibrary.Core
{
    public interface IRuleEvaluationServices
    {
        ILogger Logger { get; }

        IConfigSettingsRuleEvaluation ConfigSettings { get; }
    }
}