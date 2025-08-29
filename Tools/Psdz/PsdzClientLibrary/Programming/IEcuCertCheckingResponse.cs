using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum EcuCertCheckingStatus
    {
        CheckStillRunning,
        Empty,
        Incomplete,
        Malformed,
        Ok,
        Other,
        SecurityError,
        Unchecked,
        WrongVin17,
        Decryption_Error,
        IssuerCertError,
        Outdated,
        OwnCertNotPresent,
        Undefined,
        WrongEcuUid,
        Unknown,
        KeyError,
        NotUsed
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuCertCheckingResponse
    {
        IEcuIdentifier Ecu { get; }

        EcuCertCheckingStatus? CertificateStatus { get; }

        EcuCertCheckingStatus? BindingsStatus { get; }

        EcuCertCheckingStatus? OtherBindingsStatus { get; }

        IBindingDetailsStatus[] BindingDetailStatus { get; }

        IOtherBindingDetailsStatus[] OtherBindingDetailStatus { get; }

        EcuCertCheckingStatus? KeypackStatus { get; }

        IKeypackDetailStatus[] KeyPackDetailedStatus { get; }

        EcuCertCheckingStatus? OnlineBindingsStatus { get; }

        IBindingDetailsStatus[] OnlineBindingDetailStatus { get; }

        EcuCertCheckingStatus? OnlineCertificateStatus { get; }

        string CreationTimestamp { get; }

        IEcuPdxInfo PdxInfo { get; set; }
    }
}
