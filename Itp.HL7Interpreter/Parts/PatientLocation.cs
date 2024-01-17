namespace Itp.HL7Interpreter.Parts;

public class PatientLocation : Part
{
    [Hl7Field(Sequence = 0)]
    public string? LocationDescription { get; set; }

    [Hl7Field(Sequence = 1)]
    public string PointOfCare { get; set; }

    [Hl7Field(Sequence = 2)]
    public string Room { get; set; }

    [Hl7Field(Sequence = 3)]
    public string Bed { get; set; }

    [Hl7Field(Sequence = 4)]
    public string Facility { get; set; }

    [Hl7Field(Sequence = 5)]
    public string? LocationStatus { get; set; }

    [Hl7Field(Sequence = 6)]
    public string? PersonLocationType { get; set; }

    [Hl7Field(Sequence = 7)]
    public string? Building { get; set; }

    [Hl7Field(Sequence = 8)]
    public string? Floor { get; set; }

#nullable disable
    public PatientLocation()
    {
    }
#nullable restore

    public PatientLocation(string FacID, string NsID, string Room, string Bed)
    {
        this.Facility = FacID;
        this.PointOfCare = NsID;
        this.Room = Room;
        this.Bed = Bed;
    }
}