using System.Text;
using static Itp.SimpleHL7.HL7Constants;

namespace Itp.SimpleHL7;

public ref struct HL7Lexer
{
    private readonly Encoding Encoding;
    private ReadOnlySpan<char> Remaining;

    public HL7Separators Separators { get; }
    public HL7TokenType Type { get; private set; } = HL7TokenType.StartOfMessage;
    public ReadOnlySpan<char> Value { get; private set; } = ReadOnlySpan<char>.Empty;

    public HL7Lexer(ReadOnlySpan<char> message)
        : this(message, Encoding.UTF8)
    {
        // nop
    }

    public HL7Lexer(ReadOnlySpan<char> message, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        this.Encoding = encoding;

        this.Separators = HL7Separators.FromMSH1(message);
        this.Value = message.Slice(0, ModelHeader.Length);
        this.Remaining = message.Slice(ModelHeader.Length);
    }

    public bool MoveNext()
    {
        if (this.Type == HL7TokenType.EndOfMessage)
        {
            return false;
        }

        if (this.Remaining.IsEmpty)
        {
            this.Type = HL7TokenType.EndOfMessage;
            this.Value = ReadOnlySpan<char>.Empty;
            return true;
        }

        var firstChar = this.Remaining[0];
        var valueSpan = this.Remaining.Slice(0, 1);
        var eaten = valueSpan;

        if (firstChar == '\r' && this.Remaining.Length > 1 && this.Remaining[1] == '\n')
        {
            // Handle \r\n as \n
            eaten = valueSpan = this.Remaining.Slice(0, 2);
            this.Type = HL7TokenType.EndOfSegment;
        }
        else if (firstChar == '\r' || firstChar == '\n')
        {
            this.Type = HL7TokenType.EndOfSegment;
        }
        else if (firstChar == this.Separators.Field)
        {
            this.Type = HL7TokenType.FieldSeparator;
        }
        else if (firstChar == this.Separators.Component)
        {
            this.Type = HL7TokenType.ComponentSeparator;
        }
        else if (firstChar == this.Separators.Subcomponent)
        {
            this.Type = HL7TokenType.SubcomponentSeparator;
        }
        else if (firstChar == this.Separators.Repeat)
        {
            this.Type = HL7TokenType.RepeatSeparator;
        }
        else
        {
            eaten = Separators.TakeValue(this.Remaining);
            valueSpan = Separators.DecodeString(eaten, this.Encoding);
            this.Type = HL7TokenType.Value;
        }

        this.Value = valueSpan;
        this.Remaining = this.Remaining.Slice(eaten.Length);
        return true;
    }

    public bool MoveToNextField()
    {
        while (MoveNext())
        {
            if (this.Type == HL7TokenType.FieldSeparator 
                || this.Type == HL7TokenType.EndOfSegment)
            {
                return true;
            }
        }

        return false;
    }

    public bool MoveToNextSegment()
    {
        while (MoveNext())
        {
            if (this.Type == HL7TokenType.EndOfSegment)
            {
                return true;
            }
        }
        return false;
    }
}
