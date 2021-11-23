namespace HANReader.Core
{
    public record StreamReaderOptions(int WaitTime, int MinimumBuffer)
    {
        public static readonly StreamReaderOptions Default = new(WaitTime: 1000, MinimumBuffer: 16);
    }
}