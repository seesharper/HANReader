namespace HANReader.Tests
{
    public static class TestData
    {
        public const string FullFrame = "7E A0 79 01 02 01 10 80 93 E6 E7 00 0F 40 00 00 00 09 0C 07 E4 08 0B 02 0D 36 28 FF 80 00 00 02 0D 09 07 4B 46 4D 5F 30 30 31 09 10 36 39 37 30 36 33 31 34 30 33 30 38 32 38 30 39 09 08 4D 41 33 30 34 48 33 45 06 00 00 08 2D 06 00 00 00 00 06 00 00 00 DA 06 00 00 00 00 06 00 00 07 86 06 00 00 1D 25 06 00 00 1A D3 06 00 00 09 74 06 00 00 00 00 06 00 00 09 6F 88 8B 7E";


        //-------------------------------------------THIS IS AN INVALID CHECKSUM-------👇----------------
        public const string FullFrameWithInvalidHeaderChecksum = "7E A0 79 01 02 01 10 FF 93 E6 E7 00 0F 40 00 00 00 09 0C 07 E4 08 0B 02 0D 36 28 FF 80 00 00 02 0D 09 07 4B 46 4D 5F 30 30 31 09 10 36 39 37 30 36 33 31 34 30 33 30 38 32 38 30 39 09 08 4D 41 33 30 34 48 33 45 06 00 00 08 2D 06 00 00 00 00 06 00 00 00 DA 06 00 00 00 00 06 00 00 07 86 06 00 00 1D 25 06 00 00 1A D3 06 00 00 09 74 06 00 00 00 00 06 00 00 09 6F 88 8B 7E";

        public const string Rubbish = "AA BB CC";

        public const string StartFlag = "7E";
    }
}