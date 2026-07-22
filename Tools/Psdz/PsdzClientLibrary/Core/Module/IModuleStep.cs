using PsdzClient.Core.Container;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IModuleStep : IProtocolBasic, IProtocolBasicBase, IFastaGroupingBase, IFastaGrouping
    {
        string Title { get; set; }
    }
}
