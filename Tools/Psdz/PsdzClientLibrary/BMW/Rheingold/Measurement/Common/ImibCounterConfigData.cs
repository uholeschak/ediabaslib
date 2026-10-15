namespace BMW.Rheingold.Measurement.Common
{
    public struct ImibCounterConfigData
    {
        private string edge;

        private bool averageEnabled;

        private readonly string coupling;

        private readonly string filter;

        private bool frameEnabled;

        private readonly int frameLength;

        private readonly string function;

        private readonly float level;

        private readonly string range;

        private readonly string source;

        private readonly float timeOut;

        public bool AverageEnabled
        {
            get
            {
                return averageEnabled;
            }
            set
            {
                averageEnabled = value;
            }
        }

        public string Coupling => coupling;

        public string Edge
        {
            get
            {
                return edge;
            }
            set
            {
                edge = value;
            }
        }

        public string Filter => filter;

        public bool FrameEnabled
        {
            get
            {
                return frameEnabled;
            }
            set
            {
                frameEnabled = value;
            }
        }

        public int FrameLength => frameLength;

        public string Function => function;

        public float Level => level;

        public string Range => range;

        public string Source => source;

        public float TimeOut => timeOut;

        public ImibCounterConfigData(bool frameEnabled, bool averageEnabled, int frameLength, float timeOut, string filter, string source, string function, string coupling, string range, float level, string edge)
        {
            this.frameEnabled = frameEnabled;
            this.averageEnabled = averageEnabled;
            this.frameLength = frameLength;
            this.timeOut = timeOut;
            this.filter = filter;
            this.source = source;
            this.function = function;
            this.coupling = coupling;
            this.range = range;
            this.level = level;
            this.edge = edge;
        }
    }
}
