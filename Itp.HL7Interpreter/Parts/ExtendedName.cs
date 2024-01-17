namespace Itp.HL7Interpreter.Parts;

public class ExtendedName : Part
{
    [Hl7Field(Sequence = 1)]
    public string FamilyName { get; set; }

    [Hl7Field(Sequence = 2)]
    public string GivenName { get; set; }

    [Hl7Field(Sequence = 3)]
    public string? MiddleName { get; set; }

    [Hl7Field(Sequence = 4)]
    public string? Suffix { get; set; }

    [Hl7Field(Sequence = 5)]
    public string? Title { get; set; }

#nullable disable
    public ExtendedName()
    {
    }
#nullable restore

    public ExtendedName(string FName, string LName)
    {
        this.FamilyName = LName;
        this.GivenName = FName;
    }
}