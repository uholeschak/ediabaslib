using System;

namespace BMW.Rheingold.Measurement.Common
{
    public class ImibExceptionEventArg : EventArgs
    {
        public string defaultLanguageId;

        public Exception ExceptionData { get; private set; }

        public string LocalizedMessageId
        {
            get
            {
                if (ExceptionData != null && ExceptionData is ImibConfigException && !string.IsNullOrEmpty(((ImibConfigException)ExceptionData).LanguageId))
                {
                    return ((ImibConfigException)ExceptionData).LanguageId;
                }
                return defaultLanguageId;
            }
            private set
            {
                defaultLanguageId = value;
            }
        }

        public ImibExceptionEventArg(Exception exceptionData)
        {
            ExceptionData = exceptionData;
            LocalizedMessageId = string.Empty;
        }

        public ImibExceptionEventArg(Exception exceptionData, string localizedMessageId)
            : this(exceptionData)
        {
            LocalizedMessageId = localizedMessageId;
        }
    }
}
