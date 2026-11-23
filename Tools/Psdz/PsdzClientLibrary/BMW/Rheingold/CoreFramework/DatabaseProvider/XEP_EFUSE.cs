using System;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_EFUSE : LocalizedTitle
    {
        public long Id { get; set; }

        public string Kurzname { get; set; }

        public string Grobzeichen { get; set; }

        public string Ve_Name { get; set; }

        public string Booletool_validity { get; set; }

        public string Devtext { get; set; }

        public string Passthrough { get; set; }

        public double Ve_Nennwert { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public int Sicherheitsrelevant { get; set; }

        public bool IsPassthrough => !string.IsNullOrWhiteSpace(Passthrough);
    }
}
