using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EdiabasLib
{
    public static class StreamExtension
    {
        public static bool HasData(this Stream inStream)
        {
            if (inStream == null)
            {
                return false;
            }

            if (inStream is MemoryQueueBufferStream memoryStream)
            {
                return memoryStream.IsDataAvailable();
            }

            if (inStream is EscapeStreamReader escapeStream)
            {
                return escapeStream.IsDataAvailable();
            }
#if Android
            return inStream.IsDataAvailable();
#else
            return inStream.Length > 0;
#endif
        }

        public static int ReadByteAsync(this Stream inStream, ManualResetEvent cancelEvent = null, int timeout = 2000)
        {
            if (inStream == null)
            {
                return -1;
            }

            if (cancelEvent != null)
            {
                if (cancelEvent.WaitOne())
                {
                    return -1;
                }
            }

            if (inStream is MemoryQueueBufferStream memoryStream)
            {
                return memoryStream.ReadByte();
            }

            if (inStream is EscapeStreamReader escapeStream)
            {
                return escapeStream.ReadByte();
            }

            byte[] dataBuffer = new byte[1];
            int result = -1;
            Thread abortThread = null;
            AutoResetEvent threadFinishEvent = null;
            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    threadFinishEvent = new AutoResetEvent(false);
                    Task<int> readTask = inStream.ReadAsync(dataBuffer, 0, dataBuffer.Length, cts.Token);
                    if (cancelEvent != null)
                    {
                        WaitHandle[] waitHandles = { threadFinishEvent, cancelEvent };
                        abortThread = new Thread(() =>
                        {
                            if (WaitHandle.WaitAny(waitHandles, timeout) == 1)
                            {
                                // ReSharper disable once AccessToDisposedClosure
                                cts.Cancel();
                            }
                        });
                        abortThread.Start();
                    }

                    if (!readTask.Wait(timeout))
                    {
                        cts.Cancel();
                    }

                    if (readTask.Status == TaskStatus.RanToCompletion && readTask.Result == dataBuffer.Length &&
                        !cts.IsCancellationRequested)
                    {
                        result = dataBuffer[0];
                    }
                }
            }
            finally
            {
                if (abortThread != null)
                {
                    threadFinishEvent.Set();
                    abortThread.Join();
                }

                threadFinishEvent?.Dispose();
            }

            return result;
        }
    }
}
