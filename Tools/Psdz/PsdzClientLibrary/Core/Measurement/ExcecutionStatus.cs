using System.Globalization;

namespace BMW.Rheingold.Measurement.Common
{
    public class ExcecutionStatus
    {
        public bool Status { get; set; }

        public string Reason { get; set; }

        public override string ToString()
        {
            string text = "Status: " + Status.ToString(CultureInfo.InvariantCulture);
            if (!Status)
            {
                text = text + ", Reason: " + Reason;
            }
            return text;
        }
    }
}
