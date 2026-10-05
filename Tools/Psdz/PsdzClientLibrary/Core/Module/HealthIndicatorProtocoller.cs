using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;

namespace BMW.Rheingold.Module.ISTA
{
    internal class HealthIndicatorProtocoller
    {
        private readonly IList<string> m_Languages;

        private readonly Dictionary<string, StringBuilder> m_FastaMessagePerLanguage = new Dictionary<string, StringBuilder>();

        private List<LocalizedText> m_ProtocolLocalized = new List<LocalizedText>();

        private List<IEnumerable<BarProtocolData>> m_BarProtocolData = new List<IEnumerable<BarProtocolData>>();

        private string m_PriorText = string.Empty;

        private string m_PastText = string.Empty;

        public Dictionary<string, StringBuilder> FastaMessagePerLanguage => m_FastaMessagePerLanguage;

        public IList<LocalizedText> ProtocolLocalized => m_ProtocolLocalized;

        public bool HasData => m_BarProtocolData.Any();

        public HealthIndicatorProtocoller(IList<string> languages)
        {
            m_Languages = languages;
            m_Languages.ForEach(delegate (string x)
            {
                m_FastaMessagePerLanguage.Add(x, new StringBuilder());
            });
        }

        public void CollectBarProtocolData(IEnumerable<BarProtocolData> newBarProtocolData)
        {
            m_BarProtocolData.Add(newBarProtocolData);
        }

        public void EndCollectingBarProtocolData(string priorText, string pastText)
        {
            m_PriorText = priorText;
            m_PastText = pastText;
            if (!m_BarProtocolData.Any())
            {
                return;
            }
            foreach (KeyValuePair<string, StringBuilder> item in m_FastaMessagePerLanguage)
            {
                ProtocolHeader(item.Value);
                ProtocolAllValues(item.Value);
                ProtocolFooter(item.Value);
            }
            TextContent textContent = new TextContent(m_FastaMessagePerLanguage.Select((KeyValuePair<string, StringBuilder> x) => new LocalizedText(x.Value.ToString(), x.Key)).ToList());
            m_ProtocolLocalized = textContent.GetTextForUI(m_Languages).ToList();
        }

        private void ProtocolHeader(StringBuilder fastaMessage)
        {
            if (!m_BarProtocolData.Any())
            {
                return;
            }
            fastaMessage.Append("<spe:TEXTITEM xmlns:spe=\"http://bmw.com/2014/Spe_Text_2.0\">");
            if (!string.IsNullOrEmpty(m_PriorText))
            {
                fastaMessage.Append("<spe:PARAGRAPH>");
                fastaMessage.Append(m_PriorText);
                fastaMessage.Append("</spe:PARAGRAPH>");
            }
            IEnumerable<BarProtocolData> enumerable = m_BarProtocolData.First();
            fastaMessage.Append("<spe:TABLE><spe:TGROUP><spe:THEAD><spe:HEADROW><spe:HEADENTRY>");
            fastaMessage.Append("Zeit");
            fastaMessage.Append("</spe:HEADENTRY>");
            foreach (BarProtocolData item in enumerable)
            {
                ProtocolHeaderEntry(item, fastaMessage);
            }
            fastaMessage.Append("</spe:HEADROW></spe:THEAD><spe:TBODY>");
        }

        private void ProtocolHeaderEntry(BarProtocolData barDataEntry, StringBuilder fastaMessage)
        {
            fastaMessage.Append("<spe:HEADENTRY>");
            fastaMessage.Append("B" + barDataEntry.Index.ToString("00"));
            fastaMessage.Append("</spe:HEADENTRY>");
        }

        private void ProtocolAllValues(StringBuilder fastaMessage)
        {
            foreach (IEnumerable<BarProtocolData> barProtocolDatum in m_BarProtocolData)
            {
                ProtocolValues(barProtocolDatum, fastaMessage);
            }
        }

        private void ProtocolValues(IEnumerable<BarProtocolData> barData, StringBuilder fastaMessage)
        {
            fastaMessage.Append("<spe:ROW>");
            fastaMessage.Append("<spe:ENTRY>");
            fastaMessage.Append(DateTime.Now.ToString("HH:mm:ss.ff"));
            fastaMessage.Append("</spe:ENTRY>");
            foreach (BarProtocolData barDatum in barData)
            {
                ProtocolValue(barDatum.BarValue, fastaMessage);
            }
            fastaMessage.Append("</spe:ROW>");
        }

        private void ProtocolValue(double value, StringBuilder fastaMessage)
        {
            fastaMessage.Append("<spe:ENTRY>");
            fastaMessage.Append(value.ToString("0.00"));
            fastaMessage.Append("</spe:ENTRY>");
        }

        private void ProtocolFooter(StringBuilder fastaMessage)
        {
            fastaMessage.Append("</spe:TBODY></spe:TGROUP></spe:TABLE>");
            if (!string.IsNullOrEmpty(m_PastText))
            {
                fastaMessage.Append("<spe:PARAGRAPH>");
                fastaMessage.Append(m_PastText);
                fastaMessage.Append("</spe:PARAGRAPH>");
            }
            fastaMessage.Append("</spe:TEXTITEM>");
        }
    }
}
