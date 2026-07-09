using BMW.Rheingold.Programming.Common;

namespace PsdzClient.Core
{
    public class VoltageThreshold : IVoltageThreshold
    {
        public double MinError { get; private set; }
        public double MinWarning { get; private set; }
        public double MaxWarning { get; private set; }
        public double MaxError { get; private set; }
        public BatteryEnum BatteryType { get; private set; }

        public VoltageThreshold(BatteryEnum batteryType)
        {
            switch (batteryType)
            {
                case BatteryEnum.LFP:
                    BatteryType = BatteryEnum.LFP;
                    MinError = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Min.Error.Lfp", 10.25);
                    MinWarning = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Min.Warning.Lfp", 12.35);
                    MaxWarning = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Max.Warning.Lfp", 14.05);
                    MaxError = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Max.Error.Lfp", 14.45);
                    break;
                case BatteryEnum.LFP_NCAR:
                    BatteryType = BatteryEnum.LFP_NCAR;
                    MinError = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Min.Error.LfpNcar", 10.25);
                    MinWarning = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Min.Warning.LfpNcar", 12.35);
                    MaxWarning = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Max.Warning.LfpNcar", 14.85);
                    MaxError = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Max.Error.LfpNcar", 15.55);
                    break;
                case BatteryEnum.PbNew:
                    BatteryType = BatteryEnum.PbNew;
                    MinError = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Min.Error.PbNew", 9.95);
                    MinWarning = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Min.Warning.PbNew", 12.55);
                    MaxWarning = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Max.Warning.PbNew", 14.85);
                    MaxError = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Max.Error.PbNew", 16.05);
                    break;
                case BatteryEnum.Pb:
                    BatteryType = BatteryEnum.Pb;
                    MinError = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Min.Error.Pb", 9.95);
                    MinWarning = GetDefaultRheingoldCalmp30VoltageMinWarningPBBaseOnISTAMode();
                    MaxWarning = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Max.Warning.Pb", 14.85);
                    MaxError = ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Max.Error.Pb", 15.55);
                    break;
                default:
                    BatteryType = BatteryEnum.Unknown;
                    MinError = 9.9;
                    MinWarning = 12.3;
                    MaxWarning = 15.0;
                    MaxError = 16.1;
                    break;
            }
        }

        private double GetDefaultRheingoldCalmp30VoltageMinWarningPBBaseOnISTAMode()
        {
            double defaultValue = (ConfigSettings.IsLightModeActive ? 10.0 : 12.55);
            return ConfigSettings.GetConfigDouble("BMW.Rheingold.Clamp30.Voltage.Min.Warning.Pb", defaultValue);
        }
    }
}