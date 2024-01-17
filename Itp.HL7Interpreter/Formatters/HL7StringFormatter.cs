namespace Itp.HL7Interpreter.Formatters;

public class HL7StringFormatter : IFormatter
{
    public static IFormatter Instance { get; } = new HL7StringFormatter();

    public object Decode(string input)
    {
        return input;
    }

    public string Encode(object input)
    {
        return input?.ToString() ?? "";
    }
}