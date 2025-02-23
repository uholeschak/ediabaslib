using System.ComponentModel;

namespace PsdzClientLibrary.Core
{
    public interface IEMotor : INotifyPropertyChanged
    {
        string EMOTArbeitsverfahren { get; set; }

        string EMOTBaureihe { get; set; }

        string EMOTBezeichnung { get; set; }

        string EMOTDrehmoment { get; set; }

        string EMOTEinbaulage { get; set; }

        string EMOTKraftstoffart { get; set; }

        string EMOTLeistungsklasse { get; set; }

        string EMOTUeberarbeitung { get; set; }
    }
}