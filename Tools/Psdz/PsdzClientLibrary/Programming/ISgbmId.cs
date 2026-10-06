using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISgbmId : IComparable<ISgbmId>, IEquatable<ISgbmId>
    {
        long Id { get; }

        int MainVersion { get; }

        int PatchVersion { get; }

        string ProcessClass { get; }

        int SubVersion { get; }

        string HexString { get; }
    }
}
