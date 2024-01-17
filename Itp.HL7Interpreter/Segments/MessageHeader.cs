using Itp.HL7Interpreter.Formatters;

namespace Itp.HL7Interpreter.Segments;

public class MessageHeader : Segment
{
    public override string MessageIdentifier => "MSH";
    public override MessageHeader HeaderSegment => this;

    public string FieldSeparator { get; set; }
    public string ComponentSeparator => this.EncodingCharacters[0].ToString();
    public string RepetitionSeparator => this.EncodingCharacters[1].ToString();
    public string EscapeCharacter => this.EncodingCharacters[2].ToString();
    public string SubcomponentSeparator => this.EncodingCharacters[3].ToString();


    [Hl7Field(2, 4, true)]
    public string EncodingCharacters { get; set; }

    [Hl7Field(3, 180, false)]
    public string? SendingApplication { get; set; }

    [Hl7Field(4, 180, false)]
    public string? SendingFacility { get; set; }

    [Hl7Field(5, 180, false)]
    public string? ReceivingApplication { get; set; }

    [Hl7Field(6, 180, false)]
    public string? ReceivingFacility { get; set; }

    [Hl7Field(7, 26, false, typeof(HL7DateTimeFormatter))]
    public DateTimeOffset DateStamp { get; set; }

    [Hl7Field(9, 7, true)]
    public string? MessageType { get; set; }

    [Hl7Field(10, 20, true)]
    public string? MessageControlID { get; set; }

    [Hl7Field(11, 3, true)]
    public string ProcessingID { get; set; }

    [Hl7Field(12, 60, false)]
    public string VersionID { get; set; }

    public MessageHeader()
    {
        this.FieldSeparator = "|";
        this.EncodingCharacters = "^~\\&";
        this.VersionID = "2.4";
        this.ProcessingID = "P";
        this.DateStamp = DateTimeOffset.UtcNow;
    }

    public MessageHeader(string type, string SendingFac, string SendingApp, string messageControl)
        : this()
    {
        this.MessageControlID = messageControl;
        this.MessageType = type;
        this.SendingFacility = SendingFac;
        this.SendingApplication = SendingApp;
    }
}