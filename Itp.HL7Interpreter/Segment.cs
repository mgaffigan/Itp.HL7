using Itp.HL7Interpreter.Segments;
using System.Text;

namespace Itp.HL7Interpreter;

public abstract class Segment
{
    public virtual MessageHeader? HeaderSegment { get; set; }

    public abstract string MessageIdentifier { get; }

    public static Segment? Parse(string data, char separator, SegmentTypeResolver? resolver)
    {
        data = data.Trim();
        if (string.IsNullOrWhiteSpace(data) || data.Length < 4)
        {
            throw new ArgumentException("Data is null or too short", "data");
        }

        string str = data.Substring(0, 3);
        Type? type = null;
        if (resolver != null)
        {
            type = resolver(str);
        }
        if (type == null)
        {
            type = resolveSegmentType(str);
        }

        if (type == null)
        {
            return null;
        }

        var segment1 = (Segment)Activator.CreateInstance(type);
        Hl7FieldAttribute.SplitFields(data, separator, segment1, (segment1 is MessageHeader ? 1 : 0));
        return segment1;
    }

    private static Type? resolveSegmentType(string segmentID)
    {
        switch (segmentID.ToUpper())
        {
            case "EVN":
                return typeof(EventType);
            case "MSH":
                return typeof(MessageHeader);
            case "PID":
                return typeof(PatientIdentification);
            case "PV1":
                return typeof(PatientVisit);
            case "MSA":
                return typeof(MessageAcknowledgement);
            case "AL1":
                return typeof(PatientAllergy);
            case "DG1":
                return typeof(Diagnosis);
            case "ORC":
                return typeof(OrderCommon);
            case "RXO":
                return typeof(RxOrder);
            default:
                return null;
        }
    }

    public string ToHumanReadable()
    {
        var stringBuilder = new StringBuilder();
        this.ToHumanReadable(stringBuilder);
        return stringBuilder.ToString();
    }

    public void ToHumanReadable(StringBuilder sb)
    {
        var properCase = Hl7FieldAttribute.CamelToProperCase(this.GetType().Name);
        sb.AppendLine(properCase);
        sb.AppendLine(new string('-', properCase.Length));
        Hl7FieldAttribute.ToHumaneReadable("\r\n", this, sb, (this is MessageHeader ? 1 : 0));
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(this.MessageIdentifier);
        stringBuilder.Append(this.HeaderSegment?.FieldSeparator);
        Hl7FieldAttribute.JoinFields(this.HeaderSegment?.FieldSeparator, this, stringBuilder, (this is MessageHeader ? 1 : 0));
        return stringBuilder.ToString();
    }
}