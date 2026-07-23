using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IHttpServerResponse
    {
        int HttpFlashPort { get; }

        bool IsHttpFlashAvailable { get; }
    }
}
