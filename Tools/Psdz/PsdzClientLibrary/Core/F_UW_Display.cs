using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Xml.Serialization;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class F_UW_Display : IDtcUmweltDisplay
    {
        private readonly object current_F_UW_WERT;

        private readonly string current_F_UW_EINH;

        private readonly object first_F_UW_WERT;

        private readonly string first_F_UW_EINH;

        private readonly object second_F_UW_WERT;

        private readonly string second_F_UW_EINH;

        private readonly string F_UW_TEXT_vec;

        [PreserveSource(Hint = "XEP_ENVCONDSLABELS", Placeholder = true)]
        private readonly PlaceholderType f_UW_TEXT_EnvCondLabels;

        [XmlIgnore]
        [PreserveSource(Hint = "XEP_ENVCONDSLABELS", Placeholder = true)]
        public PlaceholderType F_UW_TEXT_EnvCondLabels => f_UW_TEXT_EnvCondLabels;

        public string F_UW_TEXT
        {
            get
            {
                if (F_UW_TEXT_vec != null)
                {
                    return F_UW_TEXT_vec;
                }
                //[-] if (f_UW_TEXT_EnvCondLabels != null)
                //[-] {
                //[-] return f_UW_TEXT_EnvCondLabels.Title;
                //[-] }
                return null;
            }
        }

        public object Current_F_UW_WERT => current_F_UW_WERT;

        public string Current_F_UW_EINH => current_F_UW_EINH;

        public object First_F_UW_WERT => first_F_UW_WERT;

        public string First_F_UW_EINH => first_F_UW_EINH;

        public object Second_F_UW_WERT => second_F_UW_WERT;

        public string Second_F_UW_EINH => second_F_UW_EINH;

        [PreserveSource(Hint = "f_UW_TEXT_EnvCondLabels.Uwident", Placeholder = true)]
        public string F_UW_IDENT => string.Empty;

        public F_UW_Display()
        {
        }

        [PreserveSource(Hint = "XEP_ENVCONDSLABELS", SignatureModified = true)]
        public F_UW_Display(PlaceholderType F_UW_TEXT_EnvCondLabels, object current_F_UW_WERT, string current_F_UW_EINH, object first_F_UW_WERT, string first_F_UW_EINH, object second_F_UW_WERT, string second_F_UW_EINH)
        {
            f_UW_TEXT_EnvCondLabels = F_UW_TEXT_EnvCondLabels;
            F_UW_TEXT_vec = null;
            this.current_F_UW_WERT = current_F_UW_WERT;
            this.current_F_UW_EINH = current_F_UW_EINH;
            this.first_F_UW_WERT = first_F_UW_WERT;
            this.first_F_UW_EINH = first_F_UW_EINH;
            this.second_F_UW_WERT = second_F_UW_WERT;
            this.second_F_UW_EINH = second_F_UW_EINH;
        }

        [PreserveSource(Hint = "XEP_ENVCONDSLABELS", SignatureModified = true)]
        public F_UW_Display(PlaceholderType F_UW_TEXT_EnvCondLabels, object F_UW_WERT, string F_UW_EINH)
        {
            f_UW_TEXT_EnvCondLabels = F_UW_TEXT_EnvCondLabels;
            F_UW_TEXT_vec = null;
            current_F_UW_WERT = F_UW_WERT;
            current_F_UW_EINH = F_UW_EINH;
            first_F_UW_WERT = F_UW_WERT;
            first_F_UW_EINH = F_UW_EINH;
            second_F_UW_WERT = F_UW_WERT;
            second_F_UW_EINH = F_UW_EINH;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public F_UW_Display(string F_UW_TEXT_vec, object current_F_UW_WERT, string current_F_UW_EINH, object first_F_UW_WERT, string first_F_UW_EINH, object second_F_UW_WERT, string second_F_UW_EINH)
        {
            this.F_UW_TEXT_vec = F_UW_TEXT_vec;
            //[-] f_UW_TEXT_EnvCondLabels = null;
            this.current_F_UW_WERT = current_F_UW_WERT;
            this.current_F_UW_EINH = current_F_UW_EINH;
            this.first_F_UW_WERT = first_F_UW_WERT;
            this.first_F_UW_EINH = first_F_UW_EINH;
            this.second_F_UW_WERT = second_F_UW_WERT;
            this.second_F_UW_EINH = second_F_UW_EINH;
        }
    }
}
