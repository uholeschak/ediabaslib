using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Module
{
    [AuthorAPI]
    public interface IXmlElement
    {
        IXmlElement SelectElement(string xpath);

        IEnumerable<IXmlElement> SelectElements(string xpath);

        string GetValue(string xpath);
    }
}
