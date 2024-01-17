using Itp.HL7Interpreter.Formatters;

namespace Itp.HL7Interpreter.Segments;

public class MessageAcknowledgement : Segment
{
    public override string MessageIdentifier => "MSA";

    [Hl7Field(1, 2, true)]
    public string AcknowledgementCode { get; set; }

    [Hl7Field(2, 20, true)]
    public string? MessageControlID { get; set; }

    [Hl7Field(3, 80, false)]
    public string? TextMessage { get; set; }

    [Hl7Field(4, 15, false, typeof(HL7IntFormatter))]
    public int? ExpectedSequenceNumber { get; set; }

    [Hl7Field(5, 1, false)]
    public string? DelayedAcknowledgementType { get; set; }

    [Hl7Field(6, 100, false)]
    public string? ErrorCondition { get; set; }

    public MessageAcknowledgement()
    {
        this.AcknowledgementCode = "AA";
    }

    public MessageAcknowledgement(string ControlID) : this()
    {
        this.MessageControlID = ControlID;
    }
}