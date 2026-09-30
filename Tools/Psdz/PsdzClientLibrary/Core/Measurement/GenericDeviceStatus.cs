using BMW.Rheingold.Measurement.Common;

namespace BMW.Rheingold.Measurement.Common
{
    public class GenericDeviceStatus
    {
        public ExcecutionStatus Status { get; set; }

        public string AnswerStatus { get; set; }

        public GenericDeviceStatus()
        {
            Status = new ExcecutionStatus();
        }

        public override string ToString()
        {
            return Status?.ToString() + ", Answer: " + AnswerStatus;
        }
    }
}
