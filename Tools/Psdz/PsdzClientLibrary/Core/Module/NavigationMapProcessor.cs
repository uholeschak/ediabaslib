using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.DatabaseProvider.Dealer;
using BMW.Rheingold.CoreFramework.Utility;
using BMW.Rheingold.ISTA.CoreFramework;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.InfoProvider.HDD.HDDLookup;
using BMW.Rheingold.InfoProvider.SWT.DTOs;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    internal class NavigationMapProcessor : INavigationMapProcessor
    {
        private readonly IEcuKom ecuKom;
        private ILogic logic;
        internal NavigationMapProcessor(IEcuKom ecuKom, ILogic logic)
        {
            this.ecuKom = ecuKom;
            this.logic = logic;
        }

        public string DecodeFromBase32(string fsc)
        {
            try
            {
                return FormatConverter.ByteArray2String(Base32.FromBase32String(fsc), 12u);
            }
            catch (Exception exception)
            {
                Log.WarningException("NavigationMapProcessor.DecodeFromBase32()", exception);
            }

            return string.Empty;
        }

        public void GetINIActivationCodes(string vin7, string vin17, string sHaendlernummer, string applicationNo, string upgradeIndex, out List<string> swIds, out List<string> activationCodes)
        {
            swIds = new List<string>();
            activationCodes = new List<string>();
            if (string.IsNullOrEmpty(vin7) || vin7.Length != 7)
            {
                Log.Info("NavigationMapProcessor.GetINIActivationCodes()", "vin7 was null or empty or was in incorrect length");
                return;
            }

            if (string.IsNullOrEmpty(vin17) || vin17.Length != 17)
            {
                Log.Info("NavigationMapProcessor.GetINIActivationCodes()", "vin17 was null or empty or was in incorrect length");
                return;
            }

            try
            {
                //[-] List<TypeFSCProvidedDto> list = new SwtProcessorV3Service(new SWTProcessorV3Impl(null)).GetFSCList(vin7, vin17, new string[1] { sHaendlernummer }, TypeResourceIndicatorDto.INI, applicationNo, upgradeIndex, 3)?.ToList();
                //[+] List<TypeFSCProvidedDto> list = null;
                List<TypeFSCProvidedDto> list = null;
                if (list == null)
                {
                    return;
                }

                List<TypeFSCProvidedDto> list2 = new List<TypeFSCProvidedDto>();
                foreach (TypeFSCProvidedDto fsc in list)
                {
                    if (fsc.fscItem != null && fsc.fscItem.swID != null && fsc.fscItem.fsc != null && !string.IsNullOrEmpty(fsc.fscItem.swID.applicationNo + fsc.fscItem.swID.upgradeIndex) && !string.IsNullOrEmpty(fsc.fscItem.fsc.Value) && !list2.Any((TypeFSCProvidedDto c) => c.fscItem.swID.applicationNo == fsc.fscItem.swID.applicationNo && c.fscItem.swID.upgradeIndex == fsc.fscItem.swID.upgradeIndex && c.fscItem.fsc.Value == fsc.fscItem.fsc.Value))
                    {
                        list2.Add(fsc);
                        swIds.Add(fsc.fscItem.swID.applicationNo + fsc.fscItem.swID.upgradeIndex);
                        activationCodes.Add(fsc.fscItem.fsc.Value);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("NavigationMapProcessor.GetINIActivationCodes()", exception);
            }
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public void GetActivationCodes(string svin, string[] sHaendlernummer, out List<string> swIds, out List<string> activationCodes)
        {
            swIds = new List<string>();
            activationCodes = new List<string>();
            try
            {
                Log.Info("NavigationMapProcessor.GetActivationCodes()", "Input svin: " + svin + ", Haendlernummer " + string.Join(", ", sHaendlernummer));
                //[-] List<IFSCProvided> iFscProvidedList = new SwtProcessorV3Service(new SWTProcessorV3Impl(logic)).GetIFscProvidedList(svin, sHaendlernummer);
                //[+] List<IFSCProvided> iFscProvidedList = null;
                List<IFSCProvided> iFscProvidedList = null;
                if (iFscProvidedList != null)
                {
                    List<IFSCProvided> list = new List<IFSCProvided>();
                    foreach (IFSCProvided fsc in iFscProvidedList)
                    {
                        if (fsc.FscItem != null && fsc.FscItem.SwID != null && fsc.FscItem.Fsc != null && !string.IsNullOrEmpty(fsc.FscItem.SwID.ApplicationNo + fsc.FscItem.SwID.UpgradeIndex) && !string.IsNullOrEmpty(fsc.FscItem.Fsc.Value) && !list.Any((IFSCProvided c) => c.FscItem.SwID.ApplicationNo == fsc.FscItem.SwID.ApplicationNo && c.FscItem.SwID.UpgradeIndex == fsc.FscItem.SwID.UpgradeIndex && c.FscItem.Fsc.Value == fsc.FscItem.Fsc.Value))
                        {
                            list.Add(fsc);
                            swIds.Add(fsc.FscItem.SwID.ApplicationNo + fsc.FscItem.SwID.UpgradeIndex);
                            activationCodes.Add(fsc.FscItem.Fsc.Value);
                        }
                    }
                }

                Log.Info("NavigationMapProcessor.GetActivationCodes()", "Output SwIds: " + string.Join(", ", swIds) + ". N. SwIds: " + swIds.Count);
            }
            catch (Exception exception)
            {
                Log.WarningException("NavigationMapProcessor.GetActivationCodes()", exception);
            }
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public void GetActivationCodes(string svin, string sHaendlernummer, out List<string> swIds, out List<string> activationCodes)
        {
            string[] sHaendlernummer2 = (string.IsNullOrEmpty(sHaendlernummer) ? null : new string[1]
            {
                sHaendlernummer
            }

            );
            GetActivationCodes(svin, sHaendlernummer2, out swIds, out activationCodes);
        }

        public string GetInternationalDealerNumber(string brand, string product)
        {
            Log.Info("NavigationMapProcessor.GetInternationalDealerNumber()", "Input Brand: " + brand + " Product: " + product);
            //[-] if (IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA"))
            //[-] {
            //[-] Log.Info("NavigationMapProcessor.GetInternationalDealerNumber()", "Running Toyota ISTA instance, so the brand provided will be ignored and used Toyota instead");
            //[-] brand = "TOYOTA";
            //[-] }
            try
            {
                //[-] Dealer dealerInstance = LicenseHelper.DealerInstance;
                //[+] Dealer dealerInstance = null;
                Dealer dealerInstance = null;
                if (dealerInstance != null && dealerInstance.HasOutlet() && dealerInstance.FirstOutlet != null)
                {
                    Brand brand2;
                    switch (brand)
                    {
                        case "RollsRoyce":
                        case "ROLLS-ROYCE PKW":
                            brand2 = Brand.RollsRoyce;
                            break;
                        case "MINI":
                        case "Mini":
                        case "MINI PKW":
                            brand2 = Brand.Mini;
                            break;
                        case "BMWi":
                        case "BMW i":
                            brand2 = Brand.BMWi;
                            break;
                        case "TOYOTA":
                            brand2 = Brand.TOYOTA;
                            break;
                        default:
                            brand2 = Brand.BMW;
                            break;
                    }

                    Product product2;
                    if (!(product == "motorcycle"))
                    {
                        if (!(product == "vehicle"))
                        {
                        }

                        product2 = Product.Vehicle;
                    }
                    else
                    {
                        product2 = Product.Motorcycle;
                    }

                    BrandName? brandName = BrandMapping.ConvertToBrandName(brand2, product2);
                    Contract contract = null;
                    //[-] contract = ((brandName != EnumConverter.ConvertBrandNameToContractsBrandName(BrandName.TOYOTA)) ? (brandName.HasValue ? dealerInstance.GetValidContract(dealerInstance.DealerData?.OutletNumber, brandName.Value, "T") : null) : dealerInstance.GetValidContract(dealerInstance.DealerData?.OutletNumber, brandName.Value, null));
                    if (contract != null)
                    {
                        Log.Info("NavigationMapProcessor.GetInternationalDealerNumber()", "international dpno was: {0}", contract.internationalDealerNumber);
                        return contract.internationalDealerNumber;
                    }

                    Log.Info("NavigationMapProcessor.GetInternationalDealerNumber()", "No valid contract found, dpno will be AG100");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("NavigationMapProcessor.GetInternationalDealerNumber()", exception);
            }

            return "AG100";
        }

        public void GetSoftwareIdAndMapName(string sSGBMID, out string sSoftwareID, out string sMapName)
        {
            Log.Info("NavigationMapProcessor.GetSoftwareIdAndMapName()", "Input sSGBMID: " + sSGBMID);
            //[-] IEnumerable<SgbmIdType> hddLogisticsEntries = BMW.Rheingold.InfoProvider.NavigationMapProcessor.getHddLogisticsEntries(sSGBMID);
            //[-] SgbmIdType sgbmIdType = SelectHddLogisticsEntry(hddLogisticsEntries, sSGBMID, ecuKom);
            //[+] SgbmIdType sgbmIdType = null;
            SgbmIdType sgbmIdType = null;
            if (sgbmIdType != null)
            {
                sSoftwareID = sgbmIdType.SWID_FscShort;
                sMapName = sgbmIdType.name;
                Log.Info("NavigationMapProcessor.GetSoftwareIdAndMapName()", "Output sSoftwareID: " + sSoftwareID + ", sMapName: " + sMapName);
            }
            else
            {
                sSoftwareID = null;
                sMapName = null;
            }
        }

        public List<string> GetInternationalDealerNumbersForAllBrandsUnderCurrentOutlet(string product)
        {
            Log.Info("NavigationMapProcessor.GetInternationalDealerNumbersForAllBrandsUnderCurrentOutlet()", "Input Product: " + product);
            //[-] string[] array = LicenseHelper.DealerInstance.GetRelevantBrandNames(ConfigSettings.OperationalMode).TrimSplit(',');
            //[+] string[] array = Array.Empty<string>();
            string[] array = Array.Empty<string>();
            List<string> haendlerNrs = new List<string>();
            array.ForEach(delegate (string m)
            {
                AddInternationalDealerNumberToList(GetInternationalDealerNumber(m, product), haendlerNrs);
            });
            Log.Info("NavigationMapProcessor.GetInternationalDealerNumbersForAllBrandsUnderCurrentOutlet()", "International Dealer Numbers Found: " + string.Join(", ", haendlerNrs) + ". Brands: \" " + string.Join(", ", array) + "\" Product: \"" + product + "\" ");
            return haendlerNrs;
        }

        private void AddInternationalDealerNumberToList(string internationalDealerNumber, List<string> haendlerNrs)
        {
            if (!haendlerNrs.Contains(internationalDealerNumber) && !"AG100".Equals(internationalDealerNumber.ToUpper()))
            {
                haendlerNrs.Add(internationalDealerNumber);
            }
        }

        private SgbmIdType SelectHddLogisticsEntry(IEnumerable<SgbmIdType> entries, string sgbmId, IEcuKom ecuKom)
        {
            if (entries == null || !entries.Any())
            {
                return null;
            }

            if (entries.Count() == 1)
            {
                return entries.First();
            }

            Log.Info("NavigationMapProcessor.SelectHddLogisticsEntry()", "No. of found entries for SGBMID '{0}': {1}", sgbmId, entries.Count());
            if (ecuKom != null)
            {
                IEcuJob ecuJob = ecuKom.ApiJob("G_MMI", "STATUS_KOMP_ID", "");
                if (ecuJob.IsOkay())
                {
                    string compId = ecuJob.getStringResult("STAT_KOMP_ID");
                    if (!string.IsNullOrEmpty(compId))
                    {
                        Log.Info("NavigationMapProcessor.SelectHddLogisticsEntry()", "HDD compatibility identifier found: {0}", compId);
                        SgbmIdType sgbmIdType = (
                            from elem in entries
                            where elem.EcuVariant != null && elem.EcuVariant.Any((EcuVariantType ecuVariant) => string.Equals(compId, ecuVariant.CompatibilityIdentifier, StringComparison.OrdinalIgnoreCase))orderby elem.SWID_FscShort descending
                            select elem).FirstOrDefault();
                        if (sgbmIdType != null)
                        {
                            Log.Info("NavigationMapProcessor.SelectHddLogisticsEntry()", "Logistics entry selected due to compatibility identifier. ([SWID_FscShort: '{0}', MapName: '{1}'])", sgbmIdType.SWID_FscShort, sgbmIdType.name);
                            return sgbmIdType;
                        }
                    }
                }
            }

            Log.Warning("NavigationMapProcessor.SelectHddLogisticsEntry()", "Correct logistics entry could not be determined. First entry will be returned.");
            return entries.First();
        }

        public void AbortHddUpdate()
        {
            logic.Fasta2Service.AddMethodCall("AbortHddUpdate").EndTime = DateTime.Now;
            logic.AbortHddUpdate = true;
        }

        public List<INavFSCProvided> GetNavigationMapsForExistingFSCs(string svin, string[] dealerNumbers)
        {
            IMethodCall methodCall = logic.Fasta2Service.AddMethodCall("GetNavigationMapsForExistingFSCs");
            //[-] List<INavFSCProvided> navigationMapsForExistingFSCs = new HddFscHandler(logic).GetNavigationMapsForExistingFSCs(logic.VecInfo.VIN17, dealerNumbers);
            //[-] methodCall.ReturnValue = navigationMapsForExistingFSCs?.ToString() ?? "";
            //[-] methodCall.EndTime = DateTime.Now;
            //[-] return navigationMapsForExistingFSCs;
            //[+] return null;
            return null;
        }

        public bool SaveFscLocally(IFSCProvided fsc)
        {
            IMethodCall methodCall = logic.Fasta2Service.AddMethodCall("SaveFscLocally");
            //[-] bool result = new HddFscManager(new InteractionWizardModel(new FormatedData("#ImportRefurbishFscAction", "ISTAGui", false).Localize(logic.Lang)), logic).SaveFscLocally(fsc);
            //[-] methodCall.ReturnValue = result.ToString() ?? "";
            //[-] methodCall.EndTime = DateTime.Now;
            //[-] return result;
            //[+] return false;
            return false;
        }

        public bool AreNavigationFscsAvailable(string svin, string[] dealerNumbers)
        {
            IMethodCall methodCall = logic.Fasta2Service.AddMethodCall("AreNavigationFscsAvailable");
            //[-] bool result = new HddFscHandler(logic).AreNavigationFscsAvailable(logic.VecInfo.VIN17, dealerNumbers);
            //[-] methodCall.ReturnValue = result.ToString() ?? "";
            //[-] methodCall.EndTime = DateTime.Now;
            //[-] return result;
            //[+] return false;
            return false;
        }

        public List<IFSCProvided> GetNavigationFscs(string svin, string[] dealerNumbers)
        {
            IMethodCall methodCall = logic.Fasta2Service.AddMethodCall("GetNavigationFscs");
            //[-] List<IFSCProvided> navigationFCSs = new HddFscHandler(logic).GetNavigationFCSs(logic.VecInfo.VIN17, dealerNumbers);
            //[-] methodCall.ReturnValue = navigationFCSs?.ToString() ?? "";
            //[-] methodCall.EndTime = DateTime.Now;
            //[-] return navigationFCSs;
            //[+] return null;
            return null;
        }

        public bool SaveFSCsLocally(List<IFSCProvided> fscList)
        {
            IMethodCall methodCall = logic.Fasta2Service.AddMethodCall("GetNavigationFscs");
            //[-] bool result = new HddFscManager(new InteractionWizardModel(new FormatedData("#ImportRefurbishFscAction", "ISTAGui", false).Localize(logic.Lang)), logic).SaveFSCsLocally(fscList);
            //[-] methodCall.ReturnValue = result.ToString() ?? "";
            //[-] methodCall.EndTime = DateTime.Now;
            //[-] return result;
            //[+] return false;
            return false;
        }

        public bool SaveNavigationFSCsLocally(string svin, string[] dealerNumbers)
        {
            List<IFSCProvided> navigationFscs = GetNavigationFscs(svin, dealerNumbers);
            return SaveFSCsLocally(navigationFscs);
        }
    }
}