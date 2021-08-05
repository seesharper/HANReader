using System.Buffers;
using HANReader.Core.Models;

namespace HANReader.Core
{
    public interface IFrameReader
    {
        ReadStatus ReadFrame(ref ReadOnlySequence<byte> buffer, out Frame frame);
    }
}