#nullable disable

namespace Itp.HL7Interpreter.Segments;

internal class RxOrder : Segment
{
    public override string MessageIdentifier => "RXO";

    [Hl7Field(1, 100, true)]
    public string Code { get; set; }

    [Hl7Field(2, 20, true)]
    public string AmounMinimum { get; set; }

    [Hl7Field(3, 20, false)]
    public string AmountMaximum { get; set; }

    [Hl7Field(4, 60, true)]
    public string Units { get; set; }

    [Hl7Field(5, 60, false)]
    public string DosageForm { get; set; }

    [Hl7Field(6, 200, false)]
    public string TreatmentSig { get; set; }

    [Hl7Field(7, 200, false)]
    public string AdminSig { get; set; }

    [Hl7Field(8, 200, false)]
    public string DeliverToLocation { get; set; }

    [Hl7Field(9, 1, false)]
    public string AllowSubstitutions { get; set; }

    [Hl7Field(10, 100, false)]
    public string DispenseCode { get; set; }

    [Hl7Field(11, 20, false)]
    public string DispenseAmount { get; set; }

    [Hl7Field(12, 60, false)]
    public string DispenseUnits { get; set; }

    [Hl7Field(13, 3, false)]
    public string NumberOfRefills { get; set; }

    [Hl7Field(14, 60, true)]
    public string ProvidersDEANumber { get; set; }

    [Hl7Field(15, 60, true)]
    public string PharmacistTreatmentProviderVerifierID { get; set; }

    [Hl7Field(16, 1, false)]
    public string NeedsHumanReview { get; set; }

    [Hl7Field(17, 20, true)]
    public string PerUnit { get; set; }

    [Hl7Field(18, 20, false)]
    public string Strength { get; set; }

    [Hl7Field(19, 60, false)]
    public string StrengthUnits { get; set; }

    [Hl7Field(20, 200, false)]
    public string Indication { get; set; }

    [Hl7Field(21, 6, false)]
    public string RateAmount { get; set; }

    [Hl7Field(22, 60, false)]
    public string RateUnits { get; set; }
}