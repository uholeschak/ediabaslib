using System.Globalization;

namespace PsdzClientLibrary.Core
{
    public class FFMResultRuleEvaluation : IFfmResultRuleEvaluation
    {
        public string Evaluation { get; set; }

        public decimal ID { get; set; }

        public string Name { get; set; }

        public bool ReEvaluationNeeded { get; set; }

        public bool? Result { get; set; }

        public FFMResultRuleEvaluation()
        {
            ReEvaluationNeeded = false;
        }

        public FFMResultRuleEvaluation(string name, bool? result)
        {
            ID = -1m;
            Name = name;
            Result = result;
        }

        public FFMResultRuleEvaluation(decimal id, string name, bool? result)
        {
            ID = id;
            Name = name;
            Result = result;
        }

        public FFMResultRuleEvaluation(decimal id, string name, string eval, bool? result)
        {
            ID = id;
            Name = name;
            Result = result;
            Evaluation = eval;
        }

        public FFMResultRuleEvaluation(decimal id, string name, string eval, bool? result, bool reeval)
        {
            ID = id;
            Name = name;
            Result = result;
            Evaluation = eval;
            ReEvaluationNeeded = reeval;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "FFMResult ID: {0} Name: {1} Result: {2} EvalutedBy: {3} Reeval: {4}", ID, Name, Result, Evaluation, ReEvaluationNeeded);
        }
    }
}