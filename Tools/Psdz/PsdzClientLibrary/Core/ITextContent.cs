namespace PsdzClient.Core
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