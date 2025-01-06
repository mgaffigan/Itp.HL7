namespace Itp.HL7Interface;

internal static class MllpConstants
{
    // MLLP is a simple protocol for sending HL7 messages over a TCP connection.
    // [\v] {message} \x1c [\r]
    // https://www.hl7.org/documentcenter/public/wg/inm/mllp_transport_specification.PDF

    // EOM marks the end of a message, any buffered data should be processed.
    public const byte MessageSeparator = 0x1c;

    // SOM and EOMEB are just for formatting and should be trimmed before processing.
    public const byte StartOfMessageByte = (byte)'\v';
    public const byte EndOfMessageExtraByte = (byte)'\r';

    public static bool IsValidMessage(ReadOnlySpan<byte> message)
    {
        // legal range is 0x1f..0xff, but we're only going to check for the EOM byte.
        // This is a very basic check, but it should be enough to catch most errors.
        return message.IndexOf(MessageSeparator) == -1;
    }

    public const int MaxMessageSize = 30 /* MB */ * 1024 * 1024;
}
