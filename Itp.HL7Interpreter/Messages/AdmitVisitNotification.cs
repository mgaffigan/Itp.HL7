using Itp.HL7Interpreter.Segments;

namespace Itp.HL7Interpreter.Messages;

public class AdmitVisitNotification : Message
{
    public PatientIdentification PatientIdentification { get; set; }

    public PatientVisit PatientVisit { get; set; }

    public AdmitVisitNotification(string MessageID, string PatID, string PatFName, string PatLName)
    {
        Add(new MessageHeader("ADT^A01", "", "", MessageID));
        Add(new EventType());
        DateTime dateTime = new DateTime();
        this.PatientIdentification = new PatientIdentification(PatID, PatFName, PatLName, null, dateTime);
        Add(this.PatientIdentification);
        this.PatientVisit = new PatientVisit();
        Add(this.PatientVisit);
    }
}