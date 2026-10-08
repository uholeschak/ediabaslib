namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum SwtActionType
    {
        ActivateStore,
        ActivateUpdate,
        ActivateUpgrade,
        Deactivate,
        ReturnState,
        WriteVin
    }
}
