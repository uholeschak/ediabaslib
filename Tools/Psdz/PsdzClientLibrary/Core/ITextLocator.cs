using PsdzClient.Core.Container;
using System.Collections.Generic;

namespace PsdzClient.Core
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ITextLocator : ISPELocator
    {
        string Text { get; }

        ITextContent TextContent { get; set; }

        ITextLocator Concat(ITextLocator theTextLocator);

        ITextLocator Concat(ITextLocator theTextLocator, bool theAddLineBreakAfter);

        ITextLocator Concat(IEnumerable<ITextLocator> theTextLocator);

        ITextLocator Concat(IEnumerable<ITextLocator> theTextLocator, bool theAddLineBreakAfter);
    }
}
