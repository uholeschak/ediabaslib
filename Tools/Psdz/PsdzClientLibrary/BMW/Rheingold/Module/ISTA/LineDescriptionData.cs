using System.Globalization;
using System.Windows;

namespace BMW.Rheingold.Module.ISTA
{
    public struct LineDescriptionData
    {
        public Point TextPosition { get; set; }

        public double Content { get; set; }

        public double DividerEndPoint { get; set; }

        public string Text => Content.ToString(CultureInfo.InvariantCulture);
    }
}
