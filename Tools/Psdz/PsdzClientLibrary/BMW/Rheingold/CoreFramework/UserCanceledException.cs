using System;

namespace BMW.Rheingold.CoreFramework
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