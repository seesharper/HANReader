using System;
using System.IO;

namespace HANReader.Tests
{
    /// <summary>
    /// A <see cref="Stream"/> implementation that returns one byte for each read.
    /// </summary>
    public class SingleByteStream : Stream
    {
        private readonly byte[] bytes;

        private long bytesRead = 0;

        public SingleByteStream(byte[] bytes) => this.bytes = bytes;

        public override bool CanRead => true;

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush() => throw new NotImplementedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (bytesRead == bytes.Length)
            {
                return 0;
            }
            buffer[offset] = bytes[bytesRead];
            bytesRead++;
            return 1;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    }
}