using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public class PerceivedSymptomWithSource
    {
        public const string SourceAutoVorbelegt = "automatisch; vorbelegt";

        public const string SourceManual = "manuell";

        [PreserveSource(Hint = "XEP_PERCEIVEDSYMPTOMSEX", Placeholder = true)]
        public PlaceholderType Symptom { get; set; }

        public string Source { get; set; }
    }
}
