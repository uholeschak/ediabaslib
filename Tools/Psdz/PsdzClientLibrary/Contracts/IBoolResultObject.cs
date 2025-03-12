using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IBoolResultObject
    {
        bool Result { get; }

        string ErrorCode { get; }

        int ErrorCodeInt { get; }

        string ErrorMessage { get; }

        string Context { get; }

        DateTime Time { get; }

        int StatusCode { get; }

        void SetValues(bool result, string errorCode, string errorMessage);
    }
}
