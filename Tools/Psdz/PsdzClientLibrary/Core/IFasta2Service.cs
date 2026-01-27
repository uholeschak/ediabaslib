using System;

namespace PsdzClient.Core
{
    [PreserveSource(Hint = "Dummy interface", SuppressWarning = true)]
    public interface IFasta2Service
    {
        bool AddServiceCode(string name, string value, LayoutGroup layoutGroup, bool allowMultipleEntries = false, bool bufferIfSessionNotStarted = false, DateTime? timeStamp = null, bool? isSystemTime = null);
    }
}
