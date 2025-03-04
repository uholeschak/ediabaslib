using PsdzClient.Core;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using PsdzClient;

namespace PsdzClientLibrary.Core
{
    public class RuleEvaluationUtill
    {
        //private readonly IRuleCache ruleCache;

        private readonly IRuleEvaluationServices ruleEvaluationServices;

        private readonly PsdzDatabase database;
        //private readonly IDataProviderRuleEvaluation dataProvider;

        private IFFMDynamicResolver ffmResolver;

        private HashSet<string> notValidRulesIds;

        private Dictionary<string, IRuleExpression> ruleExpressionsToEvaluate;

        private HashSet<string> rulesToEvaluate;

        private Vehicle vehicle;

        //private IDealer dealer;

        private Action startRuleMetrics;

        private Action stopRuleMetrics;

        public RuleEvaluationUtill(IRuleEvaluationServices ruleEvaluationServices, PsdzDatabase database, Action startRuleMetrics = null, Action stopRuleMetrics = null)
        {
            //ruleCache = dataProvider.RuleCache;
            //this.dealer = dealer;
            this.ruleEvaluationServices = ruleEvaluationServices;
            this.database = database;
            //this.dataProvider = dataProvider;
            this.startRuleMetrics = startRuleMetrics;
            this.stopRuleMetrics = stopRuleMetrics;
        }

        public HashSet<string> RetrieveNotValidRulesIds(IDictionary<string, string> rules, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            if (rules == null || !rules.Any())
            {
                return new HashSet<string>();
            }
            notValidRulesIds = new HashSet<string>();
            rulesToEvaluate = new HashSet<string>();
            ruleExpressionsToEvaluate = new Dictionary<string, IRuleExpression>();
            foreach (string item in rules.Select((KeyValuePair<string, string> x) => x.Key))
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
                UpdateRuleSetsFromDB();
                EvaluateRuleExpressions();
            }
            catch (Exception exception)
            {
                ruleEvaluationServices.Logger.ErrorException(ruleEvaluationServices.Logger.CurrentMethod(), exception);
            }
            return notValidRulesIds;
        }

        internal bool EvaluateSingleRuleExpression(Vehicle vehicle, string ruleId, IFFMDynamicResolver ffmResolver)
        {
            return !RetrieveNotValidRulesIds(new Dictionary<string, string> { { ruleId, null } }, vehicle, ffmResolver).Any();
        }

        private void AddIfNotContains<T>(HashSet<T> collection, T item)
        {
            if (!collection.Contains(item))
            {
                collection.Add(item);
            }
        }

        private void UpdateRuleSetsFromDB()
        {
            if (!rulesToEvaluate.Any())
            {
                return;
            }
            foreach (string ruleId in rulesToEvaluate)
            {
                PsdzDatabase.XepRule xepRule = database.GetRuleById(ruleId);
                if (xepRule != null)
                {
                    ruleExpressionsToEvaluate[ruleId] = xepRule.RuleExpression;
                }
            }
        }

        private void EvaluateRuleExpressions()
        {
            if (!ruleExpressionsToEvaluate.Any())
            {
                return;
            }
            startRuleMetrics?.Invoke();
            foreach (string key in ruleExpressionsToEvaluate.Keys)
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