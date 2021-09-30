using System;

namespace HANReader.Core.Models
{
    public record Frame2(DateTime Timestamp, long Framesize, Field Payload);

}