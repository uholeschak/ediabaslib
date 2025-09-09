using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EdiabasLib
{
    public static class StreamExtension
    {
        public static readonly long TickResolMs = Stopwatch.Frequency / 1000;

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
#if ANDROID
            return inStream.IsDataAvailable();
#else
            if (inStream is NetworkStream networkStream)
            {
                return networkStream.DataAvailable;
            }

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
                if (!memoryStream.IsDataAvailable(timeout, cancelEvent))
                {
                    return 0;
                }

                return memoryStream.Read(buffer, offset, count);
            }

            if (inStream is EscapeStreamReader escapeStream)
            {
                if (!escapeStream.IsDataAvailable(timeout, cancelEvent))
                {
                    return 0;
                }

                return escapeStream.Read(buffer, offset, count);
            }

#if !ANDROID
            if (inStream is NetworkStream)
            {
                // Cancel event is not supported on Windows
                inStream.ReadTimeout = timeout;
                return inStream.Read(buffer, offset, count);
            }
#endif

            long startTime = Stopwatch.GetTimestamp();
            int recLen = 0;
            for (;;)
            {
                if (recLen >= count)
                {
                    break;
                }

                Thread abortThread = null;
                AutoResetEvent threadFinishEvent = null;
                try
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource())
                    {
                        threadFinishEvent = new AutoResetEvent(false);
                        Task<int> readTask = inStream.ReadAsync(buffer, recLen + offset, count - recLen, cts.Token);
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

                        if (readTask.Status != TaskStatus.RanToCompletion || cts.IsCancellationRequested)
                        {
                            return -1;  // aborted
                        }

                        if (cancelEvent != null)
                        {
                            if (cancelEvent.WaitOne(0))
                            {
                                return -1;
                            }
                        }

                        int recBytes = readTask.Result;
                        if (recBytes > 0)
                        {
                            recLen += recBytes;
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

                if (timeout <= 0)
                {
                    break;
                }

                if (Stopwatch.GetTimestamp() - startTime > timeout * TickResolMs)
                {
                    break;
                }
            }

            return recLen;
        }
    }
}
