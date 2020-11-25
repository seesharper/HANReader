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
}


