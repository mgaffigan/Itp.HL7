#nullable disable

using Itp.HL7Interpreter.Formatters;

namespace Itp.HL7Interpreter.Segments;

public class PatientAllergy : Segment
{
    public override string MessageIdentifier => "AL1";

    [Hl7Field(1, 4, false)]
    public string SetID { get; set; }

    [Hl7Field(2, 2, false)]
    public string Type { get; set; }

    [Hl7Field(3, 60, false)]
    public string Code { get; set; }

    [Hl7Field(4, 2, false)]
    public string Severity { get; set; }

    [Hl7Field(5, 15, false)]
    public string Reaction { get; set; }

    [Hl7Field(6, 8, false, typeof(HL7DateTimeFormatter))]
    public DateTimeOffset IdentificationDate { get; set; }
}