using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using HANReader.Core.Models;

namespace HANReader.Core
{
    public class HANStreamReader
    {
        private readonly Action<string> _log;

        private readonly HANSequenceReader _HANSequenceReader;

        public HANStreamReader(TextWriter log)
        {
            _log = (message) => log.WriteLine($"{nameof(HANStreamReader)}: {message}");
            _HANSequenceReader = new HANSequenceReader(log);
        }

        public async Task StartAsync(Stream stream, Func<Frame2[], Task> processFrame = null)
        {
            PipeReader reader = PipeReader.Create(stream);

            while (true)
            {
                ReadResult readResult = await reader.ReadAsync();
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