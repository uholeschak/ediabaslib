using BMW.Rheingold.Measurement.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Measurement.Common
{
    [DataContract]
    public class DeviceImibEventArgs : EventArgs
    {
        [DataMember]
        public IEnumerable<string> ConnectedSensors { get; set; }

        [DataMember]
        public IDictionary<int, IEnumerable<MeasuringSensor>> ConnectedSensorsWithConnector { get; private set; }

        public DeviceImibEventArgs()
        {
        }

        public DeviceImibEventArgs(IEnumerable<string> connectedSensors, IDictionary<int, IEnumerable<MeasuringSensor>> connectedSensorsWithConnector)
        {
            ConnectedSensors = connectedSensors.ToList();
            if (connectedSensorsWithConnector != null)
            {
                ConnectedSensorsWithConnector = connectedSensorsWithConnector.ToDictionary((KeyValuePair<int, IEnumerable<MeasuringSensor>> x) => x.Key, (KeyValuePair<int, IEnumerable<MeasuringSensor>> y) => y.Value);
            }
        }
    }
}
