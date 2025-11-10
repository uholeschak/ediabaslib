using PsdzClientLibrary;
using System.Linq;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [PreserveSource(Hint = "Changed to public")]
    [DataContract]
    public class EcuDataGroup
    {
        [DataMember(Name = "ecuData")]
        public readonly EcuData[] ecuData;

        public override string ToString()
        {
            EcuData[] array = ecuData;
            if (array == null || !array.Any())
            {
                return string.Empty;
            }
            return string.Join("/", ecuData.Select((EcuData e) => e?.ToString()));
        }
    }
}