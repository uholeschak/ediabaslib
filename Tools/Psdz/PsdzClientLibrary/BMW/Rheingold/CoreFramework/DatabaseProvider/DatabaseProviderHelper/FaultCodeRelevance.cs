using System.Collections.ObjectModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.DatabaseProviderHelper
{
    public class FaultCodeRelevance
    {
        public decimal Id { get; set; }

        public string FaultCode { get; set; }

        public string EcuVariant { get; set; }

        public ObservableCollection<int> FaultAvailableNumbers { get; set; }
    }
}
