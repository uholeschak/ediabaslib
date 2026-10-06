using BMW.Rheingold.CoreFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using BMW.Rheingold.Measurement.Common;
using BMW.Rheingold.MeasurementCommunication;
using PBMW.Rheingold.Measurement.Model;
using PsdzClient.Core;

#pragma warning disable CS0219
namespace BMW.Rheingold.Measurement.Model
{
    public static class Mapping
    {
        private static Dictionary<Probes, MeasuringFunction> mappingProbesToKindOfMeasurement = new Dictionary<Probes, MeasuringFunction>();

        private static Dictionary<MeasuringSensor, int> mappingMeasuringSensorToConnector = new Dictionary<MeasuringSensor, int>();

        private static Dictionary<MeasuringSensor, Probes> measuringSensorToProbes = new Dictionary<MeasuringSensor, Probes>();

        private static DataSet dsImibConfig;

        public static IDictionary<Probes, MeasuringFunction> ProbesToKindOfMeasurement
        {
            get
            {
                if (!mappingProbesToKindOfMeasurement.Any())
                {
                    mappingProbesToKindOfMeasurement.Add(Probes.U, MeasuringFunction.Voltage);
                    mappingProbesToKindOfMeasurement.Add(Probes.TD, MeasuringFunction.Voltage);
                    mappingProbesToKindOfMeasurement.Add(Probes.I, MeasuringFunction.Current);
                    mappingProbesToKindOfMeasurement.Add(Probes.I100, MeasuringFunction.Current);
                    mappingProbesToKindOfMeasurement.Add(Probes.I1800, MeasuringFunction.Current);
                    mappingProbesToKindOfMeasurement.Add(Probes.R, MeasuringFunction.Resistance);
                    mappingProbesToKindOfMeasurement.Add(Probes.D, MeasuringFunction.Diode);
                    mappingProbesToKindOfMeasurement.Add(Probes.P25, MeasuringFunction.Pressure);
                    mappingProbesToKindOfMeasurement.Add(Probes.P25KV, MeasuringFunction.Pressure);
                    mappingProbesToKindOfMeasurement.Add(Probes.P100, MeasuringFunction.Pressure);
                    mappingProbesToKindOfMeasurement.Add(Probes.P400, MeasuringFunction.Pressure);
                    mappingProbesToKindOfMeasurement.Add(Probes.LP, MeasuringFunction.Pressure);
                    mappingProbesToKindOfMeasurement.Add(Probes.KV, MeasuringFunction.Voltage);
                    mappingProbesToKindOfMeasurement.Add(Probes.TRIG, MeasuringFunction.Voltage);
                    mappingProbesToKindOfMeasurement.Add(Probes.TEMP, MeasuringFunction.Temperature);
                }
                return mappingProbesToKindOfMeasurement;
            }
        }

        public static IDictionary<MeasuringSensor, int> MeasuringSensorToConnector
        {
            get
            {
                if (!mappingMeasuringSensorToConnector.Any())
                {
                    mappingMeasuringSensorToConnector.Add(MeasuringSensor.Probes1, 1);
                    mappingMeasuringSensorToConnector.Add(MeasuringSensor.Probes2, 2);
                    mappingMeasuringSensorToConnector.Add(MeasuringSensor.TemperatureSensor1, 3);
                    mappingMeasuringSensorToConnector.Add(MeasuringSensor.TemperatureSensor2, 4);
                    mappingMeasuringSensorToConnector.Add(MeasuringSensor.LowPressureSensor1, 7);
                    mappingMeasuringSensorToConnector.Add(MeasuringSensor.LowPressureSensor2, 8);
                }
                return mappingMeasuringSensorToConnector;
            }
        }

        public static IDictionary<MeasuringSensor, Probes> MeasuringSensorToProbes
        {
            get
            {
                lock (measuringSensorToProbes)
                {
                    if (!measuringSensorToProbes.Any())
                    {
                        measuringSensorToProbes.Add(MeasuringSensor.Probes1, Probes.NONE);
                        measuringSensorToProbes.Add(MeasuringSensor.Probes2, Probes.NONE);
                        measuringSensorToProbes.Add(MeasuringSensor.TemperatureSensor1, Probes.TEMP);
                        measuringSensorToProbes.Add(MeasuringSensor.TemperatureSensor2, Probes.TEMP);
                        measuringSensorToProbes.Add(MeasuringSensor.LowPressureSensor1, Probes.LP);
                        measuringSensorToProbes.Add(MeasuringSensor.LowPressureSensor2, Probes.LP);
                        measuringSensorToProbes.Add(MeasuringSensor.Clamp50A, Probes.NONE);
                        measuringSensorToProbes.Add(MeasuringSensor.Clamp100A, Probes.I100);
                        measuringSensorToProbes.Add(MeasuringSensor.Clamp1800A, Probes.I1800);
                        measuringSensorToProbes.Add(MeasuringSensor.PressureSensor3_5, Probes.NONE);
                        measuringSensorToProbes.Add(MeasuringSensor.PressureSensor100, Probes.P100);
                        measuringSensorToProbes.Add(MeasuringSensor.PressureSensor400, Probes.P400);
                        measuringSensorToProbes.Add(MeasuringSensor.TDCable, Probes.TD);
                        measuringSensorToProbes.Add(MeasuringSensor.KVClip, Probes.KV);
                        measuringSensorToProbes.Add(MeasuringSensor.TriggerClamp, Probes.TRIG);
                    }
                    return measuringSensorToProbes;
                }
            }
        }

        private static DataSet DsImibConfig
        {
            get
            {
                if (dsImibConfig == null)
                {
                    dsImibConfig = new DataSet("IMIB Data");
                    CreateSensorFunctionTable(dsImibConfig);
                    CreateFunctionCouplingTable(dsImibConfig);
                    FillImibConfig(dsImibConfig);
                }
                return dsImibConfig;
            }
        }

        public static Probes MeasuringFunctionToProbes(int connector, MeasuringSensor sensor, MeasuringFunction measuringFunction)
        {
            Probes probes = MeasuringSensorToProbesOf(sensor);
            if (connector == 1 || connector == 2)
            {
                if (probes == Probes.NONE)
                {
                    switch (measuringFunction)
                    {
                        case MeasuringFunction.Voltage:
                            probes = Probes.U;
                            break;
                        case MeasuringFunction.Resistance:
                            probes = Probes.R;
                            break;
                        case MeasuringFunction.Current:
                            probes = Probes.I;
                            break;
                        case MeasuringFunction.Diode:
                            probes = Probes.D;
                            break;
                        default:
                            Log.Warning("Mapping.MeasuringFunctionToProbes", "For connector '{0}' function '{1}' doesn't exist a probe.", connector, measuringFunction.ToString());
                            break;
                    }
                }
                return probes;
            }
            return MeasuringSensorToProbesOf(sensor);
        }

        public static Probes MeasuringSensorToProbesOf(MeasuringSensor sensor)
        {
            Probes result;
            if (MeasuringSensorToProbes.ContainsKey(sensor))
            {
                result = MeasuringSensorToProbes[sensor];
            }
            else
            {
                result = Probes.NONE;
                Log.Error("Mapping.MeasuringSensorToProbes", "Sensor '{0}' is not available.", sensor.ToString());
            }
            return result;
        }

        public static MeasuringSensor ProbesToMeasuringSensor(Probes probe, int connector)
        {
            MeasuringSensor result = MeasuringSensor.None;
            switch (probe)
            {
                case Probes.U:
                    switch (connector)
                    {
                        case 1:
                            result = MeasuringSensor.Probes1;
                            break;
                        case 2:
                            result = MeasuringSensor.Probes2;
                            break;
                        default:
                            result = MeasuringSensor.None;
                            break;
                    }
                    break;
                case Probes.I:
                    switch (connector)
                    {
                        case 1:
                            result = MeasuringSensor.Probes1;
                            break;
                        case 2:
                            result = MeasuringSensor.Probes2;
                            break;
                        default:
                            result = MeasuringSensor.None;
                            break;
                    }
                    break;
                case Probes.R:
                    switch (connector)
                    {
                        case 1:
                            result = MeasuringSensor.Probes1;
                            break;
                        case 2:
                            result = MeasuringSensor.Probes2;
                            break;
                        default:
                            result = MeasuringSensor.None;
                            break;
                    }
                    break;
                case Probes.D:
                    switch (connector)
                    {
                        case 1:
                            result = MeasuringSensor.Probes1;
                            break;
                        case 2:
                            result = MeasuringSensor.Probes2;
                            break;
                        default:
                            result = MeasuringSensor.None;
                            break;
                    }
                    break;
                case Probes.I100:
                    result = (((uint)(connector - 1) <= 1u || (uint)(connector - 3) > 1u) ? MeasuringSensor.None : MeasuringSensor.Clamp100A);
                    break;
                case Probes.I1800:
                    result = (((uint)(connector - 1) <= 1u || (uint)(connector - 3) > 1u) ? MeasuringSensor.None : MeasuringSensor.Clamp1800A);
                    break;
                case Probes.P25:
                    result = (((uint)(connector - 1) <= 1u || (uint)(connector - 3) > 1u) ? MeasuringSensor.None : MeasuringSensor.PressureSensor25);
                    break;
                case Probes.P100:
                    result = (((uint)(connector - 1) <= 1u || (uint)(connector - 3) > 1u) ? MeasuringSensor.None : MeasuringSensor.PressureSensor100);
                    break;
                case Probes.P400:
                    result = (((uint)(connector - 1) <= 1u || (uint)(connector - 3) > 1u) ? MeasuringSensor.None : MeasuringSensor.PressureSensor400);
                    break;
                case Probes.KV:
                    result = (((uint)(connector - 1) <= 1u || (uint)(connector - 3) > 1u) ? MeasuringSensor.None : MeasuringSensor.KVClip);
                    break;
                case Probes.LP:
                    switch (connector)
                    {
                        case 7:
                            result = MeasuringSensor.LowPressureSensor1;
                            break;
                        case 8:
                            result = MeasuringSensor.LowPressureSensor2;
                            break;
                        default:
                            result = MeasuringSensor.None;
                            break;
                    }
                    break;
                case Probes.TD:
                    result = (((uint)(connector - 1) <= 1u || (uint)(connector - 3) > 1u) ? MeasuringSensor.None : MeasuringSensor.TDCable);
                    break;
                case Probes.TEMP:
                    switch (connector)
                    {
                        case 3:
                            result = MeasuringSensor.TemperatureSensor1;
                            break;
                        case 4:
                            result = MeasuringSensor.TemperatureSensor2;
                            break;
                        default:
                            result = MeasuringSensor.None;
                            break;
                    }
                    break;
                case Probes.TRIG:
                    result = (((uint)(connector - 1) <= 1u || (uint)(connector - 3) > 1u) ? MeasuringSensor.None : MeasuringSensor.TriggerClamp);
                    break;
                default:
                    Log.Warning("Mapping.ProbesToMeasuringSensor()", "Probe '{0}' is not supported.", probe.ToString());
                    break;
            }
            return result;
        }

        public static IEnumerable<MeasuringCoupling> NotAllowedCouplings(MeasuringFunction function)
        {
            DataTable dataTable = DsImibConfig.Tables["FunctionToCoupling"];
            string filterExpression = $"Function = {(int)function}";
            DataRow[] array = dataTable.Select(filterExpression);
            List<MeasuringCoupling> list = new List<MeasuringCoupling>();
            foreach (MeasuringCoupling value in Enum.GetValues(typeof(MeasuringCoupling)))
            {
                if (value != MeasuringCoupling.None)
                {
                    list.Add(value);
                }
            }
            if (array != null && array.Length != 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    list.Remove((MeasuringCoupling)array[i]["Coupling"]);
                }
                list.Remove(MeasuringCoupling.GND);
            }
            return list;
        }

        public static MeasuringCoupling DefaultCoupling(MeasuringFunction function)
        {
            if ((uint)(function - 1) <= 1u || function == MeasuringFunction.Pressure)
            {
                return MeasuringCoupling.DC;
            }
            return MeasuringCoupling.None;
        }

        private static IEnumerable<string> AddUnit(IEnumerable<string> values, string unit)
        {
            List<string> list = new List<string>();
            foreach (string value in values)
            {
                list.Add($"{value}{unit}");
            }
            return list;
        }

        public static IEnumerable<string> AvailableRanges(int connector, Probes probe)
        {
            List<string> list = new List<string>();
            switch (probe)
            {
                case Probes.NONE:
                    return list;
                case Probes.U:
                case Probes.TD:
                    {
                        string unit = "V";
                        switch (connector)
                        {
                            case 1:
                            case 2:
                                list.Add("0.5");
                                list.Add("1");
                                list.Add("2");
                                list.Add("5");
                                list.Add("10");
                                list.Add("20");
                                list.Add("50");
                                list.Add("100");
                                list.Add("200");
                                list.Add("500");
                                break;
                            case 3:
                            case 4:
                                list.Add("0.5");
                                list.Add("1");
                                list.Add("5");
                                list.Add("10");
                                break;
                            default:
                                Log.Error("DeviceDmmImibR2.AvailableRanges", "For probe {0} and connector {1} doesn't exist a range.", probe.ToString(), connector.ToString(CultureInfo.InvariantCulture));
                                break;
                        }
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.I:
                    {
                        string unit = "A";
                        if (connector == 1)
                        {
                            list.Add("0.5");
                            list.Add("2");
                        }
                        else
                        {
                            Log.Error("DeviceDmmImibR2.AvailableRanges", "For probe {0} and connector {1} doesn't exist a range.", probe.ToString(), connector.ToString(CultureInfo.InvariantCulture));
                        }
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.I100:
                    {
                        string unit = "A";
                        if (connector == 3 || connector == 4)
                        {
                            list.Add("5");
                            list.Add("10");
                            list.Add("20");
                            list.Add("50");
                            list.Add("100");
                        }
                        else
                        {
                            Log.Error("DeviceDmmImibR2.AvailableRanges", "For probe {0} and connector {1} doesn't exist a range.", probe.ToString(), connector.ToString(CultureInfo.InvariantCulture));
                        }
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.I1800:
                    {
                        string unit = "A";
                        if (connector == 3 || connector == 4)
                        {
                            list.Add("100");
                            list.Add("200");
                            list.Add("500");
                            list.Add("1000");
                            list.Add("2000");
                        }
                        else
                        {
                            Log.Error("DeviceDmmImibR2.AvailableRanges", "For probe {0} and connector {1} doesn't exist a range.", probe.ToString(), connector.ToString(CultureInfo.InvariantCulture));
                        }
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.R:
                    if (connector == 1)
                    {
                        list.Add("75Ohm");
                        list.Add("150Ohm");
                        list.Add("1.5kOhm");
                        list.Add("15kOhm");
                        list.Add("150kOhm");
                        list.Add("1.5MOhm");
                        list.Add("15MOhm");
                    }
                    else
                    {
                        Log.Error("DeviceDmmImibR2.AvailableRanges", "For probe {0} and connector {1} doesn't exist a range.", probe.ToString(), connector.ToString(CultureInfo.InvariantCulture));
                    }
                    break;
                case Probes.P25:
                case Probes.P25KV:
                    {
                        string unit = "bar";
                        list.Add("1");
                        list.Add("5");
                        list.Add("25");
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.P100:
                    {
                        string unit = "bar";
                        list.Add("5");
                        list.Add("10");
                        list.Add("20");
                        list.Add("50");
                        list.Add("100");
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.P400:
                    {
                        string unit = "bar";
                        list.Add("50");
                        list.Add("100");
                        list.Add("200");
                        list.Add("400");
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.LP:
                    {
                        string unit = "bar";
                        list.Add("2.0");
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.KV:
                    {
                        string unit = "V";
                        list.Add("10000");
                        list.Add("25000");
                        list.Add("50000");
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.TRIG:
                    {
                        string unit = "V";
                        list.Add("50");
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                case Probes.TEMP:
                    {
                        string unit = "°C";
                        list.Add("200");
                        list = AddUnit(list, unit).ToList();
                        break;
                    }
                default:
                    Log.Warning("DeviceDmmImibR2.AvailableRanges", "No Range availabe for connector {0} and probe {1}.", connector, probe);
                    break;
            }
            return list;
        }

        public static string RangeR1ToR2Converter(string range, IList<string> availableRanges)
        {
            if (availableRanges == null || availableRanges.Count == 0)
            {
                return range;
            }
            if (string.IsNullOrEmpty(range) && availableRanges.Count > 0)
            {
                return availableRanges.Last();
            }
            string result = range;
            if (!availableRanges.Contains(range))
            {
                double num = UnitHelper.FromUnitPrefix(range);
                result = availableRanges.Last();
                for (int i = 0; i < availableRanges.Count(); i++)
                {
                    if (num <= UnitHelper.FromUnitPrefix(availableRanges[i]))
                    {
                        result = availableRanges[i];
                        break;
                    }
                }
            }
            return result;
        }

        public static Dictionary<Enum, string> Factory(Enum enumType)
        {
            Dictionary<Enum, string> dictionary = new Dictionary<Enum, string>();
            if (enumType is DMMFunctions)
            {
                dictionary.Add(DMMFunctions.Read, "READ");
                dictionary.Add(DMMFunctions.Configure, "CONF");
                dictionary.Add(DMMFunctions.Measure, "MEAS");
            }
            else if (enumType is MeasuringFunction)
            {
                dictionary.Add(MeasuringFunction.Current, "CURR");
                dictionary.Add(MeasuringFunction.Voltage, "VOLT");
                dictionary.Add(MeasuringFunction.Diode, "DIOD");
                dictionary.Add(MeasuringFunction.Resistance, "RES");
                dictionary.Add(MeasuringFunction.Pressure, "PRES");
                dictionary.Add(MeasuringFunction.Temperature, "TEMP");
            }
            else if (enumType is MeasuringCoupling)
            {
                dictionary.Add(MeasuringCoupling.AC, "AC");
                dictionary.Add(MeasuringCoupling.DC, "DC");
                dictionary.Add(MeasuringCoupling.GND, "GND");
            }
            else if (enumType is Devices)
            {
                dictionary.Add(Devices.DMM1, "DMM1");
                dictionary.Add(Devices.DMM2, "DMM2");
                dictionary.Add(Devices.DSO, "DSO");
                dictionary.Add(Devices.Bluetooth, "BLU");
                dictionary.Add(Devices.USB, "USB");
                dictionary.Add(Devices.Counter, "COUN");
                dictionary.Add(Devices.Stimuli, "STIM");
                dictionary.Add(Devices.Plug, "PLUG");
            }
            else if (enumType is SystemCommand)
            {
                dictionary.Add(SystemCommand.CLS, "*CLS");
                dictionary.Add(SystemCommand.ESE, "*ESE");
                dictionary.Add(SystemCommand.ESR, "*ESR");
                dictionary.Add(SystemCommand.RST, "*RST");
                dictionary.Add(SystemCommand.SRE, "*SRE");
                dictionary.Add(SystemCommand.STB, "*STB");
                dictionary.Add(SystemCommand.System, "SYST");
                dictionary.Add(SystemCommand.Display, "DISP");
            }
            else if (enumType is SystemFunctions)
            {
                dictionary.Add(SystemFunctions.Error, "ERR");
                dictionary.Add(SystemFunctions.Probe, "PROB");
                dictionary.Add(SystemFunctions.ID, "ID");
            }
            else if (enumType is ProbeFunction)
            {
                dictionary.Add(ProbeFunction.Switch, "SWIT");
            }
            else if (enumType is SystemErrorFunctions)
            {
                dictionary.Add(SystemErrorFunctions.Count, "COUN");
                dictionary.Add(SystemErrorFunctions.All, "ALL");
            }
            else if (enumType is DSOFunctions)
            {
                dictionary.Add(DSOFunctions.Aqquire, "ACQ");
                dictionary.Add(DSOFunctions.Autoscale, "AUT");
                dictionary.Add(DSOFunctions.Channel1, "CHAN1");
                dictionary.Add(DSOFunctions.Channel2, "CHAN2");
                dictionary.Add(DSOFunctions.Channel3, "CHAN3");
                dictionary.Add(DSOFunctions.Channel4, "CHAN4");
                dictionary.Add(DSOFunctions.Run, "RUN");
                dictionary.Add(DSOFunctions.Single, "SING");
                dictionary.Add(DSOFunctions.Stop, "STOP");
                dictionary.Add(DSOFunctions.Timebase, "TIM");
                dictionary.Add(DSOFunctions.Trigger, "TRIG");
                dictionary.Add(DSOFunctions.Waveform, "WAV");
            }
            else if (enumType is DSOChannelFunctions)
            {
                dictionary.Add(DSOChannelFunctions.BWLimit, "BWL");
                dictionary.Add(DSOChannelFunctions.Connector, "CONN");
                dictionary.Add(DSOChannelFunctions.Coupling, "COUP");
                dictionary.Add(DSOChannelFunctions.Probe, "PROB");
                dictionary.Add(DSOChannelFunctions.Range, "RANG");
                dictionary.Add(DSOChannelFunctions.Counter, "COUN");
            }
            else if (enumType is DSOAcquireFunctions)
            {
                dictionary.Add(DSOAcquireFunctions.Points, "POIN");
                dictionary.Add(DSOAcquireFunctions.Spoints, "SPO");
                dictionary.Add(DSOAcquireFunctions.SRate, "SRAT");
            }
            else if (enumType is DSOTimebaseFunctions)
            {
                dictionary.Add(DSOTimebaseFunctions.Pos, "POS");
                dictionary.Add(DSOTimebaseFunctions.Range, "RANG");
                dictionary.Add(DSOTimebaseFunctions.Scale, "SCAL");
            }
            else if (enumType is DSOTriggerFunctions)
            {
                dictionary.Add(DSOTriggerFunctions.Level, "LEV");
                dictionary.Add(DSOTriggerFunctions.Mode, "MODE");
                dictionary.Add(DSOTriggerFunctions.Slope, "SLOP");
                dictionary.Add(DSOTriggerFunctions.Source, "SOUR");
            }
            else if (enumType is DSOWaveformFunctions)
            {
                dictionary.Add(DSOWaveformFunctions.Data, "DATA");
                dictionary.Add(DSOWaveformFunctions.Format, "FORM");
                dictionary.Add(DSOWaveformFunctions.Mode, "MODE");
                dictionary.Add(DSOWaveformFunctions.Points, "POIN");
                dictionary.Add(DSOWaveformFunctions.Preamble, "PRE");
                dictionary.Add(DSOWaveformFunctions.Source, "SOUR");
                dictionary.Add(DSOWaveformFunctions.Type, "TYPE");
                dictionary.Add(DSOWaveformFunctions.Read, "READ");
            }
            else if (enumType is BluetoothFunctions)
            {
                dictionary.Add(BluetoothFunctions.Discover, "DISC");
                dictionary.Add(BluetoothFunctions.GetDeviceinfo, "GETD");
                dictionary.Add(BluetoothFunctions.Pair, "PAIR");
                dictionary.Add(BluetoothFunctions.Pin, "PIN");
                dictionary.Add(BluetoothFunctions.Start, "STAR");
                dictionary.Add(BluetoothFunctions.Stop, "STOP");
            }
            else if (enumType is BluetoothDiscoverFunctions)
            {
                dictionary.Add(BluetoothDiscoverFunctions.Devices, "DEV");
                dictionary.Add(BluetoothDiscoverFunctions.Status, "STAT");
            }
            else if (enumType is BluetoothPairFunctions)
            {
                dictionary.Add(BluetoothPairFunctions.Duration, "DUR");
                dictionary.Add(BluetoothPairFunctions.Request, "REQ");
            }
            else if (enumType is USBFunctions)
            {
                dictionary.Add(USBFunctions.Apply, "APPLY");
                dictionary.Add(USBFunctions.CoolingDuration, "CDUR");
                dictionary.Add(USBFunctions.CriticalCurrent, "CRIT");
                dictionary.Add(USBFunctions.Measure, "MEAS");
                dictionary.Add(USBFunctions.Read, "READ");
                dictionary.Add(USBFunctions.Stop, "STOP");
                dictionary.Add(USBFunctions.Current, "CURR");
                dictionary.Add(USBFunctions.DClass, "DCL");
                dictionary.Add(USBFunctions.DProtocol, "DPR");
                dictionary.Add(USBFunctions.DSubClass, "DSUB");
                dictionary.Add(USBFunctions.IClass, "IC");
                dictionary.Add(USBFunctions.IProtocol, "IPR");
                dictionary.Add(USBFunctions.ISubClass, "ISUB");
                dictionary.Add(USBFunctions.Duration, "DUR");
                dictionary.Add(USBFunctions.MinVoltage, "MINV");
                dictionary.Add(USBFunctions.PID, "PID");
                dictionary.Add(USBFunctions.VID, "VID");
                dictionary.Add(USBFunctions.Start, "STAR");
                dictionary.Add(USBFunctions.Status, "STAT");
                dictionary.Add(USBFunctions.Reset, "RESet");
                dictionary.Add(USBFunctions.Connection, "CONNectionstate");
            }
            else if (enumType is USBApplyFunction)
            {
                dictionary.Add(USBApplyFunction.Current, "CURR");
            }
            else if (enumType is USBMeasureFunctions)
            {
                dictionary.Add(USBMeasureFunctions.Current, "CURR");
                dictionary.Add(USBMeasureFunctions.Voltage, "VOLT");
            }
            else if (enumType is StimuliFunctions)
            {
                dictionary.Add(StimuliFunctions.Apply, "APPL");
                dictionary.Add(StimuliFunctions.Data, "DATA");
                dictionary.Add(StimuliFunctions.Pulse, "PULS");
            }
            else if (enumType is StimuliApplyFunctions)
            {
                dictionary.Add(StimuliApplyFunctions.DC, "DC");
                dictionary.Add(StimuliApplyFunctions.Rectangle, "RECT");
                dictionary.Add(StimuliApplyFunctions.Resistance, "RES");
                dictionary.Add(StimuliApplyFunctions.Sinusoid, "SIN");
                dictionary.Add(StimuliApplyFunctions.Start, "START");
                dictionary.Add(StimuliApplyFunctions.Stop, "STOP");
                dictionary.Add(StimuliApplyFunctions.Triangle, "TRI");
                dictionary.Add(StimuliApplyFunctions.User, "USER");
            }
            else if (enumType is StimuliDataFunctions)
            {
                dictionary.Add(StimuliDataFunctions.Current, "CURR");
                dictionary.Add(StimuliDataFunctions.Voltage, "VOLT");
            }
            else if (enumType is StimuliPulseFunctions)
            {
                dictionary.Add(StimuliPulseFunctions.DCycle, "DCYC");
            }
            else if (enumType is CounterFunctions)
            {
                dictionary.Add(CounterFunctions.Array, "ARR");
                dictionary.Add(CounterFunctions.BwLimit, "BWL");
                dictionary.Add(CounterFunctions.Connector, "CONN");
                dictionary.Add(CounterFunctions.Count, "COUN");
                dictionary.Add(CounterFunctions.Coupling, "COUP");
                dictionary.Add(CounterFunctions.Frequency, "FREQ");
                dictionary.Add(CounterFunctions.DutyCycle, "DUTY");
                dictionary.Add(CounterFunctions.Level, "LEV");
                dictionary.Add(CounterFunctions.Probe, "PROB");
                dictionary.Add(CounterFunctions.Range, "RANG");
                dictionary.Add(CounterFunctions.Reset, "RES");
                dictionary.Add(CounterFunctions.Run, "RUN");
                dictionary.Add(CounterFunctions.Slope, "SLOP");
                dictionary.Add(CounterFunctions.Stop, "STOP");
                dictionary.Add(CounterFunctions.Size, "SIZE");
            }
            else if (enumType is MeasuringFunctionFunction)
            {
                dictionary.Add(MeasuringFunctionFunction.Aperture, "APER");
                dictionary.Add(MeasuringFunctionFunction.BWLimit, "BWL");
            }
            else if (enumType is DisplayFunctions)
            {
                dictionary.Add(DisplayFunctions.HideMessage, "HIDemessage");
                dictionary.Add(DisplayFunctions.ShowMessage, "SHOW");
            }
            else
            {
                if (!(enumType is PlugFunction))
                {
                    throw new ArgumentException("For this parameter doesn't exist any mapping", "enumType");
                }
                dictionary.Add(PlugFunction.Load, "Load");
                dictionary.Add(PlugFunction.Unload, "UNLoad");
                dictionary.Add(PlugFunction.Get, "Gets");
                dictionary.Add(PlugFunction.Reset, "RESet");
                dictionary.Add(PlugFunction.Start, "Start");
                dictionary.Add(PlugFunction.Command, "Command");
                dictionary.Add(PlugFunction.GetProperty, "GETProperty");
                dictionary.Add(PlugFunction.SetProperty, "SETProperty");
                dictionary.Add(PlugFunction.Stop, "STOP");
                dictionary.Add(PlugFunction.Read, "READ");
                dictionary.Add(PlugFunction.GetP, "GETP");
                dictionary.Add(PlugFunction.SetP, "SETP");
            }
            return dictionary;
        }

        public static string TriggerSourceToFunction(TriggerSensor source)
        {
            string result;
            switch (source)
            {
                case TriggerSensor.Clamp50A:
                case TriggerSensor.Clamp100A:
                case TriggerSensor.Clamp1000A:
                case TriggerSensor.Clamp1800A:
                    result = "Current";
                    break;
                case TriggerSensor.PressureSensor3_5:
                case TriggerSensor.PressureSensor25:
                case TriggerSensor.PressureSensor100:
                case TriggerSensor.PressureSensor400:
                    result = "Pressure";
                    break;
                case TriggerSensor.Probes1:
                case TriggerSensor.Probes2:
                case TriggerSensor.TDCable:
                case TriggerSensor.KVClip:
                case TriggerSensor.RZVCable:
                case TriggerSensor.TriggerClamp:
                    result = "Voltage";
                    break;
                default:
                    result = string.Empty;
                    Log.Error("Mapping.TriggerSourceToRanges()", "Triggersource '{0}' not supported.", source.ToString());
                    break;
            }
            return result;
        }

        public static string TriggerSourceToRanges(TriggerSensor source)
        {
            string result;
            switch (source)
            {
                case TriggerSensor.Clamp50A:
                    result = "50A";
                    break;
                case TriggerSensor.Clamp100A:
                    result = "100A";
                    break;
                case TriggerSensor.Clamp1000A:
                    result = "1000A";
                    break;
                case TriggerSensor.Clamp1800A:
                    result = "2000A";
                    break;
                case TriggerSensor.PressureSensor3_5:
                    result = "3.5bar";
                    break;
                case TriggerSensor.PressureSensor25:
                    result = "25bar";
                    break;
                case TriggerSensor.PressureSensor100:
                    result = "100bar";
                    break;
                case TriggerSensor.PressureSensor400:
                    result = "400bar";
                    break;
                case TriggerSensor.TriggerClamp:
                    result = "50V";
                    break;
                default:
                    result = string.Empty;
                    Log.Error("Mapping.TriggerSourceToRanges()", "Triggersource '{0}' not supported.", source.ToString());
                    break;
            }
            return result;
        }

        public static string RangeToFormat(decimal range)
        {
            if (range <= 100m)
            {
                return "N3";
            }
            return "N2";
        }

        public static bool HasSensorTheMode(MeasuringSensor sensor, DMMModeType modeType)
        {
            bool flag = false;
            DataRow[] source = DsImibConfig.Tables["SensorToFuntion"].Select(string.Format(CultureInfo.InvariantCulture, "Sensor = {0}", (int)sensor));
            if (!source.Any((DataRow x) => modeType.Equals((DMMModeType)x["ModeType"])))
            {
                IEnumerable<MeasuringFunction> inner = source.Select((DataRow x) => (MeasuringFunction)x["Function"]);
                return (from fToCoup in DsImibConfig.Tables["FunctionToCoupling"].AsEnumerable()
                        join measFunc in inner on (MeasuringFunction)fToCoup["Function"] equals measFunc
                        where (DMMModeType)fToCoup["ModeType"] == modeType
                        select fToCoup["ModeType"]).Any();
            }
            return true;
        }

        public static DMMModeType ModeTypeOf(MeasuringSensor sensorName, MeasuringFunction functionName, MeasuringCoupling couplingName)
        {
            DMMModeType result = DMMModeType.None;
            DataRow dataRow = DsImibConfig.Tables["SensorToFuntion"].Rows.Find(new object[2] { sensorName, functionName });
            if (dataRow == null)
            {
                Log.Info("Mapping.ModeTypeOf", "For sensor {0} and function {1} has no mode type.", sensorName.ToString(), functionName.ToString());
            }
            else if (DMMModeType.None.Equals((DMMModeType)dataRow["ModeType"]))
            {
                DataRow dataRow2 = DsImibConfig.Tables["FunctionToCoupling"].Rows.Find(new object[2] { functionName, couplingName });
                result = ((dataRow2 != null) ? ((DMMModeType)dataRow2["ModeType"]) : DMMModeType.None);
            }
            else
            {
                result = (DMMModeType)dataRow["ModeType"];
            }
            return result;
        }

        public static IEnumerable<MeasuringFunction> FunctionOfSensor(MeasuringSensor sensorName)
        {
            return (from x in DsImibConfig.Tables["SensorToFuntion"].Select(string.Format(CultureInfo.InvariantCulture, "Sensor = {0}", (int)sensorName))
                    select x["Function"]).Cast<MeasuringFunction>();
        }

        private static void CreateSensorFunctionTable(DataSet dsImib)
        {
            DataTable dataTable = new DataTable("SensorToFuntion");
            DataColumn dataColumn = new DataColumn();
            dataColumn.DataType = typeof(MeasuringSensor);
            dataColumn.ColumnName = "Sensor";
            dataColumn.ReadOnly = true;
            dataColumn.Unique = false;
            dataTable.Columns.Add(dataColumn);
            dataColumn = new DataColumn();
            dataColumn.DataType = typeof(MeasuringFunction);
            dataColumn.ColumnName = "Function";
            dataColumn.ReadOnly = true;
            dataColumn.Unique = false;
            dataTable.Columns.Add(dataColumn);
            dataColumn = new DataColumn();
            dataColumn.DataType = typeof(DMMModeType);
            dataColumn.ColumnName = "ModeType";
            dataColumn.ReadOnly = true;
            dataColumn.Unique = false;
            dataColumn.DefaultValue = DMMModeType.None;
            dataTable.Columns.Add(dataColumn);
            dataTable.PrimaryKey = new DataColumn[2]
            {
            dataTable.Columns["Sensor"],
            dataTable.Columns["Function"]
            };
            dsImib.Tables.Add(dataTable);
        }

        private static void CreateFunctionCouplingTable(DataSet dsImib)
        {
            DataTable dataTable = new DataTable("FunctionToCoupling");
            DataColumn dataColumn = new DataColumn();
            dataColumn.DataType = typeof(MeasuringFunction);
            dataColumn.ColumnName = "Function";
            dataColumn.ReadOnly = true;
            dataColumn.Unique = false;
            dataTable.Columns.Add(dataColumn);
            dataColumn = new DataColumn();
            dataColumn.DataType = typeof(MeasuringCoupling);
            dataColumn.ColumnName = "Coupling";
            dataColumn.ReadOnly = true;
            dataColumn.Unique = false;
            dataTable.Columns.Add(dataColumn);
            dataColumn = new DataColumn();
            dataColumn.DataType = typeof(DMMModeType);
            dataColumn.ColumnName = "ModeType";
            dataColumn.ReadOnly = true;
            dataColumn.Unique = false;
            dataColumn.DefaultValue = DMMModeType.None;
            dataTable.Columns.Add(dataColumn);
            dataTable.PrimaryKey = new DataColumn[2]
            {
            dataTable.Columns["Function"],
            dataTable.Columns["Coupling"]
            };
            dsImib.Tables.Add(dataTable);
        }

        private static void FillImibConfig(DataSet dsImib)
        {
            DataTable dataTable = dsImib.Tables["SensorToFuntion"];
            DataRow dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.Probes1;
            dataRow["Function"] = MeasuringFunction.Voltage;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.Probes1;
            dataRow["Function"] = MeasuringFunction.Current;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.Probes1;
            dataRow["Function"] = MeasuringFunction.Diode;
            dataRow["ModeType"] = DMMModeType.Diode;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.Probes1;
            dataRow["Function"] = MeasuringFunction.Resistance;
            dataRow["ModeType"] = DMMModeType.Resistance;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.Probes2;
            dataRow["Function"] = MeasuringFunction.Voltage;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.LowPressureSensor1;
            dataRow["Function"] = MeasuringFunction.Pressure;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.LowPressureSensor2;
            dataRow["Function"] = MeasuringFunction.Pressure;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.PressureSensor100;
            dataRow["Function"] = MeasuringFunction.Pressure;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.PressureSensor25;
            dataRow["Function"] = MeasuringFunction.Pressure;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.PressureSensor3_5;
            dataRow["Function"] = MeasuringFunction.Pressure;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.PressureSensor400;
            dataRow["Function"] = MeasuringFunction.Pressure;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.Clamp1000A;
            dataRow["Function"] = MeasuringFunction.Current;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.Clamp100A;
            dataRow["Function"] = MeasuringFunction.Current;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.Clamp1800A;
            dataRow["Function"] = MeasuringFunction.Current;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.Clamp50A;
            dataRow["Function"] = MeasuringFunction.Current;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.TemperatureSensor1;
            dataRow["Function"] = MeasuringFunction.Temperature;
            dataTable.Rows.Add(dataRow);
            dataRow = dataTable.NewRow();
            dataRow["Sensor"] = MeasuringSensor.TemperatureSensor2;
            dataRow["Function"] = MeasuringFunction.Temperature;
            dataTable.Rows.Add(dataRow);
            DataTable dataTable2 = dsImib.Tables["FunctionToCoupling"];
            dataRow = dataTable2.NewRow();
            dataRow["Function"] = MeasuringFunction.Current;
            dataRow["Coupling"] = MeasuringCoupling.AC;
            dataRow["ModeType"] = DMMModeType.CurrentAC;
            dataTable2.Rows.Add(dataRow);
            dataRow = dataTable2.NewRow();
            dataRow["Function"] = MeasuringFunction.Current;
            dataRow["Coupling"] = MeasuringCoupling.DC;
            dataRow["ModeType"] = DMMModeType.CurrentDC;
            dataTable2.Rows.Add(dataRow);
            dataRow = dataTable2.NewRow();
            dataRow["Function"] = MeasuringFunction.Voltage;
            dataRow["Coupling"] = MeasuringCoupling.AC;
            dataRow["ModeType"] = DMMModeType.VoltageAC;
            dataTable2.Rows.Add(dataRow);
            dataRow = dataTable2.NewRow();
            dataRow["Function"] = MeasuringFunction.Voltage;
            dataRow["Coupling"] = MeasuringCoupling.DC;
            dataRow["ModeType"] = DMMModeType.VoltageDC;
            dataTable2.Rows.Add(dataRow);
        }
    }
}
