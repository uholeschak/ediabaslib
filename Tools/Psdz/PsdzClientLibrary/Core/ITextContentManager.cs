using System.Linq;
using System.Xml;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.CoreFramework.Module
{
    public interface ITextContentManager
    {
        ITextLocator __Text();

        ITextLocator __Text(string value);

        ITextLocator __Text(string value, __TextParameter[] paramArray);

        ITextLocator __StandardText(decimal value, __TextParameter[] paramArray);
    }
}
