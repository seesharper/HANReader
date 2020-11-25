using System.Buffers;
using HANReader.Core.Models;

namespace HANReader.Core
{
    public interface IFrameReader
    {
        bool TryReadFrame(ref ReadOnlySequence<byte> buffer, out Frame frame);
    }
}