using System;

namespace PsdzClient.Core.Container
{
    internal class AdapterError : IAdapterError
    {
        private IDiagnosticDeviceResult deviceResult;

        public string AdapterFullClassName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Exception Exception
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public long ID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public INativeError NativeError
        {
            get
            {
                if (deviceResult != null && deviceResult.ECUJob != null)
                {
                    return new NativeError(deviceResult.ECUJob.JobErrorCode.ToString(), deviceResult.ECUJob.JobErrorText);
                }
                return null;
            }
        }

        public AdapterError(IDiagnosticDeviceResult deviceResult)
        {
            this.deviceResult = deviceResult;
        }

        public override string ToString()
        {
            if (deviceResult != null && deviceResult.ECUJob != null)
            {
                return $"{deviceResult.ECUJob.JobErrorCode}: {deviceResult.ECUJob.JobErrorText}";
            }
            return string.Empty;
        }
    }
}
