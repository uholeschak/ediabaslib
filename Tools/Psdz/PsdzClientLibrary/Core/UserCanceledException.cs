using System;

namespace PsdzClient.Core
{
    [Serializable]
    [AuthorAPI]
    public class UserCanceledException : Exception
    {
        public UserCanceledException(string msg)
            : base(msg)
        {
        }
    }
}