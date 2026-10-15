namespace BMW.Rheingold.CoreFramework.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISfaPerEcuOptionsPair
    {
        ISfaPerEcuOptions SfaPerEcuOptions { get; }

        int EcuAddress { get; }
    }
}
