using Itp.HL7Interpreter.Segments;
using System.Text;

namespace Itp.HL7Interpreter;

public class Part
{
    public MessageHeader? HeaderSegment { get; set; }

    public void Parse(string data)
    {
        Hl7FieldAttribute.SplitFields(data, '\u005E', this, 1);
    }

    public override string ToString()
    {
        return this.ToString((this.HeaderSegment == null ? "^" : this.HeaderSegment.ComponentSeparator));
    }

    public string ToString(string fieldSeparator)
    {
        StringBuilder stringBuilder = new StringBuilder();
        Hl7FieldAttribute.JoinFields(fieldSeparator, this, stringBuilder, 0, false);
        return stringBuilder.ToString();
    }
}