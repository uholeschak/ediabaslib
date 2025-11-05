using System;

namespace PsdzClient.Core
{
    internal class TextContentManagerDummy : ITextContentManager
    {
        public ITextLocator __StandardText(decimal value, __TextParameter[] paramArray)
        {
            throw new NotImplementedException();
        }

        public ITextLocator __Text()
        {
            return __Text("null");
        }

        public ITextLocator __Text(string value)
        {
            return __Text(value, null);
        }

        public ITextLocator __Text(string id, __TextParameter[] paramArray)
        {
            return new TextLocator("### " + id + " ###");
        }
    }
}