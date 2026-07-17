using PsdzClient.Core.Container;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework
{
    public interface IEcuKomStatement
    {
        List<string> ProtocolResults { get; set; }

        bool ShowErrors { get; set; }

        IEcuJob Execute(string ecu, string job, string param, int retries, IProtocolBasic fastaProtocoler);

        IEcuJob Execute(string ecu, string job, byte[] param, int retries, IProtocolBasic fastaProtocoler);

        void AbortServiceProgram();

        bool CheckConnection();
    }
}
