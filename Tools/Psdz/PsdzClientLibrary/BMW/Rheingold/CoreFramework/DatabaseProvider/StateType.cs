namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum StateType
    {
        stopped,
        running,
        finished,
        error,
        unknown,
        idle
    }
}
