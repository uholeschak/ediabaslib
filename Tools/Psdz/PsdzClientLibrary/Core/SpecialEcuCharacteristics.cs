using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    internal class F01EcuCharacteristics : BaseEcuCharacteristics
    {
        public F01EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class F25_1404EcuCharacteristics : BaseEcuCharacteristics
    {
        public F25_1404EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

        public override bool HasBus(BusType busType, Vehicle vecInfo, ECU ecu)
        {
            if (ecu != null && ecu.SVK != null && ecu.SVK.XWE_SGBMID != null && xgbdTable != null && ecu.ID_SG_ADR == 96 && busType == BusType.MOST)
            {
                foreach (string item in ecu.SVK.XWE_SGBMID)
                {
                    foreach (IXGBMBusLogisticsEntry item2 in xgbdTable)
                    {
                        if (item.StartsWith(item2.XgbmPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            if (item2.Bus.Contains(busType))
                            {
                                return true;
                            }

                            return false;
                        }
                    }
                }
            }

            return base.HasBus(busType, vecInfo, ecu);
        }
    }

    internal class F25EcuCharacteristics : BaseEcuCharacteristics
    {
        public F25EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

        public override bool HasBus(BusType busType, Vehicle vecInfo, ECU ecu)
        {
            if (ecu != null && ecu.SVK != null && ecu.SVK.XWE_SGBMID != null && xgbdTable != null && ecu.ID_SG_ADR == 96 && busType == BusType.MOST)
            {
                foreach (string item in ecu.SVK.XWE_SGBMID)
                {
                    foreach (IXGBMBusLogisticsEntry item2 in xgbdTable)
                    {
                        if (item.StartsWith(item2.XgbmPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            if (item2.Bus.Contains(busType))
                            {
                                return true;
                            }

                            return false;
                        }
                    }
                }
            }

            return base.HasBus(busType, vecInfo, ecu);
        }
    }

    internal class RR6EcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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
                    if (item.ECU_GROBNAME.Equals("EMA_LI"))
                    {
                        hashSet.AddIfNotContains(78);
                        break;
                    }

                    if (item.ECU_GROBNAME.Equals("EMA_RE"))
                    {
                        hashSet.AddIfNotContains(77);
                        break;
                    }
                }
            }

            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    internal class R55EcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class RR2EcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class RREcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class BNT_G11_G12_G3X_SP2015 : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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
                    if (item.ECU_GROBNAME.Equals("EMA_LI"))
                    {
                        hashSet.AddIfNotContains(78);
                        break;
                    }

                    if (item.ECU_GROBNAME.Equals("EMA_RE"))
                    {
                        hashSet.AddIfNotContains(77);
                        break;
                    }
                }
            }

            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    internal class MRXEcuCharacteristics : BaseEcuCharacteristics
    {
        public MRXEcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }

            if (vecInfo.ECU == null)
            {
                vecInfo.ECU = new ObservableCollection<ECU>();
            }

            HashSet<int> hashSet = new HashSet<int>();
            if (vecInfo.HasSA("369"))
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

    internal class MREcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfigurationConfigured()", "vecInfo was null");
                return;
            }

            HashSet<int> sgList = new HashSet<int>();
            HashSet<int> hashSet = new HashSet<int>();
            if ("A67".Equals(vecInfo.Ereihe) || "V98".Equals(vecInfo.Ereihe))
            {
                hashSet.Add(96);
            }

            CalculateECUConfiguration(vecInfo, ffmResolver, sgList, hashSet);
        }
    }

    internal class E70EcuCharacteristicsAMPT : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class E70EcuCharacteristicsAMPH : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class E70EcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class E60EcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class F15EcuCharacteristics : BaseEcuCharacteristics
    {
        public F15EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class F01_1307EcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class BNT_G01_G02_G08_F97_F98_SP2015 : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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
                    if (item.ECU_GROBNAME.Equals("EMA_LI"))
                    {
                        hashSet.AddIfNotContains(78);
                        break;
                    }

                    if (item.ECU_GROBNAME.Equals("EMA_RE"))
                    {
                        hashSet.AddIfNotContains(77);
                        break;
                    }
                }
            }

            CalculateECUConfiguration(vecInfo, ffmResolver, hashSet, null);
        }
    }

    internal class E89EcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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

    internal class F56EcuCharacteristics : BaseEcuCharacteristics
    {
    }

    internal class F20EcuCharacteristics : BaseEcuCharacteristics
    {
        public override void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
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