namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IObjectCalculation
    {
        ObjectCalculationObjectType ObjectType { get; }

        string ObjectId { get; }

        void Initialize(ObjectCalculationObjectType objectType);
    }
}
