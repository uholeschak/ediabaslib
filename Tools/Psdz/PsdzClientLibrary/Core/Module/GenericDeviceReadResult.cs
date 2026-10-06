using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Measurement.Common;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BMW.Rheingold.Measurement.Common
{
    public class GenericDeviceReadResult : GenericDeviceStatus
    {
        private struct ReadingInformation
        {
            public int Start;
            public int Length;
        }

        private const string ItemToFind = "Result:Single[]={";
        public float[] Values { get; private set; }

        public GenericDeviceReadResult(float[] values)
        {
            Values = values;
        }

        public GenericDeviceReadResult(byte[] data, KindOfResult kindOfRes = KindOfResult.HVA)
        {
            if (kindOfRes == KindOfResult.HVA)
            {
                InitR1(data);
            }
            else
            {
                InitR1Generic(data);
            }
        }

        public GenericDeviceReadResult(SendResult resultData)
        {
            InitR2(resultData);
        }

        private void InitR1(byte[] data)
        {
            Values = NormalizeMeasuringData("Result:Single[]={", data);
        }

        private void InitR1Generic(byte[] data)
        {
            Values = NormalizeMeasuringDataGeneric(data, "Result:Single[]={");
        }

        private void InitR2(SendResult data)
        {
            Values = NormalizeMeasuringDataGeneric(data.RawData.ToArray(), null);
        }

        private int GenericDeviceResultPos(string itemToFind, byte[] rawData)
        {
            string data = Encoding.ASCII.GetString(rawData);
            return GenericDeviceResultPos(itemToFind, data);
        }

        private int GenericDeviceResultPos(string itemToFind, string data)
        {
            int num = data.IndexOf(itemToFind, StringComparison.Ordinal);
            if (num > -1)
            {
                num += itemToFind.Length;
            }

            return num;
        }

        private int EndResultPos(byte[] rawData)
        {
            return Encoding.ASCII.GetString(rawData).IndexOf("}\r\n>", StringComparison.Ordinal);
        }

        private ReadingInformation GetReadInfo(byte[] data, string startString)
        {
            ReadingInformation result = default;
            string text = Encoding.ASCII.GetString(data);
            int num = ((!string.IsNullOrEmpty(startString)) ? GenericDeviceResultPos(startString, text) : 0);
            if (num == -1)
            {
                return result;
            }

            Regex regex = new Regex("#8\\d{8}");
            Regex regex2 = new Regex("\\d{8}$");
            Match match = regex.Match(text, num);
            if (match.Success)
            {
                result.Start = match.Index + match.Length;
                result.Length = int.Parse(regex2.Match(match.Value).Value);
            }

            return result;
        }

        private float[] NormalizeMeasuringDataGeneric(byte[] rawData, string startString)
        {
            ReadingInformation readInfo = GetReadInfo(rawData, startString);
            float[] array = new float[0];
            if (readInfo.Start != -1 && readInfo.Length > 0)
            {
                array = new float[readInfo.Length / 4];
                try
                {
                    Buffer.BlockCopy(rawData, readInfo.Start, array, 0, readInfo.Length);
                }
                catch (Exception exception)
                {
                    Log.ErrorException("GenericDeviceReadResult.NormalizeMeasuringDataGeneric()", exception);
                }
            }

            return array;
        }

        private float[] NormalizeMeasuringData(string channelPrefix, byte[] rawData)
        {
            int num = GenericDeviceResultPos(channelPrefix, rawData);
            float[] array = new float[0];
            if (num >= 0)
            {
                if (num + 4 + 11 <= rawData.Length)
                {
                    array = new float[12];
                    try
                    {
                        Buffer.BlockCopy(rawData, num + 4, array, 0, 48);
                    }
                    catch (Exception exception)
                    {
                        Log.ErrorException("GenericDeviceReadResult.NormalizeMeasuringData()", exception);
                    }
                }
            }

            return array;
        }

        public static IDictionary<string, GenericDeviceStatusInfo> ParseStatusResult(string statusResult)
        {
            Dictionary<string, GenericDeviceStatusInfo> dictionary = new Dictionary<string, GenericDeviceStatusInfo>();
            if (!string.IsNullOrEmpty(statusResult))
            {
                foreach (string item in
                    from x in statusResult.Trim().Split(new string[3] { "[FileList]", "[PlugIns]", "[Hardware]" }, StringSplitOptions.None)
                    where !string.IsNullOrEmpty(x)select x)
                {
                    foreach (string item2 in
                        from x in item.Split(new string[1] { "\r\n" }, StringSplitOptions.None)
                        where !string.IsNullOrEmpty(x)select x)
                    {
                        string[] array = item2.Split('=');
                        if (array.Count() > 1)
                        {
                            dictionary.Add(array[0], new GenericDeviceStatusInfo(array[1]));
                            continue;
                        }

                        Log.Info("GenericDeviceReadResult.ParseStatusResult()", "The string '{0}' is has not name = value pair", item2);
                    }
                }
            }

            return dictionary;
        }

        public override string ToString()
        {
            string text = base.ToString();
            if (Values != null && Values.Any())
            {
                string text2 = string.Join(", ", Values.Select((float x) => x.ToString(CultureInfo.InvariantCulture)));
                text = text + ", Values: " + text2;
            }

            return text;
        }
    }
}