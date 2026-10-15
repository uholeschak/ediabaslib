namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
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