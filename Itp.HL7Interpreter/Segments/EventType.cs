#nullable disable

using Itp.HL7Interpreter.Formatters;

namespace Itp.HL7Interpreter.Segments;

internal class EventType : Segment
{
    public override string MessageIdentifier => "EVN";

    [Hl7Field(1, 3, false)]
    public string EventTypeCode { get; set; }

    [Hl7Field(2, 26, true, typeof(HL7DateTimeFormatter))]
    public DateTimeOffset RecordedDateTime { get; set; }

    [Hl7Field(3, 26, false, typeof(HL7DateTimeFormatter))]
    public DateTimeOffset DateTimePlannedEvent { get; set; }

    [Hl7Field(4, 3, false)]
    public string EventReasonCode { get; set; }

    [Hl7Field(5, 60, false)]
    public string OperatorID { get; set; }

    [Hl7Field(6, 26, false, typeof(HL7DateTimeFormatter))]
    public DateTimeOffset EventOccurred { get; set; }

    public EventType()
    {
        this.RecordedDateTime = DateTimeOffset.UtcNow;
    }
}