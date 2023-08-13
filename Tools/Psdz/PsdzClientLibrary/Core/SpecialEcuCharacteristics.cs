using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public class E46EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            if (vecInfo.ECU == null)
            {
                vecInfo.ECU = new ObservableCollection<ECU>();
            }
            HashSet<int> hashSet = new HashSet<int>();
            HashSet<int> hashSet2 = new HashSet<int>();
            switch (vecInfo.Motor)
            {
                case "M73":
                    hashSet.Add(16);
                    hashSet.Add(20);
                    hashSet.Add(34);
                    break;
                case "M67":
                    hashSet.Add(18);
                    hashSet.Add(19);
                    break;
                case "M57":
                    hashSet.Add(18);
                    break;
                default:
                    hashSet.Add(18);
                    break;
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(50);
            }
            if (vecInfo.hasSA("534"))
            {
                hashSet.Add(91);
            }
            if (vecInfo.hasSA("459"))
            {
                hashSet.Add(114);
            }
            if (vecInfo.hasSA("256"))
            {
                hashSet.Add(80);
            }
            if (vecInfo.hasSA("508"))
            {
                hashSet.Add(96);
            }
            if (vecInfo.hasSA("403"))
            {
                hashSet.Add(8);
            }
            if (vecInfo.hasSA("602"))
            {
                hashSet.Add(240);
            }
            if (vecInfo.hasSA("265"))
            {
                hashSet.Add(112);
            }
            if (vecInfo.hasSA("0549"))
            {
                hashSet.Add(40);
            }
            if (vecInfo.hasSA("0540"))
            {
                hashSet.Add(166);
            }
            if (vecInfo.IsABSVehicle() == false)
            {
                if (vecInfo.getECUbyECU_SGBD("DSC_MK60") != null && !vecInfo.hasSA("210"))
                {
                    //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "found DSC_MK60 used for ABS only");
                }
                else
                {
                    hashSet.Add(87);
                }
            }
            if ("M54".Equals(vecInfo.Motor, StringComparison.OrdinalIgnoreCase))
            {
                hashSet2.Add(101);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, hashSet2);
        }
    }

    public class E36EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfigurationConfigured()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            switch (vecInfo.Motor)
            {
                case "M73":
                    hashSet.Add(16);
                    hashSet.Add(20);
                    hashSet.Add(34);
                    break;
                case "M67":
                    hashSet.Add(18);
                    hashSet.Add(19);
                    break;
                case "M44":
                    hashSet.Add(18);
                    break;
                case "M43":
                case "M52":
                    hashSet.Add(18);
                    break;
                default:
                    hashSet.Add(18);
                    break;
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(50);
            }
            if (vecInfo.hasSA("534"))
            {
                hashSet.Add(91);
            }
            if (vecInfo.hasSA("875"))
            {
                hashSet.Add(185);
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(50);
            }
            if (vecInfo.hasSA("214"))
            {
                hashSet.Add(86);
                hashSet.Add(54);
            }
            if (vecInfo.hasSA("508"))
            {
                hashSet.Add(96);
            }
            if (vecInfo.hasSA("550"))
            {
                hashSet.Add(205);
            }
            if (vecInfo.hasSA("243"))
            {
                hashSet.Add(164);
            }
            if (vecInfo.hasSA("403"))
            {
                hashSet.Add(21);
            }
            if (vecInfo.hasSA("602"))
            {
                hashSet.Add(240);
            }
            if (vecInfo.hasSA("265"))
            {
                hashSet.Add(112);
            }
            if (vecInfo.hasSA("549"))
            {
                hashSet.Add(40);
            }
            if (vecInfo.hasSA("540"))
            {
                hashSet.Add(166);
            }
            if (vecInfo.hasSA("305"))
            {
                hashSet.Add(185);
            }
            if (IsGroupValid("d_0016", vecInfo, ffmResolver))
            {
                hashSet.Add(22);
            }
            if (vecInfo.IsABSVehicle() == false)
            {
                hashSet.Add(87);
            }
            if ("CAB".Equals(vecInfo.Karosserie, StringComparison.OrdinalIgnoreCase))
            {
                hashSet.Add(155);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class E39EcuCharacteristics : BaseEcuCharacteristics
    {
        private static readonly DateTime LciGr2 = DateTime.Parse("1998-09-01", new CultureInfo("en-US"));

        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfigurationConfigured()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            HashSet<int> hashSet2 = new HashSet<int>();
            switch (vecInfo.Motor)
            {
                case "M73":
                    hashSet.Add(16);
                    hashSet.Add(20);
                    hashSet.Add(34);
                    break;
                case "M67":
                    hashSet.Add(18);
                    hashSet.Add(19);
                    break;
                default:
                    hashSet.Add(18);
                    break;
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(50);
            }
            if (vecInfo.hasSA("508"))
            {
                hashSet.Add(96);
            }
            if (vecInfo.hasSA("602"))
            {
                hashSet.Add(240);
            }
            if (vecInfo.hasSA("265"))
            {
                hashSet.Add(112);
            }
            if (vecInfo.hasSA("549"))
            {
                hashSet.Add(40);
            }
            if (vecInfo.hasSA("540"))
            {
                DateTime? c_DATETIME = vecInfo.C_DATETIME;
                DateTime lciGr = LciGr2;
                if (c_DATETIME.HasValue && c_DATETIME.GetValueOrDefault() < lciGr && !"M51".Equals(vecInfo.Motor))
                {
                    hashSet.Add(166);
                }
                else
                {
                    c_DATETIME = vecInfo.C_DATETIME;
                    lciGr = LciGr2;
                    if (c_DATETIME.HasValue && c_DATETIME.GetValueOrDefault() >= lciGr && vecInfo.getECUbyECU_SGBD("ME72KWP0") != null && vecInfo.getECUbyECU_SGBD("ME72KWP1") != null && vecInfo.getECUbyECU_SGBD("MSS52DS0") != null && vecInfo.getECUbyECU_SGBD("MSS52DS1") != null && vecInfo.getECUbyECU_SGBD("MSS54DS0") != null)
                    {
                        hashSet.Add(166);
                    }
                    else
                    {
                        c_DATETIME = vecInfo.C_DATETIME;
                        lciGr = LciGr2;
                        if (c_DATETIME.HasValue && c_DATETIME.GetValueOrDefault() >= lciGr && ("M62".Equals(vecInfo.Motor) || "S52".Equals(vecInfo.Motor) || "S62".Equals(vecInfo.Motor) || "S54".Equals(vecInfo.Motor)))
                        {
                            hashSet.Add(166);
                        }
                    }
                }
            }
            if (vecInfo.hasSA("536"))
            {
                hashSet.Add(107);
            }
            if (vecInfo.IsABSVehicle() == false)
            {
                hashSet.Add(87);
            }
            if ("M54".Equals(vecInfo.Motor, StringComparison.OrdinalIgnoreCase))
            {
                hashSet2.Add(101);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, hashSet2);
        }
    }

    public class E38EcuCharacteristics : BaseEcuCharacteristics
    {
        private static readonly DateTime ShdDate = DateTime.ParseExact("01.09.1996", "dd.MM.yyyy", new CultureInfo("de-DE"));

        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfigurationConfigured()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            switch (vecInfo.Motor)
            {
                case "M73":
                    hashSet.Add(16);
                    hashSet.Add(18);
                    hashSet.Add(19);
                    hashSet.Add(20);
                    hashSet.Add(34);
                    break;
                case "M67":
                    hashSet.Add(18);
                    hashSet.Add(19);
                    break;
                default:
                    hashSet.Add(18);
                    break;
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(50);
            }
            if (vecInfo.hasSA("508"))
            {
                hashSet.Add(96);
            }
            if (vecInfo.hasSA("403") && (!vecInfo.C_DATETIME.HasValue || vecInfo.C_DATETIME >= ShdDate))
            {
                hashSet.Add(8);
            }
            if (vecInfo.hasSA("602"))
            {
                hashSet.Add(240);
            }
            if (vecInfo.hasSA("265"))
            {
                hashSet.Add(112);
            }
            if (vecInfo.hasSA("549"))
            {
                hashSet.Add(40);
            }
            if (vecInfo.hasSA("540"))
            {
                hashSet.Add(166);
            }
            if (IsGroupValid("d_b8_d0", vecInfo, ffmResolver))
            {
                hashSet.Add(184);
            }
            if (IsGroupValid("d_0030", vecInfo, ffmResolver))
            {
                hashSet.Add(48);
            }
            if (vecInfo.IsABSVehicle() == false)
            {
                hashSet.Add(87);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class E52EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            switch (vecInfo.Motor)
            {
                case "M73":
                    hashSet.Add(16);
                    hashSet.Add(20);
                    hashSet.Add(34);
                    break;
                case "M67":
                    hashSet.Add(18);
                    hashSet.Add(19);
                    break;
                case "M57":
                    hashSet.Add(18);
                    break;
                default:
                    hashSet.Add(18);
                    break;
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(50);
            }
            if (vecInfo.hasSA("534"))
            {
                hashSet.Add(91);
            }
            if (vecInfo.hasSA("459"))
            {
                hashSet.Add(114);
            }
            if (vecInfo.hasSA("256"))
            {
                hashSet.Add(80);
            }
            if (vecInfo.hasSA("508"))
            {
                hashSet.Add(96);
            }
            if (vecInfo.hasSA("403"))
            {
                hashSet.Add(8);
            }
            if (vecInfo.hasSA("602"))
            {
                hashSet.Add(240);
            }
            if (vecInfo.hasSA("265"))
            {
                hashSet.Add(112);
            }
            if (vecInfo.hasSA("0549"))
            {
                hashSet.Add(40);
            }
            if (vecInfo.hasSA("0540"))
            {
                hashSet.Add(166);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class E53EcuCharacteristics : BaseEcuCharacteristics
    {
        private static readonly DateTime DateTimeE53VTG = DateTime.ParseExact("01.09.2003", "dd.MM.yyyy", new CultureInfo("de-DE"));

        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            switch (vecInfo.Motor)
            {
                case "M73":
                    hashSet.Add(16);
                    hashSet.Add(20);
                    hashSet.Add(34);
                    break;
                case "M67":
                    hashSet.Add(18);
                    hashSet.Add(19);
                    break;
                case "M57":
                    hashSet.Add(18);
                    break;
                default:
                    hashSet.Add(18);
                    break;
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(50);
            }
            if (vecInfo.ProductionDate > DateTimeE53VTG || !vecInfo.C_DATETIME.HasValue || vecInfo.C_DATETIME > DateTimeE53VTG)
            {
                hashSet.Add(52);
            }
            if (vecInfo.hasSA("220") || vecInfo.hasSA("221"))
            {
                hashSet.Add(172);
            }
            if (vecInfo.hasSA("534"))
            {
                hashSet.Add(91);
            }
            if (vecInfo.hasSA("249"))
            {
                hashSet.Add(80);
            }
            if (vecInfo.hasSA("459"))
            {
                hashSet.Add(114);
            }
            if (vecInfo.hasSA("256"))
            {
                hashSet.Add(80);
            }
            if (vecInfo.hasSA("508"))
            {
                hashSet.Add(96);
            }
            if (vecInfo.hasSA("403"))
            {
                hashSet.Add(8);
            }
            if (vecInfo.hasSA("602"))
            {
                hashSet.Add(240);
            }
            if (vecInfo.hasSA("265"))
            {
                hashSet.Add(112);
            }
            if (vecInfo.hasSA("549"))
            {
                hashSet.Add(40);
            }
            if (vecInfo.hasSA("540"))
            {
                hashSet.Add(166);
            }
            if (vecInfo.hasSA("220"))
            {
                hashSet.Add(172);
            }
            if (vecInfo.hasSA("672"))
            {
                hashSet.Add(118);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class F01EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class F25_1404EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class F25EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class R50EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if ("W17".Equals(vecInfo.Motor, StringComparison.OrdinalIgnoreCase))
            {
                hashSet.Add(18);
            }
            else
            {
                hashSet.Add(18);
                hashSet.Add(19);
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(50);
            }
            if (vecInfo.Ereihe == "R52")
            {
                hashSet.Add(156);
            }
            else if (vecInfo.hasSA("403"))
            {
                hashSet.Add(8);
            }
            if (vecInfo.hasSA("508"))
            {
                hashSet.Add(96);
            }
            if (vecInfo.hasSA("602"))
            {
                hashSet.Add(240);
            }
            // [UH] Fix: replaced BaustandsJahr by Modelljahr and BaustandsMonat by Modellmonat
            if (vecInfo.hasSA("265") && ((vecInfo.Ereihe == "R50" && "W10".Equals(vecInfo.Motor, StringComparison.OrdinalIgnoreCase) && vecInfo.Modelljahr == "2001" && int.Parse(vecInfo.Modellmonat) >= 3 && int.Parse(vecInfo.Modellmonat) >= 12) || (vecInfo.Ereihe == "R52" && "W10".Equals(vecInfo.Motor, StringComparison.OrdinalIgnoreCase) && (int.Parse(vecInfo.Modelljahr) >= 2008 || (vecInfo.Modelljahr == "2007" && int.Parse(vecInfo.Modellmonat) >= 9))) || (vecInfo.Ereihe == "R53" && "W11".Equals(vecInfo.Motor, StringComparison.OrdinalIgnoreCase) && vecInfo.Modelljahr == "2001" && int.Parse(vecInfo.Modellmonat) == 9)))
            {
                hashSet.Add(112);
            }
            if (vecInfo.hasSA("547"))
            {
                hashSet.Add(129);
            }
            if (vecInfo.hasSA("521") || vecInfo.hasSA("432"))
            {
                hashSet.Add(232);
            }
            if (vecInfo.hasSA("249"))
            {
                hashSet.Add(80);
            }
            if (vecInfo.IsABSVehicle() == false)
            {
                hashSet.Add(87);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class RR6EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }
            ObservableCollection<ECU> eCU = vecInfo.ECU;
            if (eCU != null)
            {
                foreach (ECU item in eCU)
                {
                    if (!item.ECU_GROBNAME.Equals("EMA_LI"))
                    {
                        if (item.ECU_GROBNAME.Equals("EMA_RE"))
                        {
                            hashSet.AddIfNotContains(77);
                            break;
                        }
                        continue;
                    }
                    hashSet.AddIfNotContains(78);
                    break;
                }
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class R55EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (vecInfo != null && "R57".Equals(vecInfo.Ereihe))
            {
                hashSet.Add(36);
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class RR2EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if ("RR2".Equals(vecInfo.Ereihe))
            {
                hashSet.Add(36);
                hashSet.Add(158);
            }
            if ("AUT".Equals(vecInfo.Getriebe))
            {
                hashSet.Add(24);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class RREcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if ("RR2".Equals(vecInfo.Ereihe))
            {
                hashSet.Add(36);
                hashSet.Add(158);
            }
            if ("AUT".Equals(vecInfo.Getriebe))
            {
                hashSet.Add(24);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class BNT_G11_G12_G3X_SP2015 : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }
            ObservableCollection<ECU> eCU = vecInfo.ECU;
            if (eCU != null)
            {
                foreach (ECU item in eCU)
                {
                    if (!item.ECU_GROBNAME.Equals("EMA_LI"))
                    {
                        if (item.ECU_GROBNAME.Equals("EMA_RE"))
                        {
                            hashSet.AddIfNotContains(77);
                            break;
                        }
                        continue;
                    }
                    hashSet.AddIfNotContains(78);
                    break;
                }
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class MRXEcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            if (vecInfo.ECU == null)
            {
                vecInfo.ECU = new ObservableCollection<ECU>();
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (vecInfo.hasSA("369"))
            {
                hashSet.Add(92);
            }
            if (vecInfo.Ereihe != "K60")
            {
                hashSet.Add(96);
            }
            brSgbd = ((vecInfo.Ereihe == "K60" || vecInfo.Ereihe == "K02" || vecInfo.Ereihe == "K03" || vecInfo.Ereihe == "K08" || vecInfo.Ereihe == "K09") ? "X_KS01" : "X_K001");
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class MREcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfigurationConfigured()", "vecInfo was null");
                return;
            }
            HashSet<int> sgList = new HashSet<int>();
            HashSet<int> hashSet = new HashSet<int>();
            if (vecInfo.BrandName == BrandName.ROSENBAUER || "A67".Equals(vecInfo.Ereihe) || "V98".Equals(vecInfo.Ereihe))
            {
                hashSet.Add(96);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, sgList, hashSet);
        }
    }

    public class E70EcuCharacteristicsAMPT : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class E70EcuCharacteristicsAMPH : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class E70EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class E60EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if ("E64".Equals(vecInfo.Ereihe))
            {
                hashSet.Add(36);
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class E83EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            HashSet<int> hashSet2 = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(50);
                hashSet.Add(24);
            }
            if (vecInfo.hasSA("508"))
            {
                hashSet.Add(96);
            }
            if (vecInfo.hasSA("403"))
            {
                hashSet.Add(21);
            }
            if (vecInfo.hasSA("602"))
            {
                hashSet.Add(240);
            }
            if (vecInfo.hasSA("265"))
            {
                hashSet.Add(112);
            }
            if (vecInfo.hasSA("549"))
            {
                hashSet.Add(40);
            }
            if (vecInfo.hasSA("540"))
            {
                hashSet.Add(166);
            }
            if (vecInfo.hasSA("536"))
            {
                hashSet.Add(107);
            }
            if (vecInfo.hasSA("609"))
            {
                hashSet.Add(127);
                hashSet.Add(70);
            }
            if (vecInfo.hasSA("644"))
            {
                hashSet.Add(200);
            }
            if ("M54".Equals(vecInfo.Motor, StringComparison.OrdinalIgnoreCase))
            {
                hashSet2.Add(101);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, hashSet2);
        }
    }

    public class E85EcuCharacteristics : BaseEcuCharacteristics
    {
        private readonly DateTime lciDate = DateTime.Parse("01.01.2006", new CultureInfo("de-DE"));

        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            HashSet<int> hashSet2 = new HashSet<int>();
            if (!vecInfo.C_DATETIME.HasValue || vecInfo.C_DATETIME < lciDate)
            {
                if (vecInfo.BaseVersion == "US")
                {
                    hashSet.Add(173);
                    hashSet.Add(174);
                }
                hashSet.Add(161);
                hashSet.Add(162);
            }
            switch (vecInfo.Motor)
            {
                case "M73":
                    hashSet.Add(16);
                    hashSet.Add(20);
                    hashSet.Add(34);
                    break;
                case "M67":
                    hashSet.Add(18);
                    hashSet.Add(19);
                    break;
                case "M57":
                    hashSet.Add(18);
                    break;
                default:
                    hashSet.Add(18);
                    break;
            }
            if (vecInfo.C_DATETIME.HasValue)
            {
                DateTime? c_DATETIME = vecInfo.C_DATETIME;
                DateTime minValue = DateTime.MinValue;
                if (c_DATETIME.HasValue && c_DATETIME.GetValueOrDefault() > minValue && vecInfo.C_DATETIME < lciDate)
                {
                    hashSet.Add(161);
                    hashSet.Add(162);
                }
            }
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(50);
                hashSet.Add(24);
            }
            if (vecInfo.hasSA("534"))
            {
                hashSet.Add(91);
            }
            if (vecInfo.hasSA("459"))
            {
                hashSet.Add(114);
            }
            if (vecInfo.hasSA("256"))
            {
                hashSet.Add(80);
            }
            if (vecInfo.hasSA("508"))
            {
                hashSet.Add(96);
            }
            if (vecInfo.hasSA("403"))
            {
                hashSet.Add(8);
            }
            if (vecInfo.hasSA("602"))
            {
                hashSet.Add(240);
            }
            if (vecInfo.hasSA("265"))
            {
                hashSet.Add(112);
            }
            if (vecInfo.hasSA("549"))
            {
                hashSet.Add(40);
            }
            if (vecInfo.hasSA("540") && IsGroupValid("d_00a6", vecInfo, ffmResolver))
            {
                hashSet.Add(166);
            }
            if (IsGroupValid("d_0074", vecInfo, ffmResolver))
            {
                hashSet.Add(116);
            }
            if (vecInfo.hasSA("399"))
            {
                hashSet.Add(156);
            }
            if ("M54".Equals(vecInfo.Motor, StringComparison.OrdinalIgnoreCase))
            {
                hashSet2.Add(101);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, hashSet2);
        }
    }

    public class F15EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class F01_1307EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class BNT_G01_G02_G08_F97_F98_SP2015 : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }
            ObservableCollection<ECU> eCU = vecInfo.ECU;
            if (eCU != null)
            {
                foreach (ECU item in eCU)
                {
                    if (!item.ECU_GROBNAME.Equals("EMA_LI"))
                    {
                        if (item.ECU_GROBNAME.Equals("EMA_RE"))
                        {
                            hashSet.AddIfNotContains(77);
                            break;
                        }
                        continue;
                    }
                    hashSet.AddIfNotContains(78);
                    break;
                }
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class E89EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    public class F56EcuCharacteristics : BaseEcuCharacteristics
    {
        // No CalculateECUConfiguration
    }

    public class F20EcuCharacteristics : BaseEcuCharacteristics
    {
        // ToDo: Check on update
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                //Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            HashSet<int> hashSet = new HashSet<int>();
            HashSet<int> removeList = new HashSet<int>();
            if (GearboxUtility.HasVehicleGearboxECU(vecInfo))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }
            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, removeList);
        }
    }
}
