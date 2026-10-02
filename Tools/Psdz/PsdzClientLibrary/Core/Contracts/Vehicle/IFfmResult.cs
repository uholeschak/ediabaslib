using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

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
