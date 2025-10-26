using System;

namespace BMW.Rheingold.Psdz
{
    [Serializable]
    public sealed class JavaInstallationException : Exception
    {
        public JavaInstallationException(string message)
            : base(message)
        {
        }

        public JavaInstallationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}