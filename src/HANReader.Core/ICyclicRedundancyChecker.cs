namespace HANReader.Core
{
    public interface ICyclicRedundancyChecker
    {
        bool Check(ushort checkSequence, byte[] bytes);
    }

}