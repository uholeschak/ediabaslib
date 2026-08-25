using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.FASTA.Model
{
    public class SelectableNop : ISelectable
    {
        public string Key { get; set; }

        public int? Sequence { get; set; }

        public ISelectableEntry AddEntry(bool selectionState, IList<LocalizedText> entryTextList, IList<LocalizedText> entryLabelList)
        {
            Log.Debug("SelectableNop.AddEntry()", "Not operation executed.");
            return new SelectableEntryNop();
        }

        public void SetEntry(int index, bool selectionState)
        {
            Log.Debug("SelectableNop.SetEntry()", "Not operation executed.");
        }
    }
}
