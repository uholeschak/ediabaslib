using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    /// <summary>
    /// This stream maintains data only until the data is read, then it is purged from the stream.
    /// </summary>
    public class MemoryQueueBufferStream : Stream
    {
        /// <summary>
        /// Represents a single write into the MemoryQueueBufferStream.  Each write is a seperate chunk
        /// </summary>
        private class Chunk
        {
            /// <summary>
            /// As we read through the chunk, the start index will increment.  When we get to the end of the chunk,
            /// we will remove the chunk
            /// </summary>
            public int ChunkReadStartIndex { get; set; }

            /// <summary>
            /// Actual Data
            /// </summary>
            public byte[] Data { get; set; }
        }

        //Maintains the streams data.  The Queue object provides an easy and efficient way to add and remove data
        //Each item in the queue represents each write to the stream.  Every call to write translates to an item in the queue
        private readonly Queue<Chunk> _lstBuffers;
        private Mutex _readMutex;
        private AutoResetEvent _writeEvent;

        public bool ChunkMode { get; set; }

        public MemoryQueueBufferStream(bool chunkMode = false)
        {
            _lstBuffers = new Queue<Chunk>();
            _readMutex = new Mutex(false);
            _writeEvent = new AutoResetEvent(false);
            ChunkMode = chunkMode;
        }

        public bool IsDataAvailable(int timeout = 0, ManualResetEvent cancelEvent = null)
        {
            _writeEvent.Reset();
            if (Length > 0)
            {
                return true;
            }

            if (timeout > 0)
            {
                if (cancelEvent != null)
                {
                    WaitHandle[] waitHandles = { _writeEvent, cancelEvent };
                    if (WaitHandle.WaitAny(waitHandles, timeout) == 1)
                    {
                        return false;
                    }
                }
                else
                {
                    _writeEvent.WaitOne(timeout);
                }
                return Length > 0;
            }

            return false;
        }

        /// <summary>
        /// Reads up to count bytes from the stream, and removes the read data from the stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!AcquireReadMutex())
            {
                return -1;
            }

            try
            {
                this.ValidateBufferArgs(buffer, offset, count);

                int iRemainingBytesToRead = count;

                int iTotalBytesRead = 0;

                //Read until we hit the requested count, or until we hav nothing left to read
                while (iTotalBytesRead <= count && _lstBuffers.Count > 0)
                {
                    //Get first chunk from the queue
                    Chunk chunk = _lstBuffers.Peek();

                    //Determine how much of the chunk there is left to read
                    int iUnreadChunkLength = chunk.Data.Length - chunk.ChunkReadStartIndex;

                    //Determine how much of the unread part of the chunk we can actually read
                    int iBytesToRead = Math.Min(iUnreadChunkLength, iRemainingBytesToRead);

                    if (iBytesToRead > 0)
                    {
                        //Read from the chunk into the buffer
                        Buffer.BlockCopy(chunk.Data, chunk.ChunkReadStartIndex, buffer, offset + iTotalBytesRead, iBytesToRead);

                        iTotalBytesRead += iBytesToRead;
                        iRemainingBytesToRead -= iBytesToRead;

                        //If the entire chunk has been read,  remove it
                        if (chunk.ChunkReadStartIndex + iBytesToRead >= chunk.Data.Length)
                        {
                            _lstBuffers.Dequeue();
                        }
                        else
                        {
                            //Otherwise just update the chunk read start index, so we know where to start reading on the next call
                            chunk.ChunkReadStartIndex = chunk.ChunkReadStartIndex + iBytesToRead;
                        }

                        if (ChunkMode)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return iTotalBytesRead;
            }
            finally
            {
                ReleaseReadMutex();
            }
        }

        private void ValidateBufferArgs(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset must be non-negative");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count must be non-negative");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("requested count exceeds available size");
            }
        }

        /// <summary>
        /// Writes data to the stream
        /// </summary>
        /// <param name="buffer">Data to copy into the stream</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!AcquireReadMutex())
            {
                return;
            }

            try
            {
                this.ValidateBufferArgs(buffer, offset, count);

                //We don't want to use the buffer passed in, as it could be altered by the caller
                byte[] bufSave = new byte[count];
                Buffer.BlockCopy(buffer, offset, bufSave, 0, count);

                //Add the data to the queue
                _lstBuffers.Enqueue(new Chunk() { ChunkReadStartIndex = 0, Data = bufSave });
                if (_lstBuffers.Count > 0)
                {
                    _writeEvent.Set();
                }
            }
            finally
            {
                ReleaseReadMutex();
            }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Always returns 0
        /// </summary>
        public override long Position
        {
            get
            {
                //We're always at the start of the stream, because as the stream purges what we've read
                return 0;
            }
            set
            {
                throw new NotSupportedException(this.GetType().Name + " is not seekable");
            }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(this.GetType().Name + " is not seekable");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(this.GetType().Name + " length can not be changed");
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override long Length
        {
            get
            {
                if (!AcquireReadMutex())
                {
                    return 0;
                }

                try
                {
                    if (_lstBuffers == null)
                    {
                        return 0;
                    }

                    if (_lstBuffers.Count == 0)
                    {
                        return 0;
                    }

                    return _lstBuffers.Sum(b => b.Data.Length - b.ChunkReadStartIndex);
                }
                finally
                {
                    ReleaseReadMutex();
                }
            }
        }

        public override void Flush()
        {
        }

        public override void Close()
        {
            try
            {
                if (_readMutex != null)
                {
                    _readMutex.Close();
                    _readMutex = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool AcquireReadMutex(int timeout = 10000)
        {
            try
            {
                if (!_readMutex.WaitOne(timeout))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReleaseReadMutex()
        {
            try
            {
                _readMutex.ReleaseMutex();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
