using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using HANReader.Core;
using HANReader.Core.Models;
using Xunit;
using static HANReader.Tests.TestData;

namespace HANReader.Tests
{
    public class HANSerialPortReaderTests
    {
        [Fact]
        public async Task ShouldHandleFullFrame() =>
            await AssertNumberOfFrames(FullFrame, expectedNumberOfFrames: 1);

        [Fact]
        public async Task ShouldHandleTwoFullFrames() =>
           await AssertNumberOfFrames($"{FullFrame} {FullFrame}", expectedNumberOfFrames: 2);

        [Fact]
        public async Task ShouldHandleFullFrameWithInvalidHeaderChecksumFollowedByValidFrame() =>
            await AssertNumberOfFrames($"{FullFrameWithInvalidHeaderChecksum} {FullFrame}", expectedNumberOfFrames: 1);

        [Fact]
        public async Task ShouldHandleRubbishFollowedByValidFrame() =>
            await AssertNumberOfFrames($"{Rubbish} {FullFrame}", expectedNumberOfFrames: 1);

        [Fact]
        public async Task ShouldHandleEndflagFollowedByValidFrame() =>
            await AssertNumberOfFrames($"{StartFlag} {FullFrame}", expectedNumberOfFrames: 1);

        private static async Task AssertNumberOfFrames(string data, int expectedNumberOfFrames)
        {
            var bytes = ByteHelper.CreateByteArray(data);
            var memoryStream = new MemoryStream(bytes);
            (await ReadFrames(memoryStream)).Should().HaveCount(expectedNumberOfFrames);
            var singleByteStream = new SingleByteStream(bytes);
            (await ReadFrames(singleByteStream)).Should().HaveCount(expectedNumberOfFrames);
        }

        private static async Task<Frame[]> ReadFrames(Stream stream)
        {
            var streamReader = new Core.StreamReader(Console.Error, StreamReaderOptions.Default with { WaitTime = 0 });
            var allFrames = new List<Frame>();
            await streamReader.StartAsync(stream, async (frames) =>
            {
                allFrames.AddRange(frames);
            });
            return allFrames.ToArray();
        }
    }
}