using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient.Core.Container;
using System;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    [PreserveSource(Hint = "Dummy interface", SuppressWarning = true)]
    public interface IFasta2Service: IProtocolBasic, IProtocolBasicBase, IFastaGroupingBase, IFastaGrouping
    {
    }
}
