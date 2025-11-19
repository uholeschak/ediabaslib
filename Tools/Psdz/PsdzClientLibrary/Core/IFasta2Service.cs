using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core.Container;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Xml;
using System;
using PsdzClientLibrary;

namespace PsdzClient.Core
{
    [PreserveSource(Hint = "Dummy interface")]
    public interface IFasta2Service
    {
        bool AddServiceCode(string name, string value, LayoutGroup layoutGroup, bool allowMultipleEntries = false, bool bufferIfSessionNotStarted = false, DateTime? timeStamp = null, bool? isSystemTime = null);
    }
}
