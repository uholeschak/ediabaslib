using System.Globalization;
using System;
using BmwFileReader;

#pragma warning disable CS0649
namespace PsdzClient.Core
{
    public class SPELocator : ISPELocator
    {
        private decimal id;
        private ISPELocator[] children;
        private ISPELocator[] parents;
        private string[] dataValueNames;
        private string[] incomingLinkNames;
        private string[] outgoingLinkNames;
        public ISPELocator[] Children => children;
        public ISPELocator[] Parents => parents;

        public string Id
        {
            get
            {
                if (id == -1m)
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
        public decimal SignedId => id;
        public Exception Exception => null;
        public bool HasException => false;

        public SPELocator() : this(-1m)
        {
        }

        public SPELocator(decimal id)
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