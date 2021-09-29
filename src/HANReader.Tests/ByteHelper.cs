using System.Globalization;

namespace HANReader.Tests
{
    public static class ByteHelper
    {
        public static byte[] CreateByteArray(string data)
        {
            var stringBytes = data.Split(" ");
            byte[] bytes = new byte[stringBytes.Length];
            for (int i = 0; i < stringBytes.Length; i++)
            {
                bytes[i] = byte.Parse(stringBytes[i], NumberStyles.HexNumber);
            }

            return bytes;
        }
    }
}