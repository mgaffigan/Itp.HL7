using System.Text;
using static Itp.SimpleHL7.HL7Constants;

namespace Itp.SimpleHL7;

public record HL7Separators(char Field, char Component, char Subcomponent, char Repeat, char Escape)
{
    private readonly char[] AllSeparators = [Field, Component, Subcomponent, Repeat, Escape, '\r', '\n'];
    private readonly char[] NonEscapeSeparators = [Field, Component, Subcomponent, Repeat, '\r', '\n'];

    public static HL7Separators Default { get; } = new HL7Separators(
        Field: '|',
        Component: '^',
        Subcomponent: '&',
        Repeat: '~',
        Escape: '\\'
    );

    public string MSH2 => $"{Component}{Repeat}{Escape}{Subcomponent}";

    public static HL7Separators FromMSH12(string msh1, string msh2)
    {
        if (msh1 == "|" && msh2 == "^~\\&") return Default;
        if (msh1 == "" && msh2 == "") return Default;

        if (msh1.Length != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(msh1), "MSH-1 must be a single character");
        }
        if (msh2.Length != 4)
        {
            throw new ArgumentOutOfRangeException(nameof(msh2), "MSH-2 must be exactly four characters");
        }
        return new HL7Separators(
            Field: msh1[0],
            Component: msh2[0],
            Repeat: msh2[1],
            Escape: msh2[2],
            Subcomponent: msh2[3]
        );
    }

    public static HL7Separators FromMSH1(ReadOnlySpan<char> message)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(message.Length, ModelHeader.Length, nameof(message));
        if (!message.StartsWith("MSH"))
        {
            throw new ArgumentException("Message must start with MSH segment", nameof(message));
        }

        return new HL7Separators(
            Field: message[3],
            Component: message[4],
            Repeat: message[5],
            Escape: message[6],
            Subcomponent: message[7]
        );
    }

    public ReadOnlySpan<char> EncodeString(ReadOnlySpan<char> value)
    {
        var i = value.IndexOfAny(AllSeparators);
        if (i < 0) return value;

        var sb = new StringBuilder(value.Length + (2 /* ch */ * 3 /* ch/ch */));
        for (i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == Field)
            {
                sb.Append(Escape).Append('F').Append(Escape);
            }
            else if (c == Component)
            {
                sb.Append(Escape).Append('S').Append(Escape);
            }
            else if (c == Subcomponent)
            {
                sb.Append(Escape).Append('T').Append(Escape);
            }
            else if (c == Repeat)
            {
                sb.Append(Escape).Append('R').Append(Escape);
            }
            else if (c == Escape)
            {
                sb.Append(Escape).Append('E').Append(Escape);
            }
            else if (c == '\r' || c == '\n')
            {
                // Encode \r\n the same as \n
                if (i + 1 < value.Length && value[i + 1] == '\n') i++;
                sb.Append(Escape).Append(".br").Append(Escape);
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public ReadOnlySpan<char> DecodeString(ReadOnlySpan<char> value, Encoding enc)
    {
        var i = value.IndexOf(Escape);
        if (i < 0) return value;

        var sb = new StringBuilder(value.Length);
        for (i = 0; i < value.Length; i++)
        {
            if (value[i] == Escape)
            {
                int j = value.Slice(i + 1).IndexOf(Escape);
                if (j < 0) throw new FormatException("Invalid escape sequence: missing closing escape character");
                var code = value.Slice(i + 1, j);
                sb.Append(ParseFormattingCode(code, enc));
                // Move to the closing escape character
                i += j + 1;
            }
            else
            {
                sb.Append(value[i]);
            }
        }
        return sb.ToString();
    }

    private string ParseFormattingCode(ReadOnlySpan<char> code, Encoding enc)
    {
        if (code.Length == 1)
        {
            return code[0] switch
            {
                'F' => Field.ToString(),
                'S' => Component.ToString(),
                'T' => Subcomponent.ToString(),
                'R' => Repeat.ToString(),
                'E' => Escape.ToString(),
                _ => throw new FormatException($"Invalid escape sequence: unknown formatting code '{code[0]}'"),
            };
        }
        else if (code.Length >= 2 && code[0] == 'X')
        {
            // Hexadecimal character code
            var hex = code.Slice(1);
            var hexData = new byte[(hex.Length + 1) / 2];
            for (int i = 0; i < hexData.Length; i++)
            {
                var byteStr = hex.Slice(i * 2, Math.Min(2, hex.Length - i * 2));
                if (!byte.TryParse(byteStr, System.Globalization.NumberStyles.HexNumber, null, out byte b))
                {
                    throw new FormatException($"Invalid escape sequence: invalid hexadecimal code '{byteStr.ToString()}'");
                }
                hexData[i] = b;
            }
            return enc.GetString(hexData);
        }
        else if (code.Length >= 3 && code[0] == '.')
        {
            var command = code.Slice(1, 2);
            if (command.Equals("br", StringComparison.OrdinalIgnoreCase)
                || command.Equals("sp", StringComparison.OrdinalIgnoreCase)
                || command.Equals("ce", StringComparison.OrdinalIgnoreCase))
            {
                return Environment.NewLine;
            }
            else
            {
                // Dump the command as-is, since we don't know what it is
                return $"\\.{command.ToString()}\\";
            }
        }
        else throw new FormatException($"Invalid escape sequence: unknown formatting code '{code.ToString()}'");
    }

    internal ReadOnlySpan<char> TakeValue(ReadOnlySpan<char> remaining)
    {
        var iEndOfField = remaining.IndexOfAny(NonEscapeSeparators);
        if (iEndOfField < 0) return remaining;

        return remaining.Slice(0, iEndOfField);
    }
}
