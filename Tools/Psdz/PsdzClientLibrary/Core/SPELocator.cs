using System.Globalization;
using System;
using BmwFileReader;

namespace PsdzClient.Core
{
    public class SPELocator : ISPELocator
    {
        private string id;

#pragma warning disable CS0649
        private ISPELocator[] children;

        private ISPELocator[] parents;

        private string[] dataValueNames;

        private string[] incomingLinkNames;

        private string[] outgoingLinkNames;
#pragma warning restore CS0649

        public ISPELocator[] Children => children;

        public ISPELocator[] Parents => parents;

        public string Id
        {
            get
            {
                if (id.ConvertToInt() == -1)
                {
                    return null;
                }
                return id.ToString(CultureInfo.InvariantCulture);
            }
        }

        public string DataClassName => GetType().Name;

        public string[] OutgoingLinkNames => outgoingLinkNames;

        public string[] IncomingLinkNames => incomingLinkNames;

        public string[] DataValueNames => dataValueNames;

        public string SignedId => id;

        public Exception Exception => null;

        public bool HasException => false;

        public SPELocator()
            : this("-1")
        {
        }

        public SPELocator(string id)
        {
            this.id = id;
        }

        public string GetDataValue(string name)
        {
            return null;
        }

        public ISPELocator[] GetIncomingLinks()
        {
            return parents;
        }

        public ISPELocator[] GetIncomingLinks(string incomingLinkName)
        {
            return parents;
        }

        public ISPELocator[] GetOutgoingLinks()
        {
            return children;
        }

        public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
        {
            return children;
        }

        public T GetDataValue<T>(string name)
        {
            return default(T);
        }
    }
}
