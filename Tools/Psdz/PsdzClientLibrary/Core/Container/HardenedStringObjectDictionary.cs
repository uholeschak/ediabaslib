using System;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core.Container
{
    public class HardenedStringObjectDictionary : Dictionary<string, object>
    {
        public new void Add(string key, object value)
        {
            try
            {
                if (!ContainsKey(key))
                {
                    base.Add(key, value);
                    return;
                }
                Remove(key);
                base.Add(key, value);
            }
            catch (Exception exception)
            {
                Log.WarningException("HardenedStringObjectDictionary.Add()", exception);
            }
        }
    }
}