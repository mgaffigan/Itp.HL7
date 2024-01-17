#nullable disable

using Itp.HL7Interpreter.Formatters;
using Itp.HL7Interpreter.Parts;

namespace Itp.HL7Interpreter.Segments;

public class PatientVisit : Segment
{
    public override string MessageIdentifier => "PV1";

    [Hl7Field(1, 4, false)]
    public string SetID { get; set; }

    [Hl7Field(2, 1, false)]
    public string PatientClass { get; set; }

    [Hl7Field(3, 80, false)]
    public PatientLocation AssignedPatientLocation { get; set; }

    [Hl7Field(4, 2, false)]
    public string AdmissionType { get; set; }

    [Hl7Field(5, 20, false)]
    public string PreadmitNumber { get; set; }

    [Hl7Field(6, 80, false)]
    public string PriorPatientLocation { get; set; }

    [Hl7Field(7, 60, false)]
    public ExtendedName AttendingDoctor { get; set; }

    [Hl7Field(8, 60, false)]
    public string ReferringDoctor { get; set; }

    [Hl7Field(9, 60, false)]
    public string ConsultingDoctor { get; set; }

    [Hl7Field(10, 3, false)]
    public string HospitalService { get; set; }

    [Hl7Field(11, 80, false)]
    public string TemporaryLocation { get; set; }

    [Hl7Field(12, 2, false)]
    public string PreadmitTestIndicator { get; set; }

    [Hl7Field(13, 2, false)]
    public string ReadmissionIndicator { get; set; }

    [Hl7Field(14, 3, false)]
    public string AdmitSource { get; set; }

    [Hl7Field(15, 2, false)]
    public string AmbulatoryStatus { get; set; }

    [Hl7Field(16, 2, false)]
    public string VIPIndicator { get; set; }

    [Hl7Field(17, 60, false)]
    public string AdmittingDoctor { get; set; }

    [Hl7Field(18, 2, false)]
    public string PatientType { get; set; }

    [Hl7Field(19, 20, false)]
    public string VisitNumber { get; set; }

    [Hl7Field(20, 50, false)]
    public string FinancialClass { get; set; }

    [Hl7Field(21, 2, false)]
    public string ChargePriceIndicator { get; set; }

    [Hl7Field(36, 3, false)]
    public string DischargeDisposition { get; set; }

    [Hl7Field(37, 25, false)]
    public string DischargedtoLocation { get; set; }

    [Hl7Field(38, 2, false)]
    public string DietType { get; set; }

    [Hl7Field(39, 2, false)]
    public string ServicingFacility { get; set; }

    [Hl7Field(40, 1, false)]
    public string BedStatus { get; set; }

    [Hl7Field(41, 2, false)]
    public string AccountStatus { get; set; }

    [Hl7Field(42, 80, false)]
    public string PendingLocation { get; set; }

    [Hl7Field(43, 80, false)]
    public string PriorTemporaryLocation { get; set; }

    [Hl7Field(44, 26, false, typeof(HL7DateTimeFormatter))]
    public DateTimeOffset AdmitDateTime { get; set; }

    [Hl7Field(45, 26, false)]
    public string DischargeDateTime { get; set; }

    [Hl7Field(46, 12, false)]
    public string CurrentPatientBalance { get; set; }

    [Hl7Field(47, 12, false)]
    public string TotalCharges { get; set; }

    [Hl7Field(48, 12, false)]
    public string TotalAdjustments { get; set; }

    [Hl7Field(49, 12, false)]
    public string TotalPayments { get; set; }

    [Hl7Field(50, 20, false)]
    public string AlternateVisitID { get; set; }

    [Hl7Field(51, 1, false)]
    public string VisitIndicator { get; set; }

    [Hl7Field(52, 60, false)]
    public string OtherHealthcareProvider { get; set; }

    public override MessageHeader HeaderSegment
    {
        get => base.HeaderSegment;
        set
        {
            base.HeaderSegment = value;
            this.AssignedPatientLocation.HeaderSegment = value;
            this.AttendingDoctor.HeaderSegment = value;
        }
    }

    public PatientVisit()
    {
        this.PatientClass = "I";
        this.AssignedPatientLocation = new PatientLocation();
        this.AttendingDoctor = new ExtendedName();
    }
}