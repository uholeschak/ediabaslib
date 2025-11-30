using System.Collections.Generic;

#pragma warning disable CS0109
namespace PsdzClient.Core
{
    public interface ISearchCacheContainer
    {
        ulong CacheHits { get; }

        SearchCacheMode CacheMode { get; set; }

        ulong CacheRequests { get; }

        IDictionary<decimal, bool> EvaluatedDiagObjs { get; }

        IDictionary<decimal, bool> EvaluatedInfoObjs { get; }

        IDictionary<decimal, bool> EvaluatedRules { get; }

        void CacheClear();

        bool CheckDiagObjHit(decimal diagObjectId);

        bool CheckInfoObjHit(decimal infoObjectId);

        bool CheckRuleHit(decimal ruleId);

        bool GetDiagObjHit(decimal diagObjectId);

        bool GetInfoObjHit(decimal infoObjectId);

        bool GetRuleHit(decimal ruleId);

        void SetDiagObj(decimal diagObjectId, bool validity);

        void SetInfoObj(decimal infoObjectId, bool validity);

        void SetRule(decimal ruleId, bool validity);

        new string ToString();
    }
}