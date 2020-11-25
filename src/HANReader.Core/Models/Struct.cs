namespace HANReader.Core.Models
{
    public class Struct : Field<Field[]>
    {
        public Struct(Field[] value) : base(value)
        {
        }

        public override DataType DataType => DataType.Struct;
    }
}


