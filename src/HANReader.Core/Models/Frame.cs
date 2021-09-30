using System;

namespace HANReader.Core.Models
{
    public record Frame(DateTime Timestamp, long Framesize, Field Payload);
}