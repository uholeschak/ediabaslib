using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core.Container;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Xml;
using System;

namespace PsdzClientLibrary.Core
{
    // [UH] dummy interface
    public interface IFasta2Service
    {
        void AddServiceCode(string name, string value, LayoutGroup layoutGroup, bool allowMultipleEntries = false);
    }
}
