using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Globalization;
using System;
using PsdzClient.Core;

namespace PsdzClient.Core
{
    public class DiagnosticsBusinessDataCore
    {
        protected static DateTime DTimeRR_S2 => DateTime.ParseExact("01.06.2012", "dd.MM.yyyy", new CultureInfo("de-DE"));

        protected static DateTime DTimeF01Lci => DateTime.ParseExact("01.07.2013", "dd.MM.yyyy", new CultureInfo("de-DE"));

        // ToDo: Check on update
        public BordnetType GetBordnetType(string baureihenverbund, string prodart, string ereihe, ILogger logger)
        {
            if (prodart == "P")
            {
                if (!string.IsNullOrEmpty(baureihenverbund))
                {
                    switch (baureihenverbund.ToUpper())
                    {
                        case "R050":
                        case "E085":
                        case "E083":
                            return BordnetType.IBUS;
                        case "E060":
                        case "E065":
                        case "E070":
                        case "E89X":
                        case "R056":
                        case "RR01":
                            return BordnetType.BN2000;
                        case "M012":
                            return BordnetType.BEV2010;
                        case "F001":
                        case "F010":
                        case "F020":
                        case "F025":
                        case "F056":
                        case "G070":
                        case "I001":
                        case "I020":
                        case "M013":
                        case "RR21":
                        case "S15A":
                        case "S15C":
                        case "S18A":
                        case "S18T":
                        case "U006":
                        case "J001":
                            return BordnetType.BN2020;
                        default:
                            return BordnetType.BN2020;
                    }
                }
                logger.Warning(logger.CurrentMethod(), "Baureihenverbund is null or empty. BordnetType will be determined by Ereihe!");
                switch (ereihe)
                {
                    case "E30":
                    case "E31":
                    case "E32":
                    case "E52":
                    case "E34":
                    case "E36":
                    case "E46":
                    case "E38":
                    case "E39":
                    case "E53":
                        return BordnetType.IBUS;
                    default:
                        logger.Warning(logger.CurrentMethod(), "Ereihe is null or empty. No BordnetType can be determined!");
                        return BordnetType.UNKNOWN;
                }
            }
            if (prodart == "M")
            {
                if (!string.IsNullOrEmpty(baureihenverbund))
                {
                    switch (baureihenverbund.ToUpper())
                    {
                        case "KH24":
                        case "K024":
                            return BordnetType.BN2000_MOTORBIKE;
                        case "KS01":
                        case "XS01":
                        case "K001":
                        case "X001":
                        case "KE01":
                            return BordnetType.BN2020_MOTORBIKE;
                        case "K01X":
                            return BordnetType.BNK01X_MOTORBIKE;
                        default:
                            return BordnetType.BN2020_MOTORBIKE;
                    }
                }
                logger.Info(logger.CurrentMethod(), "Baureihenverbund was empty, returning default value.");
                return BordnetType.BN2020_MOTORBIKE;
            }
            logger.Info(logger.CurrentMethod(), "Returning BordnetType.UNKNOWN for Prodart: " + prodart);
            return BordnetType.UNKNOWN;
        }

        // ToDo: Check on update
        // Use IlSpy to decompile the method
        // vecInfo type changed to Vehicle
        public string GetMainSeriesSgbd(Vehicle vecInfo)
        {
            BordnetType bordnetType = vecInfo.BordnetType;
            if (bordnetType == BordnetType.IBUS)
            {
                return "-";
            }
            if (bordnetType == BordnetType.BEV2010)
            {
                return "E89X";
            }
            if (vecInfo.Prodart == "P")
            {
                if (!string.IsNullOrEmpty(vecInfo.Produktlinie))
                {
                    string text = vecInfo.Produktlinie.ToUpper();
                    if (text != null)
                    {
                        int length = text.Length;
                        if (length != 3)
                        {
                            if (length == 7)
                            {
                                switch (text[2])
                                {
                                    case '3':
                                        if (text == "PL3-ALT")
                                        {
                                            return "ZCS_ALL";
                                        }
                                        break;
                                    case '5':
                                        if (text == "PL5-ALT")
                                        {
                                            return "RR1";
                                        }
                                        break;
                                    case '6':
                                        if (text == "PL6-ALT")
                                        {
                                            return "E60";
                                        }
                                        break;
                                }
                            }
                        }
                        else
                        {
                            switch (text[2])
                            {
                                case '0':
                                    if (text == "PL0")
                                    {
                                        if (vecInfo.Ereihe == "E38" || vecInfo.Ereihe == "E46" || vecInfo.Ereihe == "E83" || vecInfo.Ereihe == "E85" || vecInfo.Ereihe == "E86" || vecInfo.Ereihe == "E36" || vecInfo.Ereihe == "E39" || vecInfo.Ereihe == "E52" || vecInfo.Ereihe == "E53")
                                        {
                                            return "ZCS_ALL";
                                        }
                                        if (vecInfo.Ereihe == "E65" || vecInfo.Ereihe == "E66" || vecInfo.Ereihe == "E67" || vecInfo.Ereihe == "E68")
                                        {
                                            return "E65";
                                        }
                                        goto IL_0272;
                                    }
                                    break;
                                case '2':
                                    if (text == "PL2")
                                    {
                                        return "E89X";
                                    }
                                    break;
                                case '3':
                                    if (text == "PL3")
                                    {
                                        return "R56";
                                    }
                                    break;
                                case '4':
                                    if (text == "PL4")
                                    {
                                        return "E70";
                                    }
                                    break;
                            }
                        }
                    }
                    return "F01";
                }
                IL_0272:
                return "-";
            }
            if (!(vecInfo.Prodart == "M"))
            {
                return "";
            }
            switch (vecInfo.BordnetType)
            {
                case BordnetType.BN2000_MOTORBIKE:
                case BordnetType.BNK01X_MOTORBIKE:
                    return "MRK24";
                case BordnetType.BN2020_MOTORBIKE:
                    {
                        string text = vecInfo.Baureihenverbund;
                        if (text == "K001" || text == "KE01")
                        {
                            return "X_K001";
                        }
                        if (text == "X001" || text == "XS01")
                        {
                            return "X_X001";
                        }
                        if (!(text == "KS01"))
                        {
                            return "X_X001";
                        }
                        return "X_KS01";
                    }
                default:
                    return "-";
            }
        }

        // ToDo: Check on update
        // Use IlSpy to decompile the method
        // vecInfo type changed to Vehicle
        public string GetMainSeriesSgbdAdditional(Vehicle vecInfo, ILogger logger)
        {
            logger.Info(logger.CurrentMethod(), "Entering GetMainSeriesSgbdAdditional", Array.Empty<object>());
            if (vecInfo.Prodart == "P")
            {
                if (!string.IsNullOrEmpty(vecInfo.Produktlinie))
                {
                    string text = vecInfo.Produktlinie.ToUpper();
                    if (!(text == "PL5-ALT"))
                    {
                        if (!(text == "PL6"))
                        {
                            logger.Info(logger.CurrentMethod(), "Reached default block, produck line: " + vecInfo.Produktlinie, Array.Empty<object>());
                        }
                        else if (vecInfo.C_DATETIME == null)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null", Array.Empty<object>());
                            if (vecInfo.Ereihe == "F01" || vecInfo.Ereihe == "F02" || vecInfo.Ereihe == "F03" || vecInfo.Ereihe == "F04" || vecInfo.Ereihe == "F06" || vecInfo.Ereihe == "F07" || vecInfo.Ereihe == "F10" || vecInfo.Ereihe == "F11" || vecInfo.Ereihe == "F12" || vecInfo.Ereihe == "F13" || vecInfo.Ereihe == "F18")
                            {
                                logger.Info(logger.CurrentMethod(), "Ereihe: " + vecInfo.Ereihe + ", returning F01BN2K", Array.Empty<object>());
                                return "F01BN2K";
                            }
                        }
                        else if (vecInfo.C_DATETIME < DiagnosticsBusinessDataCore.DTimeF01Lci)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is earlier than DTimeF01Lci", Array.Empty<object>());
                            return "F01BN2K";
                        }
                    }
                    else
                    {
                        if (vecInfo.C_DATETIME == null)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null", Array.Empty<object>());
                            return "RR1_2020";
                        }
                        if (vecInfo.C_DATETIME >= DiagnosticsBusinessDataCore.DTimeRR_S2)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is later than DTimeRR_S2", Array.Empty<object>());
                            return "RR1_2020";
                        }
                    }
                }
            }
            else
            {
                _ = vecInfo.Prodart == "M"; // [UH] Unused variable
            }
            logger.Info(logger.CurrentMethod(), "Returning null for product line: " + ((vecInfo != null) ? vecInfo.Produktlinie : null) + ", ereihe: " + vecInfo.Ereihe, Array.Empty<object>());
            return null;
        }
    }
}
