using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace PsdzClient.Core
{
    public class DiagnosticsBusinessDataCore
    {
        protected static readonly DateTime LciDateE36 = DateTime.Parse("1998-03-01", CultureInfo.InvariantCulture);
        protected static readonly DateTime LciDateE60 = DateTime.Parse("2005-09-01", CultureInfo.InvariantCulture);
        private static readonly HashSet<string> IBusPkw = new HashSet<string>
        {
            "R050",
            "E085",
            "E083"
        };
        private static readonly HashSet<string> BN2000Pkw = new HashSet<string>
        {
            "E060",
            "E065",
            "E070",
            "E89X",
            "R056",
            "RR01"
        };
        private static readonly HashSet<string> BN2020Pkw = new HashSet<string>
        {
            "F001",
            "F010",
            "F020",
            "F025",
            "F056",
            "G070",
            "I001",
            "I020",
            "M013",
            "RR21",
            "S15A",
            "S15C",
            "S18A",
            "S18T",
            "U006",
            "J001"
        };
        private static readonly HashSet<string> IBusEreihe = new HashSet<string>
        {
            "E30",
            "E31",
            "E32",
            "E34",
            "E36",
            "E38",
            "E39",
            "E46",
            "E52",
            "E53"
        };
        private static readonly HashSet<string> BEV2010Pkw = new HashSet<string>
        {
            "M012"
        };
        private static readonly HashSet<string> BN2000Bike = new HashSet<string>
        {
            "K024",
            "KH24"
        };
        private static readonly HashSet<string> BN2020Bike = new HashSet<string>
        {
            "K001",
            "KS01",
            "KE01",
            "X001",
            "XS01"
        };
        private static readonly HashSet<string> BNK01XBike = new HashSet<string>
        {
            "K01X"
        };
        private static readonly HashSet<string> FXXEreihe = new HashSet<string>
        {
            "F01",
            "F02",
            "F03",
            "F04",
            "F06",
            "F07",
            "F10",
            "F11",
            "F12",
            "F13",
            "F18"
        };
        private static readonly HashSet<string> ZcsAllEreihe = new HashSet<string>
        {
            "E38",
            "E46",
            "E83",
            "E85",
            "E86",
            "E36",
            "E39",
            "E52",
            "E53"
        };
        private static readonly HashSet<string> E65Ereihe = new HashSet<string>
        {
            "E65",
            "E66",
            "E67",
            "E68"
        };
        private static readonly Dictionary<string, string> ProduktlinieToSgbd = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "PL2",
                "E89X"
            },
            {
                "PL3",
                "R56"
            },
            {
                "PL3-ALT",
                "ZCS_ALL"
            },
            {
                "PL4",
                "E70"
            },
            {
                "PL5-ALT",
                "RR1"
            },
            {
                "PL6-ALT",
                "E60"
            }
        };
        private static readonly Dictionary<string, string> MotorradBaureihenverbundMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "K001",
                "X_K001"
            },
            {
                "KE01",
                "X_K001"
            },
            {
                "X001",
                "X_X001"
            },
            {
                "XS01",
                "X_X001"
            },
            {
                "KS01",
                "X_KS01"
            }
        };
        public DateTime DTimeRR_S2 => DateTime.ParseExact("01.06.2012", "dd.MM.yyyy", new CultureInfo("de-DE"));
        public DateTime DTimeF01Lci => DateTime.ParseExact("01.07.2013", "dd.MM.yyyy", new CultureInfo("de-DE"));
        public List<string> ProductLinesEpmBlacklist => new List<string>
        {
            "PL0",
            "PL2",
            "PL3",
            "PL3-ALT",
            "PL4",
            "PL5",
            "PL5-ALT",
            "PL6",
            "PL6-ALT",
            "PL7"
        };
        public DateTime DTimeF25Lci => DateTime.ParseExact("01.04.2014", "dd.MM.yyyy", new CultureInfo("de-DE"));
        public DateTime DTimeF01BN2020MostDomain => DateTime.ParseExact("30.06.2010", "dd.MM.yyyy", new CultureInfo("de-DE"));
        public DateTime DTime2022_07 => DateTime.ParseExact("01.07.2022", "dd.MM.yyyy", new CultureInfo("de-DE"));
        public DateTime DTime2023_03 => DateTime.ParseExact("01.03.2023", "dd.MM.yyyy", new CultureInfo("de-DE"));
        public DateTime DTime2023_07 => DateTime.ParseExact("01.07.2023", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public BordnetType GetBordnetType(string baureihenverbund, string prodart, string ereihe, ILogger logger)
        {
            string text = baureihenverbund?.ToUpperInvariant();
            if (string.Equals(prodart, "P", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (IBusPkw.Contains(text))
                    {
                        return BordnetType.IBUS;
                    }

                    if (BN2000Pkw.Contains(text))
                    {
                        return BordnetType.BN2000;
                    }

                    if (BEV2010Pkw.Contains(text))
                    {
                        return BordnetType.BEV2010;
                    }

                    BN2020Pkw.Contains(text);
                    return BordnetType.BN2020;
                }

                logger.Warning(logger.CurrentMethod(), "Baureihenverbund is null or empty. BordnetType will be determined by Ereihe!");
                if (IBusEreihe.Contains(ereihe))
                {
                    return BordnetType.IBUS;
                }

                logger.Warning(logger.CurrentMethod(), "Ereihe is null or empty. No BordnetType can be determined!");
                return BordnetType.UNKNOWN;
            }

            if (string.Equals(prodart, "M", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (BN2000Bike.Contains(text))
                    {
                        return BordnetType.BN2000_MOTORBIKE;
                    }

                    if (BN2020Bike.Contains(text))
                    {
                        return BordnetType.BN2020_MOTORBIKE;
                    }

                    if (BNK01XBike.Contains(text))
                    {
                        return BordnetType.BNK01X_MOTORBIKE;
                    }

                    return BordnetType.BN2020_MOTORBIKE;
                }

                logger.Info(logger.CurrentMethod(), "Baureihenverbund was empty, returning default value.");
                return BordnetType.BN2020_MOTORBIKE;
            }

            logger.Info(logger.CurrentMethod(), "Returning BordnetType.UNKNOWN for Prodart: " + prodart);
            return BordnetType.UNKNOWN;
        }

        public string GetMainSeriesSgbd(IIdentVehicle vecInfo)
        {
            return GetMainSeriesSgbd(((IReactorVehicle)vecInfo).Prodart, vecInfo.BordnetType, ((IReactorVehicle)vecInfo).Produktlinie, ((IReactorVehicle)vecInfo).Ereihe, ((IReactorVehicle)vecInfo).Baureihenverbund);
        }

        public string GetMainSeriesSgbd(string prodArt, BordnetType bordnetType, string produktLinie, string ereihe, string baureihenverbund)
        {
            switch (bordnetType)
            {
                case BordnetType.BEV2010:
                    return "E89X";
                case BordnetType.IBUS:
                    return "-";
                default:
                    if (string.Equals(prodArt, "P", StringComparison.OrdinalIgnoreCase))
                    {
                        return GetMainSeriesSgbdPkw(produktLinie, ereihe);
                    }

                    if (string.Equals(prodArt, "M", StringComparison.OrdinalIgnoreCase))
                    {
                        return GetMainSeriesSgbdMotorrad(bordnetType, baureihenverbund);
                    }

                    return "";
            }
        }

        private string GetMainSeriesSgbdMotorrad(BordnetType bordnetType, string baureihenverbund)
        {
            switch (bordnetType)
            {
                case BordnetType.BN2000_MOTORBIKE:
                case BordnetType.BNK01X_MOTORBIKE:
                    return "MRK24";
                case BordnetType.BN2020_MOTORBIKE:
                {
                    if (MotorradBaureihenverbundMap.TryGetValue(baureihenverbund, out var value))
                    {
                        return value;
                    }

                    return "X_X001";
                }

                default:
                    return "-";
            }
        }

        private string GetMainSeriesSgbdPkw(string produktLinie, string ereihe)
        {
            string text = produktLinie?.ToUpperInvariant();
            if (string.IsNullOrEmpty(text))
            {
                return "-";
            }

            if (text == "PL0")
            {
                if (ZcsAllEreihe.Contains(ereihe))
                {
                    return "ZCS_ALL";
                }

                if (E65Ereihe.Contains(ereihe))
                {
                    return "E65";
                }

                return "-";
            }

            if (ProduktlinieToSgbd.TryGetValue(text, out var value))
            {
                return value;
            }

            return "F01";
        }

        public string GetMainSeriesSgbdAdditional(IIdentVehicle vecInfo, ILogger logger)
        {
            logger.Info(logger.CurrentMethod(), "Entering GetMainSeriesSgbdAdditional");
            if (((IReactorVehicle)vecInfo).Prodart == "P")
            {
                string text = ((IReactorVehicle)vecInfo).Produktlinie?.ToUpperInvariant();
                if (!string.IsNullOrEmpty(text))
                {
                    if (text == "PL5-ALT")
                    {
                        if (!vecInfo.C_DATETIME.HasValue)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + text + ", C_DATETIME is null");
                            return "RR1_2020";
                        }

                        if (vecInfo.C_DATETIME >= DTimeRR_S2)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + text + ", C_DATETIME is later than DTimeRR_S2");
                            return "RR1_2020";
                        }

                        return null;
                    }

                    if (text == "PL6")
                    {
                        if (!vecInfo.C_DATETIME.HasValue)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + text + ", C_DATETIME is null");
                            if (FXXEreihe.Contains(((IReactorVehicle)vecInfo).Ereihe))
                            {
                                logger.Info(logger.CurrentMethod(), "Ereihe: " + ((IReactorVehicle)vecInfo).Ereihe + ", returning F01BN2K");
                                return "F01BN2K";
                            }
                        }
                        else if (vecInfo.C_DATETIME < DTimeF01Lci)
                        {
                            logger.Info(logger.CurrentMethod(), "Product line: " + text + ", C_DATETIME is earlier than DTimeF01Lci");
                            return "F01BN2K";
                        }

                        return null;
                    }

                    logger.Info(logger.CurrentMethod(), "Reached default block, produck line: " + text);
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