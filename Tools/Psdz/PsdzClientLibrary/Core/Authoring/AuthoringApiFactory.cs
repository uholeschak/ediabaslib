using PsdzClient.Core;
using System.ComponentModel;
using BMW.Authoring.API.Math;
using BMW.Authoring.Database;

namespace BMW.Authoring.API
{
    using Vehicle = BMW.Authoring.Vehicle.Vehicle;

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public static class AuthoringApiFactory
    {
        private static RandomForestObjectCreator<RandomForest> RandomForestCreator = new RandomForestObjectCreator<RandomForest>();
        private static IQDMModeAPI QDMModeAPI;
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static RandomForest GetRandomForest(string name)
        {
            return RandomForestCreator[name];
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static IQDMModeAPI GetQDMModeAPI(IAuthoringModule istaModule)
        {
            return QDMModeAPI ?? (QDMModeAPI = new QDMModeAPI(istaModule));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static BMW.Authoring.Vehicle.Vehicle GetVehicle(IAuthoringModule istaModule)
        {
            return new BMW.Authoring.Vehicle.Vehicle(istaModule);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static BMW.Authoring.Session.ISession GetSession(IAuthoringModule istaModule)
        {
            //[-] return new BMW.Authoring.Session.Session(istaModule);
            //[+] return null;
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static IDbAccess GetDbAccess(IAuthoringModule istaModule)
        {
            //[-] return new DbAccess(istaModule.DBProvider, istaModule.Vehicle, istaModule.FFMDynamicResolver);
            //[+] return null;
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ICertificateDocumentCreator GetCertificateDocumentCreator(IAuthoringModule istaModule)
        {
            //[-] DealerData dealerData = new DealerData(istaModule);
            //[-] return new CertificateDocumentCreator(istaModule, dealerData);
            //[+] return null;
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static IBackendCommunication GetBackendCommunication(IAuthoringModule istaModule)
        {
            //[-] return new BackendCommunication(istaModule);
            //[+] return null;
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static IProtocolEnrichment GetProtocolEnrichment(IAuthoringModule istaModule)
        {
            //[-] return new ProtocolEnrichment(istaModule);
            //[+] return null;
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static IMathFunctions GetMathFunctions(IAuthoringModule istaModule)
        {
            //[-] return new MathFunctions();
            //[+] return null;
            return null;
        }
    }
}