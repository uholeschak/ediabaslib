using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Session
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public class IcomNetworkConfiguration
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        public string DevType { get; set; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public string ImageVersionPackage { get; set; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public string NetworkType { get; set; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public string IPConfiguration { get; set; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public string IPAddress { get; set; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public string Netmask { get; set; }
    }
}
