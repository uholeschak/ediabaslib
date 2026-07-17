using PsdzClient.Core.Container;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IFastaGrouping : IFastaGroupingBase
    {
        IProtocolBasic ProtocolingInstance { get; }
    }
}
