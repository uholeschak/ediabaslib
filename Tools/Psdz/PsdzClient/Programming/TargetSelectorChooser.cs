using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;

namespace PsdzClient.Programming
{
	public class TargetSelectorChooser
	{
        public TargetSelectorChooser(IEnumerable<IPsdzTargetSelector> targetSelectors)
		{
			this.targetSelectors = new List<TargetSelectorChooser.TargetSelectorDetail>();
			if (targetSelectors != null && targetSelectors.Any<IPsdzTargetSelector>())
			{
				foreach (IPsdzTargetSelector psdzTargetSelector in targetSelectors)
				{
					if (!psdzTargetSelector.IsDirect)
					{
						this.targetSelectors.Add(new TargetSelectorChooser.TargetSelectorDetail(psdzTargetSelector));
					}
				}
			}
		}

		public IPsdzTargetSelector GetNewestTargetSelectorByMainSeries(string mainSeries)
		{
			IEnumerable<TargetSelectorChooser.TargetSelectorDetail> targetSelectorsByMainSeries = this.GetTargetSelectorsByMainSeries(mainSeries);
			if (targetSelectorsByMainSeries != null && targetSelectorsByMainSeries.Any<TargetSelectorChooser.TargetSelectorDetail>())
			{
				List<TargetSelectorChooser.TargetSelectorDetail> list = new List<TargetSelectorChooser.TargetSelectorDetail>(targetSelectorsByMainSeries);
				list.Sort();
				return list.Last<TargetSelectorChooser.TargetSelectorDetail>().TargetSelector;
			}
			return null;
		}

		private IEnumerable<TargetSelectorChooser.TargetSelectorDetail> GetTargetSelectorsByMainSeries(string mainSeries)
		{
			return from ts in this.targetSelectors
				   where string.Equals(mainSeries, ts.TargetSelector.Baureihenverbund, StringComparison.OrdinalIgnoreCase)
				   select ts;
		}

		public const string PatternTargetSelectorILevelContainerShort = "^(?<mainSeries>\\w{4})_(?<iLevelYear>\\d{2})_(?<iLevelMonth>\\d{2})_(?<iLevelNumber>\\d{3})$";

		public const string PatternTargetSelectorILevelContainer = "^(?<mainSeries>\\w{4})_(?<iLevelYear>\\d{2})_(?<iLevelMonth>\\d{2})_(?<iLevelNumber>\\d{3})_(?<containerType>[VTE])_(?<major>\\d{3})_(?<minor>\\d{3})_(?<patch>\\d{3})$";

		private readonly List<TargetSelectorChooser.TargetSelectorDetail> targetSelectors;

		private class TargetSelectorDetail : IComparable<TargetSelectorChooser.TargetSelectorDetail>
		{
			public TargetSelectorDetail(IPsdzTargetSelector targetSelector)
			{
				this.TargetSelector = targetSelector;
				if (!this.TryParseDetailsByShortName(targetSelector.Project))
				{
					this.ParseDetails(targetSelector.Project);
				}
			}

			internal IPsdzTargetSelector TargetSelector { get; private set; }

			private int ContainerTypeWeight { get; set; }

			private int ILevelMonth { get; set; }

			private int ILevelNumber { get; set; }

			private int ILevelYear { get; set; }

			private int Major { get; set; }

			private int Minor { get; set; }

			private int Patch { get; set; }

			public int CompareTo(TargetSelectorChooser.TargetSelectorDetail other)
			{
				int num = this.ContainerTypeWeight.CompareTo(other.ContainerTypeWeight);
				if (num != 0)
				{
					return num;
				}
				num = this.ILevelYear.CompareTo(other.ILevelYear);
				if (num != 0)
				{
					return num;
				}
				num = this.ILevelMonth.CompareTo(other.ILevelMonth);
				if (num != 0)
				{
					return num;
				}
				num = this.ILevelNumber.CompareTo(other.ILevelNumber);
				if (num != 0)
				{
					return num;
				}
				num = this.Major.CompareTo(other.Major);
				if (num != 0)
				{
					return num;
				}
				num = this.Minor.CompareTo(other.Minor);
				if (num != 0)
				{
					return num;
				}
				return this.Patch.CompareTo(other.Patch);
			}

			private int GetContainerTypeWeight(string containerType)
			{
				if (containerType != null)
				{
					if (containerType == "V")
					{
						return 2;
					}
					if (containerType == "T")
					{
						return 1;
					}
					if (containerType == "E")
					{
						return 0;
					}
				}
				return -1;
			}

			private bool TryParseDetailsByShortName(string project)
			{
				Match match = Regex.Match((!string.IsNullOrEmpty(project)) ? project.Trim() : string.Empty, "^(?<mainSeries>\\w{4})_(?<iLevelYear>\\d{2})_(?<iLevelMonth>\\d{2})_(?<iLevelNumber>\\d{3})$");
				if (match.Success)
				{
					this.ContainerTypeWeight = this.GetContainerTypeWeight("V");
					this.ILevelYear = int.Parse(match.Groups["iLevelYear"].Value, NumberStyles.None);
					this.ILevelMonth = int.Parse(match.Groups["iLevelMonth"].Value, NumberStyles.None);
					this.ILevelNumber = int.Parse(match.Groups["iLevelNumber"].Value, NumberStyles.None);
					this.Major = 255;
					this.Minor = 255;
					this.Patch = 255;
					return true;
				}
				return false;
			}

			private void ParseDetails(string project)
			{
				Match match = Regex.Match((!string.IsNullOrEmpty(project)) ? project.Trim() : string.Empty, "^(?<mainSeries>\\w{4})_(?<iLevelYear>\\d{2})_(?<iLevelMonth>\\d{2})_(?<iLevelNumber>\\d{3})_(?<containerType>[VTE])_(?<major>\\d{3})_(?<minor>\\d{3})_(?<patch>\\d{3})$");
				if (match.Success)
				{
					this.ContainerTypeWeight = this.GetContainerTypeWeight(match.Groups["containerType"].Value);
					this.ILevelYear = int.Parse(match.Groups["iLevelYear"].Value, NumberStyles.None);
					this.ILevelMonth = int.Parse(match.Groups["iLevelMonth"].Value, NumberStyles.None);
					this.ILevelNumber = int.Parse(match.Groups["iLevelNumber"].Value, NumberStyles.None);
					this.Major = int.Parse(match.Groups["major"].Value, NumberStyles.None);
					this.Minor = int.Parse(match.Groups["minor"].Value, NumberStyles.None);
					this.Patch = int.Parse(match.Groups["patch"].Value, NumberStyles.None);
					return;
				}

                this.ILevelYear = -1;
				this.ILevelMonth = -1;
				this.ILevelNumber = -1;
				this.ContainerTypeWeight = -1;
				this.Major = -1;
				this.Minor = -1;
				this.Patch = -1;
			}
		}
	}
}
