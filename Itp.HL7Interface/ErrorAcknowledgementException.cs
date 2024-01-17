using System.Runtime.Serialization;

namespace Itp.HL7Interface;

[Serializable]
public class ErrorAcknowledgementException : Exception
{
    public ErrorAcknowledgementException()
        : this("Error Processing Message")
    {
    }

    public ErrorAcknowledgementException(string message)
        : base(message)
    {
    }

    public ErrorAcknowledgementException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected ErrorAcknowledgementException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}