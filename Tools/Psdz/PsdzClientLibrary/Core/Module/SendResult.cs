using System;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Rheingold.Measurement.Common
{
    public struct SendResult
    {
        public string Data { get; set; }

        public IEnumerable<byte> RawData { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(Data);

        public SendResult(string data, IEnumerable<byte> rawData)
        {
            this = default(SendResult);
            Data = data;
            RawData = rawData;
        }

        public override string ToString()
        {
            return Data;
        }

        public void RemoveLengthHeaderIfExist()
        {
            if (!string.IsNullOrEmpty(Data) && Data.StartsWith("#8", StringComparison.OrdinalIgnoreCase))
            {
                Data = new string(Data.Skip(10).ToArray());
                RawData = RawData.Skip(10);
            }
        }
    }
}
