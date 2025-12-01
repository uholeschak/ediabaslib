using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Globalization;

namespace PsdzClient.Programming
{
    internal class TalLineHelper
    {
        private const string Smac_Identifier = "SMAC";

        public IDictionary<IPsdzEcuIdentifier, IList<IPsdzTalLine>> TalLines { get; private set; }

        public TalLineHelper(IPsdzTal tal)
        {
            TalLines = new Dictionary<IPsdzEcuIdentifier, IList<IPsdzTalLine>>();
            FilterTalLines(tal);
        }

        private void FilterTalLines(IPsdzTal tal)
        {
            foreach (IPsdzTalLine talLine in tal.TalLines)
            {
                AddTalLineToEcuIdentifier(talLine.EcuIdentifier, talLine);
                FilterSmacTransferTalLines(talLine);
            }
        }

        private void FilterSmacTransferTalLines(IPsdzTalLine talLine)
        {
            if (talLine.TaCategory != null && (talLine.TaCategories == PsdzTaCategories.SmacTransferStart || talLine.TaCategories == PsdzTaCategories.SmacTransferStatus))
            {
                Log.Info(Log.CurrentMethod(), $"Found SmacTransfer TalLine '{talLine.Id}'");
                if (talLine.TaCategories == PsdzTaCategories.SmacTransferStart)
                {
                    AddSmacTransferStarts(talLine);
                }
                else
                {
                    AddSmacTransferStatuses(talLine);
                }
            }
        }

        private void AddSmacTransferStarts(IPsdzTalLine talLine)
        {
            foreach (IPsdzTa ta in talLine.TaCategory.Tas)
            {
                if (!(ta is PsdzSmacTransferStartTA psdzSmacTransferStartTA))
                {
                    continue;
                }
                foreach (KeyValuePair<string, IList<IPsdzSgbmId>> smartActuatorDatum in psdzSmacTransferStartTA.SmartActuatorData)
                {
                    PsdzEcuIdentifier identifier = new PsdzEcuIdentifier
                    {
                        BaseVariant = "SMAC",
                        DiagnosisAddress = CalculateSmacDiagAddress(talLine.EcuIdentifier.DiagnosisAddress, smartActuatorDatum.Key)
                    };
                    AddTalLineToEcuIdentifier(identifier, talLine);
                }
            }
        }

        private void AddSmacTransferStatuses(IPsdzTalLine talLine)
        {
            foreach (IPsdzTa ta in talLine.TaCategory.Tas)
            {
                if (!(ta is PsdzSmacTransferStatusTA psdzSmacTransferStatusTA))
                {
                    continue;
                }
                foreach (string smartActuatorID in psdzSmacTransferStatusTA.SmartActuatorIDs)
                {
                    PsdzEcuIdentifier identifier = new PsdzEcuIdentifier
                    {
                        BaseVariant = "SMAC",
                        DiagnosisAddress = CalculateSmacDiagAddress(talLine.EcuIdentifier.DiagnosisAddress, smartActuatorID)
                    };
                    AddTalLineToEcuIdentifier(identifier, talLine);
                }
            }
        }

        private void AddTalLineToEcuIdentifier(IPsdzEcuIdentifier identifier, IPsdzTalLine talLine)
        {
            if (TalLines.ContainsKey(identifier))
            {
                TalLines[identifier].Add(talLine);
                return;
            }
            TalLines.Add(identifier, new List<IPsdzTalLine> { talLine });
        }

        public PsdzDiagAddress CalculateSmacDiagAddress(IPsdzDiagAddress master, string smacID)
        {
            int offset = int.Parse(master.Offset.ToString("X") + smacID, NumberStyles.HexNumber);
            return new PsdzDiagAddress
            {
                Offset = offset
            };
        }
    }
}