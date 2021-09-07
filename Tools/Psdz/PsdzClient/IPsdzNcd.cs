using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzNcd
    {
        string MotorolaSString { get; }

        byte[] CafId { get; }

        int CodingArea { get; }

        int[] CodingProofStamp { get; }

        string CodingVersion { get; }

        string OBDCRC32 { get; }

        byte[] ObdRelevantBytes { get; }

        byte[] Signature { get; }

        int SignatureBlockAddress { get; }

        int SignatureLength { get; }

        int TlIdBlockAddress { get; }

        IList<IPsdzCoding1NcdEntry> UserDataCoding1 { get; }

        byte[] UserDataCoding2 { get; }

        bool IsSigned { get; }

        bool IsValid { get; }
    }
}
