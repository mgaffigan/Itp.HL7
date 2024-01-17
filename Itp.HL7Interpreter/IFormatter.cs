namespace Itp.HL7Interpreter;

public interface IFormatter
{
    object Decode(string input);

    string Encode(object input);
}