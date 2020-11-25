using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using HANReader.Core.Models;

namespace HANReader.Core
{
    public interface IPayloadReader
    {
        Field Read(ref SequenceReader<byte> reader);
    }

    public class PayloadReader : IPayloadReader
    {
        public Field Read(ref SequenceReader<byte> reader)
        {
            reader.TryRead(out byte dataTypeByte);
            var dataType = (DataType)dataTypeByte;

            if (dataType == DataType.Struct)
            {
                reader.TryRead(out var numberOfElements);
                var fields = new List<Field>();
                for (int i = 0; i < numberOfElements; i++)
                {
                    fields.Add(Read(ref reader));
                }

                return new Struct(fields.ToArray());
            }
            else if (dataType == DataType.OctetString)
            {
                reader.TryRead(out var stringLength);
                Span<byte> stackBuffer = stackalloc byte[stringLength];
                reader.TryCopyTo(stackBuffer);
                var value = Encoding.ASCII.GetString(stackBuffer);
                reader.Advance(stringLength);
                return new OctetString(value);
            }
            else if (dataType == DataType.UnsignedInt32)
            {
                Span<byte> stackBuffer = stackalloc byte[4];
                reader.TryCopyTo(stackBuffer);
                stackBuffer.Reverse();
                var value = BitConverter.ToUInt32(stackBuffer);
                reader.Advance(4);
                return new UnsignedInt32(value);
            }
            else
            {
                throw new InvalidOperationException($"Invalid datatype {dataTypeByte}");
            }
        }
    }
}