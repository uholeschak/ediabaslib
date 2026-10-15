using System.Runtime.Serialization;

namespace BMW.ISPI.IstaOperation.Contract.ServiceProgram
{
    [DataContract]
    public class ServiceProgramMeasuringDlgAction : ServiceProgramAction
    {
        [DataMember]
        public bool IsAnswer1 { get; private set; }

        [DataMember]
        public bool IsAnswer2 { get; private set; }

        [DataMember]
        public string Value1 { get; private set; }

        [DataMember]
        public double Value2 { get; private set; }

        public ServiceProgramMeasuringDlgAction(bool isAnswer1, bool isAnswer2, string value1, double value2)
        {
            IsAnswer1 = isAnswer1;
            IsAnswer2 = isAnswer2;
            Value1 = value1;
            Value2 = value2;
        }
    }
}
