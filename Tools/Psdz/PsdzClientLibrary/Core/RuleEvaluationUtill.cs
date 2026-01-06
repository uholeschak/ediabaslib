using PsdzClient.Core;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using PsdzClient;

#pragma warning disable CS0649, CS0169
namespace PsdzClient.Core
{
    public class RuleEvaluationUtill
    {
        private readonly IRuleCache ruleCache;
        private readonly IRuleEvaluationServices ruleEvaluationServices;
        [PreserveSource(Hint = "IDataProviderRuleEvaluation", Placeholder = true)]
        private readonly PlaceholderType dataProvider;
        private IFFMDynamicResolverRuleEvaluation ffmResolver;
        private HashSet<decimal> notValidRulesIds;
        private Dictionary<decimal, IRuleExpression> ruleExpressionsToEvaluate;
        private HashSet<decimal> rulesToEvaluate;
        private IVehicleRuleEvaluation vehicle;
        [PreserveSource(Hint = "IDealer", Placeholder = true)]
        private PlaceholderType dealer;
        private Action startRuleMetrics;
        private Action stopRuleMetrics;
        [PreserveSource(Hint = "dataProvider replaced", SignatureModified = true)]
        public RuleEvaluationUtill(IRuleEvaluationServices ruleEvaluationServices, PsdzDatabase database, Action startRuleMetrics = null, Action stopRuleMetrics = null)
        {
            //[-] ruleCache = dataProvider.RuleCache;
            //[-] this.dealer = dealer;
            this.ruleEvaluationServices = ruleEvaluationServices;
            //[-] this.dataProvider = dataProvider;
            //[+] this.database = database;
            this.database = database;
            this.startRuleMetrics = startRuleMetrics;
            this.stopRuleMetrics = stopRuleMetrics;
        }

        public HashSet<decimal> RetrieveNotValidRulesIds(IDictionary<decimal, decimal?> rules, IVehicleRuleEvaluation vehicle, IFFMDynamicResolverRuleEvaluation ffmResolver)
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
                UpdateRuleSetsFromCachedRulesResults();
                UpdateRuleSetsFromDownloadedPatches(rules);
                UpdateRuleSetsFromCachedRuleExpressions(rules);
                UpdateRuleSetsFromDB();
                EvaluateRuleExpressions();
            }
            catch (Exception exception)
            {
                ruleEvaluationServices.Logger.ErrorException(ruleEvaluationServices.Logger.CurrentMethod(), exception);
            }

            return notValidRulesIds;
        }

        [PreserveSource(Hint = "ruleId as string", OriginalHash = "595C9B23B7FE1A6856DCC59AC1578A5E")]
        internal bool EvaluateSingleRuleExpression(Vehicle vehicle, string ruleId, IFFMDynamicResolver ffmResolver)
        {
            return database.EvaluateXepRulesById(ruleId, vehicle, ffmResolver, null);
        }

        private void UpdateRuleSetsFromCachedRuleExpressions(IDictionary<decimal, decimal?> rules)
        {
            if (!rulesToEvaluate.Any())
            {
                return;
            }

            HashSet<decimal> hashSet = new HashSet<decimal>();
            foreach (decimal item in rulesToEvaluate)
            {
                IRuleExpression ruleExpression = null;
                if (rules.TryGetValue(item, out var value) && value.HasValue && ruleCache.CacheXepRules.ContainsKey(value.Value))
                {
                    ruleExpression = ruleCache.CacheXepRules[value.Value];
                    AddIfNotContains(hashSet, item);
                    ruleEvaluationServices.Logger.Info(ruleEvaluationServices.Logger.CurrentMethod(), $"Mapped ID '{value.Value}' to Rule with ID '{item}'");
                }

                if (ruleExpression == null && ruleCache.CacheXepRules.ContainsKey(item))
                {
                    ruleExpression = ruleCache.CacheXepRules[item];
                    AddIfNotContains(hashSet, item);
                }

                if (ruleExpression != null)
                {
                    ruleExpressionsToEvaluate[item] = ruleExpression;
                }
            }

            if (hashSet.Any())
            {
                rulesToEvaluate = new HashSet<decimal>(rulesToEvaluate.Except(hashSet));
            }
        }

        private void UpdateRuleSetsFromCachedRulesResults()
        {
            IRuleCache obj = ruleCache;
            if (obj == null || obj.SearchCacheContainer?.CacheMode != SearchCacheMode.CacheFirst || !rulesToEvaluate.Any())
            {
                return;
            }

            HashSet<decimal> hashSet = new HashSet<decimal>();
            foreach (decimal item in rulesToEvaluate)
            {
                if (ruleCache.SearchCacheContainer.CheckRuleHit(item))
                {
                    if (!ruleCache.SearchCacheContainer.GetRuleHit(item))
                    {
                        AddIfNotContains(notValidRulesIds, item);
                        ruleCache.SearchCacheContainer.SetDiagObj(item, validity: false);
                        ruleCache.SearchCacheContainer.SetInfoObj(item, validity: false);
                    }

                    AddIfNotContains(hashSet, item);
                }
            }

            rulesToEvaluate = new HashSet<decimal>(rulesToEvaluate.Except(hashSet));
        }

        [PreserveSource(Hint = "Cleaned", OriginalHash = "DEE6195CE6ED98D2483D9642F990AB30")]
        private void UpdateRuleSetsFromDB()
        {
        }

        private void UpdateRuleSetsFromDownloadedPatches(IDictionary<decimal, decimal?> rules)
        {
            if (!rulesToEvaluate.Any())
            {
                return;
            }

            HashSet<decimal> hashSet = new HashSet<decimal>();
            foreach (decimal item in rulesToEvaluate)
            {
                decimal key = item;
                if (rules.TryGetValue(item, out var value) && value.HasValue)
                {
                    key = value.Value;
                }

                if (ruleCache.PatchXepRules.ContainsKey(key))
                {
                    ruleExpressionsToEvaluate[item] = ruleCache.PatchXepRules[key];
                    AddIfNotContains(hashSet, item);
                }
            }

            if (hashSet.Any())
            {
                rulesToEvaluate = new HashSet<decimal>(rulesToEvaluate.Except(hashSet));
            }
        }

        private void AddIfNotContains<T>(HashSet<T> collection, T item)
        {
            if (!collection.Contains(item))
            {
                collection.Add(item);
            }
        }

        [PreserveSource(Hint = "Evaluate modified", OriginalHash = "3B377A5FD3DA912B352A01E04A5DF9E9")]
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
                // [UH] [IGNORE] arguments modified
                bool flag = RuleExpression.Evaluate((Vehicle)vehicle, exp, null);
                ruleEvaluationServices.Logger.Info(ruleEvaluationServices.Logger.CurrentMethod(), "End evaluating rule: {0} Overall Validity: {1}", key, flag.ToString());
                if (!flag)
                {
                    AddIfNotContains(notValidRulesIds, key);
                    ruleCache?.SearchCacheContainer.SetDiagObj(key, validity: false);
                    ruleCache?.SearchCacheContainer?.SetInfoObj(key, validity: false);
                }

                if (ruleCache?.SearchCacheContainer != null)
                {
                    IRuleCache obj = ruleCache;
                    if (obj != null && obj.SearchCacheContainer.CacheMode == SearchCacheMode.CacheFirst)
                    {
                        ruleCache?.SearchCacheContainer.SetRule(key, flag);
                    }
                }
            }

            stopRuleMetrics?.Invoke();
        }

        [PreserveSource(Hint = "Added")]
        private readonly PsdzDatabase database;
    }
}