#nullable disable

using Itp.HL7Interpreter.Formatters;

namespace Itp.HL7Interpreter.Segments;

public class Diagnosis : Segment
{
    public override string MessageIdentifier => "DG1";

    [Hl7Field(1, 4, false)]
    public string SetID { get; set; }

    [Hl7Field(2, 2, true)]
    public string CodingMethod { get; set; }

    [Hl7Field(3, 60, false)]
    public string Code { get; set; }

    [Hl7Field(4, 40, true)]
    public string Description { get; set; }

    [Hl7Field(5, 26, false, typeof(HL7DateTimeFormatter))]
    public DateTimeOffset DiagnosisDate { get; set; }

    [Hl7Field(6, 2, true)]
    public string Type { get; set; }

    [Hl7Field(7, 60, true)]
    public string MajorDiagnosticCategory { get; set; }

    [Hl7Field(8, 4, true)]
    public string DiagnosticRelatedGroup { get; set; }

    [Hl7Field(9, 2, true)]
    public string DRGApprovalIndicator { get; set; }

    [Hl7Field(10, 2, true)]
    public string DRGGrouperReviewCode { get; set; }

    [Hl7Field(11, 60, true)]
    public string OutlierType { get; set; }

    [Hl7Field(12, 3, true)]
    public string OutlierDays { get; set; }

    [Hl7Field(13, 12, true)]
    public string OutlierCost { get; set; }

    [Hl7Field(14, 4, true)]
    public string GrouperVersionAndType { get; set; }

    [Hl7Field(15, 2, true)]
    public string DiagnosisPriority { get; set; }

    [Hl7Field(16, 60, false)]
    public string DiagnosingClinician { get; set; }

    [Hl7Field(17, 3, false)]
    public string DiagnosisClassification { get; set; }

    [Hl7Field(18, 1, false)]
    public string ConfidentialIndicator { get; set; }

    [Hl7Field(19, 26, false, typeof(HL7DateTimeFormatter))]
    public DateTimeOffset AttestationDateTime { get; set; }
}