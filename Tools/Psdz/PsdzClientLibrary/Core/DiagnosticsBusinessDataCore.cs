using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Globalization;
using System;
using PsdzClient.Core;

namespace PsdzClientLibrary.Core
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
        public string GetMainSeriesSgbd(IIdentVehicle vecInfo)
        {
            switch (vecInfo.BordnetType)
            {
                case BordnetType.BEV2010:
                    return "E89X";
                case BordnetType.IBUS:
                    return "-";
                default:
                    {
                        if (((IReactorVehicle)vecInfo).Prodart == "P")
                        {
                            if (!string.IsNullOrEmpty(((IReactorVehicle)vecInfo).Produktlinie))
                            {
                                string text = ((IReactorVehicle)vecInfo).Produktlinie.ToUpper();
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
                                                    if (!(text == "PL3-ALT"))
                                                    {
                                                        break;
                                                    }
                                                    return "ZCS_ALL";
                                                case '5':
                                                    if (!(text == "PL5-ALT"))
                                                    {
                                                        break;
                                                    }
                                                    return "RR1";
                                                case '6':
                                                    if (!(text == "PL6-ALT"))
                                                    {
                                                        break;
                                                    }
                                                    return "E60";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        switch (text[2])
                                        {
                                            case '0':
                                                break;
                                            case '2':
                                                if (!(text == "PL2"))
                                                {
                                                    goto IL_026c;
                                                }
                                                return "E89X";
                                            case '3':
                                                if (!(text == "PL3"))
                                                {
                                                    goto IL_026c;
                                                }
                                                return "R56";
                                            case '4':
                                                if (!(text == "PL4"))
                                                {
                                                    goto IL_026c;
                                                }
                                                return "E70";
                                            default:
                                                goto IL_026c;
                                        }
                                        if (text == "PL0")
                                        {
                                            if (((IReactorVehicle)vecInfo).Ereihe == "E38" || ((IReactorVehicle)vecInfo).Ereihe == "E46" || ((IReactorVehicle)vecInfo).Ereihe == "E83" || ((IReactorVehicle)vecInfo).Ereihe == "E85" || ((IReactorVehicle)vecInfo).Ereihe == "E86" || ((IReactorVehicle)vecInfo).Ereihe == "E36" || ((IReactorVehicle)vecInfo).Ereihe == "E39" || ((IReactorVehicle)vecInfo).Ereihe == "E52" || ((IReactorVehicle)vecInfo).Ereihe == "E53")
                                            {
                                                return "ZCS_ALL";
                                            }
                                            if (((IReactorVehicle)vecInfo).Ereihe == "E65" || ((IReactorVehicle)vecInfo).Ereihe == "E66" || ((IReactorVehicle)vecInfo).Ereihe == "E67" || ((IReactorVehicle)vecInfo).Ereihe == "E68")
                                            {
                                                return "E65";
                                            }
                                            goto IL_0272;
                                        }
                                    }
                                }
                                goto IL_026c;
                            }
                            goto IL_0272;
                        }
                        if (((IReactorVehicle)vecInfo).Prodart == "M")
                        {
                            switch (vecInfo.BordnetType)
                            {
                                case BordnetType.BN2000_MOTORBIKE:
                                case BordnetType.BNK01X_MOTORBIKE:
                                    return "MRK24";
                                case BordnetType.BN2020_MOTORBIKE:
                                    switch (((IReactorVehicle)vecInfo).Baureihenverbund)
                                    {
                                        case "K001":
                                        case "KE01":
                                            return "X_K001";
                                        case "X001":
                                        case "XS01":
                                            return "X_X001";
                                        case "KS01":
                                            return "X_KS01";
                                        default:
                                            return "X_X001";
                                    }
                                default:
                                    return "-";
                            }
                        }
                        return "";
                    }
                IL_026c:
                    return "F01";
                IL_0272:
                    return "-";
            }
        }

        public string GetMainSeriesSgbdAdditional(IIdentVehicle vecInfo, ILogger logger)
        {
            logger.Info(logger.CurrentMethod(), "Entering GetMainSeriesSgbdAdditional");
            if (((IReactorVehicle)vecInfo).Prodart == "P")
            {
                if (!string.IsNullOrEmpty(((IReactorVehicle)vecInfo).Produktlinie))
                {
                    string text = ((IReactorVehicle)vecInfo).Produktlinie.ToUpper();
                    if (!(text == "PL5-ALT"))
                    {
                        if (text == "PL6")
                        {
                            if (!vecInfo.C_DATETIME.HasValue)
                            {
                                logger.Info(logger.CurrentMethod(), "Product line: " + ((IReactorVehicle)vecInfo).Produktlinie + ", C_DATETIME is null");
                                if (((IReactorVehicle)vecInfo).Ereihe == "F01" || ((IReactorVehicle)vecInfo).Ereihe == "F02" || ((IReactorVehicle)vecInfo).Ereihe == "F03" || ((IReactorVehicle)vecInfo).Ereihe == "F04" || ((IReactorVehicle)vecInfo).Ereihe == "F06" || ((IReactorVehicle)vecInfo).Ereihe == "F07" || ((IReactorVehicle)vecInfo).Ereihe == "F10" || ((IReactorVehicle)vecInfo).Ereihe == "F11" || ((IReactorVehicle)vecInfo).Ereihe == "F12" || ((IReactorVehicle)vecInfo).Ereihe == "F13" || ((IReactorVehicle)vecInfo).Ereihe == "F18")
                                {
                                    logger.Info(logger.CurrentMethod(), "Ereihe: " + ((IReactorVehicle)vecInfo).Ereihe + ", returning F01BN2K");
                                    return "F01BN2K";
                                }
                            }
                            else if (vecInfo.C_DATETIME < DTimeF01Lci)
                            {
                                logger.Info(logger.CurrentMethod(), "Product line: " + ((IReactorVehicle)vecInfo).Produktlinie + ", C_DATETIME is earlier than DTimeF01Lci");
                                return "F01BN2K";
                            }
                        }
                        else
                        {
                            logger.Info(logger.CurrentMethod(), "Reached default block, produck line: " + ((IReactorVehicle)vecInfo).Produktlinie);
                        }
                    }
                    else
                    {
                        if (!vecInfo.C_DATETIME.HasValue)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + ((IReactorVehicle)vecInfo).Produktlinie + ", C_DATETIME is null");
                            return "RR1_2020";
                        }
                        if (vecInfo.C_DATETIME >= DTimeRR_S2)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + ((IReactorVehicle)vecInfo).Produktlinie + ", C_DATETIME is later than DTimeRR_S2");
                            return "RR1_2020";
                        }
                    }
                }
            }
            else
            {
                _ = ((IReactorVehicle)vecInfo).Prodart == "M";
            }
            logger.Info(logger.CurrentMethod(), "Returning null for product line: " + ((IReactorVehicle)vecInfo)?.Produktlinie + ", ereihe: " + ((IReactorVehicle)vecInfo).Ereihe);
            return null;
        }
    }
}
