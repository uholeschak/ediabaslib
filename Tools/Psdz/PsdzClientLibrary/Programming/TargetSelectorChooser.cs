using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using PsdzClient.Core;
using PsdzClient;

namespace PsdzClient.Programming
{
    internal class TargetSelectorChooser
    {
        private sealed class TargetSelectorDetail : IComparable<TargetSelectorDetail>
        {
            internal IPsdzTargetSelector TargetSelector { get; private set; }
            private int ContainerTypeWeight { get; set; }
            private int ILevelMonth { get; set; }
            private int ILevelNumber { get; set; }
            private int ILevelYear { get; set; }
            private int Major { get; set; }
            private int Minor { get; set; }
            private int Patch { get; set; }

            public TargetSelectorDetail(IPsdzTargetSelector targetSelector)
            {
                TargetSelector = targetSelector;
                if (!TryParseDetailsByShortName(targetSelector.Project))
                {
                    ParseDetails(targetSelector.Project);
                }
            }

            public int CompareTo(TargetSelectorDetail other)
            {
                int num = ContainerTypeWeight.CompareTo(other.ContainerTypeWeight);
                if (num != 0)
                {
                    return num;
                }

                num = ILevelYear.CompareTo(other.ILevelYear);
                if (num != 0)
                {
                    return num;
                }

                num = ILevelMonth.CompareTo(other.ILevelMonth);
                if (num != 0)
                {
                    return num;
                }

                num = ILevelNumber.CompareTo(other.ILevelNumber);
                if (num != 0)
                {
                    return num;
                }

                num = Major.CompareTo(other.Major);
                if (num != 0)
                {
                    return num;
                }

                num = Minor.CompareTo(other.Minor);
                if (num != 0)
                {
                    return num;
                }

                return Patch.CompareTo(other.Patch);
            }

            private int GetContainerTypeWeight(string containerType)
            {
                switch (containerType)
                {
                    case "V":
                        return 2;
                    case "T":
                        return 1;
                    case "E":
                        return 0;
                    default:
                        return -1;
                }
            }

            private bool TryParseDetailsByShortName(string project)
            {
                Match match = Regex.Match((!string.IsNullOrEmpty(project)) ? project.Trim() : string.Empty, "^(?<mainSeries>\\w{4})_(?<iLevelYear>\\d{2})_(?<iLevelMonth>\\d{2})_(?<iLevelNumber>\\d{3})$");
                if (match.Success)
                {
                    ContainerTypeWeight = GetContainerTypeWeight("V");
                    ILevelYear = int.Parse(match.Groups["iLevelYear"].Value, NumberStyles.None);
                    ILevelMonth = int.Parse(match.Groups["iLevelMonth"].Value, NumberStyles.None);
                    ILevelNumber = int.Parse(match.Groups["iLevelNumber"].Value, NumberStyles.None);
                    Major = 255;
                    Minor = 255;
                    Patch = 255;
                    return true;
                }

                return false;
            }

            private void ParseDetails(string project)
            {
                Match match = Regex.Match((!string.IsNullOrEmpty(project)) ? project.Trim() : string.Empty, "^(?<mainSeries>\\w{4})_(?<iLevelYear>\\d{2})_(?<iLevelMonth>\\d{2})_(?<iLevelNumber>\\d{3})_(?<containerType>[VTE])_(?<major>\\d{3})_(?<minor>\\d{3})_(?<patch>\\d{3})$");
                if (match.Success)
                {
                    ContainerTypeWeight = GetContainerTypeWeight(match.Groups["containerType"].Value);
                    ILevelYear = int.Parse(match.Groups["iLevelYear"].Value, NumberStyles.None);
                    ILevelMonth = int.Parse(match.Groups["iLevelMonth"].Value, NumberStyles.None);
                    ILevelNumber = int.Parse(match.Groups["iLevelNumber"].Value, NumberStyles.None);
                    Major = int.Parse(match.Groups["major"].Value, NumberStyles.None);
                    Minor = int.Parse(match.Groups["minor"].Value, NumberStyles.None);
                    Patch = int.Parse(match.Groups["patch"].Value, NumberStyles.None);
                }
                else
                {
                    Log.Warning("TargetSelectorChooser.ParseDetails()", "Project '{0}' does not seem to be a valid IC!", project);
                    ILevelYear = -1;
                    ILevelMonth = -1;
                    ILevelNumber = -1;
                    ContainerTypeWeight = -1;
                    Major = -1;
                    Minor = -1;
                    Patch = -1;
                }
            }
        }

        public const string PatternTargetSelectorILevelContainerShort = "^(?<mainSeries>\\w{4})_(?<iLevelYear>\\d{2})_(?<iLevelMonth>\\d{2})_(?<iLevelNumber>\\d{3})$";
        public const string PatternTargetSelectorILevelContainer = "^(?<mainSeries>\\w{4})_(?<iLevelYear>\\d{2})_(?<iLevelMonth>\\d{2})_(?<iLevelNumber>\\d{3})_(?<containerType>[VTE])_(?<major>\\d{3})_(?<minor>\\d{3})_(?<patch>\\d{3})$";
        private readonly List<TargetSelectorDetail> targetSelectors;
        private readonly IInteractionService interactionService;
        internal TargetSelectorChooser(IEnumerable<IPsdzTargetSelector> targetSelectors, IInteractionService interactionService = null)
        {
            this.targetSelectors = new List<TargetSelectorDetail>();
            if (targetSelectors != null && targetSelectors.Any())
            {
                foreach (IPsdzTargetSelector targetSelector in targetSelectors)
                {
                    if (!targetSelector.IsDirect)
                    {
                        this.targetSelectors.Add(new TargetSelectorDetail(targetSelector));
                    }
                }
            }

            this.interactionService = interactionService;
        }

        public IPsdzTargetSelector GetNewestTargetSelectorByMainSeries(string mainSeries, bool notifyIfTargetSelectorCouldNotBeFound = false, string localizationId = null)
        {
            IEnumerable<TargetSelectorDetail> targetSelectorsByMainSeries = GetTargetSelectorsByMainSeries(mainSeries);
            if (targetSelectorsByMainSeries != null && targetSelectorsByMainSeries.Any())
            {
                List<TargetSelectorDetail> list = new List<TargetSelectorDetail>(targetSelectorsByMainSeries);
                list.Sort();
                return list.Last().TargetSelector;
            }
            if (notifyIfTargetSelectorCouldNotBeFound)
            {
                if (interactionService == null)
                {
                    string text = "You have to pass InteractionService object to the constructor when you want to notify missing TargetSelector";
                    Log.Error(Log.CurrentMethod(), text);
                    throw new ArgumentNullException("interactionService", text);
                }
                interactionService.RegisterMessageAsync(FormatedData.Localize("#Info"), FormatedData.Localize(localizationId));
            }
            return null;
        }

        private IEnumerable<TargetSelectorDetail> GetTargetSelectorsByMainSeries(string mainSeries)
        {
            return targetSelectors.Where((TargetSelectorDetail ts) => string.Equals(mainSeries, ts.TargetSelector.Baureihenverbund, StringComparison.OrdinalIgnoreCase));
        }
    }
}