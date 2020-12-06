using System;
using System.Text.Json;
using System.Threading.Tasks;
using HANReader.Core;
using Newtonsoft.Json;
using Xunit;

namespace HANReader.Tests
{
    public class HANSerialPortReaderTests
    {
        private string fullFrame = "AA 7E A0 79 01 02 01 10 80 93 E6 E7 00 0F 40 00 00 00 09 0C 07 E4 08 0B 02 0D 36 28 FF 80 00 00 02 0D 09 07 4B 46 4D 5F 30 30 31 09 10 36 39 37 30 36 33 31 34 30 33 30 38 32 38 30 39 09 08 4D 41 33 30 34 48 33 45 06 00 00 08 2D 06 00 00 00 00 06 00 00 00 DA 06 00 00 00 00 06 00 00 07 86 06 00 00 1D 25 06 00 00 1A D3 06 00 00 09 74 06 00 00 00 00 06 00 00 09 6F 88 8B 7E";

        [Fact]
        public async Task ShouldReadFromStream()
        {

            var firstframe = ByteHelper.CreateByteArray(fullFrame);
            var secondFrame = ByteHelper.CreateByteArray(fullFrame);
            TestStream testStream = new TestStream();
            var writeTask = Task.Run(async () =>
            {
                await Task.Delay(500);
                //testStream.Write(firstframe.ToArray());
                testStream.Write(firstframe);
                await Task.Delay(500);
                testStream.Write(secondFrame);
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
    }
}