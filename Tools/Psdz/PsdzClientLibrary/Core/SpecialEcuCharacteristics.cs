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

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class F25_1404EcuCharacteristics : BaseEcuCharacteristics
    {
        public F25_1404EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }

        public override bool HasBus(BusType busType, IEcuTreeVehicle vecInfo, IEcuTreeEcu ecu)
        {
            if (ecu != null && ecu.Svk != null && ecu.Svk.XWE_SGBMID != null && xgbdTable != null)
            {
                long iD_SG_ADR = ecu.ID_SG_ADR;
                long num = iD_SG_ADR;
                if (num == 96 && busType == BusType.MOST)
                {
                    foreach (string item in ecu.Svk.XWE_SGBMID)
                    {
                        foreach (IXGBMBusLogisticsEntry item2 in xgbdTable)
                        {
                            if (item.StartsWith(item2.XgbmPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                return item2.Bus.Contains(busType);
                            }
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

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }

        public override bool HasBus(BusType busType, IEcuTreeVehicle vecInfo, IEcuTreeEcu ecu)
        {
            if (ecu != null && ecu.Svk != null && ecu.Svk.XWE_SGBMID != null && xgbdTable != null)
            {
                long iD_SG_ADR = ecu.ID_SG_ADR;
                long num = iD_SG_ADR;
                if (num == 96 && busType == BusType.MOST)
                {
                    foreach (string item in ecu.Svk.XWE_SGBMID)
                    {
                        foreach (IXGBMBusLogisticsEntry item2 in xgbdTable)
                        {
                            if (item.StartsWith(item2.XgbmPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                return item2.Bus.Contains(busType);
                            }
                        }
                    }
                }
            }

            return base.HasBus(busType, vecInfo, ecu);
        }
    }

    internal class RR6EcuCharacteristics : BaseEcuCharacteristics
    {
        public RR6EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }

            IEnumerable<IEcuTreeEcu> eCU = vecInfo.ECU;
            if (eCU != null)
            {
                foreach (IEcuTreeEcu item in eCU)
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

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class R55EcuCharacteristics : BaseEcuCharacteristics
    {
        public R55EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (vecInfo != null && "R57".Equals(vecInfo.Ereihe))
            {
                hashSet.Add(36);
            }

            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }

        public override void ShapeECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            base.ShapeECUConfiguration(vecInfo);
            IEcuTreeEcu eCU = vecInfo.getECU(99L);
            if (eCU != null && "CHAMP2R".Equals(eCU.VARIANT))
            {
                IEcuTreeEcu eCU2 = vecInfo.getECU(84L);
                if (eCU2 != null && !eCU2.IDENT_SUCCESSFULLY)
                {
                    vecInfo.RemoveEcu(eCU2);
                }
            }
        }
    }

    internal class RR2EcuCharacteristics : BaseEcuCharacteristics
    {
        public RR2EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
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

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class RREcuCharacteristics : BaseEcuCharacteristics
    {
        public RREcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
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

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class BNT_G11_G12_G3X_SP2015 : BaseEcuCharacteristics
    {
        public BNT_G11_G12_G3X_SP2015(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }

            IEnumerable<IEcuTreeEcu> eCU = vecInfo.ECU;
            if (eCU != null)
            {
                foreach (IEcuTreeEcu item in eCU)
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

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class MRXEcuCharacteristics : BaseEcuCharacteristics
    {
        public MRXEcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
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
            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class MREcuCharacteristics : BaseEcuCharacteristics
    {
        public MREcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> sgList = new HashSet<int>();
            HashSet<int> hashSet = new HashSet<int>();
            if ("A67".Equals(vecInfo.Ereihe) || "V98".Equals(vecInfo.Ereihe))
            {
                hashSet.Add(96);
            }

            CalculateECUConfiguration(vecInfo, sgList, hashSet);
        }
    }

    internal class E70EcuCharacteristicsAMPT : BaseEcuCharacteristics
    {
        public E70EcuCharacteristicsAMPT(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class E70EcuCharacteristicsAMPH : BaseEcuCharacteristics
    {
        public E70EcuCharacteristicsAMPH(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class E70EcuCharacteristics : BaseEcuCharacteristics
    {
        public E70EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class E60EcuCharacteristics : BaseEcuCharacteristics
    {
        public E60EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if ("E64".Equals(vecInfo.Ereihe))
            {
                hashSet.Add(36);
            }

            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class F15EcuCharacteristics : BaseEcuCharacteristics
    {
        public F15EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class F01_1307EcuCharacteristics : BaseEcuCharacteristics
    {
        public F01_1307EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class BNT_G01_G02_G08_F97_F98_SP2015 : BaseEcuCharacteristics
    {
        public BNT_G01_G02_G08_F97_F98_SP2015(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }

            IEnumerable<IEcuTreeEcu> eCU = vecInfo.ECU;
            if (eCU != null)
            {
                foreach (IEcuTreeEcu item in eCU)
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

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class E89EcuCharacteristics : BaseEcuCharacteristics
    {
        public E89EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
            }

            CalculateECUConfiguration(vecInfo, hashSet, null);
        }
    }

    internal class F56EcuCharacteristics : BaseEcuCharacteristics
    {
        public F56EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override bool HasBus(BusType busType, IEcuTreeVehicle vecInfo, IEcuTreeEcu ecu)
        {
            if (ecu != null && ecu.Svk != null && ecu.Svk.XWE_SGBMID != null && xgbdTable != null)
            {
                long iD_SG_ADR = ecu.ID_SG_ADR;
                long num = iD_SG_ADR;
                if (num == 96 && busType == BusType.MOST)
                {
                    foreach (string item in ecu.Svk.XWE_SGBMID)
                    {
                        foreach (IXGBMBusLogisticsEntry item2 in xgbdTable)
                        {
                            if (item.StartsWith(item2.XgbmPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                return item2.Bus.Contains(busType);
                            }
                        }
                    }
                }
            }

            return base.HasBus(busType, vecInfo, ecu);
        }
    }

    internal class F20EcuCharacteristics : BaseEcuCharacteristics
    {
        public F20EcuCharacteristics(string xmlCharacteristicFileName) : base(xmlCharacteristicFileName)
        {
        }

        public override void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            HashSet<int> hashSet = new HashSet<int>();
            HashSet<int> removeList = new HashSet<int>();
            if (GearboxHelper.HasVehicleGearboxECU(vecInfo.Motor, vecInfo.Getriebe, vecInfo.HasSA))
            {
                hashSet.Add(24);
                hashSet.Add(94);
            }

            CalculateECUConfiguration(vecInfo, hashSet, removeList);
        }

        public override bool HasBus(BusType busType, IEcuTreeVehicle vecInfo, IEcuTreeEcu ecu)
        {
            if (ecu != null && ecu.Svk != null && ecu.Svk.XWE_SGBMID != null && xgbdTable != null)
            {
                long iD_SG_ADR = ecu.ID_SG_ADR;
                long num = iD_SG_ADR;
                if (num == 96 && busType == BusType.MOST)
                {
                    foreach (string item in ecu.Svk.XWE_SGBMID)
                    {
                        foreach (IXGBMBusLogisticsEntry item2 in xgbdTable)
                        {
                            if (item.StartsWith(item2.XgbmPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                return item2.Bus.Contains(busType);
                            }
                        }
                    }
                }
            }

            return base.HasBus(busType, vecInfo, ecu);
        }
    }
}