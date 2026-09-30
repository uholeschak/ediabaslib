using BMW.Rheingold.Measurement.Common;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Measurement.Common
{
    [DataContract]
    public class DmmModel : ModelBase
    {
        private HashSet<MeasuringSensor> connectedForbiddenSensors;

        private Devices deviceForClampCheck;

        private DmmChannel[] channels;

        private DeviceImibEventArgs currentSensors;

        private InfoEventArgs infoEvent;

        [DataMember]
        public HashSet<MeasuringSensor> ConnectedForbiddenSensors
        {
            get
            {
                return connectedForbiddenSensors;
            }
            set
            {
                connectedForbiddenSensors = value;
                OnPropertyChanged("ConnectedForbiddenSensors");
            }
        }

        [DataMember]
        public Devices DeviceForClampCheck
        {
            get
            {
                return deviceForClampCheck;
            }
            set
            {
                deviceForClampCheck = value;
                OnPropertyChanged("DeviceForClampCheck");
            }
        }

        [DataMember]
        public DeviceImibEventArgs CurrentSensors
        {
            get
            {
                return currentSensors;
            }
            set
            {
                currentSensors = value;
                OnPropertyChanged("CurrentSensors");
            }
        }

        [DataMember]
        public InfoEventArgs InfoEvent
        {
            get
            {
                return infoEvent;
            }
            set
            {
                infoEvent = value;
                OnPropertyChanged("InfoEvent");
            }
        }

        [DataMember]
        public DmmChannel[] Channels
        {
            get
            {
                return channels;
            }
            set
            {
                channels = value;
                OnPropertyChanged("Channels");
            }
        }

        public DmmModel()
        {
            channels = new DmmChannel[3];
            Channels[1] = new DmmChannel(1);
            Channels[2] = new DmmChannel(2);
            connectedForbiddenSensors = new HashSet<MeasuringSensor>();
        }
    }
}
