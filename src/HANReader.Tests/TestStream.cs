using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HANReader.Tests
{
    public class TestStream : Stream
    {
        private bool isClosed = false;

        private TaskCompletionSource<byte[]> taskCompletionSource = new TaskCompletionSource<byte[]>();

        public TestStream()
        {

        }

        public override bool CanRead => throw new System.NotImplementedException();

        public override bool CanSeek => throw new System.NotImplementedException();

        public override bool CanWrite => throw new System.NotImplementedException();

        public override long Length => throw new System.NotImplementedException();

        public override long Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            taskCompletionSource.SetResult(buffer);
        }

        // public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        // {
        //     return base.ReadAsync(buffer, cancellationToken);
        // }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // if (buffer.)
            // {
            //     return 0;
            // }

            var bytes = await taskCompletionSource.Task;
            bytes.CopyTo(buffer, offset);
            taskCompletionSource = taskCompletionSource = new TaskCompletionSource<byte[]>();
            return bytes.Length;
        }

        public override void Close()
        {
            this.isClosed = true;
            base.Close();
        }
    }
}