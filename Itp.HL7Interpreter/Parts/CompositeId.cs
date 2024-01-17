namespace Itp.HL7Interpreter.Parts;

public class CompositeId : Part
{
    [Hl7Field(Sequence = 1)]
    public string ID { get; set; } = "";

    [Hl7Field(Sequence = 2)]
    public string? CheckDigit { get; set; }

    [Hl7Field(Sequence = 3)]
    public string? CheckDigitFormat { get; set; }

    [Hl7Field(Sequence = 4)]
    public string? AssigningAuthority { get; set; }

    [Hl7Field(Sequence = 5)]
    public string? IdentifierTypeCode { get; set; }

    [Hl7Field(Sequence = 6)]
    public string? AssigningFaciltiy { get; set; }

#nullable disable
    public CompositeId()
    {
    }
#nullable restore

    public CompositeId(string ID, string? qualifier = null)
    {
        this.ID = ID;
        this.IdentifierTypeCode = qualifier;
    }
}