namespace PsdzClient.Core
{
    public interface IFFMDynamicResolverRuleEvaluation
    {
        bool? Resolve(decimal id, PsdzDatabase.SwiInfoObj iObj);
    }
}