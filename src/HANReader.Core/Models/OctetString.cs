namespace HANReader.Core.Models
{
    public class OctetString : Field<string>
    {
        public OctetString(string value) : base(value)
        {
        }

        public override DataType DataType => DataType.OctetString;
    }
}


