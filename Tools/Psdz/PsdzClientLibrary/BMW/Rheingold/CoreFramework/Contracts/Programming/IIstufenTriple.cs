namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IIstufenTriple
    {
        string Current { get; }

        string Last { get; }

        string Shipment { get; }
    }
}
