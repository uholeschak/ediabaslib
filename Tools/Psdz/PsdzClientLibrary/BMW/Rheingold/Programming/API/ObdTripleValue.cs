using BMW.Rheingold.CoreFramework.Programming.Data.Obd;

namespace BMW.Rheingold.Programming.API
{
    public class ObdTripleValue : IObdTripleValue
    {
        public string CalId { get; set; }
        public string ObdId { get; set; }
        public string SubCVN { get; set; }

        public ObdTripleValue(string calid, string obdid, string subcvn)
        {
            CalId = calid;
            ObdId = obdid;
            SubCVN = subcvn;
        }
    }
}