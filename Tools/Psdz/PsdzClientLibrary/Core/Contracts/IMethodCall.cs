using System;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IMethodCall
    {
        DateTime EndTime { set; }

        string ReturnValue { set; }
    }
}
