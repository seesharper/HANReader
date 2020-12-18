using System;
using System.Buffers;

namespace HANReader.Core
{
    public interface IDateTimeReader
    {
        DateTime Read(ref SequenceReader<byte> reader);
    }

    public class DateTimeReader : IDateTimeReader
    {
        public DateTime Read(ref SequenceReader<byte> reader)
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

    }
}