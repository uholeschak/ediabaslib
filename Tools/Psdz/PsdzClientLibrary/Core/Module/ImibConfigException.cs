using System;

namespace BMW.Rheingold.Measurement.Common
{
    public class ImibConfigException : Exception
    {
        public MeasuringFunctionData ConfigData { get; private set; }

        public string ErrorCode { get; private set; }

        public string LanguageId { get; private set; }

        public string MeasuringSource { get; set; }

        public ImibConfigException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ImibConfigException(string message, Exception innerException, string errorCode)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public ImibConfigException(string message, Exception innerException, string errorCode, string languageId)
            : this(message, innerException, errorCode)
        {
            LanguageId = languageId;
        }
    }
}
