using System.Text;

namespace HANReader.Core.Models
{

    public class UnsignedInt32 : Field<uint>
    {
        public UnsignedInt32(uint value) : base(value)
        {
        }

        public override DataType DataType => DataType.UnsignedInt32;
    }
}


