using BMW.Rheingold.CoreFramework.Localization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class CheckControlMessage : ILocalizedTitle, INotifyPropertyChanged
    {
        public static Regex TitlePlaceholderRegex = new Regex("{{[^}]*}}\\s?");
        private static int faultGroupNumber = 7;
        private static string faultGroupLabel = FormatedData.Localize($"#FaultGroup{faultGroupNumber}");
        private static string unknownTitle = FormatedData.Localize("#UnknownCCMOrWarningLight");
        private HashSet<decimal> sourceDtcIds;
        public XEP_CCMESSAGE XepCCM { get; set; }

        public HashSet<decimal> SourceDtcIds
        {
            get
            {
                return sourceDtcIds;
            }

            set
            {
                if (sourceDtcIds != value)
                {
                    sourceDtcIds = value;
                    OnPropertyChanged("SourceDtcIds");
                }
            }
        }

        public long Id { get; set; }
        public long? DbId => XepCCM?.Id;
        public long Frequency { get; set; }
        public double Mileage { get; set; }
        public double Timestamp { get; set; }
        public string SGVariante { get; set; }
        public string Cause { get; set; }
        public string Longtext { get; set; }
        public int Timestamp_NS { get; set; }
        public int Occurence { get; set; }
        public int PWF_STATE { get; set; }
        public string Diag_Addr_Sender_Hex { get; set; }
        public int FaultGroupNumber => faultGroupNumber;
        public string FaultGroupLabel => faultGroupLabel;
        public string Title { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public CheckControlMessage()
        {
        }

        public CheckControlMessage(long id, double mileage, long frequency, double timestamp, XEP_CCMESSAGE xepCCM)
        {
            Mileage = mileage;
            Timestamp = timestamp;
            Id = id;
            Frequency = frequency;
            XepCCM = xepCCM;
            SetTitle();
        }

        public CheckControlMessage(int id, double mileage, double timestamp, string diag_Addr_Sender_Hex, string sGVariante, int timestamp_NS, int occurence, int pWF_STATE, XEP_CCMESSAGE xepCCM)
        {
            XepCCM = xepCCM;
            Id = id;
            Mileage = mileage;
            Timestamp = timestamp;
            SGVariante = sGVariante;
            Cause = xepCCM?.Cause;
            Longtext = xepCCM?.Longtext;
            Timestamp_NS = timestamp_NS;
            Occurence = occurence;
            PWF_STATE = pWF_STATE;
            Diag_Addr_Sender_Hex = diag_Addr_Sender_Hex;
            SetTitle();
        }

        public string GetLocalizedTitle(string language)
        {
            if (XepCCM == null)
            {
                return unknownTitle;
            }

            return XepCCM.GetLocalizedTitle(language);
        }

        public Fault ToFault()
        {
            return new Fault
            {
                FaultGroupLabel = faultGroupLabel,
                DTC = new DTC
                {
                    DTCContext = new ObservableCollection<typeDTCContext>
                    {
                        new typeDTCContext
                        {
                            F_UW_KM_SUPREME = Mileage,
                            F_UW_KM = (long)Mileage,
                            F_UW_ZEIT = (long)Timestamp,
                            F_UW_ZEIT_SUPREME = Timestamp
                        }
                    },
                    F_ORT = Id,
                    FaultGroup = faultGroupNumber,
                    F_HFK = Frequency
                },
                ECU = new ECU(),
                IsNewFaultMemoryActive = true,
                FaultLabel = Title
            };
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetTitle()
        {
            string text = XepCCM?.Title;
            if (string.IsNullOrEmpty(text))
            {
                Title = unknownTitle;
            }
            else
            {
                Title = TitlePlaceholderRegex.Replace(text, string.Empty).Trim();
            }
        }
    }
}