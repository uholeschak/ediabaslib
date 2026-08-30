using System;
using System.Collections.Generic;

namespace BMW.Rheingold.PresentationFramework
{
    public delegate void DictionaryEventHandler(object sender, DictionaryEventArgs e);

    public class DictionaryEventArgs : EventArgs
    {
        public IDictionary<string, object> Parameters { get; private set; }

        public DictionaryEventArgs()
        {
            Parameters = new Dictionary<string, object>();
        }
    }
}
