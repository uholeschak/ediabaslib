using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Contracts
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IBoolResultObject
    {
        bool Result { get; }

        string ErrorCode { get; }

        string ErrorMessage { get; }

        [Obsolete("ErrorContext is deprecated. Please use the attributr IBoolResultObject.Context")]
        string ErrorContext { get; }

        [Obsolete("ErrorTime is deprecated.Please use the attributr IBoolResultObject.Time")]
        DateTime ErrorTime { get; }

        string Context { get; }

        DateTime Time { get; }

        void SetValues(bool result, string errorCode, string errorMessage);
    }
}
