using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISPELocator
    {
        ISPELocator[] Children { get; }

        string DataClassName { get; }

        string[] DataValueNames { get; }

        Exception Exception { get; }

        bool HasException { get; }

        string Id { get; }

        string[] IncomingLinkNames { get; }

        string[] OutgoingLinkNames { get; }

        ISPELocator[] Parents { get; }

        decimal SignedId { get; }

        string GetDataValue(string name);

        T GetDataValue<T>(string name);

        ISPELocator[] GetIncomingLinks();

        ISPELocator[] GetIncomingLinks(string incomingLinkName);

        ISPELocator[] GetOutgoingLinks();

        ISPELocator[] GetOutgoingLinks(string outgoingLinkName);
    }
}
