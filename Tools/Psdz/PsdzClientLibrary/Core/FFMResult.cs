using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public class FFMResult : FFMResultRuleEvaluation, INotifyPropertyChanged, IFfmResult, IFfmResultRuleEvaluation
    {
        public new decimal ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                if (base.ID != value)
                {
                    base.ID = value;
                    OnPropertyChanged("ID");
                }
            }
        }

        public new string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                if (base.Name != value)
                {
                    base.Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public new bool? Result
        {
            get
            {
                return base.Result;
            }
            set
            {
                if (base.Result != value)
                {
                    base.Result = value;
                    OnPropertyChanged("Result");
                }
            }
        }

        public new string Evaluation
        {
            get
            {
                return base.Evaluation;
            }
            set
            {
                if (base.Evaluation != value)
                {
                    base.Evaluation = value;
                    OnPropertyChanged("Evaluation");
                }
            }
        }

        public new bool ReEvaluationNeeded
        {
            get
            {
                return base.ReEvaluationNeeded;
            }
            set
            {
                if (base.ReEvaluationNeeded != value)
                {
                    base.ReEvaluationNeeded = value;
                    OnPropertyChanged("ReEvaluationNeeded");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FFMResult()
        {
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FFMResult(IFfmResultRuleEvaluation ffmRuleEvaluation)
        {
            ID = ffmRuleEvaluation.ID;
            Name = ffmRuleEvaluation.Name;
            Result = ffmRuleEvaluation.Result;
            Evaluation = ffmRuleEvaluation.Evaluation;
            ReEvaluationNeeded = ffmRuleEvaluation.ReEvaluationNeeded;
        }

        public FFMResult(decimal id, string name, string eval, bool? result)
            : base(id, name, eval, result)
        {
        }

        public FFMResult(decimal id, string name, string eval, bool? result, bool reeval)
            : base(id, name, eval, result, reeval)
        {
        }
    }
}
