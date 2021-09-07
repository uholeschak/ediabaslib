using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PsdzClient.Utility
{
	public class SgbmIdParser
	{
		public long Id { get; private set; }

		public int MainVersion { get; private set; }

		public int PatchVersion { get; private set; }

		public string ProcessClass { get; private set; }

		public int SubVersion { get; private set; }

		public bool ParseDec(string sgbmId)
		{
			return this.Parse(sgbmId, "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9]{1,3})[_\\-\\.](?<subVersion>[0-9]{1,3})[_\\-\\.](?<patchVersion>[0-9]{1,3})$");
		}

		public bool ParseHex(string sgbmId)
		{
			return this.Parse(sgbmId, "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<subVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<patchVersion>[0-9a-fA-F]{1,2})$");
		}

		private bool Parse(string sgbmId, string pattern)
		{
			if (string.IsNullOrEmpty(sgbmId))
			{
				return false;
			}
			NumberStyles style = (pattern == "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<subVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<patchVersion>[0-9a-fA-F]{1,2})$") ? NumberStyles.AllowHexSpecifier : NumberStyles.None;
			Match match = Regex.Match(sgbmId.Trim(), pattern);
			if (match.Success)
			{
				this.ProcessClass = match.Groups["processClass"].Value;
				if (this.ProcessClass.Equals("-", StringComparison.Ordinal))
				{
					this.ProcessClass = "UNKN";
				}
				this.Id = long.Parse(match.Groups["id"].Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				this.MainVersion = int.Parse(match.Groups["mainVersion"].Value, style, CultureInfo.InvariantCulture);
				this.SubVersion = int.Parse(match.Groups["subVersion"].Value, style, CultureInfo.InvariantCulture);
				this.PatchVersion = int.Parse(match.Groups["patchVersion"].Value, style, CultureInfo.InvariantCulture);
				return true;
			}
			return false;
		}

		public const string PatternSgbmIdDec = "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9]{1,3})[_\\-\\.](?<subVersion>[0-9]{1,3})[_\\-\\.](?<patchVersion>[0-9]{1,3})$";

		public const string PatternSgbmIdHex = "^(?<processClass>-|\\w{4})[_\\-](?<id>[0-9a-fA-F]{8})[_\\-](?<mainVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<subVersion>[0-9a-fA-F]{1,2})[_\\-\\.](?<patchVersion>[0-9a-fA-F]{1,2})$";
	}
}
