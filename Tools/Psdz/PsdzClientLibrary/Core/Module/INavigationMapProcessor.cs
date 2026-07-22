using PsdzClient.Contracts;
using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface INavigationMapProcessor
    {
        string DecodeFromBase32(string fsc);

        void GetActivationCodes(string svin, string sHaendlernummer, out List<string> swIds, out List<string> activationCodes);

        void GetINIActivationCodes(string vin7, string vin17, string sHaendlernummer, string applicationNo, string upgradeIndex, out List<string> swIds, out List<string> activationCodes);

        void GetActivationCodes(string svin, string[] sHaendlernummer, out List<string> swIds, out List<string> activationCodes);

        string GetInternationalDealerNumber(string brand, string product);

        void GetSoftwareIdAndMapName(string sSGBMID, out string sSoftwareID, out string sMapName);

        List<string> GetInternationalDealerNumbersForAllBrandsUnderCurrentOutlet(string product);

        void AbortHddUpdate();

        List<INavFSCProvided> GetNavigationMapsForExistingFSCs(string svin, string[] dealerNumbers);

        bool SaveFscLocally(IFSCProvided fsc);

        bool AreNavigationFscsAvailable(string svin, string[] dealerNumbers);

        List<IFSCProvided> GetNavigationFscs(string svin, string[] dealerNumbers);

        bool SaveFSCsLocally(List<IFSCProvided> fscList);

        bool SaveNavigationFSCsLocally(string svin, string[] dealerNumbers);
    }

}
