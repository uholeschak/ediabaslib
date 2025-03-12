namespace PsdzClient.Core
{
    public interface IFfmResultRuleEvaluation
    {
        string Evaluation { get; }

        decimal ID { get; }

        string Name { get; }

        bool ReEvaluationNeeded { get; }

        bool? Result { get; }
    }
}