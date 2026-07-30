using PsdzClient;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public class ManualDiagObj : PsdzDatabase.SwiDiagObj
    {
        [XmlIgnore]
        public IEnumerable<PsdzDatabase.SwiDiagObj> SearchTreeNode { get; set; }

        public ManualDiagObj()
        {
        }

        public ManualDiagObj(IEnumerable<PsdzDatabase.SwiDiagObj> searchTreeNode)
        {
            SearchTreeNode = searchTreeNode;
        }
    }
}
