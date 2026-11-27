using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.SmartMaintenance
{
    [DataContract]
    public class EslDocumentIsta
    {
        [DataMember]
        public long OID { get; set; }

        [DataMember]
        public string Identifier { get; set; }

        [DataMember]
        public string EpmFilterID { get; set; }

        [DataMember]
        public string InfoType { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Xml { get; set; }

        [DataMember]
        public string DiagnosisCode { get; set; }

        [DataMember]
        public decimal VersionNumber { get; set; }

        [DataMember]
        public string Language { get; set; }

        [DataMember]
        public IList<LinkedMedia> LinkedMedia { get; set; } = new List<LinkedMedia>();

        public EslDocumentIsta(long oid, string identifier, string epmFilterID, string infoType, string title, string xml, string diagnosisCode, decimal versionNumber)
        {
            OID = oid;
            Identifier = identifier;
            EpmFilterID = epmFilterID;
            InfoType = infoType;
            Title = title;
            Xml = xml;
            DiagnosisCode = diagnosisCode;
            VersionNumber = versionNumber;
        }
    }
}
