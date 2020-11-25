using System;
using System.Buffers;
using HANReader.Core.Models;

namespace HANReader.Core
{


    public class FrameReader : IFrameReader
    {
        private const int FRAME_CHECK_SEQUENCE_LENGTH = 2;

        private readonly IHeaderReader headerReader;
        private readonly ICyclicRedundancyChecker cyclicRedundancyChecker;
        private readonly IDateTimeReader dateTimeReader;
        private readonly IPayloadReader payLoadReader;

        public FrameReader(IHeaderReader headerReader, ICyclicRedundancyChecker cyclicRedundancyChecker, IDateTimeReader dateTimeReader, IPayloadReader payLoadReader)
        {
            this.headerReader = headerReader;
            this.cyclicRedundancyChecker = cyclicRedundancyChecker;
            this.dateTimeReader = dateTimeReader;
            this.payLoadReader = payLoadReader;
        }

        public bool TryReadFrame(ref ReadOnlySequence<byte> buffer, out Frame frame)
        {
            var sequenceReader = new SequenceReader<byte>(buffer);

            frame = Frame.InvalidFrame;

            if (!headerReader.TryReadHeader(ref sequenceReader, out var header))
            {
                return false;
            }

            var frameSizeIncludingOpeningAndEndingFlags = header.FrameSize + 2;

            // Ensure that we have a complete frame.
            if (sequenceReader.Remaining < (frameSizeIncludingOpeningAndEndingFlags - sequenceReader.Consumed))
            {
                return false;
            }

            /// Check
            var frameBody = sequenceReader.Sequence.Slice(header.StartPosition, header.FrameSize - FRAME_CHECK_SEQUENCE_LENGTH).ToArray();
            var frameCheckSequenceBytes = sequenceReader.Sequence.Slice((header.StartPosition + header.FrameSize) - 2, FRAME_CHECK_SEQUENCE_LENGTH).ToArray();
            var frameCheckSequence = (ushort)(frameCheckSequenceBytes[1] << 8 | frameCheckSequenceBytes[0]);

            if (!cyclicRedundancyChecker.Check(frameCheckSequence, frameBody))
            {
                return false;
            }

            // Advance past the LLC PDU section. Nothing relevant here.
            sequenceReader.Advance(8);

            // Skip reading this byte since not all meters sends this byte
            // 09 which indicates the COSEM datatype "octet-string" is propably found to be redundant as we will always have a timestamp here.
            if (sequenceReader.TryPeek(out var dataType))
            {
                if (dataType == 0x09)
                {
                    sequenceReader.Advance(1);
                }
            }

            var timeStamp = dateTimeReader.Read(ref sequenceReader);

            var payload = payLoadReader.Read(ref sequenceReader);

            frame = new Frame(header, timeStamp, payload);

            // Advance past the frame check sequence bytes and the end flag;
            sequenceReader.Advance(3);

            return true;
        }
    }
}