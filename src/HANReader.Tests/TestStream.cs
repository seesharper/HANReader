using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HANReader.Tests
{
    public class TestStream : Stream
    {


        private TaskCompletionSource<byte[]> taskCompletionSource = new TaskCompletionSource<byte[]>();

        public TestStream()
        {

        }

        public override bool CanRead => throw new System.NotImplementedException();

        public override bool CanSeek => throw new System.NotImplementedException();

        public override bool CanWrite => throw new System.NotImplementedException();

        public override long Length => throw new System.NotImplementedException();

        public override long Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override void Flush() => throw new System.NotImplementedException();

        public override int Read(byte[] buffer, int offset, int count) => throw new System.NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new System.NotImplementedException();

        public override void SetLength(long value) => throw new System.NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) => taskCompletionSource.SetResult(buffer);

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var bytes = await taskCompletionSource.Task;
            bytes.CopyTo(buffer, offset);
            taskCompletionSource = taskCompletionSource = new TaskCompletionSource<byte[]>();
            return bytes.Length;
        }
    }

    public class SingleByteStream : Stream
    {
        private readonly byte[] bytes;

        private long bytesRead = 0;

        public SingleByteStream(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public override bool CanRead => true;

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

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

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}