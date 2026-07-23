using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient;
using System;
using System.Collections.Generic;

namespace PsdzClient.Core.Container
{
    [PreserveSource(Hint = "Dummy interface", SuppressWarning = true)]
    public interface IProtocolBasic : IProtocolBasicBase, IFastaGroupingBase, IFastaGrouping
    {
        //object AddMultiLanguageEFuseInfoTable(string infoTitle, Dictionary<string, TableData> multiLanguageTableData, DateTime startTime);
    }
}