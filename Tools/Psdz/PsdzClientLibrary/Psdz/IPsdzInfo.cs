using PsdzClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace BMW.Rheingold.Psdz
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IPsdzInfo
    {
        bool IsPsdzInitialized { get; }

        bool IsValidPsdzVersion { get; }

        string PsdzDataPath { get; }

        string PsdzVersion { get; }

        [PreserveSource(Hint = "For backward compatibility")]
        string ExpectedPsdzVersion { get; }
    }
}