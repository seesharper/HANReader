using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HANReader.Core.Models;

namespace HANReader.Core
{
    public class FrameReader
    {
        private const int CHECK_SEQUENCE_LENGTH = 2;

        private readonly Action<string> _log;

        private readonly ICyclicRedundancyChecker cyclicRedundancyChecker = new Crc16CyclicRedundancyChecker();

        public FrameReader(TextWriter log)
        {
            _log = (message) => log.WriteLine($"{nameof(FrameReader)}: {message}");
        }

        public IReadOnlyCollection<Frame> ReadFromSequence(ref ReadOnlySequence<byte> buffer)
        {
            const byte START_FLAG = 0x7E;
            var sequenceReader = new SequenceReader<byte>(buffer);
            var frames = new List<Frame>();
            long totallyConsumed = 0;

            while (sequenceReader.Remaining > 0)
            {
                if (!sequenceReader.TryAdvanceTo(START_FLAG))
                {
                    sequenceReader.AdvanceToEnd();
                }
                else
                {
                    // If the next byte is also equal to START_FLAG it means that
                    // we read the previous byte was actually the end flag
                    if (sequenceReader.TryPeek(out var nextByte) && nextByte == START_FLAG)
                    {
                        continue;
                    }

                    var headerStartPosition = sequenceReader.Consumed;
                    var startFlagPosition = sequenceReader.Consumed - 1;


                    if (sequenceReader.Remaining < 2)
                    {
                        break;
                    }

                    sequenceReader.TryRead(out var firstFrameFormatByte);
                    sequenceReader.TryRead(out var secondFrameFormatByte);

                    int frameFormat = firstFrameFormatByte << 8 | secondFrameFormatByte;

                    var frameFormatType = frameFormat >> 12;

                    // Get the frame size from the last 11 bits.
                    // Even though we now technically know the whole
                    // frame size, we must confirm the checksum
                    var frameSize = frameFormat & 0b0000011111111111;

                    if (!TryReadAddress(ref sequenceReader, out var clientAddress))
                    {
                        break;
                    }

                    if (!TryReadAddress(ref sequenceReader, out var serverAddress))
                    {
                        break;
                    }

                    // Ensure that we have at least the control byte and the two bytes containing the check sum
                    if (sequenceReader.Remaining < 3)
                    {
                        break;
                    }

                    // Advance past the control byte
                    sequenceReader.Advance(1);

                    sequenceReader.TryRead(out var firstHeaderCheckSequenceByte);
                    sequenceReader.TryRead(out var secondHeaderCheckSequenceByte);

                    var headerCheckSequence = (ushort)(secondHeaderCheckSequenceByte << 8 | firstHeaderCheckSequenceByte);

                    var headerBody = sequenceReader.Sequence.Slice(headerStartPosition, sequenceReader.Consumed - headerStartPosition - 2).ToArray();

                    if (!cyclicRedundancyChecker.Check(headerCheckSequence, headerBody))
                    {
                        _log("Invalid header checksum");
                        continue;
                    }

                    // At this point we have a valid header

                    var frameSizeIncludingOpeningAndEndingFlags = frameSize + 2;

                    if (sequenceReader.Remaining < (frameSizeIncludingOpeningAndEndingFlags - sequenceReader.Consumed + startFlagPosition))
                    {
                        // Now enough bytes read to read a whole frame
                        break;
                    }

                    _log("Getting framebody");
                    var frameBody = sequenceReader.Sequence.Slice(headerStartPosition, frameSize - CHECK_SEQUENCE_LENGTH).ToArray();
                    _log("Getting frame sequence bytes");
                    var frameCheckSequenceBytes = sequenceReader.Sequence.Slice(headerStartPosition + frameSize - CHECK_SEQUENCE_LENGTH, CHECK_SEQUENCE_LENGTH).ToArray();
                    var frameCheckSequence = (ushort)(frameCheckSequenceBytes[1] << 8 | frameCheckSequenceBytes[0]);


                    if (!cyclicRedundancyChecker.Check(frameCheckSequence, frameBody))
                    {
                        // The frame body has an invalid checksum so we just slice the
                        // buffer down to what we have already consumed.
                        _log("Invalid frame checksum");
                        //buffer = buffer.Slice(sequenceReader.Consumed);
                        continue;
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

                    var timeStamp = ReadTimestamp(ref sequenceReader);

                    var payload = ReadField(ref sequenceReader);

                    //Advance past the frame check sequence and the end flag (7E)
                    sequenceReader.Advance(3);
                    totallyConsumed = sequenceReader.Consumed;
                    frames.Add(new Frame(timeStamp, frameSize, payload));
                }
            }

            buffer = buffer.Slice(totallyConsumed);

            return frames.AsReadOnly();
        }

        private static bool TryReadAddress(ref SequenceReader<byte> sequenceReader, out byte[] addressBytes)
        {
            var bytesRead = new List<byte>();
            while (sequenceReader.TryRead(out var addressByte))
            {
                bytesRead.Add(addressByte);
                if ((addressByte & 0b00000001) == 1)
                {
                    break;
                }
            }

            addressBytes = bytesRead.ToArray();

            return bytesRead.Count > 0;
        }

        private static DateTime ReadTimestamp(ref SequenceReader<byte> reader)
        {
            // Advance past the length as it is always 12 bytes here.
            //reader.TryRead(out var length);
            reader.Advance(1);
            reader.TryRead(out var firstYearByte);
            reader.TryRead(out var secondYearByte);

            var year = firstYearByte << 8 | secondYearByte;
            reader.TryRead(out var month);
            reader.TryRead(out var dayOfMonth);
            reader.TryRead(out var dayOfWeek);
            reader.TryRead(out var hour);
            reader.TryRead(out var minute);
            reader.TryRead(out var second);
            reader.TryRead(out var hundredths_of_second);
            reader.TryRead(out var deviation_high_byte);
            reader.TryRead(out var deviation_low_byte);
            var deviation = deviation_high_byte << 8 | deviation_low_byte;
            reader.TryRead(out var clock_status);

            var dateTime = new DateTime(year, month, dayOfMonth, hour, minute, second);
            return dateTime;
            //return new DateTime(year, month, dayOfMonth, hour, minute, second, DateTimeKind.Local).ToUniversalTime();
        }

        private Field ReadField(ref SequenceReader<byte> reader)
        {
            reader.TryRead(out byte dataTypeByte);
            var dataType = (DataType)dataTypeByte;

            if (dataType == DataType.Struct)
            {
                reader.TryRead(out var numberOfElements);
                var fields = new List<Field>();
                for (int i = 0; i < numberOfElements; i++)
                {
                    fields.Add(ReadField(ref reader));
                }

                return new Struct(fields.ToArray());
            }
            else if (dataType == DataType.OctetString)
            {
                reader.TryRead(out var stringLength);
                Span<byte> stackBuffer = stackalloc byte[stringLength];
                reader.TryCopyTo(stackBuffer);
                var value = Encoding.ASCII.GetString(stackBuffer);
                reader.Advance(stringLength);
                return new OctetString(value);
            }
            else if (dataType == DataType.UnsignedInt32)
            {
                Span<byte> stackBuffer = stackalloc byte[4];
                reader.TryCopyTo(stackBuffer);
                stackBuffer.Reverse();
                var value = BitConverter.ToUInt32(stackBuffer);
                reader.Advance(4);
                return new UnsignedInt32(value);
            }
            else
            {
                throw new InvalidOperationException($"Invalid datatype {dataTypeByte}");
            }
        }
    }
}