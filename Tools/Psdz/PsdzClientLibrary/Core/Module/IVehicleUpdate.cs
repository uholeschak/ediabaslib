using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleUpdate
    {
        bool WriteIntegrationLevel { get; set; }

        bool WriteVehicleOrder { get; set; }

        bool WriteVehicleProfile { get; set; }

        bool WriteSvt { get; set; }

        bool UpdateMsm { get; set; }

        bool UpdatePiaPortierungsmaster { get; set; }
    }
}
