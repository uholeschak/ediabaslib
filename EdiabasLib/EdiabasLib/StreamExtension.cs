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
            byte[] dataBuffer = new byte[1];
            int result = ReadBytesAsync(inStream, dataBuffer, 0, dataBuffer.Length, cancelEvent, timeout);
            if (result != 1)
            {
                return -1;
            }

            return dataBuffer[0];
        }

        public static int ReadBytesAsync(this Stream inStream, byte[] buffer, int offset, int count, ManualResetEvent cancelEvent = null, int timeout = 2000)
        {
            if (inStream == null)
            {
                return -1;
            }

            if (cancelEvent != null)
            {
                if (cancelEvent.WaitOne(0))
                {
                    return -1;
                }
            }

            if (inStream is MemoryQueueBufferStream memoryStream)
            {
                return memoryStream.Read(buffer, offset, count);
            }

            if (inStream is EscapeStreamReader escapeStream)
            {
                return escapeStream.Read(buffer, offset, count);
            }

            int result = -1;
            Thread abortThread = null;
            AutoResetEvent threadFinishEvent = null;
            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    threadFinishEvent = new AutoResetEvent(false);
                    Task<int> readTask = inStream.ReadAsync(buffer, 0, count, cts.Token);
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

                    if (readTask.Status == TaskStatus.RanToCompletion && !cts.IsCancellationRequested)
                    {
                        result = readTask.Result;
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
