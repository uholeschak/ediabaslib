using BMW.Rheingold.CoreFramework.Contracts.FASTA;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface ISubModule : IFastaGrouping, IFastaGroupingBase, IFastaServiceProgram
    {
        IModuleStep CurrentStep { get; }
    }

}
