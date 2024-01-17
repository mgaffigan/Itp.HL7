using Itp.HL7Interpreter.Formatters;
using Itp.HL7Interpreter.Parts;

namespace Itp.HL7Interpreter.Segments;

public class PatientIdentification : Segment
{
    public override string MessageIdentifier => "PID";

    public override MessageHeader? HeaderSegment
    {
        get => base.HeaderSegment;
        set
        {
            base.HeaderSegment = value;
            this.ExtPatientID.HeaderSegment = value;
            this.IntPatientID.HeaderSegment = value;
            this.AltPatientID.HeaderSegment = value;
            this.PatientName.HeaderSegment = value;
            this.MothersMaidenName.HeaderSegment = value;
            this.PatientAlias.HeaderSegment = value;
        }
    }

    [Hl7Field(2, 20, false)]
    public CompositeId ExtPatientID { get; set; }

    [Hl7Field(3, 20, true)]
    public CompositeId IntPatientID { get; set; }

    [Hl7Field(4, 20, false)]
    public CompositeId AltPatientID { get; set; }

    [Hl7Field(5, 48, true)]
    public ExtendedName PatientName { get; set; }

    [Hl7Field(6, 48, false)]
    public ExtendedName MothersMaidenName { get; set; }

    [Hl7Field(7, 26, false, typeof(HL7DateFormatter))]
    public DateTime DateOfBirth { get; set; }

    [Hl7Field(8, 1, false)]
    public string? Sex { get; set; }

    [Hl7Field(9, 48, false)]
    public ExtendedName PatientAlias { get; set; }

    [Hl7Field(19, 16, false)]
    public string? SSN { get; set; }

    public PatientIdentification()
    {
        this.ExtPatientID = new CompositeId();
        this.IntPatientID = new CompositeId();
        this.AltPatientID = new CompositeId();
        this.PatientName = new ExtendedName();
        this.MothersMaidenName = new ExtendedName();
        this.PatientAlias = new ExtendedName();
    }

    public PatientIdentification(string PatID, string PatFName, string PatLName, string? Sex, DateTime DateOfBirth) : this()
    {
        this.IntPatientID = new CompositeId(PatID, "PI");
        this.PatientName = new ExtendedName(PatFName, PatLName);
        this.Sex = Sex;
        this.DateOfBirth = DateOfBirth;
    }
}