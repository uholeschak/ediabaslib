using zlib;

namespace FileDecoder
{
    public class ZOutputStreamMod : ZOutputStream
    {
        protected System.IO.Stream OutRenamed;

        public ZOutputStreamMod(System.IO.Stream outRenamed) : base(outRenamed)
        {
            OutRenamed = outRenamed;
        }

        public ZOutputStreamMod(System.IO.Stream outRenamed, int level) : base(outRenamed, level)
        {
            OutRenamed = outRenamed;
        }

        public override void Write(System.Byte[] b1, int off, int len)
        {
            if (len == 0)
                return;
            int err;
            byte[] b = new byte[b1.Length];
            System.Array.Copy(b1, 0, b, 0, b1.Length);
            z.next_in = b;
            z.next_in_index = off;
            z.avail_in = len;
            do
            {
                z.next_out = buf;
                z.next_out_index = 0;
                z.avail_out = bufsize;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (compress)
                    err = z.deflate(flush_Renamed_Field);
                else
                    err = z.inflate(flush_Renamed_Field);
                if (err != zlibConst.Z_OK && err != zlibConst.Z_STREAM_END)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                OutRenamed.Write(buf, 0, bufsize - z.avail_out);
                if (err == zlibConst.Z_STREAM_END)  // [UH] added
                {
                    break;
                }
            }
            while (z.avail_in > 0 || z.avail_out == 0);
        }

        public override void finish()
        {
            // ReSharper disable once TooWideLocalVariableScope
            int err;
            do
            {
                z.next_out = buf;
                z.next_out_index = 0;
                z.avail_out = bufsize;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (compress)
                {
                    err = z.deflate(zlibConst.Z_FINISH);
                }
                else
                {
                    err = z.inflate(zlibConst.Z_FINISH);
                }
                if (err != zlibConst.Z_STREAM_END && err != zlibConst.Z_OK)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                if (bufsize - z.avail_out > 0)
                {
                    OutRenamed.Write(buf, 0, bufsize - z.avail_out);
                }
                if (err == zlibConst.Z_STREAM_END)  // [UH] added
                {
                    break;
                }
            }
            while (z.avail_in > 0 || z.avail_out == 0);
            try
            {
                Flush();
            }
            catch
            {
                // ignored
            }
        }

    }
}
