using BMW.Rheingold.Programming.Common;
using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IKmmPlanElement : IComparable<IKmmPlanElement>
    {
        KmmPlanElementAction Action { get; }

        short DiagAddr { get; }

        KmmFlashFlags FlashFlags { get; }

        int FlashIndex { get; }

        int FlashSort { get; }

        string Info { get; }

        IList<string> NewZbNrList { get; }

        string OldZbNr { get; }
    }
}