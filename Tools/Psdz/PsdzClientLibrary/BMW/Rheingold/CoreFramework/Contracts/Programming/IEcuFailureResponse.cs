namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuFailureResponse
    {
        IEcuIdentifier Ecu { get; }

        string Reason { get; }
    }
}