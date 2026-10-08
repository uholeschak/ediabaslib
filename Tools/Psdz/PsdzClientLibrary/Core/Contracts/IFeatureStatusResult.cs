namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFeatureStatusResult
    {
        long FeatureId { get; }

        string FeatureStatus { get; }

        string DiagAddress { get; set; }

        string ValidationStatus { get; set; }

        IBoolResultObject ErrorResult { get; }
    }
}