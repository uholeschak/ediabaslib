using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PsdzClient;
using PsdzClientLibrary;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class EcuProgrammingVariantLocator : IEcuProgrammingVariantLocator, ISPELocator
    {
        private readonly XEP_ECUPROGRAMMINGVARIANT ecuVariant;

        private ISPELocator[] children;

        private ISPELocator[] parents;

        private readonly Vehicle vecInfo;

        private readonly IFFMDynamicResolver ffmResolver;

        public ISPELocator[] Children
        {
            get
            {
                if (children != null && children.Length != 0)
                {
                    return children;
                }
                List<ISPELocator> list = new List<ISPELocator>();
                children = list.ToArray();
                return children;
            }
        }

        public string Id => ecuVariant.Id.ToString(CultureInfo.InvariantCulture);

        public ISPELocator[] Parents
        {
            get
            {
                if (parents != null && parents.Length != 0)
                {
                    return parents;
                }
                List<ISPELocator> list = new List<ISPELocator>();
                //[-] XEP_ECUVARIANTS ecuVariantById = DatabaseProviderFactory.Instance.GetEcuVariantById(ecuVariant.EcuVariantId);
                //[+] XEP_ECUVARIANTS ecuVariantById = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuVariantById(ecuVariant.EcuVariantId.ToString(CultureInfo.InvariantCulture)));
                XEP_ECUVARIANTS ecuVariantById = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuVariantById(ecuVariant.EcuVariantId.ToString(CultureInfo.InvariantCulture)));
                if (ecuVariantById != null)
                {
                    list.Add(new EcuVariantLocator(ecuVariantById, vecInfo, ffmResolver));
                    parents = list.ToArray();
                }
                return parents;
            }
        }

        public string DataClassName => "ECUProgrammingVariant";

        public string[] OutgoingLinkNames => new string[0];

        public string[] IncomingLinkNames => new string[0];

        public string[] DataValueNames => new string[4] { "ID", "NAME", "FLASHLIMIT", "ECUVARIANTID" };

        public decimal SignedId
        {
            get
            {
                if (ecuVariant == null)
                {
                    return -1m;
                }
                return ecuVariant.Id;
            }
        }

        public Exception Exception => null;

        public bool HasException => false;

        public decimal EcuVariantId => ecuVariant.EcuVariantId;

        public decimal? FlashLimit => ecuVariant.FlashLimit;

        public string Name => ecuVariant.Name;

        public EcuProgrammingVariantLocator(XEP_ECUPROGRAMMINGVARIANT ecuVariant)
        {
            this.ecuVariant = ecuVariant;
            children = new ISPELocator[0];
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public static IEcuProgrammingVariantLocator CreateEcuProgrammingVariantLocator(string ecuVariant, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            IEcuProgrammingVariantLocator result = null;
            //[-] ICollection<XEP_ECUPROGRAMMINGVARIANT> ecuProgrammingVariantByName = DatabaseProviderFactory.Instance.GetEcuProgrammingVariantByName(ecuVariant, vecInfo, ffmResolver);
            //[+] ICollection<XEP_ECUPROGRAMMINGVARIANT> ecuProgrammingVariantByName = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuProgrammingVariantByName(ecuVariant, vecInfo, ffmResolver));
            ICollection<XEP_ECUPROGRAMMINGVARIANT> ecuProgrammingVariantByName = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuProgrammingVariantByName(ecuVariant, vecInfo, ffmResolver));
            if (ecuProgrammingVariantByName.Count == 1)
            {
                result = new EcuProgrammingVariantLocator(ecuProgrammingVariantByName.First(), vecInfo, ffmResolver);
            }
            else
            {
                Log.Warning("EcuProgrammingVariantLocator.CreateEcuProgrammingVariantLocator", "No or more than one ECU programming variant found by name: {0}", ecuVariant);
            }
            return result;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public EcuProgrammingVariantLocator(decimal id, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            //[-] ecuVariant = DatabaseProviderFactory.Instance.GetEcuProgrammingVariantById(id, vecInfo, ffmResolver);
            //[+] ecuVariant = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuProgrammingVariantById(id.ToString(CultureInfo.InvariantCulture), vecInfo, ffmResolver));
            ecuVariant = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuProgrammingVariantById(id.ToString(CultureInfo.InvariantCulture), vecInfo, ffmResolver));
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public EcuProgrammingVariantLocator(XEP_ECUPROGRAMMINGVARIANT ecuVariant, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            this.ecuVariant = ecuVariant;
            children = new ISPELocator[0];
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        public string GetDataValue(string name)
        {
            if (ecuVariant == null || string.IsNullOrEmpty(name))
            {
                return null;
            }
            switch (name.ToUpperInvariant())
            {
                case "ID":
                    return ecuVariant.Id.ToString(CultureInfo.InvariantCulture);
                case "NODECLASS":
                    return "4711";
                case "NAME":
                    return ecuVariant.Name;
                case "FLASHLIMIT":
                    if (!ecuVariant.FlashLimit.HasValue)
                    {
                        return null;
                    }
                    return ecuVariant.FlashLimit.Value.ToString(CultureInfo.InvariantCulture);
                case "ECUVARIANTID":
                    return ecuVariant.EcuVariantId.ToString(CultureInfo.InvariantCulture);
                default:
                    return string.Empty;
            }
        }

        public ISPELocator[] GetIncomingLinks()
        {
            return new ISPELocator[0];
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
            try
            {
                if (!string.IsNullOrEmpty(name) && ecuVariant != null)
                {
                    object obj = null;
                    switch (name.ToUpperInvariant())
                    {
                        case "ID":
                            obj = ecuVariant.Id;
                            break;
                        case "NODECLASS":
                            obj = "4711";
                            break;
                        case "NAME":
                            obj = ecuVariant.Name;
                            break;
                        case "ECUVARIANTID":
                            obj = ecuVariant.EcuVariantId;
                            break;
                    }
                    if (obj != null)
                    {
                        return (T)Convert.ChangeType(obj, typeof(T));
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("EcuProgrammingVariantLocator.GetDataValue<T>()", exception);
            }
            return default(T);
        }
    }
}
