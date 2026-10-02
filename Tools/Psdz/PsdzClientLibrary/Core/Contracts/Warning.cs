namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public struct Warning
    {
        public string Source { get; set; }

        public string Category { get; set; }

        public string Text { get; set; }

        public string OriginaValue { get; set; }

        public string SubstituteValue { get; set; }

        public Warning(string category, string text)
        {
            this = default(Warning);
            Category = category;
            Text = text ?? "n.a.";
        }
    }
}
