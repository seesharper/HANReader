using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using FluentAssertions;
using HANReader.Core;
using HANReader.Core.Models;
using Newtonsoft.Json;
using Xunit;

namespace HANReader.Tests
{
    public class StructConverter : System.Text.Json.Serialization.JsonConverter<Struct[]>
    {
        public override Struct[] Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {

            throw new NotImplementedException();
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, Struct[] value, System.Text.Json.JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }


    public class HANSerialPortReaderTests
    {
        private string fullFrame = "7E A0 79 01 02 01 10 80 93 E6 E7 00 0F 40 00 00 00 09 0C 07 E4 08 0B 02 0D 36 28 FF 80 00 00 02 0D 09 07 4B 46 4D 5F 30 30 31 09 10 36 39 37 30 36 33 31 34 30 33 30 38 32 38 30 39 09 08 4D 41 33 30 34 48 33 45 06 00 00 08 2D 06 00 00 00 00 06 00 00 00 DA 06 00 00 00 00 06 00 00 07 86 06 00 00 1D 25 06 00 00 1A D3 06 00 00 09 74 06 00 00 00 00 06 00 00 09 6F 88 8B 7E";

        //------------------------------------------------------------------------👇----------------
        private string fullFrameWithInvalidHeaderChecksum = "7E A0 79 01 02 01 10 FF 93 E6 E7 00 0F 40 00 00 00 09 0C 07 E4 08 0B 02 0D 36 28 FF 80 00 00 02 0D 09 07 4B 46 4D 5F 30 30 31 09 10 36 39 37 30 36 33 31 34 30 33 30 38 32 38 30 39 09 08 4D 41 33 30 34 48 33 45 06 00 00 08 2D 06 00 00 00 00 06 00 00 00 DA 06 00 00 00 00 06 00 00 07 86 06 00 00 1D 25 06 00 00 1A D3 06 00 00 09 74 06 00 00 00 00 06 00 00 09 6F 88 8B 7E";

        [Fact]
        public async Task ReadOnlySequence()
        {



            var segment = new MemorySegment<byte>(ByteHelper.CreateByteArray("10 20 30 40 50"));

            var sequence = new ReadOnlySequence<byte>(segment, 0, segment, 5);


            var stream = new SingleByteStream(ByteHelper.CreateByteArray("10 20 30 40 50"));

            PipeReader reader = PipeReader.Create(stream);

            ReadResult result = await reader.ReadAsync();
            var buffer = result.Buffer;
            var startPosition = buffer.Start;

            buffer = buffer.Slice(0, 1);

            reader.AdvanceTo(buffer.Start, buffer.End);

            result = await reader.ReadAsync();
            buffer = result.Buffer;

            sequence = sequence.Slice(2);


            //pipeReader.AdvanceTo(sequence.Start, sequence.End)
            // Means tha we have consumed

            // BufferSegment
            // //var startAndEndSeqment = new ReadOnlySequenceSegment<byte>(ByteHelper.CreateByteArray("10 20 30 40 50"));


            // var sequence = new ReadOnlySequence<byte>(startAndEndSeqment, 0, startAndEndSeqment, 4);

            //sequence = sequence.Slice(2, sequence.End);
        }




        [Fact]
        public async Task ShouldHandleFullFrame()
        {
            var firstframe = ByteHelper.CreateByteArray(fullFrame);
            var testStream = new MemoryStream(firstframe);
            var streamReader = new HANStreamReader(Console.Error);
            int totalFrames = 0;
            await streamReader.StartAsync(testStream, async (frames) =>
            {
                totalFrames += frames.Length;
            });

            totalFrames.Should().Be(1);
        }

        [Fact]
        public async Task ShouldHandleTwoFullFrames()
        {
            var firstframe = ByteHelper.CreateByteArray($"{fullFrame} {fullFrame}");
            var testStream = new SingleByteStream(firstframe);
            var streamReader = new HANStreamReader(Console.Error);
            int totalFrames = 0;
            await streamReader.StartAsync(testStream, async (frames) =>
            {
                totalFrames += frames.Length;
            });

            totalFrames.Should().Be(2);
        }

        [Fact]
        public async Task ShouldHandleTwoFullFramesInSingleBuffer()
        {
            var firstframe = ByteHelper.CreateByteArray($"{fullFrame} {fullFrame}");
            var testStream = new MemoryStream(firstframe);
            var streamReader = new HANStreamReader(Console.Error);
            int totalFrames = 0;
            await streamReader.StartAsync(testStream, async (frames) =>
            {
                totalFrames += frames.Length;
            });

            totalFrames.Should().Be(2);
        }

        [Fact]
        public async Task ShouldHandleFrameWithInvalidChecksumFollowedByValidFrame()
        {
            var firstframe = ByteHelper.CreateByteArray($"{fullFrameWithInvalidHeaderChecksum} {fullFrame}");
            var testStream = new MemoryStream(firstframe);
            var streamReader = new HANStreamReader(Console.Error);
            int totalFrames = 0;
            await streamReader.StartAsync(testStream, async (frames) =>
            {
                totalFrames += frames.Length;
            });

            totalFrames.Should().Be(1);
        }


        [Fact]
        public async Task ShouldHandleRubbishBeforeStartFlag()
        {
            var firstframe = ByteHelper.CreateByteArray($"AA AA AA {fullFrame}");
            var testStream = new SingleByteStream(firstframe);
            var streamReader = new HANStreamReader(Console.Error);
            int totalFrames = 0;
            await streamReader.StartAsync(testStream, async (frames) =>
            {
                totalFrames += frames.Length;
            });

            totalFrames.Should().Be(1);
        }



        [Fact]
        public async Task ShouldHandleStartingOnEndFlag()
        {
            var firstframe = ByteHelper.CreateByteArray($"AA AA AA 7E {fullFrame}");
            var testStream = new SingleByteStream(firstframe);
            var streamReader = new HANStreamReader(Console.Error);
            int totalFrames = 0;
            await streamReader.StartAsync(testStream, async (frames) =>
            {
                totalFrames += frames.Length;
            });

            totalFrames.Should().Be(1);
        }

        [Fact]
        public async Task ShouldHandleInvalidHeaderChecksum()
        {
            var firstframe = ByteHelper.CreateByteArray(fullFrameWithInvalidHeaderChecksum);
            var testStream = new SingleByteStream(firstframe);
            var streamReader = new HANStreamReader(Console.Error);
            int totalFrames = 0;
            await streamReader.StartAsync(testStream, async (frames) =>
            {
                totalFrames += frames.Length;
            });

            totalFrames.Should().Be(0);
        }


        [Fact]
        public async Task ShouldReadFromStream()
        {
            var firstframe = ByteHelper.CreateByteArray(fullFrame);
            var secondFrame = ByteHelper.CreateByteArray(fullFrame);
            var testStream = new TestStream();
            var writeTask = Task.Run(async () =>
            {
                await Task.Delay(500);
                testStream.Write(firstframe);
                await Task.Delay(500);
                testStream.Write(secondFrame);
                await Task.Delay(500);
                testStream.Write(Array.Empty<byte>());
            });

            var r = new HANStreamReader(Console.Error);


            //var reader = new HANSerialPortReader(new FrameReader(new HeaderReader(new Crc16CyclicRedundancyChecker()), new Crc16CyclicRedundancyChecker(), new DateTimeReader(), new PayloadReader()));

            var readerTask = r.StartAsync(testStream, async frame =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(frame));
            });

            await Task.WhenAll(writeTask, readerTask);
        }


        [Fact]
        public async Task ShouldHandleGettinIncompleteFrame()
        {
            var fullFrameBytes = ByteHelper.CreateByteArray(fullFrame);

            var secondPart = fullFrameBytes[10..];

            TestStream testStream = new TestStream();

            var writeTask = Task.Run(async () =>
            {
                var firstPart = fullFrameBytes[0..10];
                await Task.Delay(500);
                testStream.Write(firstPart, 0, firstPart.Length);
                await Task.Delay(500);
                testStream.Write(secondPart, 0, secondPart.Length);
                await Task.Delay(500);
                testStream.Write(Array.Empty<byte>());
            });

            var reader = new HANSerialPortReader(new FrameReader(new HeaderReader(new Crc16CyclicRedundancyChecker()), new Crc16CyclicRedundancyChecker(), new DateTimeReader(), new PayloadReader()));

            var readerTask = reader.StartAsync(testStream, frame =>
           {
               Console.WriteLine(JsonConvert.SerializeObject(frame));
           });

            await Task.WhenAll(writeTask, readerTask);
        }

        [Fact]
        public async Task ShouldHandleRubbishAndThenCompleteFrame()
        {
            var rubbish = new byte[] { 1, 2, 3, 4 };
            var fullFrameBytes = ByteHelper.CreateByteArray(fullFrame);
            TestStream testStream = new();

            var writeTask = Task.Run(async () =>
            {
                await Task.Delay(500);
                testStream.Write(rubbish, 0, rubbish.Length);
                await Task.Delay(500);
                testStream.Write(fullFrameBytes, 0, fullFrameBytes.Length);
                await Task.Delay(500);
                testStream.Write(fullFrameBytes, 0, fullFrameBytes.Length);
                await Task.Delay(500);
                testStream.Write(Array.Empty<byte>());
            });

            var reader = new HANSerialPortReader(new FrameReader(new HeaderReader(new Crc16CyclicRedundancyChecker()), new Crc16CyclicRedundancyChecker(), new DateTimeReader(), new PayloadReader()));

            var readerTask = reader.StartAsync(testStream, frame =>
           {
               Console.WriteLine(JsonConvert.SerializeObject(frame));
           });

            await Task.WhenAll(writeTask, readerTask);
        }


        [Fact]
        public void ShouldSkipToNextFrameWhenFrameChecksumIsInvalid()
        {
            var goodBytes = new byte[] { 126, 160, 121, 1, 2, 1, 16, 128, 147, 230, 231, 0, 15, 64, 0, 0, 0, 9, 12, 7, 229, 4, 29, 4, 20, 1, 0, 255, 128, 0, 0, 2, 13, 9, 7, 75, 70, 77, 95, 48, 48, 49, 9, 16, 54, 57, 55, 48, 54, 51, 49, 52, 48, 51, 48, 56, 50, 56, 48, 57, 9, 8, 77, 65, 51, 48, 52, 72, 51, 69, 6, 0, 0, 23, 17, 6, 0, 0, 0, 0, 6, 0, 0, 0, 126, 6, 0, 0, 0, 0, 6, 0, 0, 13, 175, 6, 0, 0, 86, 177, 6, 0, 0, 86, 32, 6, 0, 0, 9, 93, 6, 0, 0, 0, 0, 6, 0, 0, 9, 30, 79, 233, 126 };
            var fuckbytes = new byte[] { 126, 160, 121, 1, 2, 1, 16, 128, 147, 230, 231, 0, 15, 64, 0, 0, 0, 9, 12, 7, 229, 4, 29, 4, 20, 1, 10, 255, 128, 0, 0, 2, 13, 9, 7, 75, 70, 77, 95, 48, 48, 49, 9, 16, 126, 160, 121, 1, 2, 1, 16, 128, 147, 230, 231, 0, 15, 64, 0, 0, 0, 9, 12, 7, 229, 4, 29, 4, 20, 1, 10, 255, 128, 0, 0, 2, 13, 9, 7, 75, 70, 77, 95, 48, 48, 49, 9, 16, 54, 57, 55, 48, 54, 51, 49, 52, 48, 51, 48, 56, 50, 56, 48, 57, 9, 8, 77, 65, 51, 48, 52, 72, 51, 69, 6, 0, 0, 23, 13, 6, 0, 0, 0 };

            //2021 - 04 - 29T18: 01:12.344143498Z Trying to read buffer(126, 160, 121, 1, 2, 1, 16, 128, 147, 230, 231, 0, 15, 64, 0, 0, 0, 9, 12, 7, 229, 4, 29, 4, 20, 1, 10, 255, 128, 0, 0, 2, 13, 9, 7, 75, 70, 77, 95, 48, 48, 49, 9, 16, 126, 160, 121, 1, 2, 1, 16, 128, 147, 230, 231, 0, 15, 64, 0, 0, 0, 9, 12, 7, 229, 4, 29, 4, 20, 1, 10, 255, 128, 0, 0, 2, 13, 9, 7, 75, 70, 77, 95, 48, 48, 49, 9, 16, 54, 57, 55, 48, 54, 51, 49, 52, 48, 51, 48, 56, 50, 56, 48, 57, 9, 8, 77, 65, 51, 48, 52, 72, 51, 69, 6, 0, 0, 23, 13, 6, 0, 0, 0)

            // 👍 2021-04-29T18:01:02.573915819Z Trying to read buffer (126,160,121,1,2,1,16,128,147,230,231,0,15,64,0,0,0,9,12,7,229,4,29,4,20,1,0,255,128,0,0,2,13,9,7,75,70,77,95,48,48,49,9,16,54,57,55,48,54,51,49,52,48,51,48,56,50,56,48,57,9,8,77,65,51,48,52,72,51,69,6,0,0,23,17,6,0,0,0,0,6,0,0,0,126,6,0,0,0,0,6,0,0,13,175,6,0,0,86,177,6,0,0,86,32,6,0,0,9,93,6,0,0,0,0,6,0,0,9,30,79,233,126)
            var test = goodBytes.Length;
            var test2 = fuckbytes.Length;



            var headerReader = new HeaderReader();
            var sequence = new ReadOnlySequence<byte>(fuckbytes);
            var reader = new SequenceReader<byte>(sequence);
            var couldReaderHeader = headerReader.TryReadHeader(ref reader, out var header);

        }
    }


    //https://www.stevejgordon.co.uk/creating-a-readonlysequence-from-array-data-in-dotnet

    internal class MemorySegment<T> : ReadOnlySequenceSegment<T>
    {
        public MemorySegment(ReadOnlyMemory<T> memory)
        {
            Memory = memory;
        }

        public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
        {
            var segment = new MemorySegment<T>(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };

            Next = segment;

            return segment;
        }
    }
}