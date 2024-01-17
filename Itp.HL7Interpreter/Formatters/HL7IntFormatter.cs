namespace Itp.HL7Interpreter.Formatters;

public class HL7IntFormatter : IFormatter
{
    public static IFormatter Instance { get; } = new HL7IntFormatter();

    public object Decode(string input)
    {
        return !string.IsNullOrWhiteSpace(input) ? int.Parse(input) : 0;
    }

    public string Encode(object input)
    {
        return input?.ToString() ?? "";
    }
}