using System;
using System.Buffers;
using System.IO.Pipelines;
using HANReader.Core.Models;

namespace HANReader.Core
{

    public class HeaderReader : IHeaderReader
    {
        private const byte START_FLAG = 0x7E;
        private readonly ICyclicRedundancyChecker cyclicRedundancyChecker;

        public HeaderReader() : this(new Crc16CyclicRedundancyChecker())
        {
        }

        public HeaderReader(ICyclicRedundancyChecker cyclicRedundancyChecker)
        {
            this.cyclicRedundancyChecker = cyclicRedundancyChecker;
        }

        public ReadStatus TryReadHeader(ref SequenceReader<byte> sequenceReader, out Header header)
        {
            header = Header.InvalidHeader;

            if (!sequenceReader.TryAdvanceTo(START_FLAG))
            {
                return ReadStatus.NotFound;
            }

            var headerStartPosition = sequenceReader.Consumed;

            if (sequenceReader.Remaining < 2)
            {
                return ReadStatus.InComplete;
            }

            sequenceReader.TryRead(out var firstFrameFormatByte);
            sequenceReader.TryRead(out var secondFrameFormatByte);
            int frameFormat = firstFrameFormatByte << 8 | secondFrameFormatByte;

            // Right shift the frame size and segmentation bit (12 bits) leaving only the frame format type (4 bits).
            var frameFormatType = frameFormat >> 12;

            // Get the frame size from the last 11 bits.
            var frameSize = frameFormat & 0b0000011111111111;

            if (!TryReadPastAddress(ref sequenceReader) || !TryReadPastAddress(ref sequenceReader))
            {
                return ReadStatus.InComplete;
            }

            // Ensure that we have at least the control byte and the two bytes containing the check sum
            if (sequenceReader.Remaining < 3)
            {
                return ReadStatus.InComplete;
            }

            // Advance past the control byte
            sequenceReader.Advance(1);

            sequenceReader.TryRead(out var firstHeaderCheckSequenceByte);
            sequenceReader.TryRead(out var secondHeaderCheckSequenceByte);

            var headerCheckSequence = (ushort)(secondHeaderCheckSequenceByte << 8 | firstHeaderCheckSequenceByte);

            var headerBody = sequenceReader.Sequence.Slice(headerStartPosition, (sequenceReader.Consumed - headerStartPosition) - 2).ToArray();
            if (!cyclicRedundancyChecker.Check(headerCheckSequence, headerBody))
            {
                return ReadStatus.InvalidChecksum;
            }

            var frameSizeIncludingOpeningAndEndingFlags = frameSize + 2;

            // Ensure that we have a complete frame.
            // if (sequenceReader.Remaining < (frameSizeIncludingOpeningAndEndingFlags - sequenceReader.Consumed))
            // {
            //     return false;
            // }


            // var frameBody = sequenceReader.Sequence.Slice(headerStartPosition, frameSize - 2).ToArray();
            // var frameCheckSequenceBytes = sequenceReader.Sequence.Slice((headerStartPosition - 2) + frameSize, 2).ToArray();
            // var frameCheckSequence = (ushort)(frameCheckSequenceBytes[1] << 8 | frameCheckSequenceBytes[0]);

            // if (!cyclicRedundancyChecker.Check(frameCheckSequence, frameBody))
            // {
            //     return false;
            // }

            var headerSize = sequenceReader.Consumed - headerStartPosition;
            header = new Header(frameSize, headerStartPosition, headerSize);
            return ReadStatus.Complete;
        }

        private bool TryReadPastAddress(ref SequenceReader<byte> sequenceReader)
        {
            int bytesRead = 0;
            while (sequenceReader.TryRead(out var addressByte))
            {
                bytesRead++;

                if ((addressByte & 0b00000001) == 1)
                {
                    break;
                }
            }

            return bytesRead > 0;
        }
    }
}