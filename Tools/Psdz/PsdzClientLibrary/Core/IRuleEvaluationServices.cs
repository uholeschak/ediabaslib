using BMW.Rheingold.ISTA.CoreFramework;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IRuleEvaluationServices
    {
        ILogger Logger { get; }

        IConfigSettingsRuleEvaluation ConfigSettings { get; }
    }
}