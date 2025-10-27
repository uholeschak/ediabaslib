using System.Linq;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuMapper
    {
        public static IPsdzEcu Map(EcuModel ecuModel)
        {
            if (ecuModel == null)
            {
                return null;
            }
            PsdzEcu psdzEcu = new PsdzEcu
            {
                BaseVariant = ecuModel.BaseVariant,
                BnTnName = ecuModel.BnTnName,
                BusConnections = ecuModel.BusConnections?.Select((BusNameModel bus) => new PsdzBus
                {
                    Id = bus.Id,
                    Name = bus.Name,
                    DirectAccess = bus.DirectAccess
                }).ToList(),
                DiagnosticBus = ((ecuModel.DiagnosticBus != null) ? new PsdzBus
                {
                    Id = ecuModel.DiagnosticBus.Id,
                    Name = ecuModel.DiagnosticBus.Name,
                    DirectAccess = ecuModel.DiagnosticBus.DirectAccess
                } : null),
                EcuDetailInfo = EcuDetailInfoMapper.Map(ecuModel.EcuDetailInfo),
                EcuStatusInfo = EcuStatusInfoMapper.Map(ecuModel.EcuStatusInfo),
                EcuVariant = ecuModel.EcuVariant,
                GatewayDiagAddr = DiagAddressMapper.Map(ecuModel.GatewayDiagAddr),
                PrimaryKey = EcuIdentifierMapper.Map(ecuModel.PrimaryKey),
                PsdzEcuPdxInfo = EcuPdxInfoMapper.Map(ecuModel.EcuPdxInfo),
                SerialNumber = ecuModel.SerialNumber,
                StandardSvk = StandardSvkMapper.Map(ecuModel.StandardSvk),
                IsSmartActuator = ecuModel.IsSmartActuator
            };
            if (ecuModel.IsSmartActuator && ecuModel is SmartActuatorEcuModel smartActuatorEcuModel)
            {
                return new PsdzSmartActuatorEcu(psdzEcu)
                {
                    SmacID = smartActuatorEcuModel.SmacID,
                    SmacMasterDiagAddress = DiagAddressMapper.Map(smartActuatorEcuModel.SmacMasterDiagAddress)
                };
            }
            EcuPdxInfoModel ecuPdxInfo = ecuModel.EcuPdxInfo;
            if (ecuPdxInfo != null && ecuPdxInfo.SmartActuatorMaster && ecuModel is SmartActuatorMasterEcuModel smartActuatorMasterEcuModel)
            {
                return new PsdzSmartActuatorMasterEcu(psdzEcu)
                {
                    SmacMasterSVK = StandardSvkMapper.Map(smartActuatorMasterEcuModel.SmacMasterSVK),
                    SmartActuatorEcus = smartActuatorMasterEcuModel.SmartActuatorEcus.Select((SmartActuatorEcuModel x) => Map(x))
                };
            }
            return psdzEcu;
        }

        public static EcuModel Map(IPsdzEcu ecu)
        {
            if (ecu == null)
            {
                return null;
            }
            EcuModel ecuModel = new EcuModel();
            if (ecu.IsSmartActuator && ecu is PsdzSmartActuatorEcu psdzSmartActuatorEcu)
            {
                ecuModel = new SmartActuatorEcuModel
                {
                    SmacID = psdzSmartActuatorEcu.SmacID,
                    SmacMasterDiagAddress = DiagAddressMapper.Map(psdzSmartActuatorEcu.SmacMasterDiagAddress)
                };
            }
            IPsdzEcuPdxInfo psdzEcuPdxInfo = ecu.PsdzEcuPdxInfo;
            if (psdzEcuPdxInfo != null && psdzEcuPdxInfo.IsSmartActuatorMaster && ecu is PsdzSmartActuatorMasterEcu psdzSmartActuatorMasterEcu)
            {
                ecuModel = new SmartActuatorMasterEcuModel
                {
                    SmacMasterSVK = StandardSvkMapper.Map(psdzSmartActuatorMasterEcu.SmacMasterSVK),
                    SmartActuatorEcus = (from x in psdzSmartActuatorMasterEcu.SmartActuatorEcus.Select((IPsdzEcu x) => Map(x)).OfType<SmartActuatorEcuModel>()
                                         select (x)).ToList()
                };
            }
            SetEcuValues(ecu, ecuModel);
            return ecuModel;
        }

        private static void SetEcuValues(IPsdzEcu ecu, EcuModel ecuModel)
        {
            ecuModel.BaseVariant = ecu.BaseVariant;
            ecuModel.BnTnName = ecu.BnTnName;
            ecuModel.BusConnections = ecu.BusConnections?.Select((PsdzBus bus) => new BusNameModel
            {
                Id = bus.Id,
                Name = bus.Name,
                DirectAccess = bus.DirectAccess
            }).ToList();
            ecuModel.DiagnosticBus = ((ecu.DiagnosticBus != null) ? new BusNameModel
            {
                Id = ecu.DiagnosticBus.Id,
                Name = ecu.DiagnosticBus.Name,
                DirectAccess = ecu.DiagnosticBus.DirectAccess
            } : null);
            ecuModel.EcuDetailInfo = EcuDetailInfoMapper.Map(ecu.EcuDetailInfo);
            ecuModel.EcuStatusInfo = EcuStatusInfoMapper.Map(ecu.EcuStatusInfo);
            ecuModel.EcuVariant = ecu.EcuVariant;
            ecuModel.GatewayDiagAddr = DiagAddressMapper.Map(ecu.GatewayDiagAddr);
            ecuModel.PrimaryKey = EcuIdentifierMapper.Map(ecu.PrimaryKey);
            ecuModel.EcuPdxInfo = EcuPdxInfoMapper.Map(ecu.PsdzEcuPdxInfo);
            ecuModel.SerialNumber = ecu.SerialNumber;
            ecuModel.StandardSvk = StandardSvkMapper.Map(ecu.StandardSvk);
            ecuModel.IsSmartActuator = ecu.IsSmartActuator;
        }
    }
}