using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface ISelectable
    {
        string Key { get; }

        ISelectableEntry AddEntry(bool selectionState, IList<LocalizedText> entryTextList, IList<LocalizedText> entryLabelList);

        void SetEntry(int index, bool selectionState);
    }
}
