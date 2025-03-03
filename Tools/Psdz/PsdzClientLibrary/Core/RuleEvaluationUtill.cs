using PsdzClient.Core;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace PsdzClientLibrary.Core
{
    public class RuleEvaluationUtill
    {
        //private readonly IRuleCache ruleCache;

        private readonly IRuleEvaluationServices ruleEvaluationServices;

        //private readonly IDataProviderRuleEvaluation dataProvider;

        private IFFMDynamicResolver ffmResolver;

        private HashSet<decimal> notValidRulesIds;

        private Dictionary<decimal, IRuleExpression> ruleExpressionsToEvaluate;

        private HashSet<decimal> rulesToEvaluate;

        private Vehicle vehicle;

        //private IDealer dealer;

        private Action startRuleMetrics;

        private Action stopRuleMetrics;

        public RuleEvaluationUtill(IRuleEvaluationServices ruleEvaluationServices, Action startRuleMetrics = null, Action stopRuleMetrics = null)
        {
            //ruleCache = dataProvider.RuleCache;
            //this.dealer = dealer;
            this.ruleEvaluationServices = ruleEvaluationServices;
            //this.dataProvider = dataProvider;
            this.startRuleMetrics = startRuleMetrics;
            this.stopRuleMetrics = stopRuleMetrics;
        }

        public HashSet<decimal> RetrieveNotValidRulesIds(IDictionary<decimal, decimal?> rules, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (rules == null || !rules.Any())
            {
                return new HashSet<decimal>();
            }
            notValidRulesIds = new HashSet<decimal>();
            rulesToEvaluate = new HashSet<decimal>();
            ruleExpressionsToEvaluate = new Dictionary<decimal, IRuleExpression>();
            foreach (decimal item in rules.Select((KeyValuePair<decimal, decimal?> x) => x.Key))
            {
                rulesToEvaluate.Add(item);
            }
            this.vehicle = vehicle;
            this.ffmResolver = ffmResolver;
            try
            {
                //UpdateRuleSetsFromCachedRulesResults();
                //UpdateRuleSetsFromDownloadedPatches(rules);
                //UpdateRuleSetsFromCachedRuleExpressions();
                //UpdateRuleSetsFromDB();
                EvaluateRuleExpressions();
            }
            catch (Exception exception)
            {
                ruleEvaluationServices.Logger.ErrorException(ruleEvaluationServices.Logger.CurrentMethod(), exception);
            }
            return notValidRulesIds;
        }

        internal bool EvaluateSingleRuleExpression(Vehicle vehicle, decimal ruleId, IFFMDynamicResolver ffmResolver)
        {
            return !RetrieveNotValidRulesIds(new Dictionary<decimal, decimal?> { { ruleId, null } }, vehicle, ffmResolver).Any();
        }

        private void AddIfNotContains<T>(HashSet<T> collection, T item)
        {
            if (!collection.Contains(item))
            {
                collection.Add(item);
            }
        }

        private void EvaluateRuleExpressions()
        {
            if (!ruleExpressionsToEvaluate.Any())
            {
                return;
            }
            startRuleMetrics?.Invoke();
            foreach (decimal key in ruleExpressionsToEvaluate.Keys)
            {
                ruleEvaluationServices.Logger.Info(ruleEvaluationServices.Logger.CurrentMethod(), "Start evaluating rule with id: {0}", key);
                IRuleExpression exp = ruleExpressionsToEvaluate[key];
                bool flag = RuleExpression.Evaluate(vehicle, exp, ffmResolver, ruleEvaluationServices);
                ruleEvaluationServices.Logger.Info(ruleEvaluationServices.Logger.CurrentMethod(), "End evaluating rule: {0} Overall Validity: {1}", key, flag.ToString());
                if (!flag)
                {
                    AddIfNotContains(notValidRulesIds, key);
                    //ruleCache?.SearchCacheContainer.SetDiagObj(key, validity: false);
                    //ruleCache?.SearchCacheContainer?.SetInfoObj(key, validity: false);
                }
            }
            stopRuleMetrics?.Invoke();
        }
    }
}