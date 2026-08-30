using System;

namespace BMW.Rheingold.CoreFramework
{
    public class ServiceDialogMethodUnsupportedException : ArgumentException
    {
        public ServiceDialogMethodUnsupportedException()
        {
        }

        public ServiceDialogMethodUnsupportedException(string method)
            : base(method)
        {
        }
    }
}
