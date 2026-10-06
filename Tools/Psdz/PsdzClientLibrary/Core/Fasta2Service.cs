using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using System;
using System.Collections.Generic;
using BMW.Rheingold.FASTA.Models;
using PsdzClient;
using PsdzClient.Core;

namespace BMW.Rheingold.FASTA
{
    [PreserveSource(Hint = "Class cleaned", SuppressWarning = true)]
    public class Fasta2Service : Fasta2Base, IFasta2Service
    {
        public IProtocolBasic ProtocolingInstance => this;

        public Fasta2Service()
        {

        }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Identifier
        {
            get
            {
                return string.Empty;
            }
            set
            {
                Log.Info("Fasta2Service.Identifier.Set", "Set property is not implemented.");
            }
        }

        protected override void CheckProtocolTime()
        {
        }


        public IFastaGrouping CreateSubGroup(BMW.Rheingold.CoreFramework.Contracts.FASTA.GroupingType groupingType, IList<LocalizedText> subgroupTitleList)
        {
            return null;
        }

    }
}
