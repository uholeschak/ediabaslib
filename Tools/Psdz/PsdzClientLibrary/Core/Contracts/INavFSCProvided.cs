namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface INavFSCProvided
    {
        string NavMapName { get; set; }

        string FscByteString { get; }

        IFSCProvided FscObject { get; set; }
    }
}
