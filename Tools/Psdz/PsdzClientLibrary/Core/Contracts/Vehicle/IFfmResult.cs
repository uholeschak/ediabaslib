using System.ComponentModel;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IFfmResult : INotifyPropertyChanged, IFfmResultRuleEvaluation
    {
        new string Evaluation { get; }

        new decimal ID { get; }

        new string Name { get; }

        new bool ReEvaluationNeeded { get; }

        new bool? Result { get; }
    }
}
