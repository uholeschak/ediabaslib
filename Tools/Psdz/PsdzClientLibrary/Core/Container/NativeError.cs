
namespace PsdzClient.Core.Container
{
    internal class NativeError : INativeError
    {
        private string identifier;

        private string message;

        public string Identifier => identifier;

        public string Message => message;

        public NativeError(string identifier, string message)
        {
            this.identifier = identifier;
            this.message = message;
        }
    }
}