using PsdzClient.Core;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using PsdzClient;

namespace PsdzClient.Core
{
    public class RuleEvaluationUtill
    {
        private readonly IRuleEvaluationServices ruleEvaluationServices;

        private readonly PsdzDatabase database;

        private Action startRuleMetrics;

        private Action stopRuleMetrics;

        [PreserveSource(Hint = "Modified")]
        public RuleEvaluationUtill(IRuleEvaluationServices ruleEvaluationServices, PsdzDatabase database, Action startRuleMetrics = null, Action stopRuleMetrics = null)
        {
            // [IGNORE] ruleCache = dataProvider.RuleCache;
            // [IGNORE] this.dealer = dealer;
            this.ruleEvaluationServices = ruleEvaluationServices;
            this.database = database;
            //  [IGNORE] this.dataProvider = dataProvider;
            this.startRuleMetrics = startRuleMetrics;
            this.stopRuleMetrics = stopRuleMetrics;
        }

        internal bool EvaluateSingleRuleExpression(Vehicle vehicle, string ruleId, IFFMDynamicResolver ffmResolver)
        {
            return database.EvaluateXepRulesById(ruleId, vehicle, ffmResolver, null);
        }
    }
}