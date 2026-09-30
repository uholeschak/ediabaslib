using System;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Measurement.Common
{
    [DataContract]
    public class InfoEventArgs : EventArgs
    {
        [DataMember]
        public string MessageId { get; private set; }

        [DataMember]
        public string ModuleName { get; private set; }

        [DataMember]
        public ClampCalibrationState NextState { get; private set; }

        [DataMember]
        public ClampCalibrationState State { get; private set; }

        public InfoEventArgs()
        {
        }

        public InfoEventArgs(string messageId, string moduleName, ClampCalibrationState state, ClampCalibrationState nextState)
        {
            MessageId = messageId;
            ModuleName = moduleName;
            State = state;
            NextState = nextState;
        }
    }
}
