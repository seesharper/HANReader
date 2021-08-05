using System;
using System.Buffers;
using System.Globalization;
using FluentAssertions;
using HANReader.Core;
using HANReader.Core.Models;
using Xunit;

namespace HANReader.Tests
{
    public class TestData
    {
        public const string StartFlagOnly = "7E";
        public const string StartFlagOnlyWithPrecidingByte = "FF 7E";
        public const string OnlyOneByteFromFrameFormat = "7E A0";
    }


    public class HeaderReaderTests
    {
        private string sampledata = "7E A0 79 01 02 01 10 80 93 E6 E7 00 0F 40 00 00 00 09 0C 07 E4 08 0B 02 0D 36 28 FF 80 00 00 02 0D 09 07 4B 46 4D 5F 30 30 31 09 10 36 39 37 30 36 33 31 34 30 33 30 38 32 38 30 39 09 08 4D 41 33 30 34 48 33 45 06 00 00 08 2D 06 00 00 00 00 06 00 00 00 DA 06 00 00 00 00 06 00 00 07 86 06 00 00 1D 25 06 00 00 1A D3 06 00 00 09 74 06 00 00 00 00 06 00 00 09 6F 88 8B 7E 7E A0 27 01 02 01 10 5A 87 E6 E7 00 0F 40 00 00 00 09 0C 07 E4 08 0B 02 0D 36 2A FF 80 00 00 02 01 06 00 00 08 21 71 E7 7E";

        private string fullFrame = "7E A0 79 01 02 01 10 80 93 E6 E7 00 0F 40 00 00 00 09 0C 07 E4 08 0B 02 0D 36 28 FF 80 00 00 02 0D 09 07 4B 46 4D 5F 30 30 31 09 10 36 39 37 30 36 33 31 34 30 33 30 38 32 38 30 39 09 08 4D 41 33 30 34 48 33 45 06 00 00 08 2D 06 00 00 00 00 06 00 00 00 DA 06 00 00 00 00 06 00 00 07 86 06 00 00 1D 25 06 00 00 1A D3 06 00 00 09 74 06 00 00 00 00 06 00 00 09 6F 88 8B 7E";

        [Fact]
        public void ShouldReaderValidHeader()
        {
            var result = TryReadHeader(sampledata, out var header);
            result.Should().Be(ReadStatus.Complete);
            header.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldHandleStartFlagOnly()
        {
            var result = TryReadHeader("7E", out var header);
            result.Should().Be(ReadStatus.InComplete);
            header.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldHandleStartFlagWithPrecidingByte()
        {
            var result = TryReadHeader("FF 7E", out var header);
            result.Should().Be(ReadStatus.InComplete);
            header.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldOnlyOneByteFromFrameFormat()
        {
            var result = TryReadHeader("7E A0", out var header);
            result.Should().Be(ReadStatus.InComplete);
            header.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldHandleOnlyFrameFormat()
        {
            var result = TryReadHeader("7E A0 79", out var header);
            result.Should().Be(ReadStatus.InComplete);
            header.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldHandleOnlyCLientAddress()
        {
            var result = TryReadHeader("7E A0 79 01", out var header);
            result.Should().Be(ReadStatus.InComplete);
            header.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldHandleOnlyFirstByteOfExtendedAddress()
        {
            var result = TryReadHeader("7E A0 79 01 02", out var header);
            result.Should().Be(ReadStatus.InComplete);
            header.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldHandleOnlyAddresses()
        {
            var result = TryReadHeader("7E A0 79 01 02 01", out var header);
            result.Should().Be(ReadStatus.InComplete);
            header.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldHandleOnlyControlByte()
        {
            var result = TryReadHeader("7E A0 79 01 02 01 10", out var header);
            result.Should().Be(ReadStatus.InComplete);
            header.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldHandleOnlyFirstByteOfChecksum()
        {
            var result = TryReadHeader("7E A0 79 01 02 01 10 80", out var header);
            result.Should().Be(ReadStatus.InComplete);
            header.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ShouldHandleCompleteChecksum()
        {
            var result = TryReadHeader("7E A0 79 01 02 01 10 80 93", out var header);
            result.Should().Be(ReadStatus.Complete);
            header.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldHandleInvalidChecksum()
        {
            var result = TryReadHeader("7E A0 79 01 02 01 10 80 FF", out var header);
            result.Should().Be(ReadStatus.InvalidChecksum);
            header.IsValid.Should().BeFalse();
        }

        private ReadStatus TryReadHeader(string data, out Header header)
        {
            var headerReader = new HeaderReader();
            var sequence = new ReadOnlySequence<byte>(CreateByteArray(data));
            var reader = new SequenceReader<byte>(sequence);
            var result = headerReader.TryReadHeader(ref reader, out var readHeader);
            header = readHeader;
            return result;
        }

        private static byte[] CreateByteArray(string data)
        {
            var stringBytes = data.Split(" ");
            byte[] bytes = new byte[stringBytes.Length];
            for (int i = 0; i < stringBytes.Length; i++)
            {
                bytes[i] = Byte.Parse(stringBytes[i], NumberStyles.HexNumber);
            }

            return bytes;
        }
    }
}