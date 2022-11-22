using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PsdzClient.Core
{
    public class E46EcuCharacteristics : BaseEcuCharacteristics { }

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

    public class E38EcuCharacteristics : BaseEcuCharacteristics { }
    public class E52EcuCharacteristics : BaseEcuCharacteristics { }

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

    public class F01EcuCharacteristics : BaseEcuCharacteristics { }
    public class F25_1404EcuCharacteristics : BaseEcuCharacteristics { }

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

    public class R50EcuCharacteristics : BaseEcuCharacteristics { }

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

    public class R55EcuCharacteristics : BaseEcuCharacteristics { }
    public class RR2EcuCharacteristics : BaseEcuCharacteristics { }
    public class RREcuCharacteristics : BaseEcuCharacteristics { }
    public class BNT_G11_G12_G3X_SP2015 : BaseEcuCharacteristics { }
    public class MRXEcuCharacteristics : BaseEcuCharacteristics { }
    public class MREcuCharacteristics : BaseEcuCharacteristics { }
    public class E70EcuCharacteristicsAMPT : BaseEcuCharacteristics { }
    public class E70EcuCharacteristicsAMPH : BaseEcuCharacteristics { }

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

    public class E60EcuCharacteristics : BaseEcuCharacteristics { }
    public class E83EcuCharacteristics : BaseEcuCharacteristics { }
    public class E85EcuCharacteristics : BaseEcuCharacteristics { }
    public class F15EcuCharacteristics : BaseEcuCharacteristics { }
    public class F01_1307EcuCharacteristics : BaseEcuCharacteristics { }
    public class BNT_G01_G02_G08_F97_F98_SP2015 : BaseEcuCharacteristics { }
    public class E89EcuCharacteristics : BaseEcuCharacteristics { }
    public class F56EcuCharacteristics : BaseEcuCharacteristics { }
    public class F20EcuCharacteristics : BaseEcuCharacteristics { }
}
