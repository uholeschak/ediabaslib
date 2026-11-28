using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.DatabaseProvider.SmartMaintenance;
using BMW.Rheingold.CoreFramework.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Operation
{
    public static class InfoObjectUtils
    {
        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public static string GetInfoTypeDescription(InfoObject doc)
        {
            return GetInfoTypeDescription(doc.XepInfoObject);
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public static string GetInfoTypeDescription(IXepInfoObject xepInfoObject, bool isLoadedFromHotSpot = false)
        {
            string text = string.Empty;
            if (xepInfoObject == null)
            {
                return text;
            }

            //[-] XEP_WSCONFIGFILTERS infoTypes = DatabaseProviderFactory.Instance.GetInfoTypes(xepInfoObject.InfoType);
            //[+] XEP_WSCONFIGFILTERS infoTypes = null;
            XEP_WSCONFIGFILTERS infoTypes = null;
            if (infoTypes != null && !string.IsNullOrEmpty(infoTypes.Title))
            {
                text = infoTypes.Title;
            }

            if (!string.IsNullOrEmpty(xepInfoObject.InfoType))
            {
                switch (xepInfoObject.InfoType)
                {
                    case "FUB":
                        switch (xepInfoObject.InformationsTyp)
                        {
                            case "FBFunktionsbeschreibung":
                                text = new FormatedData(isLoadedFromHotSpot ? "#ComponentDescription" : "#FunctionalDescription", true).Localize();
                                break;
                            case "FBFunktionsprüfanleitung":
                                text = new FormatedData("#TestingInstructions", true).Localize();
                                break;
                            case "FBHilfehinweis":
                                text = new FormatedData("#Help", true).Localize();
                                break;
                            case "FBSollwerte":
                                text = new FormatedData("#SpecificationData", true).Localize();
                                break;
                            default:
                                text = new FormatedData("#FunctionalDescription", true).Localize();
                                break;
                        }

                        break;
                    case "SPI":
                        text = new FormatedData("#SPI", true).Localize();
                        break;
                    case "GPI":
                        text = new FormatedData("#GPI", true).Localize();
                        break;
                    case "SWS":
                        text = new FormatedData("#WorkshopEquipmentDevices", true).Localize();
                        break;
                    case "REP":
                        text = new FormatedData("#Repairinstruction", true).Localize();
                        break;
                    case "SIT":
                        text = new FormatedData("#ServiceInformationTechn", true).Localize();
                        break;
                    case "FTD":
                        text = new FormatedData("#VehicleEngineeringDiagnosis", true).Localize();
                        break;
                    case "AZD":
                        text = new FormatedData("#TighteningTorque", true).Localize();
                        break;
                    case "SWZ":
                        text = new FormatedData("#SpecialTool", true).Localize();
                        break;
                    case "KFA":
                        text = new FormatedData("#KFA", true).Localize();
                        break;
                    case "API":
                        text = new FormatedData("#API", true).Localize();
                        break;
                    case "FKB":
                        text = new FormatedData("#FaultCodeDescription", true).Localize();
                        break;
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }

            return "Info";
        }

        public static bool IsSvg(InfoObject document)
        {
            return DocumentTypeConverter.GetEnumDocumentType(document.XepInfoObject.DocumentType) == DocumentType.SVG;
        }

        public static string FormatTitle(IXepInfoObject xepInfoObj, decimal? implicitVersion = null)
        {
            if (xepInfoObj == null)
            {
                Log.Error("InfoObjectTitleConverter.FormatTitle()", "Failed to format info object null. Returning empty string instead.");
                return string.Empty;
            }

            if (xepInfoObj.Nodeclass == (decimal? )41153666)
            {
                if (!string.Equals("std", xepInfoObj.Title, StringComparison.Ordinal))
                {
                    return xepInfoObj.Title;
                }

                return FormatedData.Localize("#VehicleTest");
            }

            IList<string> list = new List<string>();
            if (!string.IsNullOrEmpty(xepInfoObj.Identifikator))
            {
                list.Add(xepInfoObj.Identifikator);
            }

            if (!string.IsNullOrEmpty(xepInfoObj.Title))
            {
                list.Add(xepInfoObj.Title);
            }

            if (implicitVersion.HasValue)
            {
                list.Add(string.Format(CultureInfo.InvariantCulture, "V.{0}", implicitVersion));
            }
            else if (xepInfoObj.VersionNumber.HasValue)
            {
                list.Add(string.Format(CultureInfo.InvariantCulture, "V.{0}", xepInfoObj.VersionNumber));
            }

            return string.Join(" - ", list);
        }

        public static string FormatTitle(EslDocumentIsta eslDocument)
        {
            return eslDocument.Identifier + " - " + eslDocument.Title;
        }

        public static string GetGraphicsPath(decimal id, string linkId, string type)
        {
            string text = linkId + "_" + id;
            return Path.Combine(ConfigSettings.GetTempFolder(), text + "." + GraphicsType.GetSuffixByFormat(type));
        }
    }
}