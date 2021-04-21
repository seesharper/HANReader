using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using HANReader.Core.Models;

namespace HANReader.Core
{
    public class HANSerialPortReader
    {
        private readonly FrameReader frameReader;

        public HANSerialPortReader(FrameReader frameReader)
        {
            this.frameReader = frameReader;
        }

        public async Task StartAsync(Stream stream, Action<Frame> processFrame = null)
        {
            var reader = PipeReader.Create(stream);

            while (true)
            {
                // We wait here until new data becomes available from the underlying stream
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;
                var test = buffer.Length;
                while (TryReadFrame(ref buffer, out Frame frame))
                {
                    Console.Error.WriteLine("Successfully read frame");
                    processFrame?.Invoke(frame);
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    Console.Error.WriteLine("Result completed");
                    break;
                }
            }
        }

        private bool TryReadFrame(ref ReadOnlySequence<byte> buffer, out Frame frame)
        {
            var bufferString = String.Join(",", buffer.ToArray().Select(p => p.ToString()).ToArray());
            Console.WriteLine($"Trying to read buffer ({bufferString})");
            if (frameReader.TryReadFrame(ref buffer, out var frame2))
            {
                var positionOfNextPossibleFrame = buffer.GetPosition(frame2.Header.StartPosition - 1 + frame2.Header.FrameSize + 2);
                buffer = buffer.Slice(positionOfNextPossibleFrame);
            }
            else
            {
                Console.WriteLine($"Could not read frame ({bufferString})");
            }
            frame = frame2;
            return frame2 != Frame.InvalidFrame;
        }
    }
}