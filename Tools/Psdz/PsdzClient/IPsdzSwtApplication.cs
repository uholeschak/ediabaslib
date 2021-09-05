using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzSwtApplication
    {
        byte[] Fsc { get; }

        byte[] FscCert { get; }

        bool IsBackupPossible { get; }

        int Position { get; }

        PsdzFscCertificateState FscCertState { get; }

        PsdzFscState FscState { get; }

        PsdzSoftwareSigState? SoftwareSigState { get; }

        PsdzSwtActionType? SwtActionType { get; }

        PsdzSwtType SwtType { get; }

        IPsdzSwtApplicationId SwtApplicationId { get; }
    }
}
