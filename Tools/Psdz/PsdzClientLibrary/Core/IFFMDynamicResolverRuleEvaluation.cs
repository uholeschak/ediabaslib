namespace PsdzClient.Core
{
    public interface IFFMDynamicResolverRuleEvaluation
    {
        // [UH] iObj type modified
        bool? Resolve(decimal id, PsdzDatabase.SwiInfoObj iObj);
    }
}