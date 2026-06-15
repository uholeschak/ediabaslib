using System.IO;

namespace BmwDeepObd
{
    public class AndroidLogWriter : TextWriter
    {
        private readonly string _tag;

        public AndroidLogWriter(string tag)
        {
            _tag = tag;
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void WriteLine(string value)
        {
            Android.Util.Log.Info(_tag, value ?? string.Empty);
        }

        public override void Write(string value)
        {
            Android.Util.Log.Info(_tag, value ?? string.Empty);
        }
    }
}
