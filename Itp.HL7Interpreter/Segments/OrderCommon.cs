#nullable disable

namespace Itp.HL7Interpreter.Segments;

public class OrderCommon : Segment
{
    public override string MessageIdentifier => "ORC";

    [Hl7Field(1, 2, true)]
    public string OrderControl { get; set; }

    [Hl7Field(2, 22, true)]
    public string PlacerOrderNumber { get; set; }

    [Hl7Field(3, 22, true)]
    public string FillerOrderNumber { get; set; }

    [Hl7Field(4, 22, false)]
    public string PlacerGroupNumber { get; set; }

    [Hl7Field(5, 2, false)]
    public string OrderStatus { get; set; }

    [Hl7Field(6, 1, false)]
    public string ResponseFlag { get; set; }

    [Hl7Field(7, 200, false)]
    public string QuantityTiming { get; set; }

    [Hl7Field(8, 200, false)]
    public string Parent { get; set; }

    [Hl7Field(9, 26, false)]
    public string DateTimeofTransaction { get; set; }

    [Hl7Field(10, 120, false)]
    public string EnteredBy { get; set; }

    [Hl7Field(11, 120, false)]
    public string VerifiedBy { get; set; }

    [Hl7Field(12, 120, false)]
    public string OrderingProvider { get; set; }

    [Hl7Field(13, 80, false)]
    public string EnterersLocation { get; set; }

    [Hl7Field(14, 40, false)]
    public string CallBackPhoneNumber { get; set; }

    [Hl7Field(15, 26, false)]
    public string OrderEffectiveDateTime { get; set; }

    [Hl7Field(16, 200, false)]
    public string OrderControlCodeReason { get; set; }

    [Hl7Field(17, 60, false)]
    public string EnteringOrganization { get; set; }

    [Hl7Field(18, 60, false)]
    public string EnteringDevice { get; set; }

    [Hl7Field(19, 120, false)]
    public string ActionBy { get; set; }
}