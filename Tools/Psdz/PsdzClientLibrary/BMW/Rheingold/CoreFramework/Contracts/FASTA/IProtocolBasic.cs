using System;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.TableDataHelper;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    [PreserveSource(Hint = "Dummy interface", SuppressWarning = true)]
    public interface IProtocolBasic : IProtocolBasicBase, IFastaGroupingBase, IFastaGrouping
    {
        object AddMultiLanguageEFuseInfoTable(string infoTitle, Dictionary<string, TableData> multiLanguageTableData, DateTime startTime);
    }
}