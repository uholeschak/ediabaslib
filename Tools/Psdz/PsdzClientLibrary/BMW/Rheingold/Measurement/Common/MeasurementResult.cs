namespace BMW.Rheingold.Measurement.Common
{
    public class MeasurementResult
    {
        public string Fehlergrund { get; set; }

        public string AnswerText { get; set; }

        public double Value { get; set; }

        public bool Status { get; set; }

        public bool IOResult { get; set; }
    }
}
