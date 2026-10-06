using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Measurement.Common;
using PsdzClient.Core;
using System.Text.RegularExpressions;

namespace BMW.Rheingold.Measurement.Common
{
    public class GenericDeviceStatusInfo : GenericDeviceStatus
    {
        private readonly long[] factors;

        public string Name { get; private set; }

        public string Value { get; private set; }

        public string Version { get; private set; }

        public long VersionAsNumber { get; private set; }

        public GenericDeviceStatusInfo(string nameVal)
        {
            factors = new long[4] { 10000000000L, 100000000L, 10000L, 1L };
            Regex regex = new Regex("'.*'");
            Regex regex2 = new Regex("@.*@");
            Regex regex3 = new Regex("\\d+\\.\\d+\\.\\d+(\\.\\d+)?");
            Match match = regex.Match(nameVal);
            if (match.Success)
            {
                Name = match.Value;
            }
            Match match2 = regex2.Match(nameVal);
            if (!match2.Success)
            {
                return;
            }
            Value = match2.Value.Trim('@');
            Match match3 = regex3.Match(Value);
            if (match3.Success)
            {
                Version = Value;
                string[] array = match3.Value.Split('.');
                for (int i = 0; i < array.Length; i++)
                {
                    if (factors.Length <= i)
                    {
                        Log.Error("GenericDeviceStatusInfo.ctor", "Too less factors.");
                        break;
                    }
                    VersionAsNumber += int.Parse(array[i]) * factors[i];
                }
            }
            else
            {
                VersionAsNumber = -1L;
            }
        }
    }
}
