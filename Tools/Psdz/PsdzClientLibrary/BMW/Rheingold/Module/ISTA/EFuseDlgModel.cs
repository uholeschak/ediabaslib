using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.TableDataHelper;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class EFuseDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private TableData resultData;

        public TableData ResultData
        {
            get
            {
                return resultData;
            }
            set
            {
                if (!object.Equals(resultData, value))
                {
                    resultData = value;
                    OnPropertyChanged("ResultData");
                }
            }
        }
    }
}
