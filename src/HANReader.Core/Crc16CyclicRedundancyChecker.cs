namespace HANReader.Core
{
    public class Crc16CyclicRedundancyChecker : ICyclicRedundancyChecker
    {
        private const ushort polynomial = 0x8408;
        private static readonly ushort[] table = new ushort[256];

        static Crc16CyclicRedundancyChecker()
        {
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }



        private static ushort ComputeChecksum(byte[] data, int start, int length)
        {
            ushort fcs = 0xffff;
            for (int i = start; i < (start + length); i++)
            {
                var index = (fcs ^ data[i]) & 0xff;
                fcs = (ushort)((fcs >> 8) ^ table[index]);
            }
            fcs ^= 0xffff;
            return fcs;
        }


        public bool Check(ushort checkSequence, byte[] bytes)
        {
            var checksum = ComputeChecksum(bytes, 0, bytes.Length);
            return checkSequence == checksum;
        }
    }

}