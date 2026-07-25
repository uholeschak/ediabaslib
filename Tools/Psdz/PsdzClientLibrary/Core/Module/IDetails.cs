using BMW.Authoring;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IDetails : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IFA FA { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IIStufe IStufe { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        DateTime Baustand { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        double Gwsz { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string HmiVersion { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        DateTime Produktionsdatum { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string VIN7 { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string VIN17 { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IFscList FscList { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IFscRepList FscRepList { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IMerkmaleFahrzeug Fahrzeug { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IMerkmaleAntriebseinheit Antriebseinheit { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IMerkmaleEMaschine EMaschine { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IMerkmaleMotor Motor { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        List<IMerkmaleHeat> Heats { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITechCampaignList TechCampaignList { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string TestGroup { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        DateTime Systemzeit { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        double SysTimeVehicle { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        DateTime Erstzulassung { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Typgenehmigungsnummer { get; }

        string SoftwareId { get; }

        string F2Date { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool MerkmalFahrzeug_IsValue(MerkmalFahrzeug merkmal, string wert, params string[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool MerkmalFahrzeug_IsValue(MerkmalFahrzeug merkmal, string[] wert);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool MerkmalAntriebseinheit_IsValue(MerkmalAntriebseinheit merkmal, string wert, params string[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool MerkmalAntriebseinheit_IsValue(MerkmalAntriebseinheit merkmal, string[] wert);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool MerkmalEMaschine_IsValue(MerkmalEMaschine merkmal, string wert, params string[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool MerkmalEMaschine_IsValue(MerkmalEMaschine merkmal, string[] wert);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool MerkmalMotor_IsValue(MerkmalMotor merkmal, string wert, params string[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool MerkmalMotor_IsValue(MerkmalMotor merkmal, string[] wert);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IMerkmalHeat_Matching MerkmalHeat_IsValue(MerkmalHeat merkmal, string wert, params string[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IMerkmalHeat_Matching MerkmalHeat_IsValue(MerkmalHeat merkmal, string[] wert);
    }
}
