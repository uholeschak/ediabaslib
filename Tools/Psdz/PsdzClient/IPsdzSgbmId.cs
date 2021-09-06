using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzSgbmId : IComparable<IPsdzSgbmId>
    {
        string HexString { get; }

        string Id { get; }

        long IdAsLong { get; }

        int MainVersion { get; }

        int PatchVersion { get; }

        string ProcessClass { get; }

        int SubVersion { get; }
    }
}
