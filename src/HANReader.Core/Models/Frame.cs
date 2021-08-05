using System;

namespace HANReader.Core.Models
{
    public class Frame
    {
        public static Frame InvalidFrame = new Frame();

        public readonly Header Header;
        public readonly DateTime TimeStamp;
        public readonly Field Payload;

        private Frame()
        {
        }

        public Frame(Header header, DateTime dateTime, Field payload)
        {
            Header = header;
            TimeStamp = dateTime;
            Payload = payload;
        }
    }




    public enum ReadStatus
    {
        /// <summary>
        /// All the bytes have been read and verified.
        /// </summary>
        Complete,

        /// <summary>
        /// All the bytes have been read, but the checksum is invalid.
        /// </summary>
        InvalidChecksum,

        /// <summary>
        /// There are still bytes to be read of the stream.
        /// </summary>
        InComplete,

        // Nothing to read or we did not find the start flag.
        NotFound
    }
}


