using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ITextContent : ISPELocator
    {
        string FormattedText { get; }

        string PlainText { get; }

        string Text { get; }

        ITextContent Concat(ITextContent theTextContent);

        ITextContent Concat(string theNewString);

        ITextContent Concat(double theNewValue);

        ITextContent Concat(double theNewValue, string theMetaInformation);
    }
}