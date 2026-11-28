namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_CCMESSAGE : LocalizedTitle
    {
        public long Id { get; set; }

        public long Cc_id { get; set; }

        public string Longtext { get; set; }

        public string Cause { get; set; }

        public long Inspection { get; set; }

        public long? Warn_light_icon_id { get; set; }

        public long Obsolete { get; set; }

        public string Warn_light_identifier { get; set; }
    }
}
