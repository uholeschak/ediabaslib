using BMW.Rheingold.Measurement.Common;
using BMW.Rheingold.Measurement.Common.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using BMW.Rheingold.MeasurementCommunication;
using PsdzClient;

namespace BMW.Rheingold.Measurement
{
    [PreserveSource(Hint = "Simplified", SuppressWarning = true)]
    public class DmmChannelData : INotifyPropertyChanged, IDisposable
    {
        private const int MaxAmountOfSensors = 12;

        private readonly IDmmManager manager;

        private bool disposed;

        public ICommand CmdHold { get; private set; }

        public ICommand CmdMinMax { get; private set; }

        public ICommand[] CmdSelectModeWithoutDiode { get; private set; }

        public ICommand[] CmdSelectMode { get; private set; }

        public bool IsSourceSelected => true;

        public IDmmManager Manager => manager;

        public DmmChannel Model => Manager.Model[DmmNo];

        public DmmMeasuredValueMinMax MeasuredValue => Manager.Model[DmmNo].MeasuredValue;

        public int DmmNo { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public DmmChannelData(int dmmNo, IDmmManager manager)
        {
            DmmNo = dmmNo;
            this.manager = manager;
            CmdSelectModeWithoutDiode = CmdSelectMode.Take(5).ToArray();
            Model.ModelChanged += ModelChanged;
        }

        ~DmmChannelData()
        {
            Dispose();
        }

        public void Initialize()
        {
        }

        public void MeasuredValueChanged()
        {
            RaisePropertyChanged("MeasuredValue");
        }

        public static string[] FormatMeasuringValue4(float value, string range, MeasuringRangeStatus rangeStatus, string unit)
        {
            return FormatMeasuringValue(value, range, rangeStatus, unit, 4);
        }

        private static string[] FormatMeasuringValue(float value, string range, MeasuringRangeStatus rangeStatus, string unit, int resultLength)
        {
            string[] array = new string[resultLength];
            string[] array2 = null;
            if (resultLength == 4 || rangeStatus == MeasuringRangeStatus.OK)
            {
                array2 = FormatMeasuringValue(value, range, unit);
            }
            switch (rangeStatus)
            {
                case MeasuringRangeStatus.OVR:
                    array[0] = "OVR ";
                    array[1] = "-";
                    break;
                case MeasuringRangeStatus.UDR:
                    array[0] = "UDR ";
                    array[1] = "-";
                    break;
                case MeasuringRangeStatus.SENSOR_ERROR:
                    array[0] = "SENSOR ";
                    array[1] = "ERR";
                    break;
                case MeasuringRangeStatus.ERR_UNKNOWN:
                    array[0] = "unknown ";
                    array[1] = "err";
                    break;
                case MeasuringRangeStatus.OK:
                    array[0] = array2[0];
                    array[1] = array2[1];
                    break;
            }
            if (resultLength == 4)
            {
                array[2] = array2[0];
                array[3] = array2[1];
            }
            return array;
        }

        private static string[] FormatMeasuringValue(float value, string range, string unit)
        {
            string[] array;
            if (Math.Abs(value) > 99f)
            {
                array = UnitHelper.ValueUnit(value, range);
                array[1] += unit;
            }
            else
            {
                array = new string[2];
                string format = "{0:" + UnitHelper.RangeToFormat(range) + "}";
                array[0] = string.Format(CultureInfo.InvariantCulture, format, value);
                array[1] = unit;
            }
            return array;
        }

        public void SetRange(bool commit, int change)
        {
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ModelChanged(object sender, PropertyChangedEventArgs e)
        {
        }


        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
            }
            disposed = true;
        }
    }

}
