namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public interface IXepEcuGroups
    {
        string Name { get; }

        decimal Id { get; }

        decimal DiagnosticAddress { get; }
    }
}
