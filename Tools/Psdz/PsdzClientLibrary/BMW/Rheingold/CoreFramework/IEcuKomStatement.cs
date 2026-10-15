using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.VehicleCommunication;

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
