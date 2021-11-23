using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using HANReader.Core.Models;

namespace HANReader.Core
{
    public class StreamReader
    {
        private readonly Action<string> _log;

        private readonly FrameReader _HANSequenceReader;
        private readonly StreamReaderOptions _options;

        public StreamReader(TextWriter log, StreamReaderOptions options)
        {
            _log = (message) => log.WriteLine($"{nameof(StreamReader)}: {message}");
            _HANSequenceReader = new FrameReader(log);
            _options = options;
        }

        public async Task StartAsync(Stream stream, Func<Frame[], Task> processFrame = null)
        {
            PipeReader reader = PipeReader.Create(stream);

            while (true)
            {
                await Task.Delay(_options.WaitTime);
                ReadResult readResult = await reader.ReadAtLeastAsync(_options.MinimumBuffer);
                ReadOnlySequence<byte> buffer = readResult.Buffer;
                _log($"Read {readResult.Buffer.Length} from {nameof(ReadResult.Buffer)}");

                var frames = _HANSequenceReader.ReadFromSequence(ref buffer);

                if (frames.Count > 0)
                {
                    await processFrame(frames.ToArray());
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted)
                {
                    break;
                }
            }
        }
    }
}