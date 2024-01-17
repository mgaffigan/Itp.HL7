#nullable disable

namespace Itp.HL7Interpreter.Segments;

public class Error : Segment
{
    public override string MessageIdentifier => "ERR";

    [Hl7Field(1, 493, false)]
    public string ErrorCodeAndLocation { get; set; }

    [Hl7Field(2, 18, false)]
    public string ErrorLocation { get; set; }

    [Hl7Field(3, 705, false)]
    public string HL7ErrorCode { get; set; }

    [Hl7Field(4, 2, true)]
    public string Severity { get; set; }

    [Hl7Field(5, 705, true)]
    public string ApplicationErrorCode { get; set; }

    [Hl7Field(6, 80, true)]
    public string ApplicationErrorParameter { get; set; }
}