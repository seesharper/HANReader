using System.Buffers;
using HANReader.Core.Models;

namespace HANReader.Core
{
    public interface IHeaderReader
    {
        ReadStatus TryReadHeader(ref SequenceReader<byte> sequenceReader, out Header header);
    }
}
namespace HANReader.Core
{


    public class Header
    {
        public static Header InvalidHeader = new Header();


        public readonly long FrameSize;
        public readonly long Length;

        public readonly long StartPosition;

        public bool IsValid
        {
            get => Length != 0;
        }

        private Header()
        {

        }

        public Header(long frameSize, long startFlagPosition, long length)
        {
            FrameSize = frameSize;
            StartPosition = startFlagPosition;
            Length = length;
        }
    }
}
