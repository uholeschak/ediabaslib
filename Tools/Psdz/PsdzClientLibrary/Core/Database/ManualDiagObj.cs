using System.Collections.Generic;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class ManualDiagObj : XEP_DIAGNOSISOBJECTSEX
    {
        [XmlIgnore]
        public IEnumerable<XEP_DIAGNOSISOBJECTSEX> SearchTreeNode { get; set; }

        public ManualDiagObj()
        {
        }

        public ManualDiagObj(IEnumerable<XEP_DIAGNOSISOBJECTSEX> searchTreeNode)
        {
            SearchTreeNode = searchTreeNode;
        }
    }
}
