using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient.Utility
{
    public class SgbmIdParser
    {
        public const string PatternSgbmIdDec = "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9]{1,3})[_\\-\\.](?<subVersion>[0-9]{1,3})[_\\-\\.](?<patchVersion>[0-9]{1,3})$";
        public const string PatternSgbmIdHex = "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<subVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<patchVersion>[0-9a-fA-F]{1,2})$";
        public long Id { get; private set; }
        public int MainVersion { get; private set; }
        public int PatchVersion { get; private set; }
        public string ProcessClass { get; private set; }
        public int SubVersion { get; private set; }

        public bool ParseDec(string sgbmId)
        {
            return Parse(sgbmId, "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9]{1,3})[_\\-\\.](?<subVersion>[0-9]{1,3})[_\\-\\.](?<patchVersion>[0-9]{1,3})$");
        }

        public bool ParseHex(string sgbmId)
        {
            return Parse(sgbmId, "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<subVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<patchVersion>[0-9a-fA-F]{1,2})$");
        }

        private bool Parse(string sgbmId, string pattern)
        {
            if (string.IsNullOrEmpty(sgbmId))
            {
                Log.Warning("SgbmIdParser.Parse()", "Input is null!");
                return false;
            }

            NumberStyles style = ((pattern == "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<subVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<patchVersion>[0-9a-fA-F]{1,2})$") ? NumberStyles.AllowHexSpecifier : NumberStyles.None);
            Match match = Regex.Match(sgbmId.Trim(), pattern);
            if (match.Success)
            {
                ProcessClass = match.Groups["processClass"].Value;
                if (ProcessClass.Equals("-", StringComparison.Ordinal))
                {
                    ProcessClass = "UNKN";
                }

                Id = long.Parse(match.Groups["id"].Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                MainVersion = int.Parse(match.Groups["mainVersion"].Value, style, CultureInfo.InvariantCulture);
                SubVersion = int.Parse(match.Groups["subVersion"].Value, style, CultureInfo.InvariantCulture);
                PatchVersion = int.Parse(match.Groups["patchVersion"].Value, style, CultureInfo.InvariantCulture);
                return true;
            }

            Log.Warning("SgbmIdParser.Parse()", "SGBMID '{0}' is not valid due to pattern '{1}'!", sgbmId, pattern);
            return false;
        }
    }
}