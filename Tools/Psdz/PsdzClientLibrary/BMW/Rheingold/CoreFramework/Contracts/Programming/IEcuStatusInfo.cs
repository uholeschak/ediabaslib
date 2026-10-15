namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IEcuStatusInfo
    {
        byte Value { get; }

        bool HasIndividualData { get; }
    }
}
