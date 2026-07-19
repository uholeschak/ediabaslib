using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient;
using PsdzClient.Core;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IVehicleContext
    {
        BrandName? VehicleBrandName { get; }

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Fahrzeug.Nr3stellig", false)]
        string Motor { get; }

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Fahrzeug.EBezeichnung", false)]
        string Ereihe { get; }

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.VIN17", false)]
        string VIN17 { get; }

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.VIN7", false)]
        string VIN7 { get; }

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        string VINType { get; }

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        string VINRangeType { get; }

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Gwsz", false)]
        decimal? Gwsz { get; }

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        string GwszUnit { get; }

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Produktionsdatum", false)]
        DateTime? ProductionDate { get; }

        [Obsolete("Please use AuthoringApiFactory.GetVehicle(this).Details.Erstzulassung", false)]
        DateTime? FirstRegistration { get; }

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        IdentificationLevel VehicleIdentLevel { get; }

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        IVciDevice MIB { get; }

        IVciDevice VCI { get; }

        IFa FA { get; }

        IEnumerable<IEcu> ECU { get; }

        //bool IsSet(ICharacteristicsLocator characteristicsLocator);

        bool IsSet(IEcuGroupLocator group);

        bool IsSet(IEcuVariantLocator variant);

        //bool IsSet(IDiagnosticObjectLocator diagObject);

        //bool IsSet(IPerceivedSymptomsLocator preceivedSymptom);

        //bool IsSet(IFaultCodeLocator faultCodeLocator);

        //bool IsSet(IEquipmentLocator equipment);

        //bool IsSet(IFaultModeLocator faultModeLocator);

        //IEnumerable<ITechnicalAction> GetTechnicalActions(bool isSoftwareCampaign);

        void SetClamp15GuardianTrigger(double voltage);

        void SetClamp30GuardianTrigger(double voltage);

        [Obsolete("Please request alternative implementation inside Authoring namespace if still needed.", false)]
        void SetPWFStateGuardianTrigger(int[] validPWFStates);
    }
}
