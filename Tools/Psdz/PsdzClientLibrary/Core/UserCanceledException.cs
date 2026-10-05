using System;
using BMW.Rheingold.CoreFramework;

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