using System.Linq;
using System.Xml;

namespace PsdzClient.Core
{
    public struct __TextParameter
    {
        private readonly string name;

        private readonly object value;

        public string Name => name;

        public object Value => value;

        public __TextParameter(string name, object value)
        {
            this.name = name;
            if (value is string)
            {
                value = string.Concat(((string)value).Where(XmlConvert.IsXmlChar));
            }
            this.value = value;
        }
    }

    public interface ITextContentManager
    {
        ITextLocator __Text();

        ITextLocator __Text(string value);

        ITextLocator __Text(string value, __TextParameter[] paramArray);

        ITextLocator __StandardText(decimal value, __TextParameter[] paramArray);
    }
}
