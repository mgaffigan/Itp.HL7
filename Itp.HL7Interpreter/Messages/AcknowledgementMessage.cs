using Itp.HL7Interpreter.Segments;

namespace Itp.HL7Interpreter.Messages;

public enum AcknowledgementErrorType { Success, UnsupportedMessageType, ApplicationInternalError, DuplicateKeyIdentifier }

public enum AcknowledgementType { Accept, Error, Reject }

public class AcknowledgementMessage : Message
{
    private static class ErrorCodes
    {
        public const string MessageAccepted = "0";
        public const string UnsupportedMessageType = "200";
        public const string ApplicationInternalError = "207";
        public const string DuplicateKeyIdentifier = "205";
    }

    private static class AckCode
    {
        public const string Accept = "AA";
        public const string Reject = "AR";
        public const string Error = "AE";
    }

    public AcknowledgementMessage(string SendingFac, string SendingApp, string AckControlNo,
        AcknowledgementType level, AcknowledgementErrorType type, string text)
        : this(SendingFac, SendingApp, AckControlNo)
    {
        SetResponse(level, type, text);
    }

    public AcknowledgementMessage(string SendingFac, string SendingApp, string AckControlNo)
    {
        long ticks = DateTime.Now.Ticks;
        Add(new MessageHeader("ACK", SendingFac, SendingApp, ticks.ToString()));
        Add(new MessageAcknowledgement(AckControlNo));
    }

    public void SetResponse(AcknowledgementType level, AcknowledgementErrorType type, string? text)
    {
        var ackSegment = GetSegment<MessageAcknowledgement>();
        if (text != null && text.Length > 80)
        {
            ackSegment.TextMessage = text.Substring(0, 80);
        }
        else
        {
            ackSegment.TextMessage = text;
        }

        switch (level)
        {
            case AcknowledgementType.Accept:
                ackSegment.AcknowledgementCode = AckCode.Accept;
                break;

            case AcknowledgementType.Reject:
                ackSegment.AcknowledgementCode = AckCode.Reject;
                break;

            case AcknowledgementType.Error:
                ackSegment.AcknowledgementCode = AckCode.Error;
                break;

            default:
                throw new ArgumentException($"Unknown error level {level}", nameof(level));
        }

        switch (type)
        {
            case AcknowledgementErrorType.ApplicationInternalError:
                ackSegment.ErrorCondition = ErrorCodes.ApplicationInternalError;
                break;

            case AcknowledgementErrorType.DuplicateKeyIdentifier:
                ackSegment.ErrorCondition = ErrorCodes.DuplicateKeyIdentifier;
                break;

            case AcknowledgementErrorType.Success:
                ackSegment.ErrorCondition = ErrorCodes.MessageAccepted;
                break;

            case AcknowledgementErrorType.UnsupportedMessageType:
                ackSegment.ErrorCondition = ErrorCodes.UnsupportedMessageType;
                break;

            default:
                throw new ArgumentException($"Unknown error level {type}", nameof(type));
        }
    }

    public void AddInformationalRecord(string name, string value)
    {
        var err = new Error();
        err.Severity = "I"; /*I=info,W=warning,E=error*/
        err.ApplicationErrorCode = name;
        err.ApplicationErrorParameter = value;
        Add(err);
    }
}