using PsdzClient.Core;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IFastaGroupingBase
    {
        DateTime StartTime { get; set; }

        DateTime EndTime { get; set; }

        string Identifier { get; set; }

        IFastaGrouping CreateSubGroup(GroupingType groupingType, IList<LocalizedText> subgroupTitleList);
    }
}
